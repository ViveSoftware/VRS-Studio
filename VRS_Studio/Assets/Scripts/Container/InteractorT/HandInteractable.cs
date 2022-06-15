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

namespace Wave.Essence.Hand
{
	public class HandInteractable : BaseInteractable
	{
		[SerializeField]
		private HandManager.HandType m_type = HandManager.HandType.Right;
		public HandManager.HandType HandType { get { return m_type; } set { m_type = value; } }

		[SerializeField]
		private HandManager.HandJoint m_Joint = HandManager.HandJoint.Wrist;
		public HandManager.HandJoint Joint { get { return m_Joint; } set { m_Joint = value; } }

		[SerializeField]
		private bool m_ignoreJoint = false;
		public bool IgnoreJoint { get { return m_ignoreJoint; } set { m_ignoreJoint = value; } }

		[SerializeField]
		private bool m_ignoreType = false;
		public bool IgnoreType { get { return m_ignoreType; } set { m_ignoreType = value; } }

		[HideInInspector]
		public bool isGrabbed { get; }

		[HideInInspector]
		public bool isTouched = false;

        public UnityEvent TouchBeginEvent, TouchEndEvent;

        private void Start()
		{
			OnTouchBegin += TouchBeginHandle;
			OnTouchEnd += TouchEndHandle;
			OnTouching += TouchingHandle;
		}
		private void OnDestroy()
		{
			OnTouchBegin -= TouchBeginHandle;
			OnTouchEnd -= TouchEndHandle;
			OnTouching -= TouchingHandle;
		}
		void TouchBeginHandle()
		{
		}
		void TouchEndHandle()
		{
        }
		void TouchingHandle()
		{
		}

		void OnTriggerEnter(Collider other)
		{
			//Log.w("HandInteractable", "OnTriggerEnter() " + other.gameObject.name, true);
			if (other.attachedRigidbody != null && InteractionHub.GetInteractor(other.attachedRigidbody, out BaseInteractor value))
			{
				//if (!s_InteractingActors.ContainsKey(value))
				//	s_InteractingActors.Add(value, Time.frameCount);

				var t = value.GetComponent<HandInteractor>();

				if (t != null)
				{
					if (m_ignoreJoint && m_ignoreType)
					{
						Log.w("HandInteractable", "touched", true);
						isTouched = true;
                        if (TouchBeginEvent != null)
                        {
                            TouchBeginEvent.Invoke();
                        }
                    }
					else if (!m_ignoreJoint && !m_ignoreType)
					{
						if (t.HandType == m_type && t.Joint == m_Joint)
						{
							Log.w("HandInteractable", "touched, type=" + t.HandType + " , joint=" + t.Joint, true);
							isTouched = true;
                            if (TouchBeginEvent != null)
                            {
                                TouchBeginEvent.Invoke();
                            }
                        }
					} else 
					{
						if (!m_ignoreJoint)
						{
							if (t.Joint == m_Joint)
							{
								Log.w("HandInteractable", "touched, Joint=" + t.Joint, true);
								isTouched = true;
                                if (TouchBeginEvent != null)
                                {
                                    TouchBeginEvent.Invoke();
                                }
                            }
						}
						else
						{
							if (t.HandType == m_type)
							{
								Log.w("HandInteractable", "touched, type=" + t.HandType, true);
								isTouched = true;
                                if (TouchBeginEvent != null)
                                {
                                    TouchBeginEvent.Invoke();
                                }
                            }
						}
					}
				}
				else
				{
					//Log.w("HandInteractable", "OnTriggerEnter() HandInteractor not found", true);
				}

				//value.NotifyCollisionEnter(this);
			}
		}

		void OnTriggerExit(Collider other)
		{
			//Log.w("HandInteractable", "OnTriggerExit() " + other.gameObject.name, true);
			if (other.attachedRigidbody != null && InteractionHub.GetInteractor(other.attachedRigidbody, out BaseInteractor value))
			{
				//if (!s_InteractingActors.ContainsKey(value))
				//	s_InteractingActors.Add(value, Time.frameCount);

				var t = value.GetComponent<HandInteractor>();

				if (t != null)
				{
					if (m_ignoreJoint && m_ignoreType)
					{
						Log.w("HandInteractable", "Exit", true);
						isTouched = false;
                        if (TouchEndEvent != null)
                        {
                            TouchEndEvent.Invoke();
                        }
                    }
					else if (!m_ignoreJoint && !m_ignoreType)
					{
						if (t.HandType == m_type && t.Joint == m_Joint)
						{
							Log.w("HandInteractable", "Exit, type=" + t.HandType + " , joint=" + t.Joint, true);
							isTouched = false;
                            if (TouchEndEvent != null)
                            {
                                TouchEndEvent.Invoke();
                            }
                        }
					}
					else
					{
						if (!m_ignoreJoint)
						{
							if (t.Joint == m_Joint)
							{
								Log.w("HandInteractable", "Exit, Joint=" + t.Joint, true);
								isTouched = false;
                                if (TouchEndEvent != null)
                                {
                                    TouchEndEvent.Invoke();
                                }
                            }
						}
						else
						{
							if (t.HandType == m_type)
							{
								Log.w("HandInteractable", "Exit, type=" + t.HandType, true);
								isTouched = false;
                                if (TouchEndEvent != null)
                                {
                                    TouchEndEvent.Invoke();
                                }
                            }
						}
					}
				}
				else
				{
					//Log.w("HandInteractable", "OnTriggerEnter() HandInteractor not found", true);
				}

				//value.NotifyCollisionEnter(this);
			}
		}
	}
}
