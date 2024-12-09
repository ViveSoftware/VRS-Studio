using System.Threading;
using UnityEngine;
using Wave.Essence.Hand;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class UseEssenceData : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.UseEssenceData";
		private void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		public bool UseXRDevice = false;

		private bool isRestartHandTrackingThreadRunning = false;
		private object handThreadLocker = new object();
		private HandManager.TrackerType m_HandTracker = HandManager.TrackerType.Natural;
		private void RestartHandTrackingThread()
		{
			if (HandManager.Instance == null) { return; }

			if (HandManager.Instance.UseXRDevice != UseXRDevice)
			{
				if (HandManager.Instance.GetPreferTracker(ref m_HandTracker))
				{
					var status = HandManager.Instance.GetHandTrackerStatus(m_HandTracker);
					if (status == HandManager.TrackerStatus.Available)
					{
						HandManager.Instance.StopHandTracker(m_HandTracker);
					}

					uint waitCount = 0;
					status = HandManager.Instance.GetHandTrackerStatus(m_HandTracker);
					while (status != HandManager.TrackerStatus.NotStart && waitCount <= 5)
					{
						DEBUG("RestartHandTrackingThread() Hand status: " + status + ", wait 1s.");
						Thread.Sleep(1000); // wait 1s.
						waitCount++;
						status = HandManager.Instance.GetHandTrackerStatus(m_HandTracker);
					}

					HandManager.Instance.UseXRDevice = UseXRDevice;
					HandManager.Instance.StartHandTracker(m_HandTracker);
				}
			}

			isRestartHandTrackingThreadRunning = false;
		}

		private bool isRestartTrackerThreadRunning = false;
		private object trackerThreadLocker = new object();
		private void RestartTrackerThread()
		{
			if (TrackerManager.Instance == null) { return; }

			if (TrackerManager.Instance.UseXRDevice != UseXRDevice)
			{
				var status = TrackerManager.Instance.GetTrackerStatus();
				if (status == TrackerManager.TrackerStatus.Available)
				{
					TrackerManager.Instance.StopTracker();
				}

				uint waitCount = 0;
				status = TrackerManager.Instance.GetTrackerStatus();
				while (status != TrackerManager.TrackerStatus.NotStart && waitCount <= 5)
				{
					DEBUG("RestartTrackerThread() Tracker status: " + status + ", wait 1s.");
					Thread.Sleep(1000); // wait 1s.
					waitCount++;
					status = TrackerManager.Instance.GetTrackerStatus();
				}

				TrackerManager.Instance.UseXRDevice = UseXRDevice;
				TrackerManager.Instance.StartTracker();
			}

			isRestartTrackerThreadRunning = false;
		}

		void Update()
		{
			if (HandManager.Instance != null &&
				HandManager.Instance.UseXRDevice != UseXRDevice &&
				!isRestartHandTrackingThreadRunning)
			{
				// Restart Hand Tracking.
				DEBUG("Update() Run RestartHandTrackingThread.");
				isRestartHandTrackingThreadRunning = true;
				Thread hand_tracking_t = new Thread(RestartHandTrackingThread);
				hand_tracking_t.Name = "RestartHandTrackingThread";
				hand_tracking_t.Start();
			}

			if (TrackerManager.Instance != null &&
				TrackerManager.Instance.UseXRDevice != UseXRDevice &&
				!isRestartTrackerThreadRunning)
			{
				// Restart Tracker
				DEBUG("Update() Run RestartTrackerThread.");
				isRestartTrackerThreadRunning = true;
				Thread tracker_t = new Thread(RestartTrackerThread);
				tracker_t.Name = "RestartTrackerThread";
				tracker_t.Start();
			}
		}
	}
}
