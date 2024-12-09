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
	[CustomEditor(typeof(BodyRoleData))]
	public class BodyRoleDataEditor : UnityEditor.Editor
	{
		SerializedProperty m_TrackerPose, m_TrackerIndexInputs, m_TrackerTypeInputs;
		private void OnEnable()
		{
			m_TrackerPose = serializedObject.FindProperty("m_TrackerPose");
			m_TrackerIndexInputs = serializedObject.FindProperty("m_TrackerIndexInputs");
			m_TrackerTypeInputs = serializedObject.FindProperty("m_TrackerTypeInputs");
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			BodyRoleData myScript = target as BodyRoleData;

			EditorGUILayout.HelpBox(
				"Selects the pose source of Tracker, either from a specific type or a RolePoseProvider.",
				MessageType.Info);
			EditorGUILayout.PropertyField(m_TrackerPose);
			if (myScript.TrackerPose == BodyRoleData.TrackerBase.IndexBase)
			{
				EditorGUILayout.PropertyField(m_TrackerIndexInputs);
			}
			else
			{
				EditorGUILayout.PropertyField(m_TrackerTypeInputs);
			}

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
				EditorUtility.SetDirty((BodyRoleData)target);
		}

#if !TMPExist
	[InitializeOnLoadMethod]
	static void CheckTextMeshProInstallation()
	{
		EditorUtility.DisplayDialog("TextMeshPro Not Found",
			"The Body Tracking sample needs TextMeshPro.",
			"OK");
	}
#endif
	}
}
#endif
