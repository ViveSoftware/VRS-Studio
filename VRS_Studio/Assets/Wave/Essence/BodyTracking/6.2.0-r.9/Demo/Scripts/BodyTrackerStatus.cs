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

using Wave.Essence.BodyTracking.RuntimeDependency;

namespace Wave.Essence.BodyTracking.Demo
{
	[RequireComponent(typeof(Text))]
	public class BodyTrackerStatus : MonoBehaviour
	{
		public TrackerLocation m_Role = TrackerLocation.Waist;

		private Text m_Text = null;
		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}
		private void Update()
		{
			for (int i = 0; i < Rdp.Tracker.s_TrackerIds.Length; i++)
			{
				Rdp.Tracker.Id id = Rdp.Tracker.s_TrackerIds[i];
				var role = Rdp.Tracker.GetTrackerRole(id);
				if (role == m_Role)
				{
					m_Text.text = m_Role + ": " + id.Name() + ", " + Rdp.Tracker.GetTrackerType(id, false).Name();
					break;
				}
				else
				{
					m_Text.text = m_Role + ": Disconnected";
				}
			}
		}
	}
}
