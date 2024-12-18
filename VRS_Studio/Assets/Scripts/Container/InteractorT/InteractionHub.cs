﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Wave.Essence.Hand
{
	public static class InteractionHub
	{
		private static Dictionary<Rigidbody, BaseInteractor> s_Interactors = new Dictionary<Rigidbody, BaseInteractor>();
		private static Dictionary<Rigidbody, BaseInteractable> s_Interactables = new Dictionary<Rigidbody, BaseInteractable>();
		private static List<BaseInteractor> m_Interactors = new List<BaseInteractor>();
		private static List<BaseInteractable> m_Interactables = new List<BaseInteractable>();

		public static void AddInteractor(Rigidbody rigid, BaseInteractor actor)
		{
			if (s_Interactors == null)
				s_Interactors = new Dictionary<Rigidbody, BaseInteractor>();
			if (!s_Interactors.ContainsKey(rigid))
				s_Interactors.Add(rigid, actor);
		}
		public static bool GetInteractor(Rigidbody rigid, out BaseInteractor actor)
		{
			if (s_Interactors == null)
				s_Interactors = new Dictionary<Rigidbody, BaseInteractor>();
			if (s_Interactors.ContainsKey(rigid))
			{
				if (s_Interactors.TryGetValue(rigid, out BaseInteractor value))
				{
					actor = value;
					return true;
				}
			}

			actor = null;
			return false;
		}
		public static void RemoveInteractor(Rigidbody rigid)
		{
			if (s_Interactors == null)
				s_Interactors = new Dictionary<Rigidbody, BaseInteractor>();
			if (s_Interactors.ContainsKey(rigid))
				s_Interactors.Remove(rigid);
		}
		public static List<BaseInteractor> GetInteractors()
		{
			if (s_Interactors == null)
				s_Interactors = new Dictionary<Rigidbody, BaseInteractor>();
			m_Interactors.Clear();
			foreach (var actor in s_Interactors) { m_Interactors.Add(actor.Value); }
			return m_Interactors;
		}

		public static void AddInteractable(Rigidbody rigid, BaseInteractable actable)
		{
			if (s_Interactables == null)
				s_Interactables = new Dictionary<Rigidbody, BaseInteractable>();
			if (!s_Interactables.ContainsKey(rigid))
				s_Interactables.Add(rigid, actable);
		}
		public static bool GetInteractable(Rigidbody rigid, out BaseInteractable actable)
		{
			if (s_Interactables == null)
				s_Interactables = new Dictionary<Rigidbody, BaseInteractable>();
			if (s_Interactables.ContainsKey(rigid))
			{
				if (s_Interactables.TryGetValue(rigid, out BaseInteractable value))
				{
					actable = value;
					return true;
				}
			}

			actable = null;
			return false;
		}
		public static void RemoveInteractable(Rigidbody rigid)
		{
			if (s_Interactables == null)
				s_Interactables = new Dictionary<Rigidbody, BaseInteractable>();
			if (s_Interactables.ContainsKey(rigid))
				s_Interactables.Remove(rigid);
		}
		public static List<BaseInteractable> GetInteractables()
		{
			if (s_Interactables == null)
				s_Interactables = new Dictionary<Rigidbody, BaseInteractable>();
			m_Interactables.Clear();
			foreach (var actable in s_Interactables) { m_Interactables.Add(actable.Value); }
			return m_Interactables;
		}
	}
}