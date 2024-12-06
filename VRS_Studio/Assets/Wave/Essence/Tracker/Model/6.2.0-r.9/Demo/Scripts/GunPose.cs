// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using Wave.Essence.Hand;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class GunPose : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Tracker.Demo.GunPose";
		void DEBUG(string msg) { Log.d(LOG_TAG, msg, true); }

		[SerializeField]
		private GameObject m_Gun = null;
		public GameObject Gun { get { return m_Gun; } set { m_Gun = value; } }

		[SerializeField]
		private bool m_IsLeft = false;
		public bool IsLeft { get { return m_IsLeft; } set { m_IsLeft = value; } }

		void Update()
		{
			if (HandManager.Instance == null || m_Gun == null) { return; }

			HandManager.HandMotion handMotion = HandManager.Instance.GetHandMotion(m_IsLeft);
			if (handMotion != HandManager.HandMotion.Hold)
			{
				m_Gun.SetActive(false);
				return;
			}

			HandManager.HandHoldRole holdRole = HandManager.Instance.GetHandHoldRole(m_IsLeft);
			if (holdRole != HandManager.HandHoldRole.Main)
			{
				m_Gun.SetActive(false);
				return;
			}

			HandManager.HandHoldType holdType = HandManager.Instance.GetHandHoldType(m_IsLeft);

			if (Log.gpl.Print)
				DEBUG("Update() " + (m_IsLeft ? "left" : "right") + " role: " + holdRole + ", type: " + holdType);

			m_Gun.SetActive(true);

			Vector3 gunPos = Vector3.zero;
			if (HandManager.Instance.GetJointPosition(HandManager.HandJoint.Palm, ref gunPos, m_IsLeft))
			{
				transform.localPosition = gunPos;
			}
			if (TrackerManager.Instance != null)
			{
				transform.localRotation = TrackerManager.Instance.GetTrackerRotation(m_IsLeft ? TrackerId.Tracker1 : TrackerId.Tracker0);
			}
			/*Quaternion gunRot = Quaternion.identity;
			if (HandManager.Instance.GetJointRotation(HandManager.HandJoint.Palm, ref gunRot, m_IsLeft))
			{
				transform.localRotation = gunRot;
				transform.Rotate(m_IsLeft ? braceletRotationOffsetLeft : braceletRotationOffsetRight);
			}*/
		}

		readonly Vector3 braceletRotationOffsetRight = new Vector3(0, 90, 60);
		readonly Vector3 braceletRotationOffsetLeft = new Vector3(0, 90, 60);
	}
}
