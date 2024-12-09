#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wave.Essence.Spectator
{
	/// <summary>
	/// Name: SpectatorCameraManager.Editor.cs
	/// Role: General script use in Unity Editor only
	/// Responsibility: Display the SpectatorCameraManager.cs in Unity Inspector
	/// </summary>
	public partial class SpectatorCameraManager
	{
		[field: SerializeField] private bool IsShowHmdPart { get; set; }
		[field: SerializeField] private bool IsShowTrackerPart { get; set; }
		[field: SerializeField] private bool IsRequireLoadJsonFile { get; set; }

		[SerializeField] private bool debugStartCamera = true;
		public bool DebugStartCamera
		{
			get => debugStartCamera;
			set
			{
				if (debugStartCamera == value)
				{
					return;
				}

				debugStartCamera = value;
				SetDebugVariables2SpectatorHandler();
			}
		}

		[SerializeField] private bool debugRenderFrame = true;
		public bool DebugRenderFrame
		{
			get => debugRenderFrame;
			set
			{
				if (debugRenderFrame == value)
				{
					return;
				}

				debugRenderFrame = value;
				SetDebugVariables2SpectatorHandler();
			}
		}

		[SerializeField] private int debugFPS = SpectatorCameraHelper.DEBUG_RENDER_FPS_DEFAULT;
		public int DebugFPS
		{
			get => debugFPS;
			set
			{
				if (debugFPS == value)
				{
					return;
				}
				
				debugFPS = Mathf.Clamp(
					value,
					SpectatorCameraHelper.DEBUG_RENDER_FPS_MIN,
					SpectatorCameraHelper.DEBUG_RENDER_FPS_MAX);
				SetDebugVariables2SpectatorHandler();
			}
		}
		
		private void SetDebugVariables2SpectatorHandler()
		{
			if (IsSpectatorCameraHandlerExist() is false)
			{
				return;
			}
			
			SpectatorHandler.debugStartCamera = DebugStartCamera;
			SpectatorHandler.debugRenderFrame = DebugRenderFrame;
			SpectatorHandler.debugFPS = DebugFPS;
		}
		
		[CustomEditor(typeof(SpectatorCameraManager))]
		public class SpectatorCameraManagerEditor : UnityEditor.Editor
		{
			private static readonly Color HighlightRegionBackgroundColor = new Color(.2f, .2f, .2f, .1f);
			
			private SerializedProperty IsShowHmdPart { get; set; }
			private SerializedProperty IsShowTrackerPart { get; set; }
			private SerializedProperty IsRequireLoadJsonFile { get; set; }
			private List<string> JsonFileList { get; set; }
			private Vector2 JsonFileScrollViewVector { get; set; }
			
			private SerializedProperty IsSmoothCameraMovement { get; set; }
			private SerializedProperty SmoothCameraMovementSpeed { get; set; }
			private SerializedProperty PanoramaResolution { get; set; }
			private SerializedProperty PanoramaOutputFormat { get; set; }
			private SerializedProperty PanoramaOutputType { get; set; }

			private SerializedProperty SpectatorCameraPrefab { get; set; }

			private void OnEnable()
			{
				IsShowHmdPart = serializedObject.FindProperty(EditorHelper.PropertyName("IsShowHmdPart"));
				IsShowTrackerPart = serializedObject.FindProperty(EditorHelper.PropertyName("IsShowTrackerPart"));
				IsRequireLoadJsonFile = serializedObject.FindProperty(EditorHelper.PropertyName("IsRequireLoadJsonFile"));
				JsonFileList = new List<string>();
				JsonFileScrollViewVector = Vector2.zero;
				
				IsSmoothCameraMovement = serializedObject.FindProperty(EditorHelper.PropertyName("IsSmoothCameraMovement"));
				SmoothCameraMovementSpeed = serializedObject.FindProperty(EditorHelper.PropertyName("SmoothCameraMovementSpeed"));
				
				PanoramaResolution = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaResolution"));
				PanoramaOutputFormat = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaOutputFormat"));
				PanoramaOutputType = serializedObject.FindProperty(EditorHelper.PropertyName("PanoramaOutputType"));

				SpectatorCameraPrefab = serializedObject.FindProperty(EditorHelper.PropertyName("SpectatorCameraPrefab"));
			}

			public override void OnInspectorGUI()
			{
				// Just return if not "SpectatorCameraManager" class
				if (!(target is SpectatorCameraManager))
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

				var script = (SpectatorCameraManager)target;

				// Button for reset value
				if (GUILayout.Button("Reset to default value", resetButtonStyle))
				{
					script.ResetSetting();
				}
				
				// Button for export setting
				if (GUILayout.Button("Export Spectator Camera HMD Setting", boldButtonStyle))
				{
					script.ExportSetting2JsonFile(SpectatorCameraHelper.AttributeFileLocation.ResourceFolder);
					AssetDatabase.Refresh();
				}

				#region Load Setting From JSON File
				
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

				// Spectator camera prefab
				EditorGUILayout.PropertyField(SpectatorCameraPrefab, new GUIContent("Spectator Camera Prefab"));
				if (SpectatorCameraPrefab.objectReferenceValue != null &&
				    PrefabUtility.GetPrefabAssetType(SpectatorCameraPrefab.objectReferenceValue) == PrefabAssetType.NotAPrefab)
				{
					// The assign object is scene object
					Debug.Log("Please assign the object as prefab only.");
					SpectatorCameraPrefab.objectReferenceValue = null;
				}

				EditorGUILayout.LabelField("\n");

				EditorGUILayout.LabelField("<b>[ General Setting ]</b>", labelStyle);

				// Setting of spectator camera reference source
				// EditorGUILayout.PropertyField(CameraSourceRef, new GUIContent("Camera Source"));
				script.CameraSourceRef = (SpectatorCameraHelper.CameraSourceRef)
					EditorGUILayout.EnumPopup("Camera Source", script.CameraSourceRef);

				#region Tracker Region

				if (script.CameraSourceRef == SpectatorCameraHelper.CameraSourceRef.Tracker)
				{
					script.FollowSpectatorCameraTracker = EditorGUILayout.ObjectField(
						"Tracker",
						script.FollowSpectatorCameraTracker,
						typeof(SpectatorCameraTracker),
						true) as SpectatorCameraTracker;

					if (script.FollowSpectatorCameraTracker == null)
					{
						// The assign object is null
						EditorGUILayout.HelpBox("Please assign the SpectatorCameraTracker", MessageType.Info, false);
					}
					else if (PrefabUtility.GetPrefabAssetType(script.FollowSpectatorCameraTracker) != PrefabAssetType.NotAPrefab)
					{
						// Don't allow assign object is prefab
						Debug.Log("Please assign the scene object.");
						script.FollowSpectatorCameraTracker = null;
					}
					else
					{
						// The assign object is scene object => ok
						EditorGUILayout.LabelField("\n");
						IsShowTrackerPart.boolValue = EditorGUILayout.Foldout(IsShowTrackerPart.boolValue, "Tracker Setting");
						if (IsShowTrackerPart.boolValue)
						{
							// If show the tracker setting
							Rect r = EditorGUILayout.BeginVertical();

							SpectatorCameraTracker trackerObject = script.FollowSpectatorCameraTracker;

							if (trackerObject != null)
							{
								EditorGUILayout.HelpBox(
									$"You are now editing the tracker setting in \"{trackerObject.gameObject.name}\" GameObject",
									MessageType.Info,
									true);
								
								trackerObject.LayerMask =
									LayerMaskHelper.LayerMaskDrawer.LayerMaskField("Camera Layer Mask",
										trackerObject.LayerMask);

								trackerObject.IsSmoothCameraMovement =
									EditorGUILayout.Toggle("Enable Smoothing Camera Movement",
										trackerObject.IsSmoothCameraMovement);

								if (trackerObject.IsSmoothCameraMovement)
								{
									EditorGUILayout.LabelField("\n");

									EditorGUILayout.LabelField("<b>[ Smooth Camera Movement Speed Setting ]</b>",
										labelStyle);
									trackerObject.SmoothCameraMovementSpeed =
										EditorGUILayout.IntSlider(
											new GUIContent("Speed of Smoothing Camera Movement"),
											trackerObject.SmoothCameraMovementSpeed,
											SpectatorCameraHelper.SMOOTH_CAMERA_MOVEMENT_MIN,
											SpectatorCameraHelper.SMOOTH_CAMERA_MOVEMENT_MAX);

									EditorGUILayout.LabelField("\n");
								}

								// Spectator camera frustum show/hide
								trackerObject.IsFrustumShowed =
									EditorGUILayout.Toggle("Enable Camera FOV Frustum", trackerObject.IsFrustumShowed);

								EditorGUILayout.LabelField("\n");

								#region VerticalFov

								EditorGUILayout.LabelField("<b>[ Vertical FOV Setting ]</b>", labelStyle);

								trackerObject.VerticalFov = EditorGUILayout.Slider(
									"Vertical FOV",
									trackerObject.VerticalFov,
									SpectatorCameraHelper.VERTICAL_FOV_MIN,
									SpectatorCameraHelper.VERTICAL_FOV_MAX);

								#endregion

								EditorGUILayout.LabelField("\n");

								#region Setting related to panorama capturing of spectator camera

								// Panorama resolution
								EditorGUILayout.LabelField("<b>[ Panorama Setting ]</b>", labelStyle);
								trackerObject.PanoramaResolution =
									(SpectatorCameraHelper.SpectatorCameraPanoramaResolution)
									EditorGUILayout.EnumPopup("Resolution", trackerObject.PanoramaResolution);

								// Panorama output format
								trackerObject.PanoramaOutputFormat = (TextureProcessHelper.PictureOutputFormat)
									EditorGUILayout.EnumPopup("Output Format", trackerObject.PanoramaOutputFormat);

								// Panorama output type
								trackerObject.PanoramaOutputType = (TextureProcessHelper.PanoramaType)
									EditorGUILayout.EnumPopup("Output Type", trackerObject.PanoramaOutputType);

								#endregion

								EditorGUILayout.LabelField("\n");

								#region Setting related to frustum

								if (trackerObject.IsFrustumShowed)
								{
									EditorGUILayout.LabelField("<b>[ Frustum Setting ]</b>",
										labelStyle);

									#region Count of frustum and frustum center line

									trackerObject.FrustumLineCount = (SpectatorCameraHelper.FrustumLineCount)
										EditorGUILayout.EnumPopup("Frustum Line Total", trackerObject.FrustumLineCount);
									trackerObject.FrustumCenterLineCount =
										(SpectatorCameraHelper.FrustumCenterLineCount)
										EditorGUILayout.EnumPopup("Frustum Center Line Total",
											trackerObject.FrustumCenterLineCount);

									#endregion

									EditorGUILayout.LabelField("\n");

									#region Width of frustum and frustum center line

									trackerObject.FrustumLineWidth =
										EditorGUILayout.Slider(
											"Frustum Line Width",
											trackerObject.FrustumLineWidth,
											SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MIN,
											SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MAX);
									trackerObject.FrustumCenterLineWidth =
										EditorGUILayout.Slider(
											"Frustum Center Line Width",
											trackerObject.FrustumCenterLineWidth,
											SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MIN,
											SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MAX);

									#endregion

									EditorGUILayout.LabelField("\n");

									#region Material of frustum and frustum center line

									trackerObject.FrustumLineColor = EditorGUILayout.ColorField(
										"Frustum Line Color", trackerObject.FrustumLineColor);
									trackerObject.FrustumCenterLineColor = EditorGUILayout.ColorField(
										"Frustum Center Line Color",
										trackerObject.FrustumCenterLineColor);

									#endregion
								}

								#endregion

								EditorGUILayout.EndVertical();
								r = new Rect(r.x, r.y, r.width, r.height);
								EditorGUI.DrawRect(r, HighlightRegionBackgroundColor);

								EditorGUILayout.LabelField("\n");
							}
						}
					}
				}

				#endregion

				#region HMD Region

				IsShowHmdPart.boolValue = EditorGUILayout.Foldout(IsShowHmdPart.boolValue, "HMD Setting");
				if (IsShowHmdPart.boolValue)
				{
					Rect r = EditorGUILayout.BeginVertical();

					// Setting of spectator camera layer mask
					script.LayerMask =
						LayerMaskHelper.LayerMaskDrawer.LayerMaskField("Camera Layer Mask", script.LayerMask);

					// Setting of smooth spectator camera movement
					EditorGUILayout.PropertyField(IsSmoothCameraMovement, new GUIContent("Enable Smoothing Camera Movement"));
					if (IsSmoothCameraMovement.boolValue)
					{
						EditorGUILayout.LabelField("\n");

						EditorGUILayout.LabelField("<b>[ Smooth Camera Movement Speed Setting ]</b>", labelStyle);
						
						// Setting of smooth spectator camera movement speed
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

					#region Setting related to frustum

					if (script.IsFrustumShowed)
					{
						EditorGUILayout.LabelField("\n");
						
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
					}

					#endregion

					EditorGUILayout.EndVertical();
					r = new Rect(r.x, r.y, r.width, r.height);
					EditorGUI.DrawRect(r, HighlightRegionBackgroundColor);
				}

				#endregion

				EditorGUILayout.LabelField("\n");

				#region Test 360 Output
				
				EditorGUILayout.LabelField("<b>[ Debug Setting ]</b>", labelStyle);
				
				script.DebugStartCamera = 
					EditorGUILayout.Toggle("Start spectator camera for debugging", script.DebugStartCamera);
				
				if (script.DebugStartCamera)
				{
					script.DebugRenderFrame =
						EditorGUILayout.Toggle("Render spectator camera view for debugging", script.DebugRenderFrame);
					script.DebugFPS =
						EditorGUILayout.IntSlider("Debugging Render fps",
							script.DebugFPS,
							SpectatorCameraHelper.DEBUG_RENDER_FPS_MIN,
							SpectatorCameraHelper.DEBUG_RENDER_FPS_MAX);
				}
				else
				{
					script.DebugRenderFrame = false;
				}

				EditorGUILayout.HelpBox("Test - Output 360 photo", MessageType.Info, true);
				if (GUILayout.Button("Test - Output 360 photo"))
				{
					script.CaptureSpectatorCamera360Photo();
				}

				#endregion
			}
		}
	}
}
#endif
