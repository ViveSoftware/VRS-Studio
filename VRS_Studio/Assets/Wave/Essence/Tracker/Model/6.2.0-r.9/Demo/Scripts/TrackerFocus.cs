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
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class TrackerFocus : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Tracker.Model.Demo.TrackerFocus";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Log.d(LOG_TAG, msg, true); }

		void UpdateTrackerFocus(int trackerId)
		{
			sb.Clear().Append("UpdateTrackerFocus() ").Append(trackerId); DEBUG(sb);
			if (TrackerManager.Instance != null)
			{
				for (int i = 0; i < TrackerUtils.s_TrackerIds.Length; i++)
				{
					if ((int)TrackerUtils.s_TrackerIds[i] == trackerId)
					{
						TrackerManager.Instance.SetFocusedTracker(TrackerUtils.s_TrackerIds[i]);
						return;
					}
				}
			}
		}

		private Dropdown m_DropDown = null;
		private Text m_DropDownText = null;
		private string[] textStrings = new string[] {
			TrackerId.Tracker0.Name(),
			TrackerId.Tracker1.Name(),
			TrackerId.Tracker2.Name(),
			TrackerId.Tracker3.Name(),
			TrackerId.Tracker4.Name(),
			TrackerId.Tracker5.Name(),
			TrackerId.Tracker6.Name(),
			TrackerId.Tracker7.Name(),
			TrackerId.Tracker8.Name(),
			TrackerId.Tracker9.Name(),
			TrackerId.Tracker10.Name(),
			TrackerId.Tracker11.Name(),
			TrackerId.Tracker12.Name(),
			TrackerId.Tracker13.Name(),
			TrackerId.Tracker14.Name(),
			TrackerId.Tracker15.Name()
		};

		void DropdownValueChanged(Dropdown change)
		{
			sb.Clear().Append("DropdownValueChanged(): ").Append(change.value); DEBUG(sb);
			UpdateTrackerFocus(change.value);
		}

		TrackerId m_FocusedTracker = TrackerId.Tracker0;
		void Start()
		{
			sb.Clear().Append("Start()"); DEBUG(sb);

			m_DropDown = GetComponent<Dropdown>();
			m_DropDown.onValueChanged.AddListener(
				delegate { DropdownValueChanged(m_DropDown); }
				);
			m_DropDownText = GetComponentInChildren<Text>();

			// clear all option item
			m_DropDown.options.Clear();

			// fill the dropdown menu OptionData
			foreach (string c in textStrings)
			{
				m_DropDown.options.Add(new Dropdown.OptionData() { text = c });
			}

			if (TrackerManager.Instance != null)
			{
				m_FocusedTracker = TrackerManager.Instance.GetFocusedTracker();
				m_DropDown.value = (int)m_FocusedTracker;
			}
		}
		void Update()
		{
			if (m_DropDownText == null) { return; }

			m_DropDownText.text = textStrings[m_DropDown.value];
		}
	}
}
