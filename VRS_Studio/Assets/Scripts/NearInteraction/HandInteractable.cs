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
using UnityEngine.Events;
using Wave.Native;

namespace Wave.Essence.Hand.NearInteraction
{
	public class HandInteractable : BaseInteractable
	{
		const string LOG_TAG = "Wave.Essence.Hand.NearInteraction.HandInteractable";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, actable.name + ", " + msg, true);
		}

		#region Inspector
		[SerializeField]
		private bool m_Grabbable = true;
		public bool Grabbable { get { return m_Grabbable; } set { m_Grabbable = value; } }

        /// <summary> Called when grab begins. </summary>
        public UnityEvent OnGrabbedBegin;

        /// <summary> Called while grabbing. </summary>
        public UnityEvent Grabbed;

        /// <summary> Called when grab releases. </summary>
        public UnityEvent OnGrabbedEnd;

        public Transform LeftGrabberTransform, RightGrabberTransform;
        #endregion

        #region Properties
        private HandInteractor m_GrabHand = null;
		public HandInteractor GrabHand { get { return m_GrabHand; } set { m_GrabHand = value; } }
		public bool IsGrabbed { get { return (m_GrabHand != null); } }
		#endregion

		protected override void Update()
		{
			base.Update();
		}

		#region Atable movement
		private Vector3 beginPositionActable = Vector3.zero, beginPositionHand = Vector3.zero;
		private bool useGravityOrigin = false;
		private void BeginGrabActable(HandInteractor hand)
		{
			if (!m_Grabbable) { return; }

			DEBUG("BeginGrabActable() by " + hand.actor.name);
			beginPositionActable = trans.position;
			beginPositionHand = hand.trans.position;

			// Do NOT use gravity when grabbing.
			useGravityOrigin = rigid.useGravity;
			rigid.useGravity = false;
            rigid.isKinematic = true;

            //Workaround
            if (hand.IsLeftHand) this.transform.SetParent(LeftGrabberTransform);
            else this.transform.SetParent(RightGrabberTransform);

            OnGrabbedBegin.Invoke();
		}
		private void GrabbingActable(HandInteractor hand)
		{
			if (!m_Grabbable) { return; }
			if (m_GrabHand == hand)
			{
				//Vector3 posOffset = hand.trans.position - beginPositionHand;
				//trans.position = beginPositionActable + posOffset;
			}
		}
		private void EndGrabActable(HandInteractor hand)
		{
			if (!m_Grabbable) { return; }

			DEBUG("EndGrabActable() by " + hand.actor.name);
			rigid.useGravity = useGravityOrigin;
            rigid.isKinematic = false;

            //Workaround
            this.transform.SetParent(null);

            OnGrabbedEnd.Invoke();
		}
		#endregion

		/// <summary> Called by a HandInteractor to inform that grab begins. </summary>
		public void GrabBeginHandle(HandInteractor hand)
		{
			DEBUG("GrabBeginHandle() by "
				+ (hand.IsLeftHand ? "Left": "Right") + " " + hand.Joint + ": " + hand.actor.name);

			m_GrabHand = hand;
			BeginGrabActable(hand);
		}
		/// <summary> Called by a HandInteractor to inform grabbing. </summary>
		public void GrabbingHandle(HandInteractor hand)
		{
			if (m_GrabHand == hand)
			{
				/*DEBUG("GrabbingHandle() by "
					+ (hand.IsLeftHand ? "Left" : "Right") + " " + hand.Joint + ": " + hand.actor.name);*/

				GrabbingActable(hand);
			}
		}
		/// <summary> Called by a HandInteractor to inform that grab ends. </summary>
		public void GrabEndHandle(HandInteractor hand)
		{
			if (m_GrabHand == hand)
			{
				DEBUG("GrabEndHandle() by "
					+ (hand.IsLeftHand ? "Left" : "Right") + " " + hand.Joint + ": " + hand.actor.name);

				m_GrabHand = null;
				EndGrabActable(hand);
			}
		}
	}
}