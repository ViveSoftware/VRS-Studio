using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wave.Native;  // Log

namespace Wave.VoiceCommand
{
	public class IntentManager : MonoBehaviour
	{
		public UnityEvent ExitAppEvent = new UnityEvent();
		public UnityEvent TurnOnPassthroughEvent = new UnityEvent();
		public UnityEvent TurnOffPassthroughEvent = new UnityEvent();
		public UnityEvent WakeUpEvent = new UnityEvent();
		public UnityEvent UnknownIntent = new UnityEvent();

		private static IntentManager m_Instance = null;
		public static IntentManager Instance
		{
			get { return m_Instance; }
		}

		private static string LOG_TAG = "IntentManager";

		void Awake() { m_Instance = this; }

		public void IntentInvoking(Dictionary<string, string> EntityDic, string Intent,
										 double IntentScore)
		{
			Log.d(LOG_TAG, "IntentInvoking");

			var VM = VoiceCommandManager.Instance;
			// Handle intent action
			if (Intent == "" || Intent == "Switch Voice Command") return;
			// Exit App
			else if (Intent == "Exit App" && IntentScore >= 0.97)
			{
				Log.d(LOG_TAG, "Exit App");
				if (EntityDic.ContainsKey("exit"))
				{
					if (EntityDic["exit"] == "exit app")
					{
						Log.d(LOG_TAG, "exit app");
						ExitAppEvent.Invoke();
					}
				}
			}
			// Manage Passthrough
			else if (Intent == "Manage Passthrough" && IntentScore >= 0.96)
			{
				if (EntityDic.ContainsKey("action") && EntityDic.ContainsKey("passthrough"))
				{
					if (EntityDic["action"] == "turn on" || EntityDic["action"] == "enable")
					{
						TurnOnPassthroughEvent.Invoke();
					}
					else if (EntityDic["action"] == "turn off" || EntityDic["action"] == "disable")
					{
						TurnOffPassthroughEvent.Invoke();
					}
				}
			}
			// Wake Up
			else if (Intent == "Wake Up" && IntentScore >= 0.96)
			{
				if (!EntityDic.ContainsKey("action") && EntityDic.ContainsKey("wake up"))
				{
					WakeUpEvent.Invoke();
				}
			}
			else
			{
				UnknownIntent.Invoke();
			}
		}
	}
}