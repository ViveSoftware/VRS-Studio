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

namespace Wave.Essence.Hand
{
	public class HandInteractor : BaseInteractor
	{
		[SerializeField]
		private HandManager.HandType m_type = HandManager.HandType.Right;
		public HandManager.HandType HandType { get { return m_type; } set { m_type = value; } }
		[SerializeField]
		private HandManager.HandJoint m_Joint = HandManager.HandJoint.Wrist;
		public HandManager.HandJoint Joint { get { return m_Joint; } set { m_Joint = value; } }
	}
}
