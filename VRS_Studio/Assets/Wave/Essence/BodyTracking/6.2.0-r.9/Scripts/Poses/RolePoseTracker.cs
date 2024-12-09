// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using UnityEngine;

using Wave.Essence.BodyTracking.RuntimeDependency;

namespace Wave.Essence.BodyTracking
{
	public class RolePoseTracker : RolePose
	{
		public Rdp.Tracker.Id trackerId = Rdp.Tracker.Id.Tracker0;

		private void Awake()
		{
			if (trackerId == Rdp.Tracker.Id.Tracker0) { m_PoseType = RolePoseType.TRACKER_0; }
			if (trackerId == Rdp.Tracker.Id.Tracker1) { m_PoseType = RolePoseType.TRACKER_1; }
			if (trackerId == Rdp.Tracker.Id.Tracker2) { m_PoseType = RolePoseType.TRACKER_2; }
			if (trackerId == Rdp.Tracker.Id.Tracker3) { m_PoseType = RolePoseType.TRACKER_3; }
			if (trackerId == Rdp.Tracker.Id.Tracker4) { m_PoseType = RolePoseType.TRACKER_4; }
			if (trackerId == Rdp.Tracker.Id.Tracker5) { m_PoseType = RolePoseType.TRACKER_5; }
			if (trackerId == Rdp.Tracker.Id.Tracker6) { m_PoseType = RolePoseType.TRACKER_6; }
			if (trackerId == Rdp.Tracker.Id.Tracker7) { m_PoseType = RolePoseType.TRACKER_7; }
			if (trackerId == Rdp.Tracker.Id.Tracker8) { m_PoseType = RolePoseType.TRACKER_8; }
			if (trackerId == Rdp.Tracker.Id.Tracker9) { m_PoseType = RolePoseType.TRACKER_9; }
			if (trackerId == Rdp.Tracker.Id.Tracker10) { m_PoseType = RolePoseType.TRACKER_10; }
			if (trackerId == Rdp.Tracker.Id.Tracker11) { m_PoseType = RolePoseType.TRACKER_11; }
			if (trackerId == Rdp.Tracker.Id.Tracker12) { m_PoseType = RolePoseType.TRACKER_12; }
			if (trackerId == Rdp.Tracker.Id.Tracker13) { m_PoseType = RolePoseType.TRACKER_13; }
			if (trackerId == Rdp.Tracker.Id.Tracker14) { m_PoseType = RolePoseType.TRACKER_14; }
			if (trackerId == Rdp.Tracker.Id.Tracker15) { m_PoseType = RolePoseType.TRACKER_15; }
		}

		bool toUpdate = false;
		protected override void OnEnable()
		{
			base.OnEnable();

			if (!toUpdate)
			{
				toUpdate = true;
				StartCoroutine(UpdatePose());
			}
		}
		protected override void OnDisable()
		{
			base.OnDisable();

			if (toUpdate)
			{
				toUpdate = false;
				StopCoroutine(UpdatePose());
			}
		}
		private IEnumerator UpdatePose()
		{
			while (toUpdate)
			{
				yield return new WaitForEndOfFrame();

				m_LocationFlag = 0;
				m_VelocityFlag = 0;

				var type = Rdp.Tracker.GetTrackerType(trackerId, false);
				if (Rdp.Tracker.GetTrackerRotation(trackerId, out m_Rotation)) { m_LocationFlag |= LocationFlag.ROTATION; }
				if (type != TrackerType.ViveSelfTrackerIM && Rdp.Tracker.GetTrackerPosition(trackerId, out m_Position)) { m_LocationFlag |= LocationFlag.POSITION; }
				if (Rdp.Tracker.GetTrackerAngularVelocity(trackerId, out m_AngularVelocity)) { m_VelocityFlag |= VelocityFlag.ANGULAR; }
				if (Rdp.Tracker.GetTrackerVelocity(trackerId, out m_LinearVelocity)) { m_VelocityFlag |= VelocityFlag.LINEAR; }
				Rdp.Tracker.GetTrackerAcceleration(trackerId, out m_Acceleration);
			}
		}
	}
}
