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
using System.Collections.Generic;
using UnityEngine;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class TrackerPose : MonoBehaviour
	{
		public enum TrackerType
		{
			Undefined,
			WristTracker,
			UltimateTracker,
		}

		[Serializable]
		public class TrackerTypeOption
		{
			public bool m_CheckType = false;
			public TrackerType m_Tracker = TrackerType.Undefined;
		}


		[SerializeField]
		private TrackerId m_Index = TrackerId.Tracker0;
		public TrackerId Index { get { return m_Index; } set { m_Index = value; } }

		[SerializeField]
		private TrackerTypeOption m_TrackerOption = new TrackerTypeOption();
		public TrackerTypeOption TrackerOption { get { return m_TrackerOption; } set { m_TrackerOption = value; } }

		[SerializeField]
		private GameObject m_Model = null;
		public GameObject Model { get { return m_Model; } set { m_Model = value; } }

		private void Update()
		{
			if (m_Model == null) { return; }
			if (TrackerManager.Instance == null ||
				(m_TrackerOption.m_CheckType && m_TrackerOption.m_Tracker == TrackerType.Undefined)
			)
			{
				if (m_Model.activeSelf) { m_Model.SetActive(false); }
				return;
			}

			bool validPose = TrackerManager.Instance.IsTrackerPoseValid(m_Index);
			bool validType = (
				!m_TrackerOption.m_CheckType || 
				(m_TrackerOption.m_CheckType && m_TrackerOption.m_Tracker == GetTrackerType(TrackerManager.Instance.GetTrackerDeviceName(m_Index)))
			);
			bool active = validPose && validType;

			if (active)
			{
				transform.localPosition = TrackerManager.Instance.GetTrackerPosition(m_Index);
				transform.localRotation = TrackerManager.Instance.GetTrackerRotation(m_Index);
			}

			if (m_Model.activeSelf != active) { m_Model.SetActive(active); }
		}

		private readonly Dictionary<TrackerType, List<string>> s_TrackerNames = new Dictionary<TrackerType, List<string>>()
		{
			{ TrackerType.WristTracker, new List<string>() {
				"Vive_Tracker_Wrist", "Vive_Wrist_Tracker" }
			},
			{ TrackerType.UltimateTracker, new List<string>() {
				"Vive_Tracker_OT", "Vive_Self_Tracker", "Vive_Ultimate_Tracker" }
			},
		};
		private TrackerType GetTrackerType(string name)
		{
			// Checks self tracker first.
			for (int i = 0; i < s_TrackerNames[TrackerType.UltimateTracker].Count; i++)
			{
				if (name.Contains(s_TrackerNames[TrackerType.UltimateTracker][i]))
					return TrackerType.UltimateTracker;
			}
			// Checks wrist tracker last.
			for (int i = 0; i < s_TrackerNames[TrackerType.WristTracker].Count; i++)
			{
				if (name.Contains(s_TrackerNames[TrackerType.WristTracker][i]))
					return TrackerType.WristTracker;
			}

			return TrackerType.Undefined;
		}
	}
}
