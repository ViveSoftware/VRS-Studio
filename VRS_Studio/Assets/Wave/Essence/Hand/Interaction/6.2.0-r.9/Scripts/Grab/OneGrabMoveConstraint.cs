using System.Text;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Hand.Interaction
{
	public class OneGrabMoveConstraint : IOneHandContraintMovement
	{
		#region Log

		const string LOG_TAG = "Wave.Essence.Hand.Interaction.OneGrabMoveConstraint";
		private StringBuilder m_sb = null;
		internal StringBuilder sb
		{
			get
			{
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		private void DEBUG(StringBuilder msg) { Log.d(LOG_TAG, msg, true); }
		private void INFO(StringBuilder msg) { Log.i(LOG_TAG, msg, true); }
		private void WARNING(StringBuilder msg) { Log.w(LOG_TAG, msg, true); }
		private void ERROR(StringBuilder msg) { Log.e(LOG_TAG, msg, true); }
		int logFrame = 0;
		bool printIntervalLog => logFrame == 0;

		#endregion

		[SerializeField]
		private Transform m_Constraint;
		[SerializeField]
		private ConstraintInfo m_NegativeXMove = ConstraintInfo.Identity;
		private float defaultNegativeXPos = 0.0f;
		public float xNegativeBoundary => defaultNegativeXPos;
		[SerializeField]
		private ConstraintInfo m_PositiveXMove = ConstraintInfo.Identity;
		private float defaultPositiveXPos = 0.0f;
		public float xPositiveBoundary => defaultPositiveXPos;
		[SerializeField]
		private ConstraintInfo m_NegativeYMove = ConstraintInfo.Identity;
		private float defaultNegativeYPos = 0.0f;
		public float yNegativeBoundary => defaultNegativeYPos;
		[SerializeField]
		private ConstraintInfo m_PositiveYMove = ConstraintInfo.Identity;
		private float defaultPositiveYPos = 0.0f;
		public float yPositiveBoundary => defaultPositiveYPos;
		[SerializeField]
		private ConstraintInfo m_NegativeZMove = ConstraintInfo.Identity;
		private float defaultNegativeZPos = 0.0f;
		public float zNegativeBoundary => defaultNegativeZPos;
		[SerializeField]
		private ConstraintInfo m_PositiveZMove = ConstraintInfo.Identity;
		private float defaultPositiveZPos = 0.0f;
		public float zPositiveBoundary => defaultPositiveZPos;

		private Pose previousHandPose = Pose.identity;
		private GrabPose currentGrabPose = GrabPose.Identity;

		public override void Initialize(IGrabbable grabbable)
		{
			if (grabbable is HandGrabInteractable handGrabbable)
			{
				if (m_Constraint == null)
				{
					m_Constraint = handGrabbable.transform;
					sb.Clear().Append("Since no constraint object is set, self will be used as the constraint object.");
					WARNING(sb);
				}
			}

			if (m_NegativeXMove.IsEnable()) { defaultNegativeXPos = m_Constraint.position.x - m_NegativeXMove.GetValue(); }
			if (m_PositiveXMove.IsEnable()) { defaultPositiveXPos = m_Constraint.position.x + m_PositiveXMove.GetValue(); }
			if (m_NegativeYMove.IsEnable()) { defaultNegativeYPos = m_Constraint.position.y - m_NegativeYMove.GetValue(); }
			if (m_PositiveYMove.IsEnable()) { defaultPositiveYPos = m_Constraint.position.y + m_PositiveYMove.GetValue(); }
			if (m_NegativeZMove.IsEnable()) { defaultNegativeZPos = m_Constraint.position.z - m_NegativeZMove.GetValue(); }
			if (m_PositiveZMove.IsEnable()) { defaultPositiveZPos = m_Constraint.position.z + m_PositiveZMove.GetValue(); }
		}

		public override void OnBeginGrabbed(IGrabbable grabbable) 
		{
			if (grabbable is HandGrabInteractable handGrabbable)
			{
				currentGrabPose = handGrabbable.bestGrabPose;
			}

			if (grabbable.grabber is HandGrabInteractor handGrabber)
			{
				HandPose handPose = HandPoseProvider.GetHandPose(handGrabber.isLeft ? HandPoseType.HAND_LEFT : HandPoseType.HAND_RIGHT);
				handPose.GetPosition(JointType.Wrist, out Vector3 wristPos);
				handPose.GetRotation(JointType.Wrist, out Quaternion wristRot);
				previousHandPose = new Pose(wristPos, wristRot);
			}
		}

		public override void UpdatePose(Pose handPose)
		{
			if (previousHandPose == Pose.identity)
			{
				previousHandPose = handPose;
				return;
			}

			Quaternion previousRotOffset = previousHandPose.rotation * Quaternion.Inverse(currentGrabPose.grabOffset.sourceRotation);
			Vector3 previousPos = previousHandPose.position + previousRotOffset * currentGrabPose.grabOffset.posOffset;

			Quaternion currentRotOffset = handPose.rotation * Quaternion.Inverse(currentGrabPose.grabOffset.sourceRotation);
			Vector3 currentPos = handPose.position + currentRotOffset * currentGrabPose.grabOffset.posOffset;

			Vector3 handOffset = currentPos - previousPos;

			if (m_NegativeXMove.IsEnable())
			{
				float x = (m_Constraint.position + handOffset).x;
				x = Mathf.Max(defaultNegativeXPos, x);
				m_Constraint.position = new Vector3(x, m_Constraint.position.y, m_Constraint.position.z);
			}
			if (m_PositiveXMove.IsEnable())
			{
				float x = (m_Constraint.position + handOffset).x;
				x = Mathf.Min(defaultPositiveXPos, x);
				m_Constraint.position = new Vector3(x, m_Constraint.position.y, m_Constraint.position.z);
			}
			if (m_NegativeYMove.IsEnable())
			{
				float y = (m_Constraint.position + handOffset).y;
				y = Mathf.Max(defaultNegativeYPos, y);
				m_Constraint.position = new Vector3(m_Constraint.position.x, y, m_Constraint.position.z);
			}
			if (m_PositiveYMove.IsEnable())
			{
				float y = (m_Constraint.position + handOffset).y;
				y = Mathf.Min(defaultPositiveYPos, y);
				m_Constraint.position = new Vector3(m_Constraint.position.x, y, m_Constraint.position.z);
			}
			if (m_NegativeZMove.IsEnable())
			{
				float z = (m_Constraint.position + handOffset).z;
				z = Mathf.Max(defaultNegativeZPos, z);
				m_Constraint.position = new Vector3(m_Constraint.position.x, m_Constraint.position.y, z);
			}
			if (m_PositiveZMove.IsEnable())
			{
				float z = (m_Constraint.position + handOffset).z;
				z = Mathf.Min(defaultPositiveZPos, z);
				m_Constraint.position = new Vector3(m_Constraint.position.x, m_Constraint.position.y, z);
			}

			previousHandPose = handPose;
		}

		public override void OnEndGrabbed(IGrabbable grabbable)
		{
			currentGrabPose = GrabPose.Identity;
			previousHandPose = Pose.identity;
		}
	}
}
