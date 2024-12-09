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
using Wave.Essence.Hand;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class ShowHand : MonoBehaviour
	{
		public List<GameObject> HandComponents = new List<GameObject>();

		[SerializeField]
		private bool m_IsLeft = false;
		public bool IsLeft { get { return m_IsLeft; } set { m_IsLeft = value; } }

		private void Update()
		{
			if (HandManager.Instance == null || HandComponents == null || HandComponents.Count == 0) { return; }

			HandManager.HandMotion handMotion = HandManager.Instance.GetHandMotion(m_IsLeft);
			HandManager.HandHoldRole holdRole = HandManager.Instance.GetHandHoldRole(m_IsLeft);
			if ((handMotion == HandManager.HandMotion.Hold) && (holdRole == HandManager.HandHoldRole.Main))
			{
				for (int i = 0; i < HandComponents.Count; i++)
					HandComponents[i].SetActive(false);
			}
			else
			{
				for (int i = 0; i < HandComponents.Count; i++)
					HandComponents[i].SetActive(true);
			}
		}
	}
}
