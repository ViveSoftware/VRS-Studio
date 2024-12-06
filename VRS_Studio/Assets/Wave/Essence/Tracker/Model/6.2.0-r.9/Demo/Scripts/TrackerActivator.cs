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

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Button))]
	public class TrackerActivator : MonoBehaviour
	{
		private Text m_Text = null;

		private void Awake()
		{
			m_Text = GetComponentInChildren<Text>();
		}
		private void Update()
		{
			if (m_Text == null) { return; }
			if (TrackerManager.Instance == null)
			{
				m_Text.text = "Unavailable";
				return;
			}

			var status = TrackerManager.Instance.GetTrackerStatus();
			if (status != TrackerManager.TrackerStatus.Available &&
				status != TrackerManager.TrackerStatus.NotStart &&
				status != TrackerManager.TrackerStatus.StartFailure)
			{
				GetComponent<Button>().interactable = false;
				m_Text.text = status.ToString();
			}
			else
			{
				GetComponent<Button>().interactable = true;
				m_Text.text = (status == TrackerManager.TrackerStatus.Available ?
					"Disable Tracker" : "Enable Tracker"
					);
			}

			if (TrackerManager.Instance.TrackerButtonPress(TrackerId.Tracker0, TrackerButton.A))
				TrackerManager.Instance.TriggerTrackerVibration(TrackerId.Tracker0);
		}
		public void ActivateTracker()
		{
			if (TrackerManager.Instance == null) { return; }

			var status = TrackerManager.Instance.GetTrackerStatus();
			if (status == TrackerManager.TrackerStatus.Available)
				TrackerManager.Instance.StopTracker();
			else
				TrackerManager.Instance.StartTracker();
		}
	}
}
