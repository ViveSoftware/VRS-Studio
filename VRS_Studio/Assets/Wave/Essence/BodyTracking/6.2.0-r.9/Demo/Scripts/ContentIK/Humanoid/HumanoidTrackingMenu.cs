// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Text;
using UnityEngine;
using UnityEngine.UI;

using Wave.Essence.BodyTracking.RuntimeDependency;

namespace Wave.Essence.BodyTracking.Demo
{
	public class HumanoidTrackingMenu : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.BodyTracking.Demo.HumanoidTrackingMenu";
		private StringBuilder m_sb = null;
		private StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Rdp.d(LOG_TAG, msg, true); }

		public HumanoidTracking humanoidTracking = null;
		public Button beginTrackingButton = null;
		public Button startCalibrationButton = null;
		public Text trackingTitle = null;
		public Text calibrationTitle = null;
		public Button logButton = null;

		private void Update()
		{
			if (humanoidTracking == null || trackingTitle == null) { return; }

			trackingTitle.text = humanoidTracking.Tracking + "\n" + "Manually Tracking";

			if (logButton != null && BodyManager.Instance != null)
			{
				logButton.GetComponentInChildren<Text>().text = BodyManager.Instance.EnableTrackingLog ? "Log\nOn" : "Log\nOff";
			}
		}

		public void SetArmMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.Arm;
		}
		public void SetUpperMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.UpperBody;
		}
		public void SetFullMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.FullBody;
		}
		public void SetUpperBodyAndLegMode()
		{
			if (humanoidTracking != null)
				humanoidTracking.Tracking = HumanoidTracking.TrackingMode.UpperBodyAndLeg;
		}	

		public void BeginTracking()
		{
			if (humanoidTracking != null)
			{
				if (beginTrackingButton != null) { beginTrackingButton.interactable = false; }
				if (startCalibrationButton != null) { startCalibrationButton.interactable = false; }
				humanoidTracking.BeginTracking();
			}
		}
		public void EndTracking()
		{
			if (humanoidTracking != null)
			{
				if (beginTrackingButton != null) { beginTrackingButton.interactable = true; }
				if (startCalibrationButton != null) { startCalibrationButton.interactable = true; }
				humanoidTracking.StopTracking();
			}
		}

		private void CalibrationStatusCallback(object sender, CalibrationStatus status)
		{
			if (startCalibrationButton != null) { startCalibrationButton.interactable = (status >= CalibrationStatus.STATUS_FINISHED); }
			if (calibrationTitle != null) { calibrationTitle.text = "Calibration " + status.Name(); }
		}
		public void StartCalibration()
		{
			if (humanoidTracking != null)
			{
				if (startCalibrationButton != null) { startCalibrationButton.interactable = false; }
				if (calibrationTitle != null) { calibrationTitle.text = "Calibration"; }
#if !WAVE_BODY_IK
				humanoidTracking.BeginCalibration(CalibrationStatusCallback);
#endif
			}
		}
		public void StopCalibration()
		{
			if (humanoidTracking != null)
			{
				if (startCalibrationButton != null) { startCalibrationButton.interactable = true; }
				if (calibrationTitle != null) { calibrationTitle.text = "Calibration"; }
#if !WAVE_BODY_IK
				humanoidTracking.StopCalibration();
#endif
			}
		}

		public void OneStepStart()
		{
			StartCalibration();
			BeginTracking();
		}
		public void OneStepStop()
		{
			EndTracking();
		}

		public void ActivateTrackingLog()
		{
			if (BodyManager.Instance == null) { return; }

			BodyManager.Instance.EnableTrackingLog = !BodyManager.Instance.EnableTrackingLog;
		}
	}
}
