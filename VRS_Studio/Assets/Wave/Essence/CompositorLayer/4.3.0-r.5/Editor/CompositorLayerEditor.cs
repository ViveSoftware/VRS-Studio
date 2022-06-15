// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Wave.Essence.CompositorLayer.Editor
{
#region Compositor Layer Editor

	using UnityEditor;
	using UnityEditor.SceneManagement;
	using Wave.Essence.CompositorLayer;
	using Wave.XR.Settings;

	[CustomEditor(typeof(CompositorLayer))]
	public class CompositorLayerEditor : Editor
	{
		static string PropertyName_LayerType = "layerType";
		static GUIContent Label_LayerType = new GUIContent("Layer Type");
		SerializedProperty Property_LayerType;

		static string PropertyName_CompositionDepth = "compositionDepth";
		static GUIContent Label_CompositionDepth = new GUIContent("Composition Depth");
		SerializedProperty Property_CompositionDepth;

		static string PropertyName_LayerShape = "layerShape";
		static GUIContent Label_LayerShape = new GUIContent("Layer Shape");
		SerializedProperty Property_LayerShape;

		static string PropertyName_LockMode = "lockMode";
		static GUIContent Label_LockMode = new GUIContent("Locked Parameter");
		SerializedProperty Property_LockMode;

		static string PropertyName_QuadWidth = "quadWidth";
		static GUIContent Label_QuadWidth = new GUIContent("Width");
		SerializedProperty Property_QuadWidth;

		static string PropertyName_QuadHeight = "quadHeight";
		static GUIContent Label_QuadHeight = new GUIContent("Height");
		SerializedProperty Property_QuadHeight;

		static string PropertyName_CylinderHeight = "cylinderHeight";
		static GUIContent Label_CylinderHeight = new GUIContent("Height");
		SerializedProperty Property_CylinderHeight;

		static string PropertyName_CylinderArcLength = "cylinderArcLength";
		static GUIContent Label_CylinderArcLength = new GUIContent("Arc Length");
		SerializedProperty Property_CylinderArcLength;

		static string PropertyName_CylinderRadius = "cylinderRadius";
		static GUIContent Label_CylinderRadius = new GUIContent("Radius");
		SerializedProperty Property_CylinderRadius;

		static string PropertyName_AngleOfArc = "angleOfArc";
		static GUIContent Label_AngleOfArc = new GUIContent("Arc Angle");
		SerializedProperty Property_AngleOfArc;

		static string PropertyName_IsDynamicLayer = "isDynamicLayer";
		static GUIContent Label_IsDynamicLayer = new GUIContent("Dynamic Layer");
		SerializedProperty Property_IsDynamicLayer;

		static string PropertyName_EnableAutoFallback = "enableAutoFallback";
		static GUIContent Label_EnableAutoFallback = new GUIContent("Auto Fallback (Beta)");
		SerializedProperty Property_EnableAutoFallback;

		static string PropertyName_RenderPriority = "renderPriority";
		static GUIContent Label_RenderPriority = new GUIContent("Render Priority");
		SerializedProperty Property_RenderPriority;

		private bool showLayerParams = false;

		public override void OnInspectorGUI()
		{
			if (Property_LayerType == null) Property_LayerType = serializedObject.FindProperty(PropertyName_LayerType);
			if (Property_CompositionDepth == null) Property_CompositionDepth = serializedObject.FindProperty(PropertyName_CompositionDepth);
			if (Property_LayerShape == null) Property_LayerShape = serializedObject.FindProperty(PropertyName_LayerShape);
			if (Property_LockMode == null) Property_LockMode = serializedObject.FindProperty(PropertyName_LockMode);
			if (Property_QuadWidth == null) Property_QuadWidth = serializedObject.FindProperty(PropertyName_QuadWidth);
			if (Property_QuadHeight == null) Property_QuadHeight = serializedObject.FindProperty(PropertyName_QuadHeight);
			if (Property_CylinderHeight == null) Property_CylinderHeight = serializedObject.FindProperty(PropertyName_CylinderHeight);
			if (Property_CylinderArcLength == null) Property_CylinderArcLength = serializedObject.FindProperty(PropertyName_CylinderArcLength);
			if (Property_CylinderRadius == null) Property_CylinderRadius = serializedObject.FindProperty(PropertyName_CylinderRadius);
			if (Property_AngleOfArc == null) Property_AngleOfArc = serializedObject.FindProperty(PropertyName_AngleOfArc);
			if (Property_IsDynamicLayer == null) Property_IsDynamicLayer = serializedObject.FindProperty(PropertyName_IsDynamicLayer);
			if (Property_EnableAutoFallback == null) Property_EnableAutoFallback = serializedObject.FindProperty(PropertyName_EnableAutoFallback);
			if (Property_RenderPriority == null) Property_RenderPriority = serializedObject.FindProperty(PropertyName_RenderPriority);

			CompositorLayer targetCompositorLayer = target as CompositorLayer;

			EditorGUILayout.HelpBox("Compositor Layer is a feature that leverages the Wave Multi-Layer Rendering Architecture to display textures on layers other than the eye buffer.", MessageType.None);

			EditorGUILayout.PropertyField(Property_LayerType, new GUIContent(Label_LayerType));
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(Property_CompositionDepth, new GUIContent(Label_CompositionDepth));
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(Property_LayerShape, new GUIContent(Label_LayerShape));
			serializedObject.ApplyModifiedProperties();
			if (Property_LayerShape.intValue == (int)CompositorLayer.LayerShape.Cylinder)
			{
				if (targetCompositorLayer.isPreviewingQuad)
				{
					targetCompositorLayer.isPreviewingQuad = false;
					if (targetCompositorLayer.generatedPreview != null)
					{
						DestroyImmediate(targetCompositorLayer.generatedPreview);
					}
				}

				Transform generatedQuadTransform = targetCompositorLayer.transform.Find(CompositorLayer.QuadUnderlayMeshName);
				if (generatedQuadTransform != null)
				{
					DestroyImmediate(generatedQuadTransform.gameObject);
				}
				
				showLayerParams = EditorGUILayout.Foldout(showLayerParams, "Show Cylinder Parameters");
				if (showLayerParams)
				{
					float radiusLowerBound = Mathf.Max(0.001f, CompositorLayer.CylinderParameterHelper.ArcLengthAndDegThetaToRadius(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.angleOfArcUpperLimit));
					float radiusUpperBound = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegThetaToRadius(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.angleOfArcLowerLimit);
					EditorGUILayout.HelpBox("Changing the Arc Length will affect the upper and lower bounds of the radius.\nUpper Bound of Radius: " + radiusUpperBound + "\nLower Bound of Radius: " + radiusLowerBound, MessageType.Info);

					EditorGUILayout.PropertyField(Property_CylinderRadius, new GUIContent(Label_CylinderRadius));
					EditorGUILayout.PropertyField(Property_LockMode, new GUIContent(Label_LockMode));
					serializedObject.ApplyModifiedProperties();
					
					EditorGUILayout.HelpBox("Arc Length and Arc Angle are correlated, adjusting one of them changes the other as well. The Radius will not be changed when adjusting these two values.", MessageType.Info);
					EditorGUILayout.Slider(Property_AngleOfArc, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, new GUIContent(Label_AngleOfArc));
					EditorGUILayout.PropertyField(Property_CylinderArcLength, new GUIContent(Label_CylinderArcLength));
					EditorGUILayout.PropertyField(Property_CylinderHeight, new GUIContent(Label_CylinderHeight));
				}
				
				//if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength)
				//{
				//	GUI.enabled = false;
				//	EditorGUILayout.PropertyField(Property_CylinderArcLength, new GUIContent(Label_CylinderArcLength));
				//	GUI.enabled = true;
				//	EditorGUILayout.Slider(Property_AngleOfArc, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, new GUIContent(Label_AngleOfArc));
				//}
				//else if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle)
				//{
				//	EditorGUILayout.PropertyField(Property_CylinderArcLength, new GUIContent(Label_CylinderArcLength));
				//	GUI.enabled = false;
				//	EditorGUILayout.Slider(Property_AngleOfArc, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, new GUIContent(Label_AngleOfArc));
				//	GUI.enabled = true;
				//}

				serializedObject.ApplyModifiedProperties();

				CompositorLayer.CylinderLayerParamAdjustmentMode currentAdjustmentMode = targetCompositorLayer.CurrentAdjustmentMode();
				if (currentAdjustmentMode == CompositorLayer.CylinderLayerParamAdjustmentMode.Radius)
				{
					CompositorLayer.CylinderParameterHelper.RadiusValidityCheck(targetCompositorLayer.cylinderArcLength, ref targetCompositorLayer.cylinderRadius, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, targetCompositorLayer.lockMode);
					if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength)
					{
						targetCompositorLayer.angleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegTheta(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius);
						if (!CompositorLayer.CylinderParameterHelper.ArcLengthValidityCheck(ref targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit))
						{
							targetCompositorLayer.angleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegTheta(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius);
						}
					}
					else if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle)
					{
						targetCompositorLayer.cylinderArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegThetaToArcLength(targetCompositorLayer.angleOfArc, targetCompositorLayer.cylinderRadius);
					}
					serializedObject.ApplyModifiedProperties();
				}
				else if (currentAdjustmentMode == CompositorLayer.CylinderLayerParamAdjustmentMode.ArcLength)
				{
					targetCompositorLayer.angleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegTheta(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius);
					if (!CompositorLayer.CylinderParameterHelper.ArcLengthValidityCheck(ref targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit))
					{
						targetCompositorLayer.angleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegTheta(targetCompositorLayer.cylinderArcLength, targetCompositorLayer.cylinderRadius);
					}
					serializedObject.ApplyModifiedProperties();
				}
				else if (currentAdjustmentMode == CompositorLayer.CylinderLayerParamAdjustmentMode.ArcAngle)
				{
					targetCompositorLayer.cylinderArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegThetaToArcLength(targetCompositorLayer.angleOfArc, targetCompositorLayer.cylinderRadius);
					serializedObject.ApplyModifiedProperties();
				}

				EditorGUILayout.HelpBox("Current Layer Aspect Ratio (Arc Length : Height) = " + targetCompositorLayer.cylinderArcLength + " : " + targetCompositorLayer.cylinderHeight, MessageType.Info);

				Vector3 compositorLayerScale = targetCompositorLayer.gameObject.transform.localScale;
				bool CylinderParamsChanged = targetCompositorLayer.ParamsChanged();

				if (targetCompositorLayer.isPreviewingCylinder)
				{
					Transform generatedPreviewTransform = targetCompositorLayer.transform.Find(CompositorLayer.CylinderPreviewName);
					
					if (generatedPreviewTransform == null)
					{
						targetCompositorLayer.isPreviewingCylinder = false;
					}
					else
					{
						targetCompositorLayer.generatedPreview = generatedPreviewTransform.gameObject;
					}

					if (CylinderParamsChanged)
					{
						//Generate vertices
						Vector3[] cylinderVertices = CompositorLayer.MeshGenerationHelper.GenerateCylinderVertex(targetCompositorLayer, targetCompositorLayer.cylinderRadius, targetCompositorLayer.cylinderHeight);

						MeshFilter cylinderMeshFilter = targetCompositorLayer.generatedPreview.GetComponent<MeshFilter>();
						//Generate Mesh
						cylinderMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateCylinderMesh(targetCompositorLayer, cylinderVertices);

						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = new Vector3(1, 1, 1);
					}

					if (targetCompositorLayer.generatedPreview.GetComponent<MeshRenderer>().sharedMaterial.mainTexture != targetCompositorLayer.textures[0])
					{
						targetCompositorLayer.generatedPreview.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = targetCompositorLayer.textures[0];
					}

					if (GUILayout.Button("Hide Cylinder Preview"))
					{
						targetCompositorLayer.isPreviewingCylinder = false;
						if (targetCompositorLayer.generatedPreview != null)
						{
							DestroyImmediate(targetCompositorLayer.generatedPreview);
						}
					}
				}
				else
				{
					if (GUILayout.Button("Show Cylinder Preview"))
					{
						targetCompositorLayer.isPreviewingCylinder = true;
						Vector3[] cylinderVertices = CompositorLayer.MeshGenerationHelper.GenerateCylinderVertex(targetCompositorLayer, targetCompositorLayer.cylinderRadius, targetCompositorLayer.cylinderHeight);
						//Add components to Game Object
						targetCompositorLayer.generatedPreview = new GameObject();
						targetCompositorLayer.generatedPreview.hideFlags = HideFlags.HideAndDontSave;
						targetCompositorLayer.generatedPreview.name = CompositorLayer.CylinderPreviewName;
						targetCompositorLayer.generatedPreview.transform.SetParent(targetCompositorLayer.gameObject.transform);
						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = new Vector3(1, 1, 1);

						MeshRenderer cylinderMeshRenderer = targetCompositorLayer.generatedPreview.AddComponent<MeshRenderer>();
						MeshFilter cylinderMeshFilter = targetCompositorLayer.generatedPreview.AddComponent<MeshFilter>();
						cylinderMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));

						if (targetCompositorLayer.textures[0] != null)
						{
							cylinderMeshRenderer.sharedMaterial.mainTexture = targetCompositorLayer.textures[0];
						}

						//Generate Mesh
						cylinderMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateCylinderMesh(targetCompositorLayer, cylinderVertices);
					}
				}

				if (targetCompositorLayer.layerType == CompositorLayer.LayerType.Underlay)
				{
					Transform generatedUnderlayMeshTransform = targetCompositorLayer.transform.Find(CompositorLayer.CylinderUnderlayMeshName);

					if (generatedUnderlayMeshTransform != null)
					{
						targetCompositorLayer.generatedUnderlayMesh = generatedUnderlayMeshTransform.gameObject;
						//if (GUILayout.Button("Update Underlay Cylinder Mesh"))
						if (CylinderParamsChanged || !targetCompositorLayer.UnderlayMeshIsValid())
						{
							if (PrefabUtility.IsPartOfPrefabInstance(targetCompositorLayer.generatedUnderlayMesh)) //If is part of prefab
							{
								Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(targetCompositorLayer.generatedUnderlayMesh);
								DestroyImmediate(prefabInstance);
							}
							targetCompositorLayer.UnderlayMeshGeneration();
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}

						if (GUILayout.Button("Remove Underlay Cylinder Mesh"))
						{
							if (PrefabUtility.IsPartOfPrefabInstance(targetCompositorLayer.generatedUnderlayMesh)) //If is part of prefab
							{
								Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(targetCompositorLayer.generatedUnderlayMesh);
								DestroyImmediate(prefabInstance);
							}
							DestroyImmediate(targetCompositorLayer.generatedUnderlayMesh);

							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}
					else
					{
						if (GUILayout.Button("Generate Underlay Cylinder Mesh"))
						{
							targetCompositorLayer.UnderlayMeshGeneration();

							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}
				}
				else
				{
					Transform generatedCylinderTransform = targetCompositorLayer.transform.Find(CompositorLayer.CylinderUnderlayMeshName);
					if (generatedCylinderTransform != null)
					{
						if (PrefabUtility.IsPartOfPrefabInstance(generatedCylinderTransform.gameObject)) //If is part of prefab
						{
							Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(generatedCylinderTransform.gameObject);
							DestroyImmediate(prefabInstance);
						}
						DestroyImmediate(generatedCylinderTransform.gameObject);
						
						EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					}
				}

				EditorGUILayout.Space(10);
				serializedObject.ApplyModifiedProperties();
			}
			else if (Property_LayerShape.intValue == (int)CompositorLayer.LayerShape.Quad)
			{
				if (targetCompositorLayer.isPreviewingCylinder)
				{
					targetCompositorLayer.isPreviewingCylinder = false;
					if (targetCompositorLayer.generatedPreview != null)
					{
						DestroyImmediate(targetCompositorLayer.generatedPreview);
					}
				}

				Transform generatedCylinderTransform = targetCompositorLayer.transform.Find(CompositorLayer.CylinderUnderlayMeshName);
				if (generatedCylinderTransform != null)
				{
					if (PrefabUtility.IsPartOfPrefabInstance(generatedCylinderTransform.gameObject)) //If is part of prefab
					{
						Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(generatedCylinderTransform.gameObject);
						DestroyImmediate(prefabInstance);
					}
					DestroyImmediate(generatedCylinderTransform.gameObject);

					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}

				showLayerParams = EditorGUILayout.Foldout(showLayerParams, "Show Quad Parameters");
				if (showLayerParams)
				{
					EditorGUILayout.PropertyField(Property_QuadWidth, new GUIContent(Label_QuadWidth));
					EditorGUILayout.PropertyField(Property_QuadHeight, new GUIContent(Label_QuadHeight));
				}
				EditorGUILayout.HelpBox("Current Layer Aspect Ratio (Width : Height) = " + targetCompositorLayer.quadWidth + " : " + targetCompositorLayer.quadHeight, MessageType.Info);

				Vector3 compositorLayerScale = targetCompositorLayer.gameObject.transform.localScale;
				bool QuadParamsChanged = targetCompositorLayer.ParamsChanged();
				if (targetCompositorLayer.isPreviewingQuad)
				{
					Transform generatedPreviewTransform = targetCompositorLayer.transform.Find(CompositorLayer.QuadPreviewName);

					if (generatedPreviewTransform == null)
					{
						targetCompositorLayer.isPreviewingCylinder = false;
					}
					else
					{
						targetCompositorLayer.generatedPreview = generatedPreviewTransform.gameObject;
					}

					if (QuadParamsChanged)
					{
						//Generate vertices
						Vector3[] quadVertices = CompositorLayer.MeshGenerationHelper.GenerateQuadVertex(targetCompositorLayer, targetCompositorLayer.quadWidth, targetCompositorLayer.quadHeight);

						MeshFilter quadMeshFilter = targetCompositorLayer.generatedPreview.GetComponent<MeshFilter>();
						//Generate Mesh
						quadMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateQuadMesh(targetCompositorLayer, quadVertices);

						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = new Vector3(1, 1, 1);
					}

					if (targetCompositorLayer.generatedPreview.GetComponent<MeshRenderer>().sharedMaterial.mainTexture != targetCompositorLayer.textures[0])
					{
						targetCompositorLayer.generatedPreview.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = targetCompositorLayer.textures[0];
					}

					if (GUILayout.Button("Hide Quad Preview"))
					{
						targetCompositorLayer.isPreviewingQuad = false;
						if (targetCompositorLayer.generatedPreview != null)
						{
							DestroyImmediate(targetCompositorLayer.generatedPreview);
						}
					}
				}
				else
				{
					if (GUILayout.Button("Show Quad Preview"))
					{
						targetCompositorLayer.isPreviewingQuad = true;
						//Generate vertices
						Vector3[] quadVertices = CompositorLayer.MeshGenerationHelper.GenerateQuadVertex(targetCompositorLayer, targetCompositorLayer.quadWidth, targetCompositorLayer.quadHeight);

						//Add components to Game Object
						targetCompositorLayer.generatedPreview = new GameObject();
						targetCompositorLayer.generatedPreview.hideFlags = HideFlags.HideAndDontSave;
						targetCompositorLayer.generatedPreview.name = CompositorLayer.QuadPreviewName;
						targetCompositorLayer.generatedPreview.transform.SetParent(targetCompositorLayer.gameObject.transform);
						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = new Vector3(1, 1, 1);

						MeshRenderer quadMeshRenderer = targetCompositorLayer.generatedPreview.AddComponent<MeshRenderer>();
						MeshFilter quadMeshFilter = targetCompositorLayer.generatedPreview.AddComponent<MeshFilter>();
						quadMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));

						if (targetCompositorLayer.textures[0] != null)
						{
							quadMeshRenderer.sharedMaterial.mainTexture = targetCompositorLayer.textures[0];
						}
						//Generate Mesh
						quadMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateQuadMesh(targetCompositorLayer, quadVertices);
					}
				}

				if (targetCompositorLayer.layerType == CompositorLayer.LayerType.Underlay)
				{
					Transform generatedUnderlayMeshTransform = targetCompositorLayer.transform.Find(CompositorLayer.QuadUnderlayMeshName);

					if (generatedUnderlayMeshTransform != null)
					{
						targetCompositorLayer.generatedUnderlayMesh = generatedUnderlayMeshTransform.gameObject;
						if (QuadParamsChanged || !targetCompositorLayer.UnderlayMeshIsValid())
						{
							if (PrefabUtility.IsPartOfPrefabInstance(targetCompositorLayer.generatedUnderlayMesh)) //If is part of prefab
							{
								Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(targetCompositorLayer.generatedUnderlayMesh);
								DestroyImmediate(prefabInstance);
							}
							targetCompositorLayer.UnderlayMeshGeneration();

							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}

						if (GUILayout.Button("Remove Underlay Quad Mesh"))
						{
							if (PrefabUtility.IsPartOfPrefabInstance(targetCompositorLayer.generatedUnderlayMesh)) //If is part of prefab
							{
								Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(targetCompositorLayer.generatedUnderlayMesh);
								DestroyImmediate(prefabInstance);
							}
							DestroyImmediate(targetCompositorLayer.generatedUnderlayMesh);

							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}
					else
					{
						if (GUILayout.Button("Generate Underlay Quad Mesh"))
						{
							targetCompositorLayer.UnderlayMeshGeneration();

							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}
				}
				else
				{
					Transform generatedQuadTransform = targetCompositorLayer.transform.Find(CompositorLayer.QuadUnderlayMeshName);
					if (generatedQuadTransform != null)
					{
						DestroyImmediate(generatedQuadTransform.gameObject);
						EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					}
				}

				EditorGUILayout.Space(10);
				serializedObject.ApplyModifiedProperties();
			}

			//Rect UI For textures
			Rect labelRect = EditorGUILayout.GetControlRect();
			EditorGUI.LabelField(new Rect(labelRect.x, labelRect.y, labelRect.width / 2, labelRect.height), new GUIContent("Left Texture", "Texture used for the left eye"));
			EditorGUI.LabelField(new Rect(labelRect.x + labelRect.width / 2, labelRect.y, labelRect.width / 2, labelRect.height), new GUIContent("Right Texture", "Texture used for the right eye"));

			Rect textureRect = EditorGUILayout.GetControlRect(GUILayout.Height(64));

			if (targetCompositorLayer.textures == null)
			{
				targetCompositorLayer.textures = new Texture[2];
			}

			if (targetCompositorLayer.textures.Length < 2)
			{
				Texture[] tmp = new Texture[2];
				for (int i = 0; i < targetCompositorLayer.textures.Length; i++)
				{
					tmp[i] = targetCompositorLayer.textures[i];
				}
				targetCompositorLayer.textures = tmp;
			}

			targetCompositorLayer.textures[0] = (Texture)EditorGUI.ObjectField(new Rect(textureRect.x, textureRect.y, 64, textureRect.height), targetCompositorLayer.textures[0], typeof(Texture), true);
			Texture right = (Texture)EditorGUI.ObjectField(new Rect(textureRect.x + textureRect.width / 2, textureRect.y, 64, textureRect.height), targetCompositorLayer.textures[1] != null ? targetCompositorLayer.textures[1] : targetCompositorLayer.textures[0], typeof(Texture), true);
			if (right == targetCompositorLayer.textures[0])
			{
				targetCompositorLayer.textures[1] = null;
				//myScript.textures[1] = right;
			}
			else
			{
				targetCompositorLayer.textures[1] = right;
			}

			EditorGUILayout.PropertyField(Property_IsDynamicLayer, Label_IsDynamicLayer);
			serializedObject.ApplyModifiedProperties();

			WaveXRSettings settingsInstance = WaveXRSettings.GetInstance();
			if (settingsInstance != null && settingsInstance.enableAutoFallbackForMultiLayerProperty)
			{
				EditorGUILayout.PropertyField(Property_RenderPriority, new GUIContent(Label_RenderPriority));
				serializedObject.ApplyModifiedProperties();

				if (!targetCompositorLayer.FallbackMeshIsValid() || targetCompositorLayer.ParamsChanged())
				{
					foreach (Transform child in targetCompositorLayer.transform)
					{
						if (child.gameObject.name == CompositorLayer.FallbackMeshName)
						{
							if (PrefabUtility.IsPartOfPrefabInstance(child.gameObject)) //If is part of prefab
							{
								Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(child.gameObject);
								DestroyImmediate(prefabInstance);
							}
							DestroyImmediate(child.gameObject);
							EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
						}
					}

					//Create fallback mesh
					targetCompositorLayer.AutoFallbackMeshGeneration();
				}
			}
			else
			{
				if (targetCompositorLayer.generatedFallbackMesh != null)
				{
					Debug.Log("Destroy Fallback Mesh");

					if (PrefabUtility.IsPartOfPrefabInstance(targetCompositorLayer.generatedFallbackMesh)) //If is part of prefab
					{
						Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(targetCompositorLayer.generatedFallbackMesh);
						DestroyImmediate(prefabInstance);
					}
					DestroyImmediate(targetCompositorLayer.generatedFallbackMesh);

					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

					targetCompositorLayer.generatedFallbackMesh = null;
				}
			}
		}
	}


#endregion
}
#endif
