// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Button))]
	public class HoldActivator : MonoBehaviour
	{
		private const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.HoldActivator";
		private static void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		const string kHoldGunOn = "PLAYER02PUM_HOLD_GUN_ON";
		const string kHoldGunOff = "PLAYER02PUM_HOLD_GUN_OFF";

		private Text m_Text = null;

		private void Awake()
		{
			GetComponent<Button>().interactable = true;
			m_Text = GetComponentInChildren<Text>();
		}
		void Start()
		{
			ActivateHoldGun();
		}

		bool holdGun = true;
		public void ActivateHoldGun()
		{
			if (m_Text == null) { return; }
			holdGun = !holdGun;
			DEBUG("ActivateHoldGun() " + holdGun);

			IntPtr ptrParameterName = Marshal.StringToHGlobalAnsi(holdGun ? kHoldGunOn : kHoldGunOff);
			Interop.WVR_SetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParameterName);
			Marshal.FreeHGlobal(ptrParameterName);

			m_Text.text = holdGun ? "Disable Hold" : "Enable Hold";
		}
	}
}
