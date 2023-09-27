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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.CompositorLayer
{
	public class CompositorLayerManager : MonoBehaviour
	{
		private int maxLayerCount = 0;

		private static CompositorLayerManager instance = null;
		private List<CompositorLayer> compositorLayers = new List<CompositorLayer>();
		private List<CompositorLayer> compositorLayersToBeSubscribed = new List<CompositorLayer>();
		private List<CompositorLayer> compositorLayersToBeUnsubscribed = new List<CompositorLayer>();

		private const string LOG_TAG = "Wave_CompositorLayerManager";

		private static void DEBUG(string log)
		{
#if UNITY_ANDROID
			Log.d(LOG_TAG, log);

#elif UNITY_EDITOR
			Debug.Log(LOG_TAG + ": DEBUG " + log);

#endif
		}

		private static void ERROR(string log)
		{
#if UNITY_ANDROID
			Log.e(LOG_TAG, log);

#elif UNITY_EDITOR
			Debug.Log(LOG_TAG + ": ERROR " + log);

#endif
		}

		#region public parameter access functions
		public static CompositorLayerManager GetInstance()
		{
			if (instance == null)
			{
				GameObject compositorLayerManagerGO = new GameObject("MultiLayerManager", typeof(CompositorLayerManager));
				instance = compositorLayerManagerGO.GetComponent<CompositorLayerManager>();
			}

			return instance;
		}

		public int MaxLayerCount()
		{
			return maxLayerCount;
		}

		public int RemainingLayerCount()
		{
			int count = maxLayerCount - compositorLayers.Count;
			if (count < 0)
			{
				return 0;
			}

			return count;
		}

		public int CurrentLayerCount()
		{
			return compositorLayers.Count;
		}
		#endregion

		#region Monobehaviour Lifecycle
		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(instance);
				instance = this;
			}
		}

		void Start()
		{
			maxLayerCount = (int)Interop.WVR_GetMaxCompositionLayerCount();
		}

		void Update()
		{
			bool compositorLayerStatusUpdateNeeded = false;

			//Process Sub and Unsub list in bulk at once per frame
			if (compositorLayersToBeUnsubscribed.Count > 0)
			{
				foreach (CompositorLayer layerToBeRemoved in compositorLayersToBeUnsubscribed)
				{
					Log.d(LOG_TAG, "compositorLayersToBeUnsubscribed: Processing");
					if (compositorLayers.Contains(layerToBeRemoved) && !compositorLayersToBeSubscribed.Contains(layerToBeRemoved))
					{
						layerToBeRemoved.TerminateLayer();
						compositorLayers.Remove(layerToBeRemoved);
					}
				}
				compositorLayersToBeUnsubscribed.Clear();
				compositorLayerStatusUpdateNeeded = true;
			}

			if (compositorLayersToBeSubscribed.Count > 0)
			{
				DEBUG("compositorLayersToBeSubscribed: Processing");
				foreach (CompositorLayer layerToBeAdded in compositorLayersToBeSubscribed)
				{
					if (!compositorLayers.Contains(layerToBeAdded))
					{
						compositorLayers.Add(layerToBeAdded);
						DEBUG("Add new layer");
					}
					else if (layerToBeAdded.isRenderPriorityChanged)
					{
						DEBUG("Layer RenderPriority changed");
					}
				}
				compositorLayersToBeSubscribed.Clear();
				compositorLayerStatusUpdateNeeded = true;
			}

			if (compositorLayerStatusUpdateNeeded)
			{
				DEBUG("compositorLayerStatusUpdateNeeded");
				UpdateLayerStatus();
				compositorLayerStatusUpdateNeeded = false;
			}
		}
		#endregion

		public void SubscribeToLayerManager(CompositorLayer layerToBeAdded)
		{
			if (compositorLayersToBeSubscribed == null)
			{
				DEBUG("SubscribeToLayerManager: Layer List not found. Creating a new one.");
				compositorLayersToBeSubscribed = new List<CompositorLayer>();
			}

			if (!compositorLayersToBeSubscribed.Contains(layerToBeAdded))
			{
				DEBUG("SubscribeToLayerManager: Add layer");
				compositorLayersToBeSubscribed.Add(layerToBeAdded);
			}
		}

		public void UnsubscribeFromLayerManager(CompositorLayer layerToBeRemoved)
		{
			if (compositorLayersToBeUnsubscribed == null)
			{
				DEBUG("UnsubscribeFromLayerManager: Layer List not found. Creating a new one.");
				compositorLayersToBeUnsubscribed = new List<CompositorLayer>();
			}

			if (!compositorLayersToBeUnsubscribed.Contains(layerToBeRemoved))
			{
				DEBUG("UnsubscribeFromLayerManager: Remove layer");
				compositorLayersToBeUnsubscribed.Add(layerToBeRemoved);
			}
		}

		private void UpdateLayerStatus()
		{
			SortCompositorLayers();
			RenderCompositorLayers();
		}

		private void SortCompositorLayers()
		{
			if (compositorLayers == null)
			{
				return;
			}

			CompositorLayerRenderPriorityComparer renderPriorityComparer = new CompositorLayerRenderPriorityComparer();
			compositorLayers.Sort(renderPriorityComparer);
		}

		private void RenderCompositorLayers()
		{
			maxLayerCount = (int)Interop.WVR_GetMaxCompositionLayerCount();

			for (int layerIndex=0; layerIndex < compositorLayers.Count; layerIndex++)
			{
				if (layerIndex < maxLayerCount) //Render as normal layers
				{
					if (compositorLayers[layerIndex].RenderAsLayer()) //Successfully initialized
					{
						DEBUG("RenderCompositorLayers: Layer " + compositorLayers[layerIndex].name + " Initialized successfully, Priority: " + compositorLayers[layerIndex].GetRenderPriority() + " layerIndex: " + layerIndex);
					}
					else
					{
						DEBUG("RenderCompositorLayers: Layer Initialization failed." + " layerIndex: " + layerIndex);
					}
				}
				else //Fallback if enabled
				{
					compositorLayers[layerIndex].RenderInGame();
					DEBUG("RenderCompositorLayers: Layer " + compositorLayers[layerIndex].name + " Rendering in game, Priority: " + compositorLayers[layerIndex].GetRenderPriority() + " layerIndex: " + layerIndex);
				}
			}

		}

		class CompositorLayerRenderPriorityComparer : IComparer<CompositorLayer>
		{
			public int Compare(CompositorLayer layerX, CompositorLayer layerY)
			{
				//Rule1: Higher Render Priority -> Front of the list
				//Rule2: Same Render Priority -> Do not move layer
				if (layerX.GetRenderPriority() > layerY.GetRenderPriority())
				{
					return -1;
				}
				else if (layerX.GetRenderPriority() < layerY.GetRenderPriority())
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}


