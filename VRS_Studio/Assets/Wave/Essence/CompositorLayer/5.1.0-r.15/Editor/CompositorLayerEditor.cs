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

		static string PropertyName_QuadWidth = "m_QuadWidth";
		static GUIContent Label_QuadWidth = new GUIContent("Width");
		SerializedProperty Property_QuadWidth;

		static string PropertyName_QuadHeight = "m_QuadHeight";
		static GUIContent Label_QuadHeight = new GUIContent("Height");
		SerializedProperty Property_QuadHeight;

		static string PropertyName_CylinderHeight = "m_CylinderHeight";
		static GUIContent Label_CylinderHeight = new GUIContent("Height");
		SerializedProperty Property_CylinderHeight;

		static string PropertyName_CylinderArcLength = "m_CylinderArcLength";
		static GUIContent Label_CylinderArcLength = new GUIContent("Arc Length");
		SerializedProperty Property_CylinderArcLength;

		static string PropertyName_CylinderRadius = "m_CylinderRadius";
		static GUIContent Label_CylinderRadius = new GUIContent("Radius");
		SerializedProperty Property_CylinderRadius;

		static string PropertyName_AngleOfArc = "m_CylinderAngleOfArc";
		static GUIContent Label_AngleOfArc = new GUIContent("Arc Angle");
		SerializedProperty Property_AngleOfArc;

		static string PropertyName_IsDynamicLayer = "isDynamicLayer";
		static GUIContent Label_IsDynamicLayer = new GUIContent("Dynamic Layer");
		SerializedProperty Property_IsDynamicLayer;

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
					float radiusLowerBound = Mathf.Max(0.001f, CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.angleOfArcUpperLimit));
					float radiusUpperBound = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.angleOfArcLowerLimit);
					EditorGUILayout.HelpBox("Changing the Arc Length will affect the upper and lower bounds of the radius.\nUpper Bound of Radius: " + radiusUpperBound + "\nLower Bound of Radius: " + radiusLowerBound, MessageType.Info);

					EditorGUILayout.PropertyField(Property_CylinderRadius, new GUIContent(Label_CylinderRadius));
					EditorGUILayout.PropertyField(Property_LockMode, new GUIContent(Label_LockMode));
					serializedObject.ApplyModifiedProperties();
					
					EditorGUILayout.HelpBox("Arc Length and Arc Angle are correlated, adjusting one of them changes the other as well. The Radius will not be changed when adjusting these two values.", MessageType.Info);
					if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength)
					{
						GUI.enabled = false;
						EditorGUILayout.PropertyField(Property_CylinderArcLength, new GUIContent(Label_CylinderArcLength));
						GUI.enabled = true;
						EditorGUILayout.Slider(Property_AngleOfArc, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, new GUIContent(Label_AngleOfArc));
					}
					else if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle)
					{
						EditorGUILayout.PropertyField(Property_CylinderArcLength, new GUIContent(Label_CylinderArcLength));
						GUI.enabled = false;
						EditorGUILayout.Slider(Property_AngleOfArc, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, new GUIContent(Label_AngleOfArc));
						GUI.enabled = true;
					}
					EditorGUILayout.PropertyField(Property_CylinderHeight, new GUIContent(Label_CylinderHeight));
				}



				serializedObject.ApplyModifiedProperties();

				CompositorLayer.CylinderLayerParamAdjustmentMode currentAdjustmentMode = targetCompositorLayer.CurrentAdjustmentMode();

				switch(currentAdjustmentMode)
				{
					case CompositorLayer.CylinderLayerParamAdjustmentMode.ArcLength:
						{
							targetCompositorLayer.CylinderAngleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.CylinderRadius);
							float cylinderArcLengthRef = targetCompositorLayer.CylinderArcLength;
							if (!ArcLengthValidityCheck(ref cylinderArcLengthRef, targetCompositorLayer.CylinderRadius, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit))
							{
								targetCompositorLayer.CylinderArcLength = cylinderArcLengthRef;
								targetCompositorLayer.CylinderAngleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.CylinderRadius);
							}
							serializedObject.ApplyModifiedProperties();
							break;
						}
					case CompositorLayer.CylinderLayerParamAdjustmentMode.ArcAngle:
						{
							targetCompositorLayer.CylinderArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(targetCompositorLayer.CylinderAngleOfArc, targetCompositorLayer.CylinderRadius);
							serializedObject.ApplyModifiedProperties();
							break;
						}
					case CompositorLayer.CylinderLayerParamAdjustmentMode.Radius:
					default:
						{
							float cylinderRadiusRef = targetCompositorLayer.CylinderRadius;
							RadiusValidityCheck(targetCompositorLayer.CylinderArcLength, ref cylinderRadiusRef, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit, targetCompositorLayer.lockMode);
							targetCompositorLayer.CylinderRadius = cylinderRadiusRef;
							if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength)
							{
								targetCompositorLayer.CylinderAngleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.CylinderRadius);
								float cylinderArcLengthRef = targetCompositorLayer.CylinderArcLength;
								if (!ArcLengthValidityCheck(ref cylinderArcLengthRef, targetCompositorLayer.CylinderRadius, targetCompositorLayer.angleOfArcLowerLimit, targetCompositorLayer.angleOfArcUpperLimit))
								{
									targetCompositorLayer.CylinderArcLength = cylinderArcLengthRef;
									targetCompositorLayer.CylinderAngleOfArc = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(targetCompositorLayer.CylinderArcLength, targetCompositorLayer.CylinderRadius);
								}
							}
							else if (targetCompositorLayer.lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle)
							{
								targetCompositorLayer.CylinderArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(targetCompositorLayer.CylinderAngleOfArc, targetCompositorLayer.CylinderRadius);
							}
							serializedObject.ApplyModifiedProperties();
							break;
						}
				}

				EditorGUILayout.HelpBox("Current Layer Aspect Ratio (Arc Length : Height) = " + targetCompositorLayer.CylinderArcLength + " : " + targetCompositorLayer.CylinderHeight, MessageType.Info);

				bool CylinderParamsChanged = targetCompositorLayer.LayerDimensionsChanged();
				targetCompositorLayer.UpdatePreviousLayerDimensions();

				if (targetCompositorLayer.isPreviewingCylinder)
				{
					Transform generatedPreviewTransform = targetCompositorLayer.transform.Find(CompositorLayer.CylinderPreviewName);

					if (generatedPreviewTransform != null)
					{
						targetCompositorLayer.generatedPreview = generatedPreviewTransform.gameObject;

						if (CylinderParamsChanged)
						{
							//Generate vertices
							Vector3[] cylinderVertices = CompositorLayer.MeshGenerationHelper.GenerateCylinderVertex(targetCompositorLayer.CylinderAngleOfArc, targetCompositorLayer.CylinderRadius, targetCompositorLayer.CylinderHeight);

							MeshFilter cylinderMeshFilter = targetCompositorLayer.generatedPreview.GetComponent<MeshFilter>();
							//Generate Mesh
							cylinderMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateCylinderMesh(targetCompositorLayer.CylinderAngleOfArc, cylinderVertices);

							targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
							targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

							targetCompositorLayer.generatedPreview.transform.localScale = targetCompositorLayer.GetNormalizedLocalScale(targetCompositorLayer.transform, Vector3.one);
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
						targetCompositorLayer.isPreviewingCylinder = false;
					}
				}
				else
				{
					if (GUILayout.Button("Show Cylinder Preview"))
					{
						targetCompositorLayer.isPreviewingCylinder = true;
						Vector3[] cylinderVertices = CompositorLayer.MeshGenerationHelper.GenerateCylinderVertex(targetCompositorLayer.CylinderAngleOfArc, targetCompositorLayer.CylinderRadius, targetCompositorLayer.CylinderHeight);
						//Add components to Game Object
						targetCompositorLayer.generatedPreview = new GameObject();
						targetCompositorLayer.generatedPreview.hideFlags = HideFlags.HideAndDontSave;
						targetCompositorLayer.generatedPreview.name = CompositorLayer.CylinderPreviewName;
						targetCompositorLayer.generatedPreview.transform.SetParent(targetCompositorLayer.gameObject.transform);
						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = targetCompositorLayer.GetNormalizedLocalScale(targetCompositorLayer.transform, Vector3.one);

						MeshRenderer cylinderMeshRenderer = targetCompositorLayer.generatedPreview.AddComponent<MeshRenderer>();
						MeshFilter cylinderMeshFilter = targetCompositorLayer.generatedPreview.AddComponent<MeshFilter>();
						cylinderMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));

						if (targetCompositorLayer.textures[0] != null)
						{
							cylinderMeshRenderer.sharedMaterial.mainTexture = targetCompositorLayer.textures[0];
						}

						//Generate Mesh
						cylinderMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateCylinderMesh(targetCompositorLayer.CylinderAngleOfArc, cylinderVertices);
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
				bool QuadParamsChanged = targetCompositorLayer.LayerDimensionsChanged();
				targetCompositorLayer.UpdatePreviousLayerDimensions();
				if (targetCompositorLayer.isPreviewingQuad)
				{
					Transform generatedPreviewTransform = targetCompositorLayer.transform.Find(CompositorLayer.QuadPreviewName);

					if (generatedPreviewTransform != null)
					{
						targetCompositorLayer.generatedPreview = generatedPreviewTransform.gameObject;

						if (QuadParamsChanged)
						{
							//Generate vertices
							Vector3[] quadVertices = CompositorLayer.MeshGenerationHelper.GenerateQuadVertex(targetCompositorLayer.quadWidth, targetCompositorLayer.quadHeight);

							MeshFilter quadMeshFilter = targetCompositorLayer.generatedPreview.GetComponent<MeshFilter>();
							//Generate Mesh
							quadMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateQuadMesh(targetCompositorLayer, quadVertices);

							targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
							targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

							targetCompositorLayer.generatedPreview.transform.localScale = targetCompositorLayer.GetNormalizedLocalScale(targetCompositorLayer.transform, Vector3.one);
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
						targetCompositorLayer.isPreviewingQuad = false;
					}
				}
				else
				{
					if (GUILayout.Button("Show Quad Preview"))
					{
						targetCompositorLayer.isPreviewingQuad = true;
						//Generate vertices
						Vector3[] quadVertices = CompositorLayer.MeshGenerationHelper.GenerateQuadVertex(targetCompositorLayer.quadWidth, targetCompositorLayer.quadHeight);

						//Add components to Game Object
						targetCompositorLayer.generatedPreview = new GameObject();
						targetCompositorLayer.generatedPreview.hideFlags = HideFlags.HideAndDontSave;
						targetCompositorLayer.generatedPreview.name = CompositorLayer.QuadPreviewName;
						targetCompositorLayer.generatedPreview.transform.SetParent(targetCompositorLayer.gameObject.transform);
						targetCompositorLayer.generatedPreview.transform.localPosition = Vector3.zero;
						targetCompositorLayer.generatedPreview.transform.localRotation = Quaternion.identity;

						targetCompositorLayer.generatedPreview.transform.localScale = targetCompositorLayer.GetNormalizedLocalScale(targetCompositorLayer.transform, Vector3.one);

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

			EditorGUILayout.PropertyField(Property_RenderPriority, Label_RenderPriority);
			serializedObject.ApplyModifiedProperties();
		}

		public static bool RadiusValidityCheck(float inArcLength, ref float inRadius, float thetaLowerLimit, float thetaUpperLimit, CompositorLayer.CylinderLayerParamLockMode lockMode)
		{
			bool isValid = true;

			if (inRadius <= 0)
			{
				inRadius = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, thetaUpperLimit);
				isValid = false;
				return isValid;
			}

			float degThetaResult = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(inArcLength, inRadius);

			if (degThetaResult < thetaLowerLimit)
			{
				if (lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle) //Angle locked, increase arc length
				{
					ArcLengthValidityCheck(ref inArcLength, inRadius, thetaLowerLimit, thetaUpperLimit);
					inRadius = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, thetaLowerLimit);
				}
				else if (lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength) //ArcLength Locked, keep angle at min
				{
					inRadius = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, thetaLowerLimit);
				}
				isValid = false;
			}
			else if (degThetaResult > thetaUpperLimit)
			{
				if (lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcAngle) //Angle locked, decrease arc length
				{
					ArcLengthValidityCheck(ref inArcLength, inRadius, thetaLowerLimit, thetaUpperLimit);
					inRadius = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, thetaUpperLimit);
				}
				else if (lockMode == CompositorLayer.CylinderLayerParamLockMode.ArcLength) //ArcLength Locked, keep angle at max
				{
					inRadius = CompositorLayer.CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, thetaUpperLimit);
				}
				isValid = false;
			}

			return isValid;
		}

		public static bool ArcLengthValidityCheck(ref float inArcLength, float inRadius, float thetaLowerLimit, float thetaUpperLimit)
		{
			bool isValid = true;

			if (inArcLength <= 0)
			{
				inArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(thetaLowerLimit, inRadius);
				isValid = false;
				return isValid;
			}

			float degThetaResult = CompositorLayer.CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(inArcLength, inRadius);

			if (degThetaResult < thetaLowerLimit)
			{
				inArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(thetaLowerLimit, inRadius);
				isValid = false;
			}
			else if (degThetaResult > thetaUpperLimit)
			{
				inArcLength = CompositorLayer.CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(thetaUpperLimit, inRadius);
				isValid = false;
			}

			return isValid;
		}
	}
	#endregion
}
#endif
