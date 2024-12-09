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
using UnityEngine.UI;
using Wave.Essence.Hand;

namespace Wave.Essence.Tracker.Model.Demo
{
	[RequireComponent(typeof(Text))]
	public class DisplayText : MonoBehaviour
	{
		public enum TextSelect
		{
			Motion,
			Role,
			Type
		}

		public bool IsLeftHand = false;
		public TextSelect TextFor = TextSelect.Motion;

		private Text m_Text = null;

		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}

		void Update()
		{
			if (HandManager.Instance == null || m_Text == null) { return; }

			HandManager.HandMotion handMotion = HandManager.Instance.GetHandMotion(IsLeftHand);
			string handMotionText = handMotion.ToString();
			if (handMotion == HandManager.HandMotion.Pinch && HandManager.Instance.GetPinchStrength(IsLeftHand) < 0.5f)
				handMotionText = "Pure";

			HandManager.HandHoldRole holdRole = HandManager.Instance.GetHandHoldRole(IsLeftHand);
			HandManager.HandHoldType holdType = HandManager.Instance.GetHandHoldType(IsLeftHand);

			m_Text.text = (IsLeftHand ? "Left " : "Right ");
			if (TextFor == TextSelect.Motion) { m_Text.text += "Hand Motion: " + handMotionText; }
			if (TextFor == TextSelect.Role) { m_Text.text += "Hold Role: " + holdRole; }
			if (TextFor == TextSelect.Type) { m_Text.text += "Hold Type: " + holdType; }
		}
	}
}
