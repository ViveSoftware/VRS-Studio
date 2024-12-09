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
	public class HumanoidIKMenu : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.BodyTracking.Demo.HumanoidIKMenu";
		private StringBuilder m_sb = null;
		private StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Rdp.d(LOG_TAG, msg, true); }

		public HumanoidIKSample humanoidIK = null;
		public Button beginTrackingButton = null;
		public Button endTrackingButton = null;

		public void BeginTracking()
		{
			if (humanoidIK != null)
			{
				sb.Clear().Append("BeginTracking()"); DEBUG(sb);
				humanoidIK.BeginTracking();
			}
		}
		public void EndTracking()
		{
			if (humanoidIK != null)
			{
				sb.Clear().Append("EndTracking()"); DEBUG(sb);
				humanoidIK.StopTracking();
			}
		}

		private void Update()
		{
			if (humanoidIK != null)
			{
				var status = humanoidIK.GetTrackingStatus();
				if (beginTrackingButton != null)
				{
					beginTrackingButton.interactable = (
						status == HumanoidIKSample.TrackingStatus.NotStart ||
						status == HumanoidIKSample.TrackingStatus.StartFailure
					);
				}
				if (endTrackingButton != null)
				{
					endTrackingButton.interactable = (status == HumanoidIKSample.TrackingStatus.Available);
				}
			}
		}
	}
}
