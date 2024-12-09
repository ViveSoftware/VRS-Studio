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
namespace Wave.Essence.BodyTracking.Demo
{
	public class HumanoidTrackingTeleport : MonoBehaviour
	{
		public HumanoidTracking humanoidTracking = null;
		public Transform offsetOrigin = null;
		public Transform offset1 = null;
		public Transform offset2 = null;
		public Transform offset3 = null;

		public void TeleportOrigin()
		{
			if (humanoidTracking != null && offsetOrigin != null)
				humanoidTracking.AvatarOffset = offsetOrigin;
		}
		public void Teleport1()
		{
			if (humanoidTracking != null && offset1 != null)
				humanoidTracking.AvatarOffset = offset1;
		}
		public void Teleport2()
		{
			if (humanoidTracking != null && offset2 != null)
				humanoidTracking.AvatarOffset = offset2;
		}
		public void Teleport3()
		{
			if (humanoidTracking != null && offset3 != null)
				humanoidTracking.AvatarOffset = offset3;
		}
	}
}
