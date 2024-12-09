// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class BraceletPose : MonoBehaviour
	{
		[SerializeField]
		private bool m_IsLeft = false;
		public bool IsLeft { get { return m_IsLeft; } set { m_IsLeft = value; } }

		private TrackerId m_Tracker = TrackerId.Tracker0;

		public enum BraceletMode
		{
			bottom = 0,
			side = 1,
		}

		[SerializeField]
		private GameObject m_Bracelet = null;
		public GameObject Bracelet { get { return m_Bracelet; } set { m_Bracelet = value; } }

		#region Bracelet Pose
		Dictionary<TrackerId, Vector3> s_BraceletPosOffset = new Dictionary<TrackerId, Vector3>()
		{
			{ TrackerId.Tracker0, Vector3.zero },
			{ TrackerId.Tracker1, Vector3.zero },
			{ TrackerId.Tracker2, Vector3.zero },
			{ TrackerId.Tracker3, Vector3.zero },
			{ TrackerId.Tracker4, Vector3.zero },
			{ TrackerId.Tracker5, Vector3.zero },
			{ TrackerId.Tracker6, Vector3.zero },
			{ TrackerId.Tracker7, Vector3.zero },
			{ TrackerId.Tracker8, Vector3.zero },
			{ TrackerId.Tracker9, Vector3.zero },
			{ TrackerId.Tracker10, Vector3.zero },
			{ TrackerId.Tracker11, Vector3.zero },
			{ TrackerId.Tracker12, Vector3.zero },
			{ TrackerId.Tracker13, Vector3.zero },
			{ TrackerId.Tracker14, Vector3.zero },
			{ TrackerId.Tracker15, Vector3.zero },
		};
		readonly Vector3 kBraceletPositionOffset = new Vector3(0, 0, -0.03f);
		Vector3 m_BraceletPos = Vector3.zero;
		private void UpdateBraceletPosition()
		{
			if (TrackerManager.Instance == null) { return; }

			m_BraceletPos = TrackerManager.Instance.GetTrackerPosition(m_Tracker) + s_BraceletPosOffset[m_Tracker];
		}

		Dictionary<TrackerId, Quaternion> s_BraceletRotOffset = new Dictionary<TrackerId, Quaternion>()
		{
			{ TrackerId.Tracker0, Quaternion.identity },
			{ TrackerId.Tracker1, Quaternion.identity },
			{ TrackerId.Tracker2, Quaternion.identity },
			{ TrackerId.Tracker3, Quaternion.identity },
			{ TrackerId.Tracker4, Quaternion.identity },
			{ TrackerId.Tracker5, Quaternion.identity },
			{ TrackerId.Tracker6, Quaternion.identity },
			{ TrackerId.Tracker7, Quaternion.identity },
			{ TrackerId.Tracker8, Quaternion.identity },
			{ TrackerId.Tracker9, Quaternion.identity },
			{ TrackerId.Tracker10, Quaternion.identity },
			{ TrackerId.Tracker11, Quaternion.identity },
			{ TrackerId.Tracker12, Quaternion.identity },
			{ TrackerId.Tracker13, Quaternion.identity },
			{ TrackerId.Tracker14, Quaternion.identity },
			{ TrackerId.Tracker15, Quaternion.identity },
		};
		Quaternion m_BraceletRot = Quaternion.identity;
		private void UpdateBraceletRotation()
		{
			if (TrackerManager.Instance == null) { return; }

			m_BraceletRot = TrackerManager.Instance.GetTrackerRotation(m_Tracker) * s_BraceletRotOffset[m_Tracker];
		}
		#endregion

		private bool UpdateTrackerId()
		{
			if (m_Bracelet == null || TrackerManager.Instance == null) { return false; }

			for (int i = 0; i < TrackerUtils.s_TrackerIds.Length; i++)
			{
				var trackerId = TrackerUtils.s_TrackerIds[i];
				if (TrackerManager.Instance.IsTrackerConnected(trackerId))
				{
					TrackerRole role = TrackerManager.Instance.GetTrackerRole(trackerId);
					if ((m_IsLeft && (role == TrackerRole.Pair1_Left)) ||
						(!m_IsLeft && (role == TrackerRole.Pair1_Right)))
					{
						m_Tracker = trackerId;
						return true;
					}
				}
			}
			if (Log.gpl.Print)
			{
				Log.d("Wave.Essence.Tracker.Model.Demo.BraceletPose", "UpdateTrackerId() cannot find " + (m_IsLeft ? "left" : "right") + " tracker.", true);
			}
			return false;
		}

		void Update()
		{
			if (m_Bracelet == null || TrackerManager.Instance == null) { return; }

			if(!UpdateTrackerId()) { return; }

			if (TrackerManager.Instance.IsTrackerConnected(m_Tracker))
			{
				m_Bracelet.SetActive(true);

				UpdateBraceletPosition();
				UpdateBraceletRotation();

				transform.localPosition = m_BraceletPos;
				transform.localRotation = m_BraceletRot;
			}
			else
			{
				m_Bracelet.SetActive(false);
			}
		}
	}
}
