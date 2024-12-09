using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Wave.XR;
using Wave.XR.Settings;

namespace Wave.Essence.Spectator
{
	/// <summary>
	/// Name: SpectatorCameraManager.cs
	/// Role: Manager (Singleton)
	/// Responsibility: Manage the spectator camera
	/// </summary>
	public partial class SpectatorCameraManager : MonoBehaviour, ISpectatorCameraSetting
	{
		// Singleton
		private static SpectatorCameraManager _instance;
		public static SpectatorCameraManager Instance => _instance;

		[SerializeField] private SpectatorCameraHelper.CameraSourceRef cameraSourceRef;
		public SpectatorCameraHelper.CameraSourceRef CameraSourceRef
		{
			get => cameraSourceRef;
			set
			{
				if (cameraSourceRef == value)
				{
					return;
				}
				cameraSourceRef = value;
				
				if (IsSpectatorCameraHandlerExist() is false)
				{
					return;
				}
				CameraStateChangingProcessing();
			}
		}

		[SerializeField] private LayerMask layerMask;
		public LayerMask LayerMask
		{
			get => layerMask;
			set
			{
				if (layerMask == value)
				{
					return;
				}
				layerMask = value;
				
				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SpectatorHandler.SetCullingMask(layerMask);
			}
		}

		[field: SerializeField] public bool IsSmoothCameraMovement { get; set; }
		[field: SerializeField] public int SmoothCameraMovementSpeed { get; set; }

		[SerializeField] private bool isFrustumShowed;
		public bool IsFrustumShowed
		{
			get => isFrustumShowed;
			set
			{
				if (isFrustumShowed == value)
				{
					return;
				}
				isFrustumShowed = value;

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumLine();
				SetupFrustumCenterLine();
			}
		}
		
		[SerializeField] private float verticalFov;
		public float VerticalFov
		{
			get => verticalFov;
			set
			{
				if (Math.Abs(verticalFov - value) < SpectatorCameraHelper.COMPARE_FLOAT_MEDIUM_THRESHOLD)
				{
					return;
				}
				verticalFov = Mathf.Clamp(
					value,
					SpectatorCameraHelper.VERTICAL_FOV_MIN,
					SpectatorCameraHelper.VERTICAL_FOV_MAX);

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SpectatorHandler.SetFixedFOV(verticalFov);
				SetupFrustumLine();
				SetupFrustumCenterLine();
			}
		}

		#region Panorama properties
		
		[field: SerializeField] public SpectatorCameraHelper.SpectatorCameraPanoramaResolution PanoramaResolution { get; set; }
		[field: SerializeField] public TextureProcessHelper.PictureOutputFormat PanoramaOutputFormat { get; set; }
		[field: SerializeField] public TextureProcessHelper.PanoramaType PanoramaOutputType { get; set; }
		
		#endregion

		[SerializeField] private SpectatorCameraHelper.FrustumLineCount frustumLineCount;
		public SpectatorCameraHelper.FrustumLineCount FrustumLineCount
		{
			get => frustumLineCount;
			set
			{
				if (frustumLineCount == value)
				{
					return;
				}
				frustumLineCount = value;

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumLine();
			}
		}
		
		[SerializeField] private SpectatorCameraHelper.FrustumCenterLineCount frustumCenterLineCount;
		public SpectatorCameraHelper.FrustumCenterLineCount FrustumCenterLineCount
		{
			get => frustumCenterLineCount;
			set
			{
				if (frustumCenterLineCount == value)
				{
					return;
				}
				frustumCenterLineCount = value;

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumCenterLine();
			}
		}
		
		[SerializeField] private float frustumLineWidth;
		public float FrustumLineWidth
		{
			get => frustumLineWidth;
			set
			{
				if (Math.Abs(frustumLineWidth - value) < SpectatorCameraHelper.COMPARE_FLOAT_SUPER_SMALL_THRESHOLD)
				{
					return;
				}
				frustumLineWidth = Mathf.Clamp(
					value,
					SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MIN,
					SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_MAX);

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumLine();
			}
		}

		[SerializeField] private float frustumCenterLineWidth;
		public float FrustumCenterLineWidth
		{
			get => frustumCenterLineWidth;
			set
			{
				if (Math.Abs(frustumCenterLineWidth - value) < SpectatorCameraHelper.COMPARE_FLOAT_SUPER_SMALL_THRESHOLD)
				{
					return;
				}
				frustumCenterLineWidth = Mathf.Clamp(value, SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MIN,
					SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_MAX);

				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumCenterLine();
			}
		}
		
		[SerializeField] private Color frustumLineColor;
		public Color FrustumLineColor
		{
			get => frustumLineColor;
			set
			{
				if (frustumLineColor == value)
				{
					return;
				}
				frustumLineColor = value;
				
				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumLine();
			}
		}
		
		[SerializeField] private Color frustumCenterLineColor;
		public Color FrustumCenterLineColor
		{
			get => frustumCenterLineColor;
			set
			{
				if (frustumCenterLineColor == value)
				{
					return;
				}
				frustumCenterLineColor = value;
				
				if (IsSpectatorCameraHandlerExist() is false || IsCameraSourceAsHmd() is false)
				{
					return;
				}
				SetupFrustumCenterLine();
			}
		}

		#region Varibles of the camera prefeb, camera gameobject, camera handler and main (rig) camera
		
		// The camera prefab
		[field: SerializeField] private GameObject SpectatorCameraPrefab { get; set; }
		
		// The camera that is loaded in the scene (create the camera prefab)
		public GameObject SpectatorCamera { get; private set; }
		
		// The camera handle from WaveXR SDK
		private WaveXRSpectatorCameraHandle _spectatorHandler;
		public WaveXRSpectatorCameraHandle SpectatorHandler
		{
			get => _spectatorHandler;
			private set
			{
				if (_spectatorHandler == value)
				{
					return;
				}
				
				_spectatorHandler = value;
				
#if UNITY_EDITOR
				SetDebugVariables2SpectatorHandler();
#endif
				InitCameraPoseAndPrefab();
				CameraStateChangingProcessing();
			}
		}

		// Main camera in the scene, it should be the camera that is attached to the XR/WaveRig
		private Camera MainCamera { get; set; }

		#endregion
		
		#region Varibles of last value of camera FOV and main camera position and rotation

		// The previous value of the position of the main (HMD) camera (P.S. only use in CameraSourceRef.Hmd mode)
		private Vector3 CameraLastPosition { get; set; }

		// The previous value of the rotation of the main (HMD) camera (P.S. only use in CameraSourceRef.Hmd mode)
		private Quaternion CameraLastRotation { get; set; }

		#endregion

		#region Variables of visualization FOV

		private GameObject FrustumLineRoot { get; set; }
		private GameObject FrustumCenterLineRoot { get; set; }
		private List<LineRenderer> _frustumLineList;
		private List<LineRenderer> _frustumCenterLineList;

		#endregion
		
		#region Varibles of tracker list includes all trackers in the scene, the current tracker candidate, and its index in the tracker list
		
		// All tracker candidates in the scene
		private List<SpectatorCameraTracker> _spectatorCameraTrackerList;
		public IReadOnlyList<SpectatorCameraTracker> SpectatorCameraTrackerList => _spectatorCameraTrackerList;
		
		// The tracker candidate that the camera will follow in "Tracker mode"
		[SerializeField] private SpectatorCameraTracker followSpectatorCameraTracker;
		public SpectatorCameraTracker FollowSpectatorCameraTracker
		{
			get => followSpectatorCameraTracker;
			set
			{
				if (followSpectatorCameraTracker == value)
				{
					return;
				}
				followSpectatorCameraTracker = value;
				
				if (IsSpectatorCameraHandlerExist() is false)
				{
					return;
				}
				AddSpectatorCameraTracker(value);
				CameraStateChangingProcessing();
			}
		}

		// The current index of tracker candidate
		private int FollowSpectatorCameraTrackerIndex
		{
			get
			{
				if (!(_spectatorCameraTrackerList is null) && !(FollowSpectatorCameraTracker is null))
				{
					return _spectatorCameraTrackerList.IndexOf(FollowSpectatorCameraTracker);
				}
				else
				{
					return -1;
				}
			}
		}
		
		#endregion

		// Variable of camera recording status
		private bool IsRecording { get; set; }

		#region Public Functions of camera setting I/O
		
		public void ResetSetting()
		{
			CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
			LayerMask = SpectatorCameraHelper.LayerMaskDefault;
			IsSmoothCameraMovement = SpectatorCameraHelper.IS_SMOOTH_CAMERA_MOVEMENT_DEFAULT;
			SmoothCameraMovementSpeed = SpectatorCameraHelper.SMOOTH_CAMERA_MOVEMENT_SPEED_DEFAULT;
			IsFrustumShowed = SpectatorCameraHelper.IS_FRUSTUM_SHOWED_DEFAULT;
			VerticalFov = SpectatorCameraHelper.VERTICAL_FOV_DEFAULT;
			PanoramaResolution = SpectatorCameraHelper.PANORAMA_RESOLUTION_DEFAULT;
			PanoramaOutputFormat = SpectatorCameraHelper.PANORAMA_OUTPUT_FORMAT_DEFAULT;
			PanoramaOutputType = SpectatorCameraHelper.PANORAMA_TYPE_DEFAULT;
			FrustumLineCount = SpectatorCameraHelper.FRUSTUM_LINE_COUNT_DEFAULT;
			FrustumCenterLineCount = SpectatorCameraHelper.FRUSTUM_CENTER_LINE_COUNT_DEFAULT;
			FrustumLineWidth = SpectatorCameraHelper.FRUSTUM_LINE_WIDTH_DEFAULT;
			FrustumCenterLineWidth = SpectatorCameraHelper.FRUSTUM_CENTER_LINE_WIDTH_DEFAULT;
			FrustumLineColor = SpectatorCameraHelper.LineColorDefault;
			FrustumCenterLineColor = SpectatorCameraHelper.LineColorDefault;

#if UNITY_EDITOR
			DebugStartCamera = true;
			DebugRenderFrame = true;
			DebugFPS = SpectatorCameraHelper.DEBUG_RENDER_FPS_DEFAULT;
#endif
		}

		public void ExportSetting2JsonFile(in SpectatorCameraHelper.AttributeFileLocation attributeFileLocation)
		{
#if !UNITY_EDITOR
			if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.ResourceFolder)
			{
				Debug.LogError("It's not allowed to save setting to resource folder in runtime mode");
				return;
			}
#endif
			
			var data = new SpectatorCameraHelper.SpectatorCameraAttribute(
				CameraSourceRef,
				Vector3.zero, // HMD does not need to save the position
				Quaternion.identity, // HMD does not need to save the rotation
				LayerMask,
				IsSmoothCameraMovement,
				SmoothCameraMovementSpeed,
				IsFrustumShowed,
				VerticalFov,
				PanoramaResolution,
				PanoramaOutputFormat,
				PanoramaOutputType,
				FrustumLineCount,
				FrustumCenterLineCount,
				FrustumLineWidth,
				FrustumCenterLineWidth,
				FrustumLineColor,
				FrustumCenterLineColor);

#if UNITY_EDITOR
			if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.ResourceFolder)
			{
				SpectatorCameraHelper.SaveAttributeData2ResourcesFolder(
					SceneManager.GetActiveScene().name,
					gameObject.name,
					data);
			}
			else if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.PersistentFolder)
			{
				SpectatorCameraHelper.SaveAttributeData2PersistentFolder(
					SceneManager.GetActiveScene().name,
					gameObject.name,
					data);
			}
#else
			SpectatorCameraHelper.SaveAttributeData2PersistentFolder(
				SceneManager.GetActiveScene().name,
				gameObject.name,
				data);
#endif
		}

		public void LoadSettingFromJsonFile(in string jsonFilePath)
		{
			bool loadSuccess = SpectatorCameraHelper.LoadAttributeFileFromFolder(
				jsonFilePath,
				out SpectatorCameraHelper.SpectatorCameraAttribute data);
			if (loadSuccess)
			{
				ApplyData(data);
			}
			else
			{
				Debug.Log($"Load setting from {jsonFilePath} file to {gameObject.name} failed.");
			}
		}

		public void LoadSettingFromJsonFile(
			in string sceneName,
			in string hmdName,
			in SpectatorCameraHelper.AttributeFileLocation attributeFileLocation)
		{
			if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(hmdName))
			{
				Debug.LogError("Scene name or hmd name is null or empty");
				return;
			}

			var loadSuccess = false;
			SpectatorCameraHelper.SpectatorCameraAttribute data = new SpectatorCameraHelper.SpectatorCameraAttribute();
			if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.ResourceFolder)
			{
				loadSuccess = SpectatorCameraHelper.LoadAttributeFileFromResourcesFolder(
					sceneName,
					hmdName,
					out data);
			}
			else if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.PersistentFolder)
			{
				loadSuccess = SpectatorCameraHelper.LoadAttributeFileFromPersistentFolder(
					sceneName,
					hmdName,
					out data);
			}
			
			if (loadSuccess)
			{
				ApplyData(data);
			}
			else
			{
				var fileDirectory = string.Empty;
				if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.ResourceFolder)
				{
					fileDirectory = SpectatorCameraHelper.ResourcesFolderPath;
				}
				else if (attributeFileLocation is SpectatorCameraHelper.AttributeFileLocation.PersistentFolder)
				{
					fileDirectory = SpectatorCameraHelper.PersistentFolderPath;
				}
				var fileName = 
					SpectatorCameraHelper.GetSpectatorCameraAttributeFileNamePattern(sceneName, hmdName);
				
				Debug.Log(
					$"Load setting from {fileDirectory}/{fileName} file to {hmdName} failed.");
			}
		}

		public void ApplyData(in SpectatorCameraHelper.SpectatorCameraAttribute data)
		{
			CameraSourceRef = data.source;
			
			LayerMask = data.layerMask;
			IsSmoothCameraMovement = data.isSmoothCameraMovement;
			SmoothCameraMovementSpeed = data.smoothCameraMovementSpeed;
			IsFrustumShowed = data.isFrustumShowed;
			VerticalFov = data.verticalFov;
			PanoramaResolution = data.panoramaResolution;
			PanoramaOutputFormat = data.panoramaOutputFormat;
			PanoramaOutputType = data.panoramaOutputType;
			FrustumLineCount = data.frustumLineCount;
			FrustumCenterLineCount = data.frustumCenterLineCount;
			FrustumLineWidth = data.frustumLineWidth;
			FrustumCenterLineWidth = data.frustumCenterLineWidth;
			FrustumLineColor = data.frustumLineColor;
			FrustumCenterLineColor = data.frustumCenterLineColor;
		}
		
		#endregion

		#region Public functions of add/remove tracker 
		
		public void AddSpectatorCameraTracker(SpectatorCameraTracker tracker)
		{
			if (_spectatorCameraTrackerList is null)
			{
				_spectatorCameraTrackerList = new List<SpectatorCameraTracker>();
			}

			if (_spectatorCameraTrackerList.Contains(tracker))
			{
				return;
			}
			
			_spectatorCameraTrackerList.Add(tracker);
		}
		
		public void RemoveSpectatorCameraTracker(SpectatorCameraTracker tracker)
		{
			if (_spectatorCameraTrackerList is null)
			{
				return;
			}

			if (_spectatorCameraTrackerList.Contains(tracker))
			{
				_spectatorCameraTrackerList.Remove(tracker);
				
				if (FollowSpectatorCameraTracker == tracker)
				{
					if (_spectatorCameraTrackerList.Count > 0)
					{
						// If the tracker that is removed is the current tracker candidate and there are still
						// some trackers in the list, change the current tracker candidate to the first tracker
						// in the list
						FollowSpectatorCameraTracker = _spectatorCameraTrackerList[0];
					}
					else
					{
						// If the tracker that is removed is the current tracker candidate and there is no
						// tracker in the list, change the camera source to HMD and set the current tracker
						// candidate to null
						CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
						FollowSpectatorCameraTracker = null;
					}
				}
			}
		}
		
		#endregion

		#region Public function of 360 photo capture

		/// <summary>
		/// Capture the 360 photo at the current spectator camera position and rotation.
		/// There are two types of panorama type: <b>Monoscopic</b> and <b>Stereoscopic</b>
		/// can be chosen.
		/// </summary>
		public void CaptureSpectatorCamera360Photo()
		{
			if (WaveXRSettings.GetInstance().allowSpectatorCameraCapture360Image is false)
			{
				Debug.LogError(
					"The spectator camera capture 360 image feature is not enabled, pls enable it on the" +
					"WaveXR SDK page in Unity editor project setting first.");
				return;
			}

			// If the spectator handle is not set, return
			if (SpectatorHandler is null)
			{
				Debug.LogError(
					"Cannot init the function for capturing 360, pls make sure the spectator handler is init.");
				return;
			}

			#region Create a new camera component for capture 360 photo

			Transform refTransform;
			LayerMask layerMaskValue;
			SpectatorCameraHelper.SpectatorCameraPanoramaResolution panoramaResolutionValue;
			TextureProcessHelper.PictureOutputFormat panoramaOutputFormatValue;
			TextureProcessHelper.PanoramaType panoramaOutputTypeValue;
			
			// Create a new GameObject which position is according to the CameraSourceRef.
			// If CameraSourceRef = HMD, refer the transform from camera main (hmd),
			// otherwise, refer the transform from tracker.
			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if (IsMainCameraExist())
					{
						refTransform = MainCamera.transform;
						layerMaskValue = LayerMask;
						panoramaResolutionValue = PanoramaResolution;
						panoramaOutputFormatValue = PanoramaOutputFormat;
						panoramaOutputTypeValue = PanoramaOutputType;
					}
					else
					{
						Debug.LogWarning("Cannot find the main camera in the scene to capture 360 photo.");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}
					
					refTransform = FollowSpectatorCameraTracker.transform;
					layerMaskValue = FollowSpectatorCameraTracker.LayerMask;
					panoramaResolutionValue = FollowSpectatorCameraTracker.PanoramaResolution;
					panoramaOutputFormatValue = FollowSpectatorCameraTracker.PanoramaOutputFormat;
					panoramaOutputTypeValue = FollowSpectatorCameraTracker.PanoramaOutputType;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			// Rotation ignore but only Y-axis (yaw).
			var spectatorCamera360 = new GameObject
			{
				transform =
				{
					position = refTransform.position
				}
			}.AddComponent<Camera>();

			// Set the spectatorCamera360's stereo target eye according to the panorama type
			spectatorCamera360.stereoTargetEye = panoramaOutputTypeValue is TextureProcessHelper.PanoramaType.Stereoscopic
				? StereoTargetEyeMask.Both
				: StereoTargetEyeMask.None;
			// Set the spectatorCamera360's culling mask
			spectatorCamera360.cullingMask = layerMaskValue;
			// Set the spectatorCamera360's eye distance
			spectatorCamera360.stereoSeparation = SpectatorCameraHelper.STEREO_SEPARATION_DEFAULT;

			#endregion

			RenderTexture capture360ResultEquirect = TextureProcessHelper.Capture360RenderTexture(
				spectatorCamera360,
				(int)panoramaResolutionValue,
				panoramaOutputTypeValue);

			// Destroy the gameObject
			Destroy(spectatorCamera360.gameObject);

			if (capture360ResultEquirect is null)
			{
				Debug.LogWarning(
					"Capture360RenderTexture return null, pls check the error log on the above for more details.");
				return;
			}

			var filename = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

			try
			{
				TextureProcessHelper.SaveRenderTextureToDisk(
					imageAlbumName: SpectatorCameraHelper.SAVE_PHOTO360_ALBUM_NAME
					, imageNameWithoutFileExtension: filename
					, imageOutputFormat: panoramaOutputFormatValue
					, sourceRenderTexture: capture360ResultEquirect
					, yawRotation: refTransform.rotation.eulerAngles.y
#if !UNITY_ANDROID || UNITY_EDITOR
					, saveDirectory: Application.persistentDataPath
#endif
				);
			}
			catch (Exception e)
			{
				Debug.LogError($"Error on output the panoramic photo: {e}.");
			}
			finally
			{
				// Release the Temporary RenderTexture
				RenderTexture.ReleaseTemporary(capture360ResultEquirect);
			}
		}

		#endregion

		#region Public functions of safety check
		
		public bool IsCameraSourceAsHmd()
		{
			return CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Hmd;
		}
		
		public bool IsCameraSourceAsTracker()
		{
			return CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Tracker;
		}
		
		public bool IsFollowTrackerEqualTo(SpectatorCameraTracker tracker)
		{
			return FollowSpectatorCameraTracker == tracker;
		}
		
		public bool IsSpectatorCameraHandlerExist()
		{
			if (SpectatorHandler is null)
			{
				SpectatorHandler = WaveXRSpectatorCameraHandle.GetInstance();
			}

			return !(SpectatorHandler is null);
		}
		
		public bool IsMainCameraExist()
		{
			if (MainCamera is null)
			{
				MainCamera = Camera.main;
			}
			
			return !(MainCamera is null);
		}
		
		public bool IsFollowTrackerExist()
		{
			return !(FollowSpectatorCameraTracker is null);
		}
		
		#endregion
		
		#region Functions of visualization camera view ray

		/// <summary>
		/// Setup the frustum line
		/// </summary>
		public void SetupFrustumLine()
		{
			bool isFrustumShowedValue;
			SpectatorCameraHelper.FrustumLineCount frustumLineCountValue;
			float frustumLineWidthValue;
			Color frustumLineColorValue;

			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if (IsMainCameraExist())
					{
						isFrustumShowedValue = IsFrustumShowed;
						frustumLineCountValue = FrustumLineCount;
						frustumLineWidthValue = FrustumLineWidth;
						frustumLineColorValue = FrustumLineColor;
					}
					else
					{
						Debug.LogWarning("Main camera does not exist in the scene");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}
					
					isFrustumShowedValue = FollowSpectatorCameraTracker.IsFrustumShowed;
					frustumLineCountValue = FollowSpectatorCameraTracker.FrustumLineCount;
					frustumLineWidthValue = FollowSpectatorCameraTracker.FrustumLineWidth;
					frustumLineColorValue = FollowSpectatorCameraTracker.FrustumLineColor;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (
#if UNITY_EDITOR
				SpectatorHandler.debugStartCamera is false ||
#else
				IsRecording is false ||
#endif
				isFrustumShowedValue is false ||
			    frustumLineCountValue is SpectatorCameraHelper.FrustumLineCount.None)
			{
				if (!(FrustumLineRoot is null))
				{
					FrustumLineRoot.SetActive(false);
				}

				return;
			}

			if (FrustumLineRoot is null)
			{
				FrustumLineRoot = new GameObject(SpectatorCameraHelper.FRUSTUM_LINE_ROOT_NAME_DEFAULT);
				FrustumLineRoot.transform.SetParent(SpectatorCamera.transform, false);
			}

			FrustumLineRoot.SetActive(true);

			if (!(_frustumLineList is null) && _frustumLineList.Count > 0)
			{
				// Destroy all the line renderer and then re-init
				// in order to make sure that using new variables
				// e.g. line count, width and color
				foreach (LineRenderer item in _frustumLineList)
				{
					Destroy(item.gameObject);
				}

				_frustumLineList.Clear();
			}

			SetupLineRenderer(
				lineCount: (int)frustumLineCountValue,
				lineWidth: frustumLineWidthValue,
				lineNamePrefix: SpectatorCameraHelper.FRUSTUM_LINE_NAME_PREFIX_DEFAULT,
				lineMaterial: new Material(Shader.Find(SpectatorCameraHelper.LINE_SHADER_NAME_DEFAULT))
				{
					color = frustumLineColorValue
				},
				lineParent: FrustumLineRoot.transform,
				lineList: out _frustumLineList);
		}

		/// <summary>
		/// Setup the frustum center line
		/// </summary>
		public void SetupFrustumCenterLine()
		{
			bool isFrustumShowedValue;
			SpectatorCameraHelper.FrustumCenterLineCount frustumCenterLineCountValue;
			float frustumCenterLineWidthValue;
			Color frustumCenterLineColorValue;
			
			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if (IsMainCameraExist())
					{
						isFrustumShowedValue = IsFrustumShowed;
						frustumCenterLineCountValue = FrustumCenterLineCount;
						frustumCenterLineWidthValue = FrustumCenterLineWidth;
						frustumCenterLineColorValue = FrustumCenterLineColor;
					}
					else
					{
						Debug.LogWarning("Main camera does not exist in the scene");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}

					isFrustumShowedValue = FollowSpectatorCameraTracker.IsFrustumShowed;
					frustumCenterLineCountValue = FollowSpectatorCameraTracker.FrustumCenterLineCount;
					frustumCenterLineWidthValue = FollowSpectatorCameraTracker.FrustumCenterLineWidth;
					frustumCenterLineColorValue = FollowSpectatorCameraTracker.FrustumCenterLineColor;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
				
			if (
#if UNITY_EDITOR
			SpectatorHandler.debugStartCamera is false ||
#else
			IsRecording is false ||
#endif
			isFrustumShowedValue is false ||
			frustumCenterLineCountValue is SpectatorCameraHelper.FrustumCenterLineCount.None)
			{
				if (!(FrustumCenterLineRoot is null))
				{
					FrustumCenterLineRoot.SetActive(false);
				}

				return;
			}

			if (FrustumCenterLineRoot is null)
			{
				FrustumCenterLineRoot = new GameObject(SpectatorCameraHelper.FRUSTUM_CENTER_LINE_ROOT_NAME_DEFAULT);
				FrustumCenterLineRoot.transform.SetParent(SpectatorCamera.transform, false);
			}

			FrustumCenterLineRoot.SetActive(true);

			if (!(_frustumCenterLineList is null) && _frustumCenterLineList.Count > 0)
			{
				// Destroy all the line renderer and then re-init
				// in order to make sure that using new variables
				// e.g. line count, width and color
				foreach (LineRenderer item in _frustumCenterLineList)
				{
					Destroy(item.gameObject);
				}

				_frustumCenterLineList.Clear();
			}

			SetupLineRenderer(
				lineCount: (int)frustumCenterLineCountValue,
				lineWidth: frustumCenterLineWidthValue,
				lineNamePrefix: SpectatorCameraHelper.FRUSTUM_CENTER_LINE_NAME_PREFIX_DEFAULT,
				lineMaterial: new Material(Shader.Find(SpectatorCameraHelper.LINE_SHADER_NAME_DEFAULT))
				{
					color = frustumCenterLineColorValue
				},
				lineParent: FrustumCenterLineRoot.transform,
				lineList: out _frustumCenterLineList);
		}

		/// <summary>
		/// Setup the line renderer in order to render the frustum and frustum center line
		/// </summary>
		/// <param name="lineCount">The total number of the LineRenderer</param>
		/// <param name="lineWidth">The width of the line</param>
		/// <param name="lineNamePrefix">The GameObject name prefix. It attaches the LineRenderer component</param>
		/// <param name="lineMaterial">The material of the line</param>
		/// <param name="lineParent">The parent GameObject of all GameObjects that include the LineRenderer component</param>
		/// <param name="lineList">The return value, which is the list of the Line Renderer and all of it is already initiated with the input parameter</param>
		private void SetupLineRenderer(
			in int lineCount,
			in float lineWidth,
			in string lineNamePrefix,
			in Material lineMaterial,
			in Transform lineParent,
			out List<LineRenderer> lineList)
		{
			lineList = new List<LineRenderer>(lineCount);

			for (int i = 0; i < lineCount; i++)
			{
				var obj = new GameObject($"{lineNamePrefix}{i}");
				obj.transform.SetParent(lineParent, false);
				// Set to "UI" layer
				obj.layer = LayerMask.NameToLayer("UI");

				var lr = obj.AddComponent<LineRenderer>();
				lr.useWorldSpace = false;
				lr.sharedMaterial = lineMaterial;
				lr.startWidth = lineWidth;
				lr.endWidth = lineWidth;
				lr.alignment = LineAlignment.View;

				lineList.Add(lr);
			}

			CalculateLineRendererPosition(lineList);
		}

		/// <summary>
		/// Calculate the line renderer position
		/// </summary>
		/// <param name="lineList">The list that saves all LineRenderer</param>
		private void CalculateLineRendererPosition(List<LineRenderer> lineList)
		{
			if (SpectatorHandler is null)
			{
				return;
			}
			
			var frustumCornersVector = new Vector3[SpectatorCameraHelper.FRUSTUM_OUT_CORNERS_COUNT];

			Camera spectatorHandlerInternalCamera;
			float retryTime = 0;
			while (true)
			{
				spectatorHandlerInternalCamera = SpectatorHandler.GetCamera();
				if (!(spectatorHandlerInternalCamera is null))
				{
					break;
				}
				
				retryTime += Time.unscaledDeltaTime;
				if (retryTime > SpectatorCameraHelper.MAX_SECOND_GET_INTERNAL_SPECTATOR_CAMERA)
				{
					Debug.LogWarning(
						"Draw frustum: cannot get the internal camera from the spectator handler in " +
						$"{SpectatorCameraHelper.MAX_SECOND_GET_INTERNAL_SPECTATOR_CAMERA} seconds.");
					return;
				}
			}

			spectatorHandlerInternalCamera.CalculateFrustumCorners(
				new Rect(0, 0, 1, 1),
				spectatorHandlerInternalCamera.farClipPlane,
				Camera.MonoOrStereoscopicEye.Mono,
				frustumCornersVector);

			// Debug.Log("Get FOV and then destroy the camera");
			// Destroy(spectatorCameraBuffer.gameObject);

			int setLineStep = lineList.Count / SpectatorCameraHelper.FRUSTUM_OUT_CORNERS_COUNT;
			for (var currentCorner = 0;
			     currentCorner < SpectatorCameraHelper.FRUSTUM_OUT_CORNERS_COUNT;
			     currentCorner++)
			{
				for (var currentLineStep = 0; currentLineStep < setLineStep; currentLineStep++)
				{
					Vector3 currentVector = Vector3.Lerp(
						frustumCornersVector[currentCorner],
						currentCorner < SpectatorCameraHelper.FRUSTUM_OUT_CORNERS_COUNT - 1
							// If currentCorner is not the last one, draw the line between current corner and next corner.
							? frustumCornersVector[currentCorner + 1]
							// Otherwise, draw the line between the last corner and the first corner.
							: frustumCornersVector[0],
						currentLineStep / (float)setLineStep);

					SetLineRendererPosition(
						lineList[currentCorner * setLineStep + currentLineStep],
						currentVector,
						SpectatorCameraHelper.FRUSTUM_LINE_BEGIN_DEFAULT);
				}
			}
		}

		/// <summary>
		/// Set the line renderer position
		/// </summary>
		/// <param name="lineRenderer">The LineRenderer that will set to</param>
		/// <param name="endPoint">The end position of the line</param>
		/// <param name="startOffset">The offset of the start position of the line</param>
		private static void SetLineRendererPosition(LineRenderer lineRenderer, Vector3 endPoint, float startOffset = 0)
		{
			lineRenderer.SetPosition(0, startOffset < SpectatorCameraHelper.COMPARE_FLOAT_SMALL_THRESHOLD
				? Vector3.zero
				: (endPoint).normalized * startOffset);
			lineRenderer.SetPosition(1, endPoint);
		}

		#endregion
		
		#region Private functions of initialization of the camera

		/// <summary>
		/// Init the camera pose and prefab
		/// </summary>
		private void InitCameraPoseAndPrefab()
		{
			if (SpectatorHandler is null)
			{
				return;
			}

			Vector3 position;
			Quaternion rotation;
			
			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if (IsMainCameraExist())
					{
						Transform mainCameraTransform = MainCamera.transform;
						position = mainCameraTransform.position;
						rotation = mainCameraTransform.rotation;
					}
					else
					{
						Debug.LogWarning("Main camera does not exist in the scene");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}

					Transform followTrackerTransform = FollowSpectatorCameraTracker.transform;
					position = followTrackerTransform.position;
					rotation = followTrackerTransform.rotation;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			SpectatorCameraCallback(true);
			CreateCameraPrefab();
			
			// Init camera prefab position
			SpectatorCamera.transform.position = position;
			// Init camera prefab rotation
			SpectatorCamera.transform.rotation = rotation;
			// Init camera handler position and rotation
			SpectatorHandler.SetFixedPose(position, rotation);
		}
		
		/// <summary>
		/// Create the spectator camera (prefab)
		/// </summary>
		private void CreateCameraPrefab()
		{
			if (!(SpectatorCamera is null))
			{
				Destroy(SpectatorCamera);
			}

			if (!(SpectatorCameraPrefab is null))
			{
				SpectatorCamera = Instantiate(SpectatorCameraPrefab);
			}
			else
			{
				SpectatorCamera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				SpectatorCamera.transform.localScale = 
					SpectatorCameraHelper.SpectatorCameraSpherePrefabScaleDefault;
			}

			DontDestroyOnLoad(SpectatorCamera);
		}
		
		private void CameraStateChangingProcessing()
		{
			if (SpectatorHandler is null)
			{
				return;
			}
			
			// Update layer mask
			// Update vertical fov
			// Update last position and rotation (avoid camera pose interpolation between two different camera source)
			// Update is frustum showed

			LayerMask layerMaskValue;
			float verticalFovValue;
			Transform changeCameraTransform;

			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if (IsMainCameraExist())
					{
						layerMaskValue = LayerMask;
						verticalFovValue = VerticalFov;
						changeCameraTransform = MainCamera.transform;
					}
					else
					{
						Debug.LogWarning("Main camera does not exist in the scene");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}

					layerMaskValue = FollowSpectatorCameraTracker.LayerMask;
					verticalFovValue = FollowSpectatorCameraTracker.VerticalFov;
					changeCameraTransform = FollowSpectatorCameraTracker.transform;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			CameraLastPosition = changeCameraTransform.position;
			CameraLastRotation = changeCameraTransform.rotation;
			SpectatorHandler.SetCullingMask(layerMaskValue);
			SpectatorHandler.SetFixedFOV(verticalFovValue);
			SetupFrustumLine();
			SetupFrustumCenterLine();
		}

		private IEnumerator WaitForSecondsToInitFrustum(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			SetupFrustumLine();
			SetupFrustumCenterLine();
		}

		#endregion

		#region Private callback functions of the camera

		/// <summary>
		/// Spectator camera call back function entry point
		/// </summary>
		/// <param name="init"></param>
		private void SpectatorCameraCallback(bool init)
		{
			Debug.Log("SpectatorCameraCallback");

			if (SpectatorHandler is null)
			{
				return;
			}

			if (init)
			{
				SpectatorHandler.OnSpectatorStart += OnSpectatorStart;
				SpectatorHandler.OnSpectatorStop += OnSpectatorStop;
			}
			else
			{
				SpectatorHandler.OnSpectatorStart -= OnSpectatorStart;
				SpectatorHandler.OnSpectatorStop -= OnSpectatorStop;
			}
		}

		/// <summary>
		/// Call back function when spectator start
		/// </summary>
		private void OnSpectatorStart()
		{
			Debug.Log("OnSpectatorStart");

			IsRecording = true;

			StartCoroutine(WaitForSecondsToInitFrustum(
				SpectatorCameraHelper.INTERVAL_SECOND_INTERNAL_SPECTATOR_CAMERA));
		}

		/// <summary>
		/// Call back function when spectator stop
		/// </summary>
		private void OnSpectatorStop()
		{
			Debug.Log("OnSpectatorStop");

			IsRecording = false;

			SetupFrustumLine();
			SetupFrustumCenterLine();
		}

		#endregion
		
		#region Unity lifecycle event functions

		private void Awake()
		{
			if (!(_instance is null) && _instance != this)
			{
				Destroy(this);
			}
			else
			{
				_instance = this;
				
				// Check this gameObject have parent or not, if so, set it to no game parent.
				if (!(transform.parent is null))
				{
					transform.SetParent(null);
				}
			
				DontDestroyOnLoad(_instance.gameObject);
			}
		}

		private void OnEnable()
		{
			SpectatorCameraCallback(true);
		}

		private IEnumerator Start()
		{
			#region Check the spectator camera feature is enabled or not
			
			var waveXRSettings = WaveXRSettings.GetInstance();
			if (!(waveXRSettings is null))
			{
				if (waveXRSettings.allowSpectatorCamera is false)
				{
					Debug.LogError(
						"You need to open the spectator camera feature on the WaveXR SDK page in Unity editor project setting first.");
					yield break;
				}
			}
			else
			{
				Debug.LogError("SpectatorCameraManager.Start: WaveXRSettings is null");
				yield break;
			}
			
			#endregion

			if (!(SpectatorHandler is null))
			{
				yield break;
			}

			var findSpectatorHandlerSecond = 0f;
			while (SpectatorHandler is null)
			{
				if (findSpectatorHandlerSecond >= SpectatorCameraHelper.MAX_SECOND_GET_SPECTATOR_HANDLER)
				{
					Debug.LogWarning(
						"Cannot get the spectator handler from class \"WaveXRSpectatorCameraHandle\" " +
						$"in {SpectatorCameraHelper.MAX_SECOND_GET_SPECTATOR_HANDLER} seconds. Please reopen" +
						$"tha app and try again.");
					Destroy(this);
					yield break;
				}

				yield return new WaitForSeconds(SpectatorCameraHelper.INTERVAL_SECOND_GET_SPECTATOR_HANDLER);
				SpectatorHandler = WaveXRSpectatorCameraHandle.GetInstance();
				findSpectatorHandlerSecond += Time.unscaledDeltaTime;
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void Update()
		{
			if (SpectatorHandler is null)
			{
				SpectatorHandler = WaveXRSpectatorCameraHandle.GetInstance();
				return;
			}
			
			Vector3 position;
			Quaternion rotation;
			bool isSmoothCameraMovementValue;
			float smoothCameraMovementSpeedValue;
			
			switch (CameraSourceRef)
			{
				case SpectatorCameraHelper.CameraSourceRef.Hmd:
				{
					if(IsMainCameraExist())
					{
						Transform mainCameraTransform = MainCamera.transform;
						position = mainCameraTransform.position;
						rotation = mainCameraTransform.rotation;
						isSmoothCameraMovementValue = IsSmoothCameraMovement;
						smoothCameraMovementSpeedValue = SmoothCameraMovementSpeed;
					}
					else
					{
						Debug.LogWarning("Main camera does not exist in the scene");
						return;
					}
				}
					break;
				case SpectatorCameraHelper.CameraSourceRef.Tracker:
				{
					if (IsFollowTrackerExist() is false)
					{
						if (SpectatorCameraTrackerList.Count > 0)
						{
							// If there is no tracker assign to the FollowSpectatorCameraTracker and there are
							// some trackers in the scene, change to use the first tracker in the list as default
							FollowSpectatorCameraTracker = SpectatorCameraTrackerList[0];
						}
						else
						{
							// If there is no tracker in the scene, change to use the HMD as default
							CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
							return;
						}
					}

					isSmoothCameraMovementValue = FollowSpectatorCameraTracker.IsSmoothCameraMovement;
					smoothCameraMovementSpeedValue = FollowSpectatorCameraTracker.SmoothCameraMovementSpeed;

					Transform trackerTransform = FollowSpectatorCameraTracker.transform;
					position = trackerTransform.position;
					rotation = trackerTransform.rotation;
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (isSmoothCameraMovementValue)
			{
				position = Vector3.Lerp(CameraLastPosition, position, Time.deltaTime * smoothCameraMovementSpeedValue);
				
				// To avoid problem:
				// https://discussions.unity.com/t/error-compareapproximately-ascalar-0-0f-with-quaternion-lerp/161461/2
				float t = Mathf.Clamp(Time.deltaTime * smoothCameraMovementSpeedValue, 0f, 1 - Mathf.Epsilon);
				rotation = Quaternion.Lerp(CameraLastRotation, rotation, t);
			}
			
			CameraLastPosition = position;
			CameraLastRotation = rotation;
			
			// Set camera handler position and rotation
			SpectatorHandler.SetFixedPose(position, rotation);
			
			// Set camera prefab position and rotation
			SpectatorCamera.transform.position = position;
			SpectatorCamera.transform.rotation = rotation;
		}
		
		private void OnDisable()
		{
			if (!(SpectatorHandler is null))
			{
				SpectatorHandler.ClearFixedPose();
			}
			SpectatorCameraCallback(false);
		}
		
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			Debug.Log($"OnSceneLoaded: {scene.name}");
			
			CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
			MainCamera = null;
			FollowSpectatorCameraTracker = null;
			_spectatorCameraTrackerList?.Clear();
		}
		
		private void OnDestroy()
		{
			Destroy(SpectatorCamera);
			_instance = null;
			
			if (!(SpectatorHandler is null))
			{
				SpectatorHandler.ClearFixedPose();
			}
			SpectatorCameraCallback(false);
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
		
		#endregion
	}
}
