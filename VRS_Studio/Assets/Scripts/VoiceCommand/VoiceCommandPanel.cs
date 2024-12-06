using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;  // String

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Intent;
using Microsoft.CognitiveServices.Speech.Translation;

using Wave.Native;  // Log

using System.Threading.Tasks;  // Task
using HTC.ViveSoftware.ExpLab.HandInteractionDemo;

// using HTC.Triton.LensUI; //LensUIButton

namespace Wave.VoiceCommand.Sample.Typing
{
	public class VoiceCommandPanel : MonoBehaviour
	{
		private static string LOG_TAG = "VoiceCommandPanel";

		public Text HintList;
		public Text RecognitionContent;
		public GameObject RecognitionPrompt;
		public CustomInputField panelInputField;

		private const string HintText =
			"<size=30><b>Try saying:</b></size>\n";
		private List<string> FeatureListStr = null;

		// From event handler.
		string RecognitionContentStr = String.Empty;
		string IntentStr = String.Empty;
		string EntityStr = String.Empty;

		string Intent = String.Empty;
		double IntentScore = 0f;
		// key: entity value, value: entity type
		public Dictionary<string, string> EntityDic = new Dictionary<string, string>();
		// private readonly object RecognitionContentLock = new object();

		bool isWaitingForWakeUpPhrase = false;
		bool isWaitingForIntentPhrase = false;
		bool isWaitingForIntentCompletion = false;

		float intentCompletionThreshold = 1f;
		float intentCompletionTimer = 0f;
		bool timerNeeded = false;
		bool isTimerStarted = false;
		bool resetTimer = false;

		#region RunOnMainThread
		Queue<Action> m_TasksRunOnMainThread = new Queue<Action>();
		public void RunOnMainThread(Action task)
		{
			lock (m_TasksRunOnMainThread) { m_TasksRunOnMainThread.Enqueue(task); }
		}
		private class SynthesResultOnPanelHandler
		{
			private Action<string, string> m_Action = null;
			private string m_Language = String.Empty;
			private string m_Recognitionresult = String.Empty;

			public SynthesResultOnPanelHandler(Action<string, string> Task, string Language,
											   string RecognitionResults)
			{
				m_Action = Task;
				m_Language = Language;
				m_Recognitionresult = RecognitionResults;
			}
			public void Run() { m_Action(m_Language, m_Recognitionresult); }
		}
		Queue<SynthesResultOnPanelHandler> m_TasksRunOnMainThreadTwoParams =
			new Queue<SynthesResultOnPanelHandler>();
		private void RunOnMainThreadTwoParams(Action<string, string> Task, string Language,
											  string RecognitionResults)
		{
			lock (m_TasksRunOnMainThreadTwoParams)
			{
				SynthesResultOnPanelHandler Handler =
					new SynthesResultOnPanelHandler(Task, Language, RecognitionResults);
				m_TasksRunOnMainThreadTwoParams.Enqueue(Handler);
			}
		}
		#endregion

		// Update is called once per frame
		void Update()
		{
			// Update text from event handler
			lock (m_TasksRunOnMainThread)
			{
				if (m_TasksRunOnMainThread.Count > 0)
				{
					var task = m_TasksRunOnMainThread.Dequeue();
					task();
				}
			}
			lock (m_TasksRunOnMainThreadTwoParams)
			{
				if (m_TasksRunOnMainThreadTwoParams.Count > 0)
				{
					SynthesResultOnPanelHandler Handler = m_TasksRunOnMainThreadTwoParams.Dequeue();
					Handler.Run();
				}
			}

			//Handle Wake up intent listening
			if (!isWaitingForIntentPhrase && !isWaitingForIntentCompletion)
			{
				UpdateHintText();
				if (RobotAssistantLoSCaster.isAlreadyInLoS && !isWaitingForWakeUpPhrase)
				{
					StartListeningForWakeUpIntent();
				}
				else if (!RobotAssistantLoSCaster.isAlreadyInLoS && isWaitingForWakeUpPhrase)
				{
					StopListeningForWakeUpIntent();
				}
			}
			
			// Handle voice response and intent action
			if (Intent != "")
			{
				Log.d(LOG_TAG, "Intent Handling: " + Intent);
				var IM = IntentManager.Instance;
				if (isWaitingForWakeUpPhrase && Intent == "Wake Up" && IntentScore >= 0.96)
				{
					IM.IntentInvoking(EntityDic, Intent, IntentScore);
				}
				else if (isWaitingForIntentPhrase && Intent != "Wake Up")
				{
					if (Intent != "Switch Voice Command")
					{
						IM.IntentInvoking(EntityDic, Intent, IntentScore);
					}
					else
					{
						HandleVoiceCommandIntent(EntityDic, Intent, IntentScore);
					}
					isWaitingForIntentPhrase = false;
					isWaitingForIntentCompletion = true;
				}
				Intent = "";
			}

			if (isWaitingForIntentCompletion) //For recognize/translate only 
			{
				if (timerNeeded)
				{
					if (isTimerStarted)
					{
						if (resetTimer)
						{
							intentCompletionTimer = 0f;
							resetTimer = false;
						}
						else
						{
							intentCompletionTimer += Time.deltaTime;
						}
					}
					else
					{
						intentCompletionTimer = 0f;
						isTimerStarted = true;
					}

					if (intentCompletionTimer >= intentCompletionThreshold)
					{
						SpeechIntentCompletionAction();
						timerNeeded = false;
						resetTimer = false;
						isTimerStarted = false;
						intentCompletionTimer = 0f;
					}
				}
				else
				{
					resetTimer = false;
					isTimerStarted = false;
					intentCompletionTimer = 0f;
				}
			}
		}

		private void OnEnable()
		{
			Log.d(LOG_TAG, "OnEnable");
			//m_ChooseStatus = ChooseStatus.Choosing;
			RecognitionContent.text = RecognitionContentStr = String.Empty;

			VoiceCommandManager VM = VoiceCommandManager.Instance;
			VM.CheckMicrophoneRecordAudioPermission();
			SetupToChoosingStatus();

			isWaitingForWakeUpPhrase = false;
			isWaitingForIntentPhrase = false;
			isWaitingForIntentCompletion = false;

			IntentManager IM = IntentManager.Instance;
			IM.WakeUpEvent.AddListener(OnReceiveWakeUpIntent);
			IM.ExitAppEvent.AddListener(ExitApp);
			IM.TurnOnPassthroughEvent.AddListener(TurnOnPassthrough);
			IM.TurnOffPassthroughEvent.AddListener(TurnOffPassthrough);
			IM.UnknownIntent.AddListener(UnknownIntentAction);
		}
		private async void OnDisable()
		{
			Log.d(LOG_TAG, "OnDisable");
			//m_ChooseStatus = ChooseStatus.Choosing;
			RecognitionContent.text = RecognitionContentStr = String.Empty;
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.StopAll();

			IntentManager IM = IntentManager.Instance;
			IM.WakeUpEvent.RemoveListener(OnReceiveWakeUpIntent);
			IM.ExitAppEvent.RemoveListener(ExitApp);
			IM.TurnOnPassthroughEvent.RemoveListener(TurnOnPassthrough);
			IM.TurnOffPassthroughEvent.RemoveListener(TurnOffPassthrough);
			IM.UnknownIntent.RemoveListener(UnknownIntentAction);
		}

		private const string FeatureList_Recognition_US_EN = "Recognize English.";
		private const string FeatureList_Recognition_US_CN = "識別英文。";
		private const string FeatureList_Recognition_CH_EN = "Recognize Chinese.";
		private const string FeatureList_Recognition_CH_CN = "識別中文。";
		private const string FeatureList_Translation_US_EN = "Translate from English to Chinese.";
		private const string FeatureList_Translation_US_CN = "翻譯英文到中文。";
		private const string FeatureList_Translation_CH_EN = "Translate from Chinese to English.";
		private const string FeatureList_Translation_CH_CN = "翻譯中文到英文。";
		private const string FeatureList_BackToChoosing_US = "Try another voice feature.";
		private const string FeatureList_BackToChoosing_CH = "嘗試其它語音功能。";
		private const string FeatureList_Dismiss_US = "Bye-Bye. See you.";
		private const string FeatureList_Dismiss_CH = "再見。";

		#region SetupFeatureListStatus
		private ActiveLanguage currentActiveLanguage = ActiveLanguage.EN;
		private enum ActiveLanguage
		{
			EN,
			CN,
		}

		private async void SetupToChoosingStatus()
		{
			FeatureListStr = new List<string> {
			FeatureList_Recognition_US_EN,       FeatureList_Recognition_CH_EN,
			FeatureList_Translation_US_EN,       FeatureList_Translation_CH_EN,
		    };
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.StopAll();

			isWaitingForIntentCompletion = false;

			UpdateHintText();
		}
		// Means chosen FeatureList_Recognition_US or FeatureList_Translation_US
		private void SetupFeatureListEnglishChoosedStatus()
		{
			currentActiveLanguage = ActiveLanguage.EN;
			FeatureListStr = new List<string> {
			FeatureList_Recognition_US_EN,       FeatureList_Recognition_CH_EN,
			FeatureList_Translation_US_EN,       FeatureList_Translation_CH_EN,
			FeatureList_BackToChoosing_US,       FeatureList_Dismiss_US };
		}
		// Means chosen FeatureList_Recognition_CH or FeatureList_Translation_CH
		private void SetupFeatureListChineseChoosedStatus()
		{
			currentActiveLanguage = ActiveLanguage.CN;
			FeatureListStr = new List<string> {
			FeatureList_Recognition_US_CN,       FeatureList_Recognition_CH_CN,
			FeatureList_Translation_US_CN,       FeatureList_Translation_CH_CN,
			FeatureList_BackToChoosing_CH,       FeatureList_Dismiss_CH };
		}
		#endregion

		private async void OnReceiveWakeUpIntent()
		{
			await SynthesizeVoiceAssistantSpeech("I'm here. How may I help you?");

			isWaitingForWakeUpPhrase = false;
			isWaitingForIntentPhrase = true;
			UpdateHintText();

			if (currentActiveLanguage == ActiveLanguage.EN)
			{
				await RecognizeIntentEnglish();
			}
			else if (currentActiveLanguage == ActiveLanguage.CN)
			{
				await RecognizeIntentChinese();
			}
		}

		private async void StartListeningForWakeUpIntent()
		{
			Log.d(LOG_TAG, "StartListeningForWakeUpIntent");

			if (isWaitingForIntentPhrase || isWaitingForIntentCompletion) return;

			isWaitingForWakeUpPhrase = true;

			UpdateHintText();

			if (currentActiveLanguage == ActiveLanguage.EN)
			{
				await RecognizeIntentEnglish();
			}
			else if (currentActiveLanguage == ActiveLanguage.CN)
			{
				await RecognizeIntentChinese();
			}
		}
		private async void StopListeningForWakeUpIntent()
		{
			Log.d(LOG_TAG, "StopListeningForWakeUpIntent");

			isWaitingForWakeUpPhrase = false;

			if (isWaitingForIntentPhrase || isWaitingForIntentCompletion) return;

			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.StopIntent();
		}

		private void UpdateHintText()
		{
			if (isWaitingForIntentPhrase) //Show intent hint list
			{
				string TextContent = HintText;
				for (int i = 0; i < FeatureListStr.Count; i++)
				{
					TextContent += System.Environment.NewLine + FeatureListStr[i];
				}

				TextContent += System.Environment.NewLine + "Enable Passthrough.";
				TextContent += System.Environment.NewLine + "Disable Passthrough.";
				TextContent += System.Environment.NewLine + "Exit App.";

				HintList.text = TextContent;
			}
			else //Show wake up hist text
			{
				string TextContent = "Look at the robot and try saying \"Hi\"!";

				HintList.text = TextContent;
			}
		}

		#region Intent Actions

		private async void SpeechIntentCompletionAction()
		{
			Log.d(LOG_TAG, "SpeechIntentCompletionAction");
			
			await SynthesizeVoiceAssistantSpeech("Done, call me again when you need me.");

			SetupToChoosingStatus();
		}

		private async void UnknownIntentAction()
		{
			await SynthesizeVoiceAssistantSpeech("Sorry, I don't understand. Could you please say that again?");

			isWaitingForIntentPhrase = true;
			isWaitingForIntentCompletion = false;
		}


		private async void ExitApp()
		{
			await SynthesizeVoiceAssistantSpeech("Exiting application now. See you next time.");

			Log.d(LOG_TAG, "Exit App");
			Application.Quit();
		}

		private async void TurnOnPassthrough()
		{
			await SynthesizeVoiceAssistantSpeech("OK, turning on passthrough.");

			Log.d(LOG_TAG, "Turn on passthrough");
			Interop.WVR_ShowPassthroughOverlay(true);
			isWaitingForIntentPhrase = true;
			isWaitingForIntentCompletion = false;

			if (currentActiveLanguage == ActiveLanguage.EN)
			{
				await RecognizeIntentEnglish();
			}
			else if (currentActiveLanguage == ActiveLanguage.CN)
			{
				await RecognizeIntentChinese();
			}
		}

		private async void TurnOffPassthrough()
		{
			await SynthesizeVoiceAssistantSpeech("OK, turning off passthrough.");

			Log.d(LOG_TAG, "Turn off passthrough");
			Interop.WVR_ShowPassthroughOverlay(false);
			isWaitingForIntentCompletion = false;
		}

		private void NotifyPaneltoSynthesisResults(string Language, string RecognitionContentStr)
		{
			RunOnMainThread(DisableRecognitionPrompt);

			if (panelInputField.text != "") panelInputField.text = panelInputField.text + "\n";

			if (RecognitionContentStr != "") panelInputField.text = panelInputField.text + RecognitionContentStr;

			Log.d(LOG_TAG, "NotifyPaneltoSynthesisResults, Language is " + Language +
							   " and RecognitionContentStr is " + RecognitionContentStr);
		}

		private void UpdateRecognitionContent()
		{
			// Update UI text
			if (RecognitionContent != null)
			{
				if (!RecognitionPrompt.activeSelf) RecognitionPrompt.SetActive(true);

				RecognitionContent.text = RecognitionContentStr;
			}
		}

		private void DisableRecognitionPrompt()
		{
			if (RecognitionContent != null)
			{
				if (RecognitionPrompt.activeSelf) RecognitionPrompt.SetActive(false);

				RecognitionContent.text = "";
			}
		}

		string voiceCommandEntityString = "voice command";
		public async void HandleVoiceCommandIntent(Dictionary<string, string> EntityDic, string Intent,
								 double IntentScore)
		{
			Debug.Log("HandleVoiceCommandIntent");
			var VM = VoiceCommandManager.Instance;
			// Handle intent action
			if (Intent == "") return;
			// Manage Passthrough
			else if (Intent == "Switch Voice Command" && IntentScore >= 0.96)
			{
				if (EntityDic.ContainsKey(voiceCommandEntityString))
				{
					if (EntityDic[voiceCommandEntityString] == "recognize english")
					{
						await RecognizeEnglish();
					}
					else if (EntityDic[voiceCommandEntityString] == "recognize chinese")
					{
						await RecognizeChinese();
					}
					else if (EntityDic[voiceCommandEntityString] == "translate from english to chinese")
					{
						await TranslateEnglish();
					}
					else if (EntityDic[voiceCommandEntityString] == "translate from chinese to english")
					{
						await TranslateChinese();
					}
					else if (EntityDic[voiceCommandEntityString] == "finish")
					{
						SetupToChoosingStatus();
					}
				}
			}
			else
			{
				await SynthesizeVoiceAssistantSpeech("Sorry, I don't understand. Could you please say that again?");
			}
		}


		private async Task SynthesizeVoiceAssistantSpeech(string dialog, string language = "en-US")
		{
			string voice = "en-US-GuyNeural";

			if (language.Equals("en-US"))
			{
				voice = "en-US-GuyNeural";
			}
			else if (language.Equals("zh-TW"))
			{
				voice = "zh-TW-YunJheNeural";
			}

			await VoiceCommandManager.Instance.ReInitializeSpeechSynthesizer(language, voice);

			//VoiceCommandManager.Instance.SpeechSynthesizerComponent.SynthesisStarted += OnVoiceAssistantSpeechBegin;

			OnVoiceAssistantSpeechBegin(dialog);
			await VoiceCommandManager.Instance.StartSynthesis(dialog);
		}

		private void OnVoiceAssistantSpeechBegin(string dialog)
		{
			StartCoroutine(VoiceAssistantSpeechBubbleCoroutine(dialog));
		}

		IEnumerator VoiceAssistantSpeechBubbleCoroutine(string dialog)
		{
			RobotAssistantManager robotAssistantManagerInstance = RobotAssistantManager.Instance;
			robotAssistantManagerInstance.robotAssistantSpeechBubble.ClearSpeechBubble();
			robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(true);
			yield return new WaitForSecondsRealtime(1.5f);
			robotAssistantManagerInstance.robotAssistantSpeechBubble.RobotLines = dialog;
			robotAssistantManagerInstance.robotAssistantSpeechBubble.typingInterval = 0.05f;
			yield return StartCoroutine(robotAssistantManagerInstance.robotAssistantSpeechBubble.PlayTypingWordAnim());
			yield return new WaitForSecondsRealtime(1f);
			robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(false);
		}
		#endregion

		private async Task RecognizeEnglish()
		{
			SetupFeatureListEnglishChoosedStatus();
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("en-US");
			VM.SpeechRecognizerComponent.Recognizing += RecognizingHandler;
			VM.SpeechRecognizerComponent.Recognized += RecognizedHandler;
			VM.SpeechRecognizerComponent.Canceled += CanceledHandler;
			await SynthesizeVoiceAssistantSpeech("Sure! Please speak English now.");
			await VM.StartRecognition();
		}
		private async Task RecognizeChinese()
		{
			SetupFeatureListChineseChoosedStatus();
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("zh-TW");
			VM.SpeechRecognizerComponent.Recognizing += RecognizingHandler;
			VM.SpeechRecognizerComponent.Recognized += RecognizedHandler;
			VM.SpeechRecognizerComponent.Canceled += CanceledHandler;
			await SynthesizeVoiceAssistantSpeech("沒問題!請開始講中文吧!", "zh-TW");
			await VM.StartRecognition();
		}
		private async Task TranslateEnglish()
		{
			SetupFeatureListEnglishChoosedStatus();
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("en-US");
			VM.TranslationRecognizerComponent.Recognizing += TranslationRecognizingHandler;
			VM.TranslationRecognizerComponent.Recognized += TranslationRecognizedHandler;
			VM.TranslationRecognizerComponent.Canceled += TranslationCanceledHandler;
			await SynthesizeVoiceAssistantSpeech("Great! Let's Translate English to Chinese.");
			await VM.StartTranslation();
		}
		private async Task TranslateChinese()
		{
			SetupFeatureListChineseChoosedStatus();
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("zh-TW");
			VM.TranslationRecognizerComponent.Recognizing += TranslationRecognizingHandler;
			VM.TranslationRecognizerComponent.Recognized += TranslationRecognizedHandler;
			VM.TranslationRecognizerComponent.Canceled += TranslationCanceledHandler;
			await SynthesizeVoiceAssistantSpeech("太棒了!讓我們來翻譯中文到英文。", "zh-TW");
			await VM.StartTranslation();
		}
		private async Task RecognizeIntentEnglish()
		{
			Log.d(LOG_TAG, "RecognizeIntentEnglish");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("en-US");
			VM.IntentRecognizerComponent.Recognizing += IntentRecognizingHandler;
			VM.IntentRecognizerComponent.Recognized += IntentRecognizedHandler;
			VM.IntentRecognizerComponent.Canceled += IntentCanceledHandler;
			await VM.StartIntent();
		}
		private async Task RecognizeIntentChinese()
		{
			Log.d(LOG_TAG, "RecognizeIntentChinese");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			await VM.ReInitializeAll("zh-TW");
			VM.IntentRecognizerComponent.Recognizing += IntentRecognizingHandler;
			VM.IntentRecognizerComponent.Recognized += IntentRecognizedHandler;
			VM.IntentRecognizerComponent.Canceled += IntentCanceledHandler;
			await VM.StartIntent();
		}
		#region Recognition Event Handlers
		private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
		{
			RecognitionContentStr = e.Result.Text;
			Log.d(LOG_TAG, "RecognizingHandler:" + RecognitionContentStr);
			RunOnMainThread(UpdateRecognitionContent);

			if (isTimerStarted) resetTimer = true;
		}
		private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
		{
			// string TextToSpeechMessage = String.Empty;
			RecognitionContentStr = e.Result.Text;
			Log.d(LOG_TAG, "RecognizedHandler:" + RecognitionContentStr);
			RunOnMainThread(UpdateRecognitionContent);
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			RunOnMainThreadTwoParams(NotifyPaneltoSynthesisResults,
									 VM.SpeechRecognizerComponent.RecognizedLanguage,
									 RecognitionContentStr);

			timerNeeded = true;
		}
		private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
		{
			RecognitionContentStr = e.ErrorDetails.ToString();
			Log.d(LOG_TAG, "CanceledHandler:" + RecognitionContentStr);
			RunOnMainThread(UpdateRecognitionContent);

			timerNeeded = false;
		}

		#endregion

		#region Translation Event Handlers
		private void TranslationRecognizingHandler(object sender, TranslationRecognitionEventArgs e)
		{
			Log.d(LOG_TAG, "TranslationRecognizingHandler");
			// lock (RecognitionContentLock)
			//{
			RecognitionContentStr = "[From]" + e.Result.Text;
			foreach (var element in e.Result.Translations)
			{
				RecognitionContentStr += System.Environment.NewLine + "[To]" + element.Value;
			}
			Log.d(LOG_TAG, "TranslationRecognizingHandler:" + RecognitionContentStr);
			RunOnMainThread(UpdateRecognitionContent);

			if (isTimerStarted) resetTimer = true;
			//}
		}
		private void TranslationRecognizedHandler(object sender, TranslationRecognitionEventArgs e)
		{
			Log.d(LOG_TAG, "TranslationRecognizedHandler");
			string TextToSpeechMessage = String.Empty;
			// lock (RecognitionContentLock)
			//{
			if (e.Result.Reason == ResultReason.NoMatch)
			{
				Log.d(LOG_TAG, "[Translate]RecognizedHandler skipped due to empty message");
				return;
			}
			TextToSpeechMessage = "[From]" + e.Result.Text;
			foreach (var element in e.Result.Translations)
			{
				TextToSpeechMessage += System.Environment.NewLine + "[To]" + element.Value;
			}
			RecognitionContentStr = TextToSpeechMessage;
			RunOnMainThread(UpdateRecognitionContent);

			timerNeeded = true;
			//}

			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (VM.TranslationRecognizerComponent.RecognizedLanguage.Equals("en-US"))
			{
				RunOnMainThreadTwoParams(NotifyPaneltoSynthesisResults, "zh-TW", TextToSpeechMessage);
				// await SynthesizeVoiceAssistantSpeech(, );
			}
			else if (VM.TranslationRecognizerComponent.RecognizedLanguage.Equals("zh-TW"))
			{
				RunOnMainThreadTwoParams(NotifyPaneltoSynthesisResults, "en-US", TextToSpeechMessage);
				// await SynthesizeVoiceAssistantSpeech(TextToSpeechMessage);
			}
		}
		private void TranslationCanceledHandler(object sender,
												TranslationRecognitionCanceledEventArgs e)
		{
			// lock(RecognitionContentLock) {
			RecognitionContentStr = e.ErrorDetails.ToString();
			Log.d(LOG_TAG, "TranslationCanceledHandler:" + RecognitionContentStr);
			RunOnMainThread(UpdateRecognitionContent);

			timerNeeded = false;
			//}
		}
		#endregion

		#region Intent Recognition Event Handlers
		private void IntentRecognizingHandler(object sender, IntentRecognitionEventArgs e)
		{
			RecognitionContentStr = e.Result.Text;
			Log.d(LOG_TAG, "IntentRecognizingHandler:" + RecognitionContentStr);
			IntentStr = EntityStr = "";
			Intent = "";
			IntentScore = 0f;
			EntityDic.Clear();
			//RunOnMainThread(UpdateRecognitionContent);
		}
		private void IntentRecognizedHandler(object sender, IntentRecognitionEventArgs e)
		{
			// string TextToSpeechMessage = String.Empty;
			if (e.Result.Reason == ResultReason.RecognizedIntent)
			{
				RecognitionContentStr = e.Result.Text;
				Log.d(LOG_TAG, "IntentRecognizedHandler(text):" + RecognitionContentStr);
				Log.d(LOG_TAG, "IntentRecognizedHandler(intent_id):" + e.Result.IntentId);

				if (RecognitionContentStr.Length > 0)
				{
					var json = e.Result.Properties.GetProperty(
						PropertyId.LanguageUnderstandingServiceResponse_JsonResult);
					if (json.Length > 0)
					{
						var jsonRoot = JsonUtility.FromJson<JsonRoot>(json);
						Intent = jsonRoot.topScoringIntent.intent;
						IntentScore = jsonRoot.topScoringIntent.score;
						IntentStr = Intent + ":" + IntentScore;
						//Log.d(LOG_TAG, "IntentRecognizedHandler(IntentName):" + IntentName +
						//				   "IntentRecognizedHandler(IntentScore):" + IntentScore);

						var entities = jsonRoot.entities;
						for (int i = 0; i < entities.Count; i++)
						{
							EntityDic.Add(entities[i].type, entities[i].entity);
							EntityStr += entities[i].type + ":" + entities[i].entity;
							EntityStr += "  ";
						}
					}
				}
				Log.d(LOG_TAG, "IntentRecognizedHandler(intent name):" + IntentStr);
				Log.d(LOG_TAG, "IntentRecognizedHandler(entity name):" + EntityStr);
			}
			else if (e.Result.Reason == ResultReason.RecognizedSpeech)
			{
				RecognitionContentStr = e.Result.Text;
				Log.d(LOG_TAG, "IntentRecognizedHandler(text):" + RecognitionContentStr);
				Log.d(LOG_TAG, "Intent not recognized.");
			}
			else if (e.Result.Reason == ResultReason.NoMatch)
			{
				Log.d(LOG_TAG, "NOMATCH: Speech could not be recognized.");
			}
			var VM = VoiceCommandManager.Instance;
			//RunOnMainThread(UpdateRecognitionContent);
		}

		private void IntentCanceledHandler(object sender, IntentRecognitionCanceledEventArgs e)
		{
			var cancellation = CancellationDetails.FromResult(e.Result);
			Log.d(LOG_TAG, "IntentCanceledHandler(canceled reason):" + cancellation.Reason);
			if (cancellation.Reason == CancellationReason.Error)
			{
				IntentStr = cancellation.ErrorCode.ToString() + " " + cancellation.ErrorDetails.ToString();
				Log.d(LOG_TAG, "IntentCanceledHandler(canceled error):" + IntentStr);
				Log.d(LOG_TAG, "Intent canceled: Did you update the subscription info?");
			}
			//RunOnMainThread(UpdateRecognitionContent);
		}
		#endregion
	}

	// For parsing intent recognition result json string
	[Serializable]
	public class JsonRoot
	{
		public string query;
		public TopScoringIntent topScoringIntent;
		public List<Entities> entities;
	}
	[Serializable]
	public class TopScoringIntent
	{
		public string intent;
		public double score;
	}
	[Serializable]
	public class Entities
	{
		public string entity;
		public string type;
		public int startIndex;
		public int endIndex;
		public double score;
		public Resolution resolution;
	}
	[Serializable]
	public class Resolution
	{
		public List<string> values;
	}
}