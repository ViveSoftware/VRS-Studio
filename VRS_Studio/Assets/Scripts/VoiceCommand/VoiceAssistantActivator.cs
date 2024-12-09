using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native; //Log
using HTC.Triton.LensUI; //LensUIButton
using System.Threading.Tasks;

using Microsoft.CognitiveServices.Speech;

namespace Wave.VoiceCommand.Sample.Typing
{
	public class VoiceAssistantActivator : MonoBehaviour
	{
		private static string LOG_TAG = "VoiceAssistantActivator";

		public GameObject VoiceCommandPanel;
		public LensUIButton LensUIButton_VC;
		public Image ButtonImage_VC;

		private bool bNeedButtonBlink = false;
		private const float BlinkPeriod = 0.25f;

		private const string Hint_Str = "Feel free to input words. Also, do you notice the blinking button on the bottom? Try pressing it to wake up the voice assistant.";
		private bool FiredHint = false;

		private bool IsVoiceAssistantActivated = false;

		private void Start()
		{
			Log.d(LOG_TAG, "Start");

			VoiceCommandManager VM = VoiceCommandManager.Instance;

			if (VM == null)
			{
				VM = gameObject.AddComponent<VoiceCommandManager>();
			}

			if (LensUIButton_VC == null)
			{
				UnityEngine.Debug.LogError("LensUIButton_VC property is null! Assign a LensUIButton to it.");
			}
			else if (ButtonImage_VC == null)
			{
				UnityEngine.Debug.LogError("ButtonImage_VC property is null! Assign a ButtonImage to it.");
			}
			else if (VoiceCommandPanel == null)
			{
				UnityEngine.Debug.LogError("VoiceCommandPanel property is null! Assign a VoiceCommandPanel to it.");
			}
			else
			{
				VM.CheckMicrophoneRecordAudioPermission();
			}
		}

		public void ButtonClick()
		{
			Log.d(LOG_TAG, "ButtonClick");
			VoiceCommandManager VM = VoiceCommandManager.Instance;
			if (!VM.IsMicPermissionGranted && !VM.IsWaitPermissionCallBack)
			{
				VM.CheckMicrophoneRecordAudioPermission();
				return;
			}
			IsVoiceAssistantActivated = IsVoiceAssistantActivated ? false : true;
			Log.d(LOG_TAG, "IsVoiceAssistantActivated?" + IsVoiceAssistantActivated.ToString());

			UpdateButtonImage();

			if (IsVoiceAssistantActivated)
			{
				Log.d(LOG_TAG, "VoiceCommandPanel.SetActive(true);");
				VoiceCommandPanel.SetActive(true);
			}
			else
			{
				Log.d(LOG_TAG, "VoiceCommandPanel.SetActive(false);");
				VoiceCommandPanel.SetActive(false);
			}
		}

		private void UpdateButtonImage()
		{
			ButtonImage_VC.color = (IsVoiceAssistantActivated) ? new Color32(255, 255, 0, 255) : new Color32(0, 0, 0, 255); //Black, Yellow
		}

		#region Show the beginning hints.
		private async Task ShowHintAfterPermissionGranted()
		{
			if (!FiredHint)
			{
				VoiceCommandManager VM = VoiceCommandManager.Instance;
				if (VM.IsMicPermissionGranted)
				{
					Log.d(LOG_TAG, "ShowHintAfterPermissionGranted");
					FiredHint = true;
					await VM.ReInitializeSpeechSynthesizer();
					VM.SpeechSynthesizerComponent.SynthesisStarted += ShowHintSynthesisStartedHandler;
					VM.SpeechSynthesizerComponent.SynthesisCompleted += ShowHintSynthesisCompletedHandler;
					await VM.StartSynthesis(Hint_Str);
				}
			}
		}
		private void ShowHintSynthesisStartedHandler(object sender, SpeechSynthesisEventArgs e)
		{
			Log.d(LOG_TAG, "SynthesisStartedHandler");
			BlinkButton();
		}
		private void ShowHintSynthesisCompletedHandler(object sender, SpeechSynthesisEventArgs e)
		{
			Log.d(LOG_TAG, "SynthesisCompletedHandler");
			bNeedButtonBlink = false;
		}
		void BlinkButton()
		{
			bNeedButtonBlink = true;
			StartCoroutine("ExecuteAfterTime");
		}
		IEnumerator ExecuteAfterTime()
		{
			while (bNeedButtonBlink)
			{
				ButtonImage_VC.color = new Color32(255, 255, 0, 255); //Yellow
				yield return new WaitForSeconds(BlinkPeriod);
				ButtonImage_VC.color = new Color32(0, 0, 0, 255); //Black
				yield return new WaitForSeconds(BlinkPeriod);
			}
		}
		#endregion
		void Awake()
		{
			FiredHint = false;
		}
		async void Update()
		{
			await ShowHintAfterPermissionGranted();
		}
	}
}
