// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.UI;
using Wave.Essence.Hand;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Button))]
	public class WearBracelet : MonoBehaviour
	{
		private const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.WearBracelet";
		private static void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		private Text m_Text = null;

		private void Awake()
		{
			GetComponent<Button>().interactable = true;
			m_Text = GetComponentInChildren<Text>();
		}
		void Start()
		{
			if (m_Text == null || HandManager.Instance == null) { return; }

			DEBUG("Start() Disable fusion.");
			HandManager.Instance.FuseWristPositionWithTracker(false);
		}

		public void FuseWristPositionWithTracker()
		{
			if (m_Text == null || HandManager.Instance == null) { return; }

			bool fused = HandManager.Instance.IsWristPositionFused();

			DEBUG("FuseWristPositionWithTracker() " + (!fused));
			HandManager.Instance.FuseWristPositionWithTracker(!fused);
		}

		private void Update()
		{
			bool fused = HandManager.Instance.IsWristPositionFused();
			m_Text.text = fused ? "Doff Bracelet" : "Wear Bracelet";
		}
	}
}
