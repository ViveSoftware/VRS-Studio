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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wave.Native;

namespace Wave.Essence.Hand.NearInteraction
{
	public class HandInteractor : BaseInteractor
	{
		const string LOG_TAG = "Wave.Essence.Hand.NearInteraction.HandInteractor";
		void DEBUG(string msg)
		{
            Log.d(LOG_TAG, (m_IsLeftHand ? "Left" : "Right") + " " + m_Joint + ", " + msg, true);
        }

		#region Inspector
		[SerializeField]
		private bool m_IsLeftHand = false;
		public bool IsLeftHand { get { return m_IsLeftHand; } set { m_IsLeftHand = value; } }

		[SerializeField]
		private JointType m_Joint = JointType.Wrist;
		public JointType Joint { get { return m_Joint; } set { m_Joint = value; } }

		[SerializeField]
		private bool m_IsGrabHand = false;
		public bool IsGrabHand { get { return m_IsGrabHand; } set { m_IsGrabHand = value; } }

		[SerializeField]
		private float m_SphereCollidingRadius = 0.1f;
		public float SphereCollidingRadius { get { return m_SphereCollidingRadius; } set { m_SphereCollidingRadius = value; } }

		[SerializeField]
		private int m_CollidingLayerMask = ~0;
		public int CollidingLayerMask { get { return m_CollidingLayerMask; } set { m_CollidingLayerMask = value; } }

		[SerializeField]
		private QueryTriggerInteraction m_QueryTrigger = QueryTriggerInteraction.Collide;
		public QueryTriggerInteraction QueryTrigger { get { return m_QueryTrigger; } set { m_QueryTrigger = value; } }

		/// <summary> Called when grab begins. </summary>
		public UnityEvent OnGrabBegin;

		/// <summary> Called while grabbing. </summary>
		public UnityEvent OnGrabbing;

		/// <summary> Called when grab releases. </summary>
		public UnityEvent OnGrabEnd;
		#endregion

		#region Properties
		private NearHandData m_Hand;
		public NearHandData Hand { get { return m_Hand; } set { m_Hand = value; } }

		public bool isTracked { get { return m_Hand.isTracked; } }

		private HandInteractable m_GrabbedObject = null;
		public HandInteractable GrabbedObject { get { return m_GrabbedObject; } set { m_GrabbedObject = value; } }
		public bool isGrabbing { get { return (m_GrabbedObject != null); } }

		private List<HandInteractable> m_GrabCandidates = new List<HandInteractable>();
		public List<HandInteractable> GrabCandidates { get { return m_GrabCandidates; } }
		#endregion

		private void Start()
		{
			OnGrabBegin.AddListener(HandleGrabBegin);
			OnGrabbing.AddListener(HandleGrabbing);
			OnGrabEnd.AddListener(HandleGrabEnd);
		}
		private void OnDestroy()
		{
			OnGrabBegin.RemoveAllListeners();
			OnGrabbing.RemoveAllListeners();
			OnGrabEnd.RemoveAllListeners();
		}
		private void FixedUpdate()
		{
			m_Hand = VIUHand.Get(m_IsLeftHand);
			HandleGrabMotion();
		}

		private GrabClassifier m_GrabClassifier = null;

		/// <summary> Handles the grab motion in the Palm Hand Interactor only. </summary>
		private void HandleGrabMotion()
		{
			if (!m_IsGrabHand) { return; }
			if (m_GrabClassifier == null) {
				DEBUG("HandleGrabMotion() Creates a grab classifier.");
				m_GrabClassifier = new GrabClassifier(this, SphereCollidingRadius, CollidingLayerMask, QueryTrigger);
			}

			UpdateGrabCandidates(ref m_GrabCandidates);
			m_GrabClassifier.FixedUpdateGrabState();

			CheckGrabEnd();
			CheckGrabBegin();
			CheckGrabbing();
		}

		private Collider[] s_GrabCandidates = new Collider[16];
		private void UpdateGrabCandidates(ref List<HandInteractable> candidates)
		{
			/// Calculates the sphere colliding candidates.
			candidates.Clear();
			int collidingCount = GrabClassifier.GetSphereColliderResults(
				position: trans.position,
				resultsBuffer: ref s_GrabCandidates,
				sphereCollidingRadius: SphereCollidingRadius,
				collidingLayerMask: CollidingLayerMask,
				queryTrigger: QueryTrigger);

			for (int i = 0; i < collidingCount; i++)
			{
				if (InteractionHub.GetInteractable(s_GrabCandidates[i].attachedRigidbody, out BaseInteractable value))
				{
					HandInteractable actable = value.actable.GetComponent<HandInteractable>();
					if (actable != null) { candidates.Add(actable); }
				}
			}
		}

		void HandleGrabBegin()
		{
			if (m_GrabbedObject != null)
			{
				DEBUG("HandleGrabBegin() m_GrabbedObject: " + m_GrabbedObject.actable.name);
				m_GrabbedObject.GrabBeginHandle(this);
			}
		}
		private void CheckGrabBegin()
		{
			if (m_GrabbedObject != null) { return; }

			HandInteractable newGrabObject = null;
			if (m_GrabClassifier.FixedUpdateGrabBegin(out newGrabObject))
			{
				m_GrabbedObject = newGrabObject;
				OnGrabBegin.Invoke();
			}
		}

		void HandleGrabbing()
		{
			if (m_GrabbedObject != null)
			{
				m_GrabbedObject.GrabbingHandle(this);
			}
		}
		private void CheckGrabbing()
		{
			if (m_GrabbedObject != null) { OnGrabbing.Invoke(); }
		}

		void HandleGrabEnd()
		{
			if (m_GrabbedObject != null)
			{
				DEBUG("HandleGrabEnd() m_GrabbedObject: " + m_GrabbedObject.actable.name);
				m_GrabbedObject.GrabEndHandle(this);
			}
		}
		private void CheckGrabEnd()
		{
			if (m_GrabbedObject == null) { return; }

			HandInteractable releaseObject = null;
			if (m_GrabClassifier.FixedUpdateGrabEnd(out releaseObject))
			{
				OnGrabEnd.Invoke();
				m_GrabbedObject = null;
			}
		}
	}
}
