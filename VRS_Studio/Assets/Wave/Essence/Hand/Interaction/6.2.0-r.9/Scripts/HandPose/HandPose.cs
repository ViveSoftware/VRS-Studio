using System.Linq;
using System.Text;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Hand.Interaction
{
	public abstract class HandPose : MonoBehaviour
	{
		#region Log
		private const string LOG_TAG = "Wave.Essence.Hand.Interaction.HandPose";
		private static StringBuilder m_sb = null;
		protected static StringBuilder sb
		{
			get
			{
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		protected void DEBUG(StringBuilder msg) { Log.d($"{LOG_TAG}.{m_PoseType}", msg, true); }
		protected void WARNING(StringBuilder msg) { Log.w($"{LOG_TAG}.{m_PoseType}", msg, true); }
		protected void ERROR(StringBuilder msg) { Log.e($"{LOG_TAG}.{m_PoseType}", msg, true); }
		#endregion

		protected HandPoseType m_PoseType = HandPoseType.UNKNOWN;
		protected bool m_Initialized = false;
		protected bool m_IsTracked = false;
		protected const int poseCount = (int)JointType.Count;
		protected Vector3[] m_Position = Enumerable.Repeat(Vector3.zero, poseCount).ToArray();
		protected Vector3[] m_LocalPosition = Enumerable.Repeat(Vector3.zero, poseCount).ToArray();
		protected Quaternion[] m_Rotation = Enumerable.Repeat(Quaternion.identity, poseCount).ToArray();
		protected Quaternion[] m_LocalRotation = Enumerable.Repeat(Quaternion.identity, poseCount).ToArray();

		protected virtual void OnEnable()
		{
			HandPoseProvider.RegisterHandPose(m_PoseType, this);
		}
		protected virtual void OnDisable()
		{
			HandPoseProvider.UnregisterHandPose(m_PoseType);
		}

		public virtual void SetType(HandPoseType poseType)
		{
			m_PoseType = poseType;
			m_Initialized = true;
		}

		public virtual bool IsTracked()
		{
			return m_IsTracked;
		}

		public virtual bool GetRotation(JointType joint, out Quaternion value, bool local = false)
		{
			value = Quaternion.identity;
			if (joint != JointType.Count)
			{
				value = local ? m_LocalRotation[(int)joint] : m_Rotation[(int)joint];
				return true;
			}
			return false;
		}

		public virtual bool GetPosition(JointType joint, out Vector3 value, bool local = false)
		{
			value = Vector3.zero;
			if (joint != JointType.Count)
			{
				value = local ? m_LocalPosition[(int)joint] : m_Position[(int)joint];
				return true;
			}
			return false;
		}
	}
}
