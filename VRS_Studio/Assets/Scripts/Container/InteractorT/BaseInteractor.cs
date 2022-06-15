// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Hand
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	public class BaseInteractor : MonoBehaviour, IInteractor
	{
		const string LOG_TAG = "Wave.Essence.Hand.BaseInteractor";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}
		void INFO(string msg) { Log.w(LOG_TAG, msg, true); }

		#region IInteractor
		private GameObject m_actor = null;
		public GameObject actor { get { return m_actor; } }

		private Transform m_Transform = null;
		public Transform trans { get { return m_Transform; } }

		private Rigidbody m_rigid = null;
		public Rigidbody rigid { get { return m_rigid; } }
		#endregion

		[SerializeField]
		private IActionEvent.ActionTaker m_ActionTaker = IActionEvent.ActionTaker.Hand;
		public IActionEvent.ActionTaker actionTaker { get { return m_ActionTaker; } set { m_ActionTaker = value; } }

		[HideInInspector]
		[Tooltip("The interactor is interacting with interactable(s).")]
		public bool interacting = false;

		/// The interactables those are interacting with this interactor.
		private Dictionary<BaseInteractable, float> s_InteractingActables = new Dictionary<BaseInteractable, float>();
		public List<BaseInteractable> GetInteractingActables()
		{
			List<BaseInteractable> actables = new List<BaseInteractable>();
			foreach (var actable in s_InteractingActables)
				actables.Add(actable.Key);
			return actables;
		}

		protected virtual void OnEnable()
		{
			m_actor = gameObject;
			m_Transform = transform;
			m_rigid = GetComponent<Rigidbody>();
			if (m_rigid != null)
			{
				INFO("OnEnable() add interactor " + gameObject.name);
				InteractionHub.AddInteractor(m_rigid, this);
			}
		}
		protected virtual void OnDisable()
		{
			if (m_rigid != null)
			{
				INFO("OnDisable() remove interactor " + gameObject.name);
				InteractionHub.RemoveInteractor(m_rigid);
			}
		}

		public void NotifyCollisionEnter(BaseInteractable actable)
		{
			//INFO("NotifyCollisionEnter() devin" + actable.actable.name);
			if (actable == null) { return; }
			if (!s_InteractingActables.ContainsKey(actable))
			{
				INFO("NotifyCollisionEnter() " + actable.actable.name);
				s_InteractingActables.Add(actable, Time.frameCount);
			}
		}
		public void NotifyCollisionExit(BaseInteractable actable)
		{
			if (actable == null) { return; }
			if (s_InteractingActables.ContainsKey(actable))
			{
				INFO("NotifyCollisionExit() " + actable.actable.name);
				s_InteractingActables.Remove(actable);
			}
		}
	}
}
