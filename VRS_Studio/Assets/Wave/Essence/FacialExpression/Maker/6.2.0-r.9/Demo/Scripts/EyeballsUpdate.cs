using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Wave.Essence.Eye;
using Wave.OpenXR;

namespace Wave.Essence.FacialExpression.Maker.Demo
{
	public class EyeballsUpdate : MonoBehaviour
	{
		public Transform eyeBallL, eyeBallR;
		public InputDevice eyeTrackingDev, hmd;
		public Quaternion defRotL, defRotR;

		private GameObject[] EyeAnchors;
		private static float[] eyeExps = new float[(int)InputDeviceEye.Expressions.MAX];

		void GetDevice()
		{
			List<InputDevice> devices = new List<InputDevice>();
			InputDevices.GetDevices(devices);

			defRotL = eyeBallL.localRotation;
			defRotR = eyeBallR.localRotation;

			foreach (var dev in devices)
			{
				if (dev != null && ((dev.characteristics & InputDeviceCharacteristics.EyeTracking) != 0) && dev.name == "Wave Eye Tracking")
					eyeTrackingDev = dev;

				if (dev != null && ((dev.characteristics & InputDeviceCharacteristics.HeadMounted) != 0))
					hmd = dev;
			}
			if (!eyeTrackingDev.isValid)
			{
				Debug.LogError("No EyeTrackingDevice");
			}
		}

        IEnumerator Start()
		{
			if (eyeBallL == null || eyeBallR == null) yield break;
			while (!eyeTrackingDev.isValid)
			{
				GetDevice();
				yield return new WaitForSeconds(0.1f);
			}
			Debug.LogError("No EyeTrackingDevice");

			CreateEyeAnchors();
			if (InputDeviceEye.HasEyeExpressionValue())// Eye expressions
				InputDeviceEye.GetEyeExpressionValues(out float[] exps);
		}

		void Update()
		{
			//Debug.Log("Update(6) 999: isValid " + eyeTrackingDev.isValid+", Available:"+ InputDeviceEye.IsEyeTrackingAvailable() +", tracked:"+ InputDeviceEye.IsEyeTrackingTracked());
			if (eyeTrackingDev == null || !eyeTrackingDev.isValid) { /*Debug.Log("Update(6) 999: eyeTrackingDev null ");*/ return; }
			if (eyeBallL == null || eyeBallR == null)
			{
				/*Debug.Log("Update(6) 999: eyeBallL or eyeBallR transform null ");*/ return;
			}
			Quaternion hmdRot;
			if (!eyeTrackingDev.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyeData)) { /*Debug.Log("Update(6) 999: eyeTrackingDev.TryGetFeatureValue false");*/ return; }
			if (!hmd.TryGetFeatureValue(CommonUsages.centerEyeRotation, out hmdRot))
				hmdRot = Quaternion.identity;

			eyeData.TryGetLeftEyeRotation(out Quaternion rotL);
			eyeData.TryGetRightEyeRotation(out Quaternion rotR);

			var toLocalRot = Quaternion.Inverse(hmdRot);
			float delTime = Time.deltaTime;
			eyeBallL.transform.localRotation = Quaternion.Slerp(eyeBallL.transform.localRotation, rotL * toLocalRot * defRotL, /*0.25f*/1.0f* delTime);
			eyeBallR.transform.localRotation = Quaternion.Slerp(eyeBallR.transform.localRotation, rotR * toLocalRot * defRotR, /*0.25f*/1.0f* delTime);

			// for test and debug
			Vector3 GazeDirectionCombinedLocal = Vector3.zero;
			EyeManager.Instance.GetCombindedEyeDirectionNormalized(out GazeDirectionCombinedLocal);
			//UpdateGazeRay(GazeDirectionCombinedLocal);
			//UpdateEyePosition();
		}
		private void CreateEyeAnchors()
		{
			//EyeAnchors = new GameObject[2];
			//for (int i = 0; i < 2; ++i)
			//{
			//	EyeAnchors[i] = new GameObject();
			//	EyeAnchors[i].name = "EyeAnchor_" + i;
			//	EyeAnchors[i].transform.SetParent(gameObject.transform);
			//	EyeAnchors[i].transform.localPosition = EyesModels[i].localPosition;
			//	EyeAnchors[i].transform.localRotation = EyesModels[i].localRotation;
			//	EyeAnchors[i].transform.localScale = EyesModels[i].localScale;
			//}
			EyeAnchors[0].transform.SetParent(gameObject.transform);
			EyeAnchors[0].transform.localPosition = eyeBallL.localPosition;
			EyeAnchors[0].transform.localRotation = eyeBallL.localRotation;
			EyeAnchors[0].transform.localScale = eyeBallL.localScale;
			EyeAnchors[1].transform.SetParent(gameObject.transform);
			EyeAnchors[1].transform.localPosition = eyeBallR.localPosition;
			EyeAnchors[1].transform.localRotation = eyeBallR.localRotation;
			EyeAnchors[1].transform.localScale = eyeBallR.localScale;

		}
		public void UpdateGazeRay(Vector3 gazeDirectionCombinedLocal)
		{
			//for (int i = 0; i < EyesModels.Length; ++i)
			//{
			//	Vector3 target = EyeAnchors[i].transform.TransformPoint(gazeDirectionCombinedLocal);
			//	EyesModels[i].LookAt(target);
			//}
			Vector3 target = eyeBallL.transform.TransformPoint(gazeDirectionCombinedLocal);
			eyeBallL.LookAt(target);
			target = eyeBallR.transform.TransformPoint(gazeDirectionCombinedLocal);
			eyeBallR.LookAt(target);
		}
		void UpdateEyePosition()
		{

			Vector3 /*GazeOriginCombinedLocal, */GazeDirectionCombinedLocal = Vector3.zero;
			if (eyeExps[(int)InputDeviceEye.Expressions.LEFT_IN] > eyeExps[(int)InputDeviceEye.Expressions.LEFT_OUT])
			{
				GazeDirectionCombinedLocal.x = eyeExps[(int)InputDeviceEye.Expressions.LEFT_IN];
			}
			else
			{
				GazeDirectionCombinedLocal.x = -eyeExps[(int)InputDeviceEye.Expressions.LEFT_OUT];
			}
			if (eyeExps[(int)InputDeviceEye.Expressions.LEFT_UP] > eyeExps[(int)InputDeviceEye.Expressions.LEFT_DOWN])
			{
				GazeDirectionCombinedLocal.y = eyeExps[(int)InputDeviceEye.Expressions.LEFT_UP];
			}
			else
			{
				GazeDirectionCombinedLocal.y = -eyeExps[(int)InputDeviceEye.Expressions.LEFT_DOWN];
			}
			GazeDirectionCombinedLocal.z = (float)1.0;

			Vector3 target = EyeAnchors[0].transform.TransformPoint(GazeDirectionCombinedLocal);
			eyeBallL.LookAt(target);
			Debug.Log("UpdateEyePosition(6) EyeSample0: " + target.x + ", " + target.y + "," + target.z);

			if (eyeExps[(int)InputDeviceEye.Expressions.RIGHT_IN] > eyeExps[(int)InputDeviceEye.Expressions.RIGHT_OUT])
			{
				GazeDirectionCombinedLocal.x = -eyeExps[(int)InputDeviceEye.Expressions.RIGHT_IN];
			}
			else
			{
				GazeDirectionCombinedLocal.x = eyeExps[(int)InputDeviceEye.Expressions.RIGHT_OUT];
			}
			if (eyeExps[(int)InputDeviceEye.Expressions.RIGHT_UP] > eyeExps[(int)InputDeviceEye.Expressions.RIGHT_DOWN])
			{
				GazeDirectionCombinedLocal.y = eyeExps[(int)InputDeviceEye.Expressions.RIGHT_UP];
			}
			else
			{
				GazeDirectionCombinedLocal.y = -eyeExps[(int)InputDeviceEye.Expressions.RIGHT_DOWN];
			}
			GazeDirectionCombinedLocal.z = (float)1.0;

			target = EyeAnchors[1].transform.TransformPoint(GazeDirectionCombinedLocal);
			eyeBallR.LookAt(target);
			Debug.Log("UpdateEyePosition(6) EyeSample1: " + target.x + ", " + target.y + "," + target.z);
		}



	}
}
