// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Button))]
	public class GunModeActivator : MonoBehaviour
	{
		private const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.GunModeActivator";
		private static void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		[SerializeField]
		private Text m_CallbackText = null;
		public Text CallbackText { get { return m_CallbackText; } set { m_CallbackText = value; } }

		const string kGunModeOn = "PLAYER01PUM_GUNMODE=1";
		const string kGunModeOff = "PLAYER01PUM_GUNMODE=0";

		private Text m_Text = null;
		private WVR_TrackerInfoNotify m_InfoNotify;

		private void Awake()
		{
			GetComponent<Button>().interactable = true;
			m_Text = GetComponentInChildren<Text>();
			DEBUG("Awake() Set WVR_TrackerInfoCallback");
			m_InfoNotify.callback = new WVR_TrackerInfoCallback(TrackerInfoCallback);
		}
		void Start()
		{
			ActivateGunMode();
		}
		private void Update()
		{
			if (m_CallbackText != null) { m_CallbackText.text = callbackString; }
		}

		bool m_GunMode = true;
		public void ActivateGunMode()
		{
			if (m_Text == null) { return; }
			m_GunMode = !m_GunMode;
			DEBUG("ActivateGunMode() " + m_GunMode + ", " + (m_GunMode ? kGunModeOn : kGunModeOff));

			IntPtr ptrParameterName = Marshal.StringToHGlobalAnsi(m_GunMode ? kGunModeOn : kGunModeOff);
			Interop.WVR_SetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParameterName);
			Marshal.FreeHGlobal(ptrParameterName);

			if (m_GunMode)
			{
				DEBUG("ActivateGunMode() WVR_RegisterTrackerInfoCallback");
				Interop.WVR_RegisterTrackerInfoCallback(ref m_InfoNotify);
			}
			else
			{
				DEBUG("ActivateGunMode() WVR_UnregisterTrackerInfoCallback");
				Interop.WVR_UnregisterTrackerInfoCallback();
			}

			m_Text.text = m_GunMode ? "Disable Gun Mode" : "Enable Gun Mode";
		}

		static string callbackString = "";
		[MonoPInvokeCallback(typeof(WVR_TrackerInfoCallback))]
		public static void TrackerInfoCallback(WVR_TrackerId trackerId, IntPtr cbInfo, ref UInt64 timestamp)
		{
			string strValue = Marshal.PtrToStringAnsi(cbInfo);
			callbackString = trackerId.ToString() + ": " + strValue + ", " + timestamp.ToString();
			DEBUG("TrackerInfoCallback() " + callbackString);
		}
	}
}
