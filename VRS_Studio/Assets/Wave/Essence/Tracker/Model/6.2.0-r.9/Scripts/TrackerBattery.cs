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
using Wave.Native;

namespace Wave.Essence.Tracker.Model
{
	public class TrackerBattery : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Tracker.Model.TrackerBattery";
		static void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		[SerializeField]
		private TrackerId m_Tracker = TrackerId.Tracker0;
		public TrackerId Tracker { get { return m_Tracker; } set { m_Tracker = value; } }

		public MeshRenderer BatteryMesh = null;
		public Texture2D BatteryLife25 = null;
		public Texture2D BatteryLife50 = null;
		public Texture2D BatteryLife75 = null;
		public Texture2D BatteryLife100 = null;

		private void Update()
		{
			ChangeBatteryTexture();
		}

		private float m_Life = 0;
		void ChangeBatteryTexture()
		{
			if (TrackerManager.Instance == null) { return; }
			if (BatteryMesh == null ||
				BatteryLife25 == null ||
				BatteryLife50 == null ||
				BatteryLife75 == null ||
				BatteryLife100 == null)
			{
				return;
			}

			float batteryLife = TrackerManager.Instance.GetTrackerBatteryLife(m_Tracker);
			if (m_Life == batteryLife) { return; }

			m_Life = batteryLife;
			DEBUG("ChangeBatteryTexture() " + m_Life);

			if (0 <= batteryLife && batteryLife <= .25f)
				BatteryMesh.material.mainTexture = BatteryLife25;
			if (.25 < batteryLife && batteryLife <= .5f)
				BatteryMesh.material.mainTexture = BatteryLife50;
			if (.50 < batteryLife && batteryLife <= .75f)
				BatteryMesh.material.mainTexture = BatteryLife75;
			if (.75 < batteryLife && batteryLife <= 1)
				BatteryMesh.material.mainTexture = BatteryLife75;
		}
	}
}
