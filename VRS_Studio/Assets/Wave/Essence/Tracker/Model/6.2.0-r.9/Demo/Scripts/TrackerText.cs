// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Text))]
	public class TrackerText : MonoBehaviour
	{
		public enum TextSelect
		{
			Connection,
			Role,
			Position,
			Rotation,
			Press,
			Touch,
			Battery,
			Extend,
			DeviceName,
			Acceleration,
		}

		#region Inspector
		[SerializeField]
		private TrackerId m_Tracker = TrackerId.Tracker0;
		public TrackerId Tracker { get { return m_Tracker; } set { m_Tracker = value; } }

		public TextSelect TextFor = TextSelect.Connection;
		#endregion

		private TrackerButton[] s_Buttons =
		{
			TrackerButton.System,
			TrackerButton.Menu,
			TrackerButton.Grip,
			TrackerButton.A,
			TrackerButton.B,
			TrackerButton.X,
			TrackerButton.Y,
			TrackerButton.Touchpad,
			TrackerButton.Trigger,
		};
		private readonly TrackerButton[] s_RightButtons =
		{
			TrackerButton.System,
			TrackerButton.Grip,
			TrackerButton.A,
			TrackerButton.B,
			TrackerButton.Touchpad,
			TrackerButton.Trigger,
		};
		private readonly TrackerButton[] s_LeftButtons =
		{
			TrackerButton.Menu,
			TrackerButton.Grip,
			TrackerButton.X,
			TrackerButton.Y,
			TrackerButton.Touchpad,
			TrackerButton.Trigger,
		};

		private enum TrackerType
		{
			Invalid = 0,
			HMD = 1,
			Controller = 2,
			Hand = 3,
			WristTracker = 4,
			ViveSelfTracker = 5,
			ViveSelfTrackerIM = 6
		}
		private readonly Dictionary<TrackerType, List<string>> s_TrackerNames = new Dictionary<TrackerType, List<string>>()
		{
			{ TrackerType.WristTracker, new List<string>() {
				"Vive_Tracker_Wrist", "Vive_Wrist_Tracker" }
			},
			{ TrackerType.ViveSelfTracker, new List<string>() {
				"Vive_Tracker_OT", "Vive_Self_Tracker", "Vive_Ultimate_Tracker" }
			},
			{ TrackerType.ViveSelfTrackerIM, new List<string>() {
				"Vive_Tracker_IU", "Vive_Self_Tracker_IM", "Vive_3Dof_Tracker_A", "Vive_Tracking_Tag" }
			}
		};
		private TrackerType GetBodyTrackingType(TrackerId id)
		{
			if (TrackerManager.Instance != null && TrackerManager.Instance.GetTrackerDeviceName(id, out string name))
			{
				// Checks self tracker first.
				for (int i = 0; i < s_TrackerNames[TrackerType.ViveSelfTracker].Count; i++)
				{
					if (name.Contains(s_TrackerNames[TrackerType.ViveSelfTracker][i]))
						return TrackerType.ViveSelfTracker;
				}
				// Checks self tracker im next.
				for (int i = 0; i < s_TrackerNames[TrackerType.ViveSelfTrackerIM].Count; i++)
				{
					if (name.Contains(s_TrackerNames[TrackerType.ViveSelfTrackerIM][i]))
						return TrackerType.ViveSelfTrackerIM;
				}
				// Checks wrist tracker last.
				for (int i = 0; i < s_TrackerNames[TrackerType.WristTracker].Count; i++)
				{
					if (name.Contains(s_TrackerNames[TrackerType.WristTracker][i]))
						return TrackerType.WristTracker;
				}
			}

			return TrackerType.Invalid;
		}

		#region MonoBehaviour overrides
		private Text m_Text = null;

		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}

		private void Start()
		{
			TrackerManager.Instance.OnSwipeToRight.AddListener(SwipeToRight);
			TrackerManager.Instance.OnSwipeToLeft.AddListener(SwipeToLeft);
			TrackerManager.Instance.OnSwipeToUp.AddListener(SwipeToUp);
			TrackerManager.Instance.OnSwipeToDown.AddListener(SwipeToDown);
		}

		private void Update()
		{
			if (m_Text == null || TrackerManager.Instance == null) { return; }
			CheckTrackerConnection();

			m_Text.text = m_Tracker.Name() + " ";

			switch (TextFor)
			{
				case TextSelect.Connection: TextConnection(); break;
				case TextSelect.Role: TextRole(); break;
				case TextSelect.Position: TextPosition(); break;
				case TextSelect.Rotation: TextRotation(); break;
				case TextSelect.Press: TextPress(); break;
				case TextSelect.Touch: TextTouch(); break;
				case TextSelect.Battery: TextBattery(); break;
				case TextSelect.Extend: TextExtend(); break;
				case TextSelect.DeviceName: TextDeviceName(); break;
				case TextSelect.Acceleration: TextAcceleration(); break;
				default:
					break;
			}
		}
		#endregion

		private bool m_Connected = false, m_ConnectedEx = false;
		private void CheckTrackerConnection()
		{
			if (TrackerManager.Instance == null) { return; }

			m_Connected = TrackerManager.Instance.IsTrackerConnected(m_Tracker);
			if (!m_ConnectedEx && m_Connected)
			{
				TrackerType trackerType = GetBodyTrackingType(m_Tracker);
				TrackerRole trackerRole = TrackerManager.Instance.GetTrackerRole(m_Tracker);
				if (trackerType == TrackerType.WristTracker)
				{
					/// If role match Pair1_Left or Pair1_Right, it means tracker is wrist tracker.
					if (trackerRole == TrackerRole.Pair1_Left)
					{
						Log.d("Wave.Essence.Tracker.Model.Demo.TrackerText", "CheckTrackerConnection() " + m_Tracker.Name() + " Sets button list to left.", true);
						s_Buttons = s_LeftButtons;
					}
					else if (trackerRole == TrackerRole.Pair1_Right)
					{
						Log.d("Wave.Essence.Tracker.Model.Demo.TrackerText", "CheckTrackerConnection() " + m_Tracker.Name() + " Sets button list to right.", true);
						s_Buttons = s_RightButtons;
					}
				}
			}
			m_ConnectedEx = m_Connected;
		}

		void TextConnection()
		{
			m_Text.text += "Connected: " + TrackerManager.Instance.IsTrackerConnected(m_Tracker);
			if (TrackerManager.Instance.GetTrackerTrackingState(m_Tracker, out UnityEngine.XR.InputTrackingState state))
			{
				m_Text.text += ", pose: (";
				if (state == UnityEngine.XR.InputTrackingState.All) { m_Text.text += "all"; }
				else
				{
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.Position)) { m_Text.text += "P "; }
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.Rotation)) { m_Text.text += "R "; }
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.Velocity)) { m_Text.text += "V "; }
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.AngularVelocity)) { m_Text.text += "AV "; }
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.Acceleration)) { m_Text.text += "A "; }
					if (state.HasFlag(UnityEngine.XR.InputTrackingState.AngularAcceleration)) { m_Text.text += "AA "; }
				}
				m_Text.text += ")";
			}
			else
				m_Text.text += ", pose: invalid";
		}
		void TextRole()
		{
			m_Text.text += "Role: " + TrackerManager.Instance.GetTrackerRole(m_Tracker);
		}
		void TextPosition()
		{
			Vector3 pos = TrackerManager.Instance.GetTrackerPosition(m_Tracker);
			m_Text.text += "X: " + pos.x + ", Y: " + pos.y + ", Z: " + pos.z;
		}
		void TextRotation()
		{
			Vector3 rot = TrackerManager.Instance.GetTrackerRotation(m_Tracker).eulerAngles;
			m_Text.text += "pitch: " + rot.x + ", yaw: " + rot.y + ", roll: " + rot.z;
		}
		void TextPress()
		{
			for (int i = 0; i < s_Buttons.Length; i++)
			{
				if (TrackerManager.Instance.TrackerButtonHold(m_Tracker, s_Buttons[i]))
				{
					m_Text.text += "Pressed: " + s_Buttons[i];
					return;
				}
			}
			m_Text.text += "N/A";
		}
		private Vector2 direction = Vector2.zero;
		void TextTouch()
		{
			for (int i = 0; i < s_Buttons.Length; i++)
			{
				if (TrackerManager.Instance.TrackerButtonTouching(m_Tracker, s_Buttons[i]))
				{
					m_Text.text += "Touched: " + s_Buttons[i];
					m_Text.text += ", " + TrackerManager.Instance.GetTrackerButtonAxisType(m_Tracker, s_Buttons[i]);
					Vector2 axis = TrackerManager.Instance.TrackerButtonAxis(m_Tracker, s_Buttons[i]);
					m_Text.text += ", (" + axis.x.ToString("F2") + ", " + axis.y.ToString("F2") + ")";
					direction = Vector2.zero;
					return;
				}
			}
			m_Text.text += "N/A";
			m_Text.text += direction != Vector2.zero ? SwipeDirecton() : ", Swipe: N/A";
		}
		void SwipeToRight(uint id)
		{
			if(id==(uint)Tracker)
				direction = Vector2.right;
		}
		void SwipeToLeft(uint id)
		{
			if (id == (uint)Tracker)
				direction = Vector2.left;
		}
		void SwipeToUp(uint id)
		{
			if (id == (uint)Tracker)
				direction = Vector2.up;
		}
		void SwipeToDown(uint id)
		{
			if (id == (uint)Tracker)
				direction = Vector2.down;
		}
		string SwipeDirecton()
		{
			string text = ", Swipe to ";
			if (direction == Vector2.right)
				text += "Right";
			else if (direction == Vector2.left)
				text += "Left";
			else if (direction == Vector2.up)
				text += "Up";
			else if (direction == Vector2.down)
				text += "Down";
			return text;
		}
		void TextBattery()
		{
			m_Text.text += "Battery: " + TrackerManager.Instance.GetTrackerBatteryLife(m_Tracker) * 100 + "%";
		}
		void TextExtend()
		{
			UInt64 timestamp = 0;
			var extData = TrackerManager.Instance.GetTrackerExtData(m_Tracker, out timestamp);
			string extStr = "";
			for (int i = 0; i < extData.Length; i++) { extStr += extData[i].ToString(); }
			m_Text.text += "Extend: " + extStr + ", " + timestamp;
		}
		void TextDeviceName()
		{
			if (TrackerManager.Instance.GetTrackerDeviceName(m_Tracker, out string name)) { m_Text.text += name; }
			TrackerId focused = TrackerManager.Instance.GetFocusedTracker();
			if (focused == m_Tracker) { m_Text.text += " (focused)"; }
		}
		void TextAcceleration()
		{
			var acceleration = TrackerManager.Instance.GetTrackerAcceleration(m_Tracker);
			m_Text.text += "Acceleration (" + acceleration.x.ToString() + ", " + acceleration.y.ToString() + ", " + acceleration.z.ToString() + ")";
		}
	}
}
