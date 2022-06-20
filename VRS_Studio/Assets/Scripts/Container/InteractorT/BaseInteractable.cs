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
using Wave.Native;

namespace Wave.Essence.Hand
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	public class BaseInteractable : MonoBehaviour, IInteractable
	{
		const string LOG_TAG = "Wave.Essence.Hand.BaseInteractable";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}
		void INFO(string msg) { Log.w(LOG_TAG, msg, true); }

		#region IInteractable
		private GameObject m_actable = null;
		public GameObject actable { get { return m_actable; } }

		private Transform m_Transform = null;
		public Transform trans { get { return m_Transform; } }

		private Rigidbody m_rigid = null;
		public Rigidbody rigid { get { return m_rigid; } }
		#endregion

		/// <summary>
		/// Called when this interactable begins colliding with any interactors.
		/// This interactable was not colliding with any interactors last frame.
		/// </summary>
		public Action OnTouchBegin;
		/// <summary>
		/// Called when this interactable ceases colliding with any interactors.
		/// This interactable was colliding with interactors last frame.
		/// </summary>
		public Action OnTouchEnd;
		/// <summary>
		/// Called every frame during which one or more interactors is colliding with this interactable.
		/// </summary>
		public Action OnTouching;

		[SerializeField]
		private IActionEvent.ActionTaker m_ActionTaker = IActionEvent.ActionTaker.Hand;
		public IActionEvent.ActionTaker actionTaker { get { return m_ActionTaker; } set { m_ActionTaker = value; } }

		[HideInInspector]
		[Tooltip("The interactable is interacting with interactor(s).")]
		public bool interacting = false;

		/// The interactors those are interacting with this interactable.
		private Dictionary<BaseInteractor, float> s_InteractingActors = new Dictionary<BaseInteractor, float>();
		public List<BaseInteractor> GetInteractingActors()
		{
			List<BaseInteractor> actors = new List<BaseInteractor>();
			foreach (var actor in s_InteractingActors)
				actors.Add(actor.Key);
			return actors;
		}

		protected virtual void OnEnable()
		{
			m_actable = gameObject;
			m_Transform = transform;
			m_rigid = GetComponent<Rigidbody>();
			if (m_rigid != null)
			{
				INFO("OnEnable() add interactable " + gameObject.name);
				InteractionHub.AddInteractable(m_rigid, this);
			}

		}
		protected virtual void OnDisable()
		{
			if (m_rigid != null)
			{
				INFO("OnDisable() remove interactable " + gameObject.name);
				InteractionHub.RemoveInteractable(m_rigid);
			}
		}

		private List<BaseInteractor> lastInteractors = new List<BaseInteractor>();
		protected List<BaseInteractor> currInteractors = new List<BaseInteractor>();
		void FixedUpdate()
		{
			currInteractors.Clear();
			foreach (var actor in s_InteractingActors) { currInteractors.Add(actor.Key); }

			for (int i = 0; i < currInteractors.Count; i++)
			{
				if (!lastInteractors.Contains(currInteractors[i]))
				{
					// Do something of new interactors currInteractors[i].
				}
			}
			for (int i = 0; i < lastInteractors.Count; i++)
			{
				if (!currInteractors.Contains(lastInteractors[i]))
				{
					// Do something of previous interactors lastInteractors[i].
				}
			}

			lastInteractors.Clear();
			for (int i = 0; i < currInteractors.Count; i++) { lastInteractors.Add(currInteractors[i]); }
		}

		void OnCollisionEnter(Collision collision)
		{
			//INFO("OnCollisionEnter() devin " + collision.gameObject.name);
			if (collision.rigidbody == null) { return; }

			INFO("OnCollisionEnter() " + collision.gameObject.name);

			if (collision.impulse.magnitude > 0)
			{
				if (InteractionHub.GetInteractor(collision.rigidbody, out BaseInteractor value))
				{
					if (!s_InteractingActors.ContainsKey(value))
						s_InteractingActors.Add(value, Time.frameCount);

					value.NotifyCollisionEnter(this);
				}

				OnTouchBegin();
			}
		}

		//void OnTriggerEnter(Collider other)
		//{
		//	INFO("OnTriggerEnter() " + other.gameObject.name);
		//	//other.

		//	if (InteractionHub.GetInteractor(other.attachedRigidbody, out BaseInteractor value))
		//	{
		//		if (!s_InteractingActors.ContainsKey(value))
		//			s_InteractingActors.Add(value, Time.frameCount);

		//		var t = value.GetComponent<HandInteractor>();

		//		if (t != null)
		//		{
		//			INFO("OnTriggerEnter() HandInteractor found, type=" + t.HandType + " , joint=" + t.Joint);
		//		}
		//		else
		//		{
		//			INFO("OnTriggerEnter() HandInteractor not found");
		//		}

		//		value.NotifyCollisionEnter(this);
		//	}
		//}

		//void OnTriggerExit(Collider other)
		//{
		//	INFO("OnTriggerExit() " + other.gameObject.name);
		//}

		//void OnCollisionExit(Collision collision)
		//{
		//	if (collision.rigidbody == null) { return; }

		//	INFO("OnCollisionExit() " + collision.gameObject.name);

		//	if (InteractionHub.GetInteractor(collision.rigidbody, out BaseInteractor value))
		//	{
		//		if (s_InteractingActors.ContainsKey(value))
		//			s_InteractingActors.Remove(value);

		//		value.NotifyCollisionExit(this);
		//	}

		//	OnTouchEnd();
		//}
	}
}
