#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wave.Essence.Spectator
{
	/// <summary>
	/// Name: SpectatorCameraTracker.Editor.cs
	/// Role: General script use in Unity Editor only
	/// Responsibility: Display the SpectatorCameraTracker.cs in Unity Inspector
	/// </summary>
	public partial class SpectatorCameraTracker
	{
		[field: SerializeField] private bool IsRequireLoadJsonFile { get; set; }
		
		[CustomEditor(typeof(SpectatorCameraTracker))]
		public class SpectatorCameraTrackerSettingEditor : UnityEditor.Editor
		{
			private SerializedProperty IsRequireLoadJsonFile { get; set; }
			private List<string> JsonFileList { get; set; }
			private Vector2 JsonFileScrollViewVector { get; set; }
			
			private SerializedProperty IsSmoothCameraMovement { get; set; }
			private SerializedProperty SmoothCameraMovementSpeed { get; set; }
			private SerializedProperty PanoramaResolution { get; set; }
			private SerializedProperty PanoramaOutputFormat { get; set; }
			private SerializedProperty PanoramaOutputType { get; set; }

			private void OnEnable()
			{
				IsRequireLoadJsonFile = serializedObject.FindProperty(EditorHelper.PropertyName("IsRequireLoadJsonFile"));
				JsonFileList = new List<string>();
				JsonFileScrollViewVector = Vector2.zero;
				
				IsSmoothCameraMovement = serializedObject.FindProperty(EditorHelper.PropertyName("IsSmoothCameraMovement"));
				SmoothCameraMovementSpeed = serializedObject.FindProperty(EditorHelper.PropertyName("SmoothCameraMovementSpeed"));
				PanoramaResolution = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaResolution"));
				PanoramaOutputFormat = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaOutputFormat"));
				PanoramaOutputType = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaOutputType"));
			}

			public override void OnInspectorGUI()
			{
				// Just return if not "SpectatorCameraTrackerSetting" class
				if (!(target is SpectatorCameraTracker))
				{
					return;
				}

				serializedObject.Update();

				DrawGUI();

				if (GUI.changed)
				{
					EditorUtility.SetDirty(target);
				}

				serializedObject.ApplyModifiedProperties();
			}

			private void DrawGUI()
			{
				#region GUIStyle

				var labelStyle = new GUIStyle()
				{
					richText = true,
					alignment = TextAnchor.MiddleCenter,
					normal = new GUIStyleState
					{
						textColor = EditorGUIUtility.isProSkin ? Color.green : Color.black
					}
				};

				var resetButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold,
					normal = new GUIStyleState
					{
						textColor = EditorGUIUtility.isProSkin ? Color.yellow : Color.red
					},
					hover = new GUIStyleState
					{
						textColor = Color.red
					},
					active = new GUIStyleState
					{
						textColor = Color.cyan
					},
				};

				var boldButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold
				};

				#endregion

				var script = (SpectatorCameraTracker)target;

				// Button for reset value
				if (GUILayout.Button("Reset to default value", resetButtonStyle))
				{
					script.ResetSetting();
				}

				// Button for export setting
				if (GUILayout.Button("Export Spectator Camera Tracker Setting", boldButtonStyle))
				{
					script.ExportSetting2JsonFile(SpectatorCameraHelper.AttributeFileLocation.ResourceFolder);
					AssetDatabase.Refresh();
				}

				#region Load setting from JSON file
				
				GUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(IsRequireLoadJsonFile.boolValue);
				if (GUILayout.Button("Load Setting From JSON File in Resources Folder", boldButtonStyle) ||
				    IsRequireLoadJsonFile.boolValue)
				{
					IsRequireLoadJsonFile.boolValue = true;
					var searchPattern =
						$"{SpectatorCameraHelper.ATTRIBUTE_FILE_PREFIX_NAME}*.{SpectatorCameraHelper.ATTRIBUTE_FILE_EXTENSION}";

					if (JsonFileList == null)
					{
						JsonFileList = new List<string>();
					}
					JsonFileList.Clear();

					var dir = new DirectoryInfo(SpectatorCameraHelper.ResourcesFolderPath);
					var files = dir.GetFiles(searchPattern);
					foreach (var item in files)
					{
						JsonFileList.Add(item.Name);
					}

					if (JsonFileList.Count == 0)
					{
						Debug.Log(
							"Can't find any JSON file related to the spectator camera setting in the Resources folder.");
						IsRequireLoadJsonFile.boolValue = false;
					}
				}
				EditorGUI.EndDisabledGroup();

				if (IsRequireLoadJsonFile.boolValue)
				{
					if (GUILayout.Button("Cancel"))
					{
						IsRequireLoadJsonFile.boolValue = false;
					}
				}
				GUILayout.EndHorizontal();

				if (IsRequireLoadJsonFile.boolValue)
				{
					Rect r = EditorGUILayout.BeginVertical();

					JsonFileScrollViewVector = EditorGUILayout.BeginScrollView(JsonFileScrollViewVector,
						GUILayout.Width(r.width),
						GUILayout.Height(80));
					
					for (int i = 0; i < JsonFileList.Count; i++)
					{
						if (GUILayout.Button(JsonFileList[i]))
						{
							var path = Path.Combine(
								SpectatorCameraHelper.ResourcesFolderPath,
								JsonFileList[i]);
							script.LoadSettingFromJsonFile(path);
						}
					}

					EditorGUILayout.EndScrollView();
					
					EditorGUILayout.EndVertical();
				}
				
				#endregion

				EditorGUILayout.LabelField("\n");

				#region Setting related to spectator camera and its frustum show/hide

				EditorGUILayout.LabelField("<b>[ General Setting ]</b>", labelStyle);

				// Setting of spectator camera layer mask
				script.LayerMask =
					LayerMaskHelper.LayerMaskDrawer.LayerMaskField("Camera Layer Mask", script.LayerMask);

				// Setting of smooth spectator camera movement
				EditorGUILayout.PropertyField(IsSmoothCameraMovement,
					new GUIContent("Enable Smoothing Camera Movement"));
				if (IsSmoothCameraMovement.boolValue)
				{
					EditorGUILayout.LabelField("\n");

					EditorGUILayout.LabelField("<b>[ Smooth Camera Movement Speed Setting ]</b>", labelStyle);

					EditorGUILayout.IntSlider(
						SmoothCameraMovementSpeed,
						SpectatorCameraHelper.SMOOTH_CAMERA_MOVEMENT_MIN,
						SpectatorCameraHelper.SMOOTH_CAMERA_MOVEMENT_MAX,
						"Speed of Smoothing Camera Movement");

					EditorGUILayout.LabelField("\n");
				}

				// Spectator camera frustum show/hide
				script.IsFrustumShowed =
					EditorGUILayout.Toggle("Enable Camera FOV Frustum", script.IsFrustumShowed);

				#endregion

				EditorGUILayout.LabelField("\n");

				#region VerticalFov

				EditorGUILayout.LabelField("<b>[ Vertical FOV Setting ]</b>", labelStyle);

				// FOV
				script.VerticalFov = EditorGUILayout.Slider(
					"Vertical FOV",
					script.VerticalFov,
					SpectatorCameraHelper.VERTICAL_FOV_MIN,
					SpectatorCameraHelper.VERTICAL_FOV_MAX);

				#endregion

				EditorGUILayout.LabelField("\n");

				#region Setting related to panorama capturing of spectator camera

				// Panorama resolution
				EditorGUILayout.LabelField("<b>[ Panorama Setting ]</b>", labelStyle);
				EditorGUILayout.PropertyField(PanoramaResolution, new GUIContent("Resolution"));

				// Panorama output format
				EditorGUILayout.PropertyField(PanoramaOutputFormat, new GUIContent("Output Format"));

				// Panorama output type
				EditorGUILayout.PropertyField(PanoramaOutputType, new GUIContent("Output Type"));

				#endregion

				EditorGUILayout.LabelField("\n");

				#region Setting related to frustum

				if (script.IsFrustumShowed)
				{
					EditorGUILayout.LabelField("<b>[ Frustum Setting ]</b>", labelStyle);

					// Count of frustum and frustum center line
					script.FrustumLineCount= (SpectatorCameraHelper.FrustumLineCount)
						EditorGUILayout.EnumPopup("Frustum Line Total", script.FrustumLineCount);
					script.FrustumCenterLineCount = (SpectatorCameraHelper.FrustumCenterLineCount)
						EditorGUILayout.EnumPopup("Frustum Center Line Total", script.FrustumCenterLineCount);

					EditorGUILayout.LabelField("\n");

					// Width of frustum and frustum center line
					script.FrustumLineWidth =
						EditorGUILayout.Slider(
							"Frustum Line Width",
							script.FrustumLineWidth,
							SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MIN,
							SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MAX);
					script.FrustumCenterLineWidth =
						EditorGUILayout.Slider(
							"Frustum Center Line Width",
							script.FrustumCenterLineWidth,
							SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MIN,
							SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MAX);

					EditorGUILayout.LabelField("\n");

					// Color of frustum and frustum center line
					script.FrustumLineColor = EditorGUILayout.ColorField(
						"Frustum Line Color", script.FrustumLineColor);
					script.FrustumCenterLineColor = EditorGUILayout.ColorField(
						"Frustum Center Line Color",
						script.FrustumCenterLineColor);

					EditorGUILayout.LabelField("\n");
				}

				#endregion
			}
		}
	}
}
#endif
