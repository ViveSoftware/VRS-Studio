// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using UnityEngine;

namespace Wave.Essence.BodyTracking
{
	public abstract class RolePose : MonoBehaviour
	{
		[Flags]
		public enum LocationFlag { ROTATION = 1, POSITION = 2}
		[Flags]
		public enum VelocityFlag { ANGULAR = 1, LINEAR = 2 }

		protected RolePoseType m_PoseType = RolePoseType.UNKNOWN;

		protected LocationFlag m_LocationFlag = 0;
		protected VelocityFlag m_VelocityFlag = 0;
		protected Quaternion m_Rotation = Quaternion.identity;
		protected Vector3 m_Position = Vector3.zero;
		protected Vector3 m_AngularVelocity = Vector3.zero;
		protected Vector3 m_LinearVelocity = Vector3.zero;
		protected Vector3 m_Acceleration = Vector3.zero;

		protected virtual void OnEnable()
		{
			RolePoseProvider.RegisterRolePose(m_PoseType, this);
		}
		protected virtual void OnDisable()
		{
			RolePoseProvider.RemoveRolePose(m_PoseType);
		}

		public virtual bool IsTracked()
		{
			return (m_LocationFlag != 0);
		}
		public virtual bool GetRotation(out Quaternion value)
		{
			value = m_Rotation;
			return m_LocationFlag.HasFlag(LocationFlag.ROTATION);
		}
		public virtual bool GetPosition(out Vector3 value)
		{
			value = m_Position;
			return m_LocationFlag.HasFlag(LocationFlag.POSITION);
		}
		public virtual bool GetAngularVelocity(out Vector3 value)
		{
			value = m_AngularVelocity;
			return m_VelocityFlag.HasFlag(VelocityFlag.ANGULAR);
		}
		public virtual bool GetLinearVelocity(out Vector3 value)
		{
			value = m_LinearVelocity;
			return m_VelocityFlag.HasFlag(VelocityFlag.LINEAR);
		}
		public virtual bool GetAcceleration(out Vector3 value)
		{
			value = m_Acceleration;
			return m_LocationFlag.HasFlag(LocationFlag.POSITION);
		}
	}
}
