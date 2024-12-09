// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.UI;

namespace Wave.Essence.BodyTracking.Demo
{
	public class AvatarTrackingMenu : MonoBehaviour
	{
		public AvatarTrackingSample ikScript = null;
		public Button beginTrackingButton = null;
		public Button startCalibrationButton = null;
		public Text trackingTitle = null;
		public Text calibrationTitle = null;

		private void Update()
		{
			if (ikScript != null && trackingTitle != null)
			{
				string autoUpdateText = ikScript.autoUpdate ? "Automatically Tracking" : "Manually Tracking";
				trackingTitle.text = ikScript.TrackingMode.Name() + "\n" + autoUpdateText;
			}
		}

		public void SetArmMode()
		{
			if (ikScript != null)
				ikScript.SetArmMode();
		}
		public void SetUpperMode()
		{
			if (ikScript != null)
				ikScript.SetUpperMode();
		}
		public void SetFullMode()
		{
			if (ikScript != null)
				ikScript.SetFullMode();
		}
		public void SetUpperBodyAndLegMode()
		{
			if (ikScript != null)
				ikScript.SetUpperBodyAndLegMode();
		}
		public void BeginTracking()
		{
			if (ikScript != null)
			{
				if (beginTrackingButton != null) { beginTrackingButton.interactable = false; }
				ikScript.BeginTracking();
			}
		}
		public void EndTracking()
		{
			if (ikScript != null)
			{
				if (beginTrackingButton != null) { beginTrackingButton.interactable = true; }
				ikScript.StopTracking();
			}
		}

		private void CalibrationStatusCallback(object sender, CalibrationStatus status)
		{
			if (startCalibrationButton != null) { startCalibrationButton.interactable = (status >= CalibrationStatus.STATUS_FINISHED); }
			if (calibrationTitle != null) { calibrationTitle.text = "Calibration " + status.Name(); }
		}
		public void StartCalibration()
		{
			if (ikScript != null)
			{
				if (startCalibrationButton != null) { startCalibrationButton.interactable = false; }
				if (calibrationTitle != null) { calibrationTitle.text = "Calibration"; }
				ikScript.BeginCalibration(CalibrationStatusCallback);
			}
		}
		public void StopCalibration()
		{
			if (ikScript != null)
			{
				if (startCalibrationButton != null) { startCalibrationButton.interactable = true; }
				if (calibrationTitle != null) { calibrationTitle.text = "Calibration"; }
				ikScript.StopCalibration();
			}
		}
	}
}
