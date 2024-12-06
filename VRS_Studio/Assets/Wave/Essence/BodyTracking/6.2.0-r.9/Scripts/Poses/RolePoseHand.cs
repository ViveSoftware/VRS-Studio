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
	public class RolePoseHand : RolePose
	{
		public bool isLeft = false;

		private void Awake()
		{
			m_PoseType = isLeft ? RolePoseType.HAND_LEFT : RolePoseType.HAND_RIGHT;
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
				if (!Rdp.Hand.IsTracked(isLeft)) { continue; }
				if (Rdp.Hand.GetJointRotation(Rdp.Hand.Joint.Palm, ref m_Rotation, isLeft)) { m_LocationFlag |= LocationFlag.ROTATION; }
				if (Rdp.Hand.GetJointPosition(Rdp.Hand.Joint.Palm, ref m_Position, isLeft)) { m_LocationFlag |= LocationFlag.POSITION; }
				if (Rdp.Hand.GetWristAngularVelocity(ref m_AngularVelocity, isLeft)) { m_VelocityFlag |= VelocityFlag.ANGULAR; }
				if (Rdp.Hand.GetWristLinearVelocity(ref m_LinearVelocity, isLeft)) { m_VelocityFlag |= VelocityFlag.LINEAR; }
			}
		}
	}
}
