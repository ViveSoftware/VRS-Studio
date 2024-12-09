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

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(TrackerManager))]
	public class WavePath : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.WavePath";
		void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		internal TrackerManager mInst = null;
		private void Awake()
		{
			mInst = GetComponent<TrackerManager>();
		}
		bool isWavePath = false, finished = false;
		private void Update()
		{
			if (mInst == null || finished) { return; }

			if (!isWavePath &&
				(mInst.GetTrackerStatus() == TrackerManager.TrackerStatus.Available)
			)
			{
				DEBUG("Update() StopTracker.");
				mInst.StopTracker();
				isWavePath = true;
			}
			if (isWavePath && mInst.GetTrackerStatus() == TrackerManager.TrackerStatus.NotStart)
			{
				DEBUG("Update() Change to wave path.");
				mInst.UseXRDevice = false;
				DEBUG("Update() StartTracker.");
				mInst.StartTracker();
				finished = true;
			}
		}
	}
}
