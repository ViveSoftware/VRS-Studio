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
	public class DebugRotation : MonoBehaviour
	{
		public Vector4 Rot;
		public void Rotate()
		{
			transform.rotation = new Quaternion(Rot.x, Rot.y, Rot.z, Rot.w);
			Debug.Log("Wave.Essence.BodyTracking.Demo.DebugRotation " + gameObject.name
				+ " rotation ("
				+ transform.rotation.eulerAngles.x.ToString() + ", "
				+ transform.rotation.eulerAngles.y.ToString() + ", "
				+ transform.rotation.eulerAngles.z.ToString() + ")");
		}
	}
}
