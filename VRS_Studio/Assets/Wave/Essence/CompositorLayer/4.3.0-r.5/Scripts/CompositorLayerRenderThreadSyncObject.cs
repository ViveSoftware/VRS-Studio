// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wave.Essence.CompositorLayer
{ 
	public class CompositorLayerRenderThreadSyncObject
	{
		private static IntPtr GetFunctionPointerForDelegate(Delegate del)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			return IntPtr.Zero;
#elif UNITY_ANDROID
			return Marshal.GetFunctionPointerForDelegate(del);
#else
			return IntPtr.Zero;
#endif
		}

		public delegate void CompositorLayerRenderEventDelegate(int eventID);
		private static readonly CompositorLayerRenderEventDelegate handle = new CompositorLayerRenderEventDelegate(RunSyncObjectInRenderThread);
		private static readonly IntPtr handlePtr = GetFunctionPointerForDelegate(handle);

		public delegate void TaskQueueDelagate(PreAllocatedQueue taskQueue);

		private static List<CompositorLayerRenderThreadSyncObject> taskList = new List<CompositorLayerRenderThreadSyncObject>();

		private readonly PreAllocatedQueue queue = new PreAllocatedQueue();
		public PreAllocatedQueue Queue { get { return queue; } }

		private readonly TaskQueueDelagate receiver;
		private readonly int taskID;

		public CompositorLayerRenderThreadSyncObject(TaskQueueDelagate taskQueueDelegate)
		{
			receiver = taskQueueDelegate;
			if (receiver == null)
				throw new ArgumentNullException("receiver should not be null");

			taskList.Add(this);
			taskID = taskList.IndexOf(this);
		}

		~CompositorLayerRenderThreadSyncObject()
		{
			try { taskList.RemoveAt(taskID); } finally { }
		}

		[MonoPInvokeCallback(typeof(CompositorLayerRenderEventDelegate))]
		private static void RunSyncObjectInRenderThread(int taskID)
		{
			taskList[taskID].ReceiveEvent();
		}

		// Run in GameThread
		public void IssueEvent()
		{
#if UNITY_EDITOR && UNITY_ANDROID
			if (Application.isEditor)
			{
				receiver(queue);
				return;
			}
#endif

			// Let the render thread run the RunSyncObjectInRenderThread(id)
#if UNITY_ANDROID
			GL.IssuePluginEvent(handlePtr, taskID);
#else
			receiver(queue);
			return;
#endif
		}

		private void ReceiveEvent()
		{
			receiver(queue);
		}
	}

	public class Task
	{
		public bool isFree = true;
	}

	public class TaskPool
	{
		private readonly List<Task> pool = new List<Task>(2) { };
		private int index = 0;

		public TaskPool() { }

		private int Next(int value)
		{
			if (++value >= pool.Count)
				value = 0;
			return value;
		}

		public T Obtain<T>() where T : Task, new()
		{
			int c = pool.Count;
			int i = index;
			for (int j = 0; j < c; i++, j++)
			{
				if (i >= c)
					i = 0;
				if (pool[i].isFree)
				{
					//Debug.LogError("Obtain idx=" + i);
					index = i;
					return (T)pool[i];
				}
			}
			index = Next(i);
			var newItem = new T()
			{
				isFree = true
			};
			pool.Insert(index, newItem);
			//Debug.LogError("Obtain new one.  Pool.Count=" + pool.Count);
			return newItem;
		}

		public void Lock(Task msg)
		{
			msg.isFree = false;
		}

		public void Release(Task msg)
		{
			msg.isFree = true;
		}
	}

	public class PreAllocatedQueue : TaskPool
	{
		private readonly List<Task> list = new List<Task>(2) { null, null };
		private int queueBegin = 0;
		private int queueEnd = 0;

		public PreAllocatedQueue() : base() { }

		private int Next(int value)
		{
			if (++value >= list.Count)
				value = 0;
			return value;
		}

		public void Enqueue(Task msg)
		{
			Lock(msg);
			queueEnd = Next(queueEnd);

			if (queueEnd == queueBegin)
			{
				list.Insert(queueEnd, msg);
				queueBegin++;
			}
			else
			{
				list[queueEnd] = msg;
			}
		}

		public Task Dequeue()
		{
			queueBegin = Next(queueBegin);
			return list[queueBegin];
		}
	}
}
