﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; //List
using Wave.Native; //Log

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System; //String
using HTC.ViveSoftware.ExpLab.HandInteractionDemo; //CustomInputField
using HTC.Triton.LensUI; //LensUIButton
using System.Threading.Tasks;

namespace Wave.VoiceCommand
{
	public class SpeechToText : MonoBehaviour
	{
		private static string LOG_TAG = "VoiceCommand";

		public CustomInputField InputFieldText;
		public LensUIButton LensUIButton_VC;
		public Image ButtonImage_VC;

		#region ButtonFunctionStat
		private const int ButtonFunctionStatCount = 5;
		enum ButtonFunctionStat
		{
			None,
			Recognition_US,
			Recognition_CH,
			Translation_US,
			Translation_CH,
		}
		private ButtonFunctionStat ButtonStat = ButtonFunctionStat.None;
		#endregion

		//Message from Recognition/Translation Event Handler.
		private string message = String.Empty;
		//For integrate with InputField.
		private string InputFieldTextWhlieStartRecognize = String.Empty;

		private VoiceCommandManager.RecognitionHandlerStat HandlerStat
		{
			get
			{
				VoiceCommandManager VM = VoiceCommandManager.Instance;
				VoiceCommandManager.RecognitionHandlerStat m_HandlerStat = VoiceCommandManager.RecognitionHandlerStat.None;
				if (VM.IsStartedSpeechRecognition)
				{
					m_HandlerStat = VM.SpeechRecognitionHandlerStat;
				}
				else if (VM.IsStartedTranslation)
				{
					m_HandlerStat = VM.TranslationRecognitionHandlerStat;
				}
				return m_HandlerStat;
			}
		}

		enum SingleFunctionStat
		{
			Recognition,
			Translation
		}
		private SingleFunctionStat FunctionStat = SingleFunctionStat.Recognition;

		private readonly object threadLocker = new object();

		private bool IsButtonClickProcessing = false;
		private readonly object ButtonClickProcessingLocker = new object();

		//To fire hint once.
		bool FiredHint = false;

		//MultiThread commuticate
		Queue<Action> m_TasksRunOnMainThread = new Queue<Action>();
		public void RunOnMainThread(Action task)
		{
			lock (m_TasksRunOnMainThread)
			{
				m_TasksRunOnMainThread.Enqueue(task);
			}
		}

		// OnClick callback of LensUIButton_VC.
		public async void ButtonClick()
		{
			Log.d(LOG_TAG, "ButtonClick");
			lock (ButtonClickProcessingLocker)
			{
				if (IsButtonClickProcessing)
				{
					Log.d(LOG_TAG, "IsButtonClickProcessing is true, skip it");
					return;
				}
				IsButtonClickProcessing = true;
			}

			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (!VM.IsMicPermissionGranted && !VM.IsWaitPermissionCallBack)
			{
				VM.CheckMicrophoneRecordAudioPermission();
				return;
			}

			//Iterative go next.
			bool ButtonStatIsNone = (ButtonStat == ButtonFunctionStat.None) ? true : false;
			if ((ButtonStatIsNone && !VM.IsStartedRecognition) || (!ButtonStatIsNone && VM.IsStartedRecognition))
			{
				int ButtonStatInt = (int)ButtonStat;
				Log.d(LOG_TAG, "[ButtonStat] Go from " + ButtonStat.ToString());
				ButtonStat = (ButtonFunctionStat)((ButtonStatInt + 1) % ButtonFunctionStatCount);
				Log.d(LOG_TAG, "[ButtonStat] To " + ButtonStat.ToString());
			}

			switch (ButtonStat)
			{
				case ButtonFunctionStat.None:
					Log.d(LOG_TAG, "ButtonClick: None");
					await VM.ReInitializeAll("en-US");
					AddSpeechRecognitionHandler();
					break;
				case ButtonFunctionStat.Recognition_US:
					Log.d(LOG_TAG, "ButtonClick: Recognition_US");
					await VM.ReInitializeAll("en-US");
					AddSpeechRecognitionHandler();
					FunctionStat = SingleFunctionStat.Recognition;
					await StartRecognition();
					break;
				case ButtonFunctionStat.Recognition_CH:
					Log.d(LOG_TAG, "ButtonClick: Recognition_CH");
					await VM.ReInitializeAll("zh-TW");
					AddSpeechRecognitionHandler();
					FunctionStat = SingleFunctionStat.Recognition;
					await StartRecognition();
					break;
				case ButtonFunctionStat.Translation_US:
					Log.d(LOG_TAG, "ButtonClick: Recognition_US");
					await VM.ReInitializeAll("en-US");
					AddTranslationHandler();
					FunctionStat = SingleFunctionStat.Translation;
					await StartRecognition();
					break;
				case ButtonFunctionStat.Translation_CH:
					Log.d(LOG_TAG, "ButtonClick: Recognition_CH");
					await VM.ReInitializeAll("zh-TW");
					AddTranslationHandler();
					FunctionStat = SingleFunctionStat.Translation;
					await StartRecognition();
					break;
				default:
					Log.e(LOG_TAG, "Should never print this line.");
					break;
			}

			lock (ButtonClickProcessingLocker)
			{
				IsButtonClickProcessing = false;
			}
		}

		private async Task StartRecognition()
		{
			Log.d(LOG_TAG, "StartRecognition");
			VoiceCommandManager VM = VoiceCommandManager.Instance;

			lock (threadLocker)
			{
				if (InputFieldText != null)
				{
					InputFieldTextWhlieStartRecognize = InputFieldText.text;
				}
			}

			RunOnMainThread(UpdateOutputText);

			if (SingleFunctionStat.Recognition == FunctionStat)
			{
				await VM.StartRecognition();
			}
			else if (SingleFunctionStat.Translation == FunctionStat)
			{
				await VM.StartTranslation();
			}
		}

		private async Task StopRecognition()
		{
			Log.d(LOG_TAG, "StopRecognition");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (SingleFunctionStat.Recognition == FunctionStat)
			{
				await VM.StopRecognition();
			}
			else if (SingleFunctionStat.Translation == FunctionStat)
			{
				await VM.StopTranslation();
			}
		}

		private async void Start()
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (InputFieldText == null)
			{
				UnityEngine.Debug.LogError("InputFieldText property is null! Assign a CustomInputField to it.");
			}
			else if (LensUIButton_VC == null)
			{
				UnityEngine.Debug.LogError("LensUIButton_VC property is null! Assign a LensUIButton to it.");
			}
			else if (ButtonImage_VC == null)
			{
				UnityEngine.Debug.LogError("ButtonImage_VC property is null! Assign a ButtonImage to it.");
			}
			else
			{
				VM.CheckMicrophoneRecordAudioPermission();
				await VM.ReInitializeAll();
			}
		}

		async private void FeatureHintFireOnceAfterPermissionGranted()
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (VM.IsMicPermissionGranted && !FiredHint)
			{
				FiredHint = true;
				await VM.FireOnceSpeakTextAsync("en-US", "Please try to intput words by typing and try the voice features by pressing the bottom left button.");
			}
		}
		void Awake()
		{
			FiredHint = false;
		}
		private void Update()
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (!VM.IsMicPermissionGranted) return;
			FeatureHintFireOnceAfterPermissionGranted();

			if (LensUIButton_VC != null)
			{
				LensUIButton_VC.interactable = !VM.IsReinitProcessing;
			}
			// Run UpdateOutputText on main thread or will encounter null access.
			lock (m_TasksRunOnMainThread)
			{
				if (m_TasksRunOnMainThread.Count > 0)
				{
					var task = m_TasksRunOnMainThread.Dequeue();
					task();
				}

			}
			StopRecognitionIfOtherButtonsPressed();
			UpdateButtonImage();
		}

		private async void StopRecognitionIfOtherButtonsPressed()
		{
			//Log.d(LOG_TAG, "StopRecognitionIfOtherButtonsPressed");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			bool NeedStopRecognized = false;

			lock (threadLocker)
			{
				if (LensUIButton_VC != null)
				{
					LensUIButton_VC.interactable = !VM.IsReinitProcessing;
				}
				if (InputFieldText != null && InputFieldText.text != null
						&& VM.IsStartedRecognition && !(VoiceCommandManager.RecognitionHandlerStat.Recognizing == HandlerStat)
						&& !String.Equals(InputFieldText.text, InputFieldTextWhlieStartRecognize))
				{
					Log.d(LOG_TAG, "StopRecognition because inputfield content changed (other key pressed).");
					NeedStopRecognized = true;
				}
			}
			if (NeedStopRecognized)
			{
				await StopRecognition();
			}
			InputFieldText.MoveTextEnd(false);
		}

		private async void UpdateOutputText()
		{
			Log.d(LOG_TAG, "UpdateOutputText");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			bool NeedStopRecognized = false;

			lock (threadLocker)
			{
				/*
				if (LensUIButton_VC != null)
				{
						LensUIButton_VC.interactable = !VM.IsReinitProcessing;
				}
				*/
				if (InputFieldText != null && InputFieldText.text != null && VM.IsStartedRecognition)
				{
					switch (HandlerStat)
					{
						case VoiceCommandManager.RecognitionHandlerStat.None:
							Log.d(LOG_TAG, "UpdateOutputText: None (Never enter this case)");
							break;
						case VoiceCommandManager.RecognitionHandlerStat.Recognizing:
							Log.d(LOG_TAG, "UpdateOutputText: Recognizing");
							if (String.IsNullOrEmpty(InputFieldTextWhlieStartRecognize))
							{
								InputFieldText.text = message;
							}
							else
							{
								InputFieldText.text = InputFieldTextWhlieStartRecognize + System.Environment.NewLine + message;
							}
							break;
						case VoiceCommandManager.RecognitionHandlerStat.Recognized:
							Log.d(LOG_TAG, "UpdateOutputText: Recognized");
							InputFieldText.text = InputFieldTextWhlieStartRecognize;
							break;
						case VoiceCommandManager.RecognitionHandlerStat.Canceled:
							Log.d(LOG_TAG, "UpdateOutputText: Canceled");
							InputFieldText.text = InputFieldTextWhlieStartRecognize;
							NeedStopRecognized = true;
							break;
						default:
							Log.e(LOG_TAG, "Should never print this line.");
							break;
					}
				}
			}
			if (NeedStopRecognized)
			{
				await StopRecognition();
			}
			InputFieldText.MoveTextEnd(false);
		}



		void UpdateButtonImage()
		{
			Log.d(LOG_TAG, "UpdateButtonImage: ButtonStat(" + ButtonStat.ToString() + ")");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			//Log.d(LOG_TAG, "UpdateButtonImage VM.IsStartedRecognition(" + VM.IsStartedRecognition.ToString() + "), "
			//        + "VM.IsStartedSpeechRecognition(" + VM.IsStartedSpeechRecognition.ToString() + "), "
			//        + "VM.IsStartedTranslation(" + VM.IsStartedTranslation.ToString() + ")"
			//        );
			if (!VM.IsStartedRecognition)
			{
				//Black
				ButtonImage_VC.color = new Color32(0, 0, 0, 255);
			}
			else if (ButtonFunctionStat.Recognition_US == ButtonStat)
			{
				//Yellow
				ButtonImage_VC.color = new Color32(255, 255, 0, 255);
			}
			else if (ButtonFunctionStat.Recognition_CH == ButtonStat)
			{
				//Green
				ButtonImage_VC.color = new Color32(0, 255, 0, 255);
			}
			else if (ButtonFunctionStat.Translation_US == ButtonStat)
			{
				//Blue
				ButtonImage_VC.color = new Color32(0, 0, 255, 255);
			}
			else if (ButtonFunctionStat.Translation_CH == ButtonStat)
			{
				//Purple
				ButtonImage_VC.color = new Color32(255, 0, 255, 255);
			}
		}

		private void AddSpeechRecognitionHandler()
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			VM.SpeechRecognizerComponent.Recognizing += RecognizingHandler;
			VM.SpeechRecognizerComponent.Recognized += RecognizedHandler;
			VM.SpeechRecognizerComponent.Canceled += CanceledHandler;
			VM.SpeechRecognizerComponent.SessionStarted += SessionStartedHandler;
		}
		#region SpeechRecognitionHandler
		private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
		{
			lock (threadLocker)
			{
				message = e.Result.Text;
				Log.d(LOG_TAG, "RecognizingHandler:" + message);
			}
			RunOnMainThread(UpdateOutputText);
		}
		async private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			string TextToSpeechMessage = String.Empty;
			lock (threadLocker)
			{
				message = e.Result.Text;

				if (String.IsNullOrEmpty(message))
				{
					Log.d(LOG_TAG, "RecognizedHandler skipped due to empty message");
					return;
				}
				if (String.IsNullOrEmpty(InputFieldTextWhlieStartRecognize))
				{
					InputFieldTextWhlieStartRecognize = message;
				}
				else
				{
					InputFieldTextWhlieStartRecognize += System.Environment.NewLine + message;
				}
				Log.d(LOG_TAG, "RecognizedHandler:" + message);
				TextToSpeechMessage = message;
			}
			RunOnMainThread(UpdateOutputText);

			await VM.FireOnceSpeakTextAsync(VM.SpeechRecognizerComponent.RecognizedLanguage, TextToSpeechMessage);
		}
		private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
		{
			lock (threadLocker)
			{
				message = e.ErrorDetails.ToString();
				//TODO: Handle the key token is expired.
				Log.d(LOG_TAG, "CanceledHandler:" + message);
				message = "Please make sure your network, speech key and location/region are available.";

				if (String.IsNullOrEmpty(InputFieldTextWhlieStartRecognize))
				{
					InputFieldTextWhlieStartRecognize = message;
				}
				else
				{
					InputFieldTextWhlieStartRecognize += System.Environment.NewLine + message;
				}
			}
			RunOnMainThread(UpdateOutputText);
		}

		async private void SessionStartedHandler(object sender, SessionEventArgs e)
		{
			//At the time recognition is started.
			Log.d(LOG_TAG, "SessionStartedHandler");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			switch (VM.TranslationRecognizerComponent.RecognizedLanguage)
			{
				case "en-US":
					await VM.FireOnceSpeakTextAsync(VM.SpeechRecognizerComponent.RecognizedLanguage, "Please speak english");
					break;
				case "zh-TW":
					await VM.FireOnceSpeakTextAsync(VM.SpeechRecognizerComponent.RecognizedLanguage, "請講中文");
					break;
				default:
					Log.e(LOG_TAG, "Should never print this line.");
					break;
			}
		}
		#endregion

		private void AddTranslationHandler()
		{
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			VM.TranslationRecognizerComponent.Recognizing += TranslationRecognizingHandler;
			VM.TranslationRecognizerComponent.Recognized += TranslationRecognizedHandler;
			VM.TranslationRecognizerComponent.Canceled += TranslationCanceledHandler;
			VM.TranslationRecognizerComponent.SessionStarted += TranslationSessionStartedHandler;
		}
		#region TranslationHandler
		private void TranslationRecognizingHandler(object sender, TranslationRecognitionEventArgs e)
		{
			lock (threadLocker)
			{
				message = "[From]" + e.Result.Text;
				foreach (var element in e.Result.Translations)
				{
					message += System.Environment.NewLine + "[To]" + element.Value;
				}
				Log.d(LOG_TAG, "[Translate]RecognizingHandler:" + message);
			}
			RunOnMainThread(UpdateOutputText);
		}
		async private void TranslationRecognizedHandler(object sender, TranslationRecognitionEventArgs e)
		{
			string TextToSpeechMessage = String.Empty;
			lock (threadLocker)
			{
				if (e.Result.Reason == ResultReason.NoMatch)
				{
					Log.d(LOG_TAG, "[Translate]RecognizedHandler skipped due to empty message");
					return;
				}
				if (String.IsNullOrEmpty(InputFieldTextWhlieStartRecognize))
				{
					message = "[From]" + e.Result.Text;
					foreach (var element in e.Result.Translations)
					{
						message += System.Environment.NewLine + "[To]" + element.Value;
						TextToSpeechMessage = element.Value;
					}

					InputFieldTextWhlieStartRecognize = message;
				}
				else
				{
					message = "[From]" + e.Result.Text;
					foreach (var element in e.Result.Translations)
					{
						message += System.Environment.NewLine + "[To]" + element.Value;
						TextToSpeechMessage = element.Value;
					}

					InputFieldTextWhlieStartRecognize += System.Environment.NewLine + message;
				}
				Log.d(LOG_TAG, "[Translate]RecognizedHandler:" + message);
			}
			RunOnMainThread(UpdateOutputText);
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (VM.TranslationRecognizerComponent.RecognizedLanguage.Equals("en-US"))
			{
				await VM.FireOnceSpeakTextAsync("zh-TW", TextToSpeechMessage);
			}
			else if (VM.TranslationRecognizerComponent.RecognizedLanguage.Equals("zh-TW"))
			{
				await VM.FireOnceSpeakTextAsync("en-US", TextToSpeechMessage);
			}
		}
		private void TranslationCanceledHandler(object sender, TranslationRecognitionCanceledEventArgs e)
		{
			lock (threadLocker)
			{
				message = e.ErrorDetails.ToString();
				Log.d(LOG_TAG, "[Translate]CanceledHandler:" + message);
				message = "Please make sure your network, speech key and location/region are available.";
				if (String.IsNullOrEmpty(InputFieldTextWhlieStartRecognize))
				{
					InputFieldTextWhlieStartRecognize = message;
				}
				else
				{
					InputFieldTextWhlieStartRecognize += System.Environment.NewLine + message;
				}
			}
			RunOnMainThread(UpdateOutputText);
		}
		async private void TranslationSessionStartedHandler(object sender, SessionEventArgs e)
		{
			//At the time recognition is started.
			Log.d(LOG_TAG, "TranslationSessionStartedHandler");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			switch (VM.TranslationRecognizerComponent.RecognizedLanguage)
			{
				case "en-US":
					await VM.FireOnceSpeakTextAsync(VM.TranslationRecognizerComponent.RecognizedLanguage, "Translate english to chinese");
					break;
				case "zh-TW":
					await VM.FireOnceSpeakTextAsync(VM.TranslationRecognizerComponent.RecognizedLanguage, "翻譯中文到英文");
					break;
				default:
					Log.e(LOG_TAG, "Should never print this line.");
					break;
			}
		}
		#endregion
	}
}