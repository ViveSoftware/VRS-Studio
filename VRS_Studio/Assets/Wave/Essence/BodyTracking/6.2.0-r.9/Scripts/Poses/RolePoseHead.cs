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

namespace Wave.Essence.BodyTracking
{
	public class RolePoseHead : RolePose
	{
		private void Awake()
		{
			m_PoseType = RolePoseType.HMD;
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

				XR_Device device = XR_Device.Head;

				m_LocationFlag = 0;
				m_VelocityFlag = 0;
				if (!WXRDevice.IsTracked(device)) { continue; }
				if (WXRDevice.GetRotation(device, ref m_Rotation)) { m_LocationFlag |= LocationFlag.ROTATION; }
				if (WXRDevice.GetPosition(device, ref m_Position)) { m_LocationFlag |= LocationFlag.POSITION; }
				if (WXRDevice.GetAngularVelocity(device, ref m_AngularVelocity)) { m_VelocityFlag |= VelocityFlag.ANGULAR; }
				if (WXRDevice.GetVelocity(device, ref m_LinearVelocity)) { m_VelocityFlag |= VelocityFlag.LINEAR; }
				WXRDevice.GetAcceleration(device, ref m_Acceleration);
			}
		}
	}
}
