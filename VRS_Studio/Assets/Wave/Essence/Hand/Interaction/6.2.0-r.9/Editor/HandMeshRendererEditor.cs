// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace Wave.Essence.Hand.Interaction
{
	[CustomEditor(typeof(HandMeshRenderer))]
	class HandMeshRendererEditor : UnityEditor.Editor
	{
		private HandMeshRenderer handMeshRenderer = null;
		private SerializedProperty m_Handedness, m_EnableCollider, m_UseRuntimeModel, m_UseScale, customizedBonePoses;

		private bool showBones = false;
		public static readonly GUIContent findJoints = EditorGUIUtility.TrTextContent("Find Joints");
		public static readonly GUIContent clearJoints = EditorGUIUtility.TrTextContent("All Clear");

		private void OnEnable()
		{
			handMeshRenderer = target as HandMeshRenderer;
			m_Handedness = serializedObject.FindProperty("m_Handedness");
			m_EnableCollider = serializedObject.FindProperty("m_EnableCollider");
			m_UseRuntimeModel = serializedObject.FindProperty("m_UseRuntimeModel");
			m_UseScale = serializedObject.FindProperty("m_UseScale");
			customizedBonePoses = serializedObject.FindProperty("customizedBonePoses");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Please check if your model is used to bind left hand poses", MessageType.None);
			EditorGUILayout.PropertyField(m_Handedness, new GUIContent("Handedness"));

			EditorGUILayout.HelpBox("Please check if you want the hand model with collision enabled.", MessageType.None);
			EditorGUILayout.PropertyField(m_EnableCollider, new GUIContent("Enable Collider"));

			EditorGUILayout.HelpBox("Use skeleton, mesh and pose from runtime", MessageType.None);
			EditorGUILayout.PropertyField(m_UseRuntimeModel, new GUIContent("Use Runtime Model"));

			if (!m_UseRuntimeModel.boolValue)
			{
				EditorGUILayout.HelpBox("Use scale from runtime.", MessageType.None);
				EditorGUILayout.PropertyField(m_UseScale, new GUIContent("Use Scale"));
			}

			showBones = EditorGUILayout.Foldout(showBones, "Hand Bones Reference");
			if (showBones)
			{
				EditorGUILayout.HelpBox("Please change rotation to make sure your model should palm faces forward and fingers points up in global axis.", MessageType.Info);

				using (new EditorGUILayout.HorizontalScope())
				{
					using (new EditorGUI.DisabledScope())
					{
						if (GUILayout.Button(findJoints))
						{
							handMeshRenderer.AutoDetect();
						}
					}

					using (new EditorGUI.DisabledScope())
					{
						if (GUILayout.Button(clearJoints))
						{
							handMeshRenderer.ClearDetect();
						}
					}
				}

				bool isDetected = false;
				for (int i = 0; i < customizedBonePoses.arraySize; i++)
				{
					SerializedProperty bone = customizedBonePoses.GetArrayElementAtIndex(i);
					Transform boneTransform = (Transform)bone.objectReferenceValue;
					if (boneTransform != null)
					{
						isDetected = true;
						break;
					}
				}
				if (isDetected)
				{
					for (int i = 0; i < customizedBonePoses.arraySize; i++)
					{
						SerializedProperty bone = customizedBonePoses.GetArrayElementAtIndex(i);
						EditorGUILayout.PropertyField(bone, new GUIContent(handMeshRenderer.boneMap[i].DisplayName));
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
