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

#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.BodyTracking.Editor
{
	[CustomEditor(typeof(HumanoidTracking))]
	public class HumanoidTrackingEditor : UnityEditor.Editor
	{
		SerializedProperty m_Tracking, m_AvatarHeight, m_AvatarOffset, m_AvatarScale, m_JointCoordinate;
		SerializedProperty m_Head,
			m_LeftWrist, m_RightWrist,
			m_LeftHandheld, m_RightHandheld,
			m_LeftHand, m_RightHand,
			m_Hips,
			m_LeftLowerLeg, m_RightLowerLeg,
			m_LeftFoot, m_RightFoot,
			m_LeftToes, m_RightToes;

		private void OnEnable()
		{
			m_Tracking = serializedObject.FindProperty("m_Tracking");
			m_AvatarHeight = serializedObject.FindProperty("m_AvatarHeight");
			m_AvatarOffset = serializedObject.FindProperty("m_AvatarOffset");
			m_AvatarScale = serializedObject.FindProperty("m_AvatarScale");
			m_JointCoordinate = serializedObject.FindProperty("m_JointCoordinate");

			m_Head = serializedObject.FindProperty("m_Head");
			m_LeftWrist = serializedObject.FindProperty("m_LeftWrist");
			m_RightWrist = serializedObject.FindProperty("m_RightWrist");
			m_LeftHandheld = serializedObject.FindProperty("m_LeftHandheld");
			m_RightHandheld = serializedObject.FindProperty("m_RightHandheld");
			m_LeftHand = serializedObject.FindProperty("m_LeftHand");
			m_RightHand = serializedObject.FindProperty("m_RightHand");
			m_Hips = serializedObject.FindProperty("m_Hips");
			m_LeftLowerLeg = serializedObject.FindProperty("m_LeftLowerLeg");
			m_RightLowerLeg = serializedObject.FindProperty("m_RightLowerLeg");
			m_LeftFoot = serializedObject.FindProperty("m_LeftFoot");
			m_RightFoot = serializedObject.FindProperty("m_RightFoot");
			m_LeftToes = serializedObject.FindProperty("m_LeftToes");
			m_RightToes = serializedObject.FindProperty("m_RightToes");
		}

		static bool customizeExtrinsicOptions = false;
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			HumanoidTracking myScript = target as HumanoidTracking;

			EditorGUILayout.HelpBox(
				"Calibration methods: Arm, Upper Body, Full Body or Upper Bodg + Leg.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_Tracking);

			EditorGUILayout.HelpBox(
				"Moves the avatar by applying the transform offset.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_AvatarOffset);

			EditorGUILayout.HelpBox(
				"Resize the avatar.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_AvatarScale);

			/*EditorGUILayout.HelpBox(
				"Defines the coordinate of an Avatar's each joint, keep null for Unity identity (x-right, y-up and z-forward).",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_JointCoordinate);*/

			myScript.CustomSettings = EditorGUILayout.Toggle("Custom Settings", myScript.CustomSettings);
			if (myScript.CustomSettings)
			{
				EditorGUILayout.HelpBox(
					"Sets up the avatar's height instead of using Humanoid (head's height - toes's height).",
					MessageType.Info);
				EditorGUILayout.PropertyField(m_AvatarHeight);
			}

			GUIStyle foldoutStyle = EditorStyles.foldout;
			foldoutStyle.fontSize = 12;
			foldoutStyle.fontStyle = FontStyle.Bold;
			customizeExtrinsicOptions = EditorGUILayout.Foldout(customizeExtrinsicOptions, "Tracked Device Extrinsics");
			foldoutStyle.fontStyle = FontStyle.Normal;
			if (customizeExtrinsicOptions)
			{
				EditorGUILayout.HelpBox(
					"Sets up the tracked devices' extrinsics instead of using default extrinsics.",
					MessageType.Info);
				myScript.CustomizeExtrinsics = EditorGUILayout.Toggle("Customize Extrinsics", myScript.CustomizeExtrinsics);
				if (myScript.CustomizeExtrinsics)
				{
					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_Head);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftWrist);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightWrist);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftHandheld);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightHandheld);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftHand);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightHand);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_Hips);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftLowerLeg);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightLowerLeg);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftFoot);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightFoot);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_LeftToes);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(); GUILayout.Space(10);
					EditorGUILayout.PropertyField(m_RightToes);
					GUILayout.EndHorizontal();
				}
			}

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
				EditorUtility.SetDirty((HumanoidTracking)target);
		}
	}
}
#endif
