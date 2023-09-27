// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Wave.Native;
using System.Runtime.InteropServices;
using System.Collections;
using Wave.XR.Settings;

namespace Wave.Essence.CompositorLayer
{
	public class CompositorLayer : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField]
		public bool isPreviewingCylinder = false;
		[SerializeField]
		public bool isPreviewingQuad = false;
		[SerializeField]
		public GameObject generatedPreview = null;

		public const string CylinderPreviewName = "CylinderPreview";
		public const string QuadPreviewName = "QuadPreview";
#endif
		public const string CylinderUnderlayMeshName = "Underlay Alpha Mesh (Cylinder)";
		public const string QuadUnderlayMeshName = "Underlay Alpha Mesh (Quad)";
		public const string FallbackMeshName = "FallbackMeshGO";
		public const string CaptureMeshName = "CompositorLayerCaptureGO";

		[Tooltip("Specify Compositor Layer Type.")]
		[SerializeField]
		public LayerType layerType = LayerType.Overlay;

		[Tooltip("Specify Layer Composition Depth.")]
		[SerializeField]
		public uint compositionDepth = 0;

		[SerializeField]
		public bool useCustomUnderlayAlphaShaderPath = false;

		[SerializeField]
		public string customUnderlayAlphaShaderPath = string.Empty;

		[SerializeField]
		public bool useCustomTextureBlitShaderPath = false;

		[SerializeField]
		public string customTextureBlitShaderPath = string.Empty;

		[Tooltip("Specify Layer Shape.")]
		[SerializeField]
		public LayerShape layerShape = LayerShape.Quad;

		[Tooltip("Width of a Quad Layer")]
		[SerializeField]
		[Min(0.001f)]
		private float m_QuadWidth = 1f;
		public float quadWidth { get { return m_QuadWidth; } }
#if UNITY_EDITOR
		public float QuadWidth { get { return m_QuadWidth; } set { m_QuadWidth = value; } }
#endif

		[Tooltip("Height of a Quad Layer")]
		[SerializeField]
		[Min(0.001f)]
		private float m_QuadHeight = 1f;
		public float quadHeight { get { return m_QuadHeight; } }
#if UNITY_EDITOR
		public float QuadHeight { get { return m_QuadHeight; } set { m_QuadHeight = value; } }
#endif

		[Tooltip("Height of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderHeight = 1f;
		public float cylinderHeight { get { return m_CylinderHeight; }}
#if UNITY_EDITOR
		public float CylinderHeight { get { return m_CylinderHeight; } set { m_CylinderHeight = value; } }
#endif

		[Tooltip("Arc Length of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderArcLength = 1f;
		public float cylinderArcLength { get { return m_CylinderArcLength; } }
#if UNITY_EDITOR
		public float CylinderArcLength { get { return m_CylinderArcLength; } set { m_CylinderArcLength = value; } }
#endif

		[Tooltip("Radius of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		private float m_CylinderRadius = 1f;
		public float cylinderRadius { get { return m_CylinderRadius; } }
#if UNITY_EDITOR
		public float CylinderRadius { get { return m_CylinderRadius; } set { m_CylinderRadius = value; } }
#endif

		[Tooltip("Central angle of arc of Cylinder Layer")]
		[SerializeField]
		[Range(5f, 180f)]
		private float m_CylinderAngleOfArc = 180f;
		public float cylinderAngleOfArc { get { return m_CylinderAngleOfArc; } }
#if UNITY_EDITOR
		public float CylinderAngleOfArc { get { return m_CylinderAngleOfArc; } set { m_CylinderAngleOfArc = value; } }
#endif

#if UNITY_EDITOR
		[Tooltip("Cylinder Layer parameter to be locked when changing the radius.")]
		[SerializeField]
		public CylinderLayerParamLockMode lockMode = CylinderLayerParamLockMode.ArcLength;
#endif

		[Tooltip("Specify whether Layer needs to be updated each frame or not.")]
		[SerializeField]
		public bool isDynamicLayer = false;

		[Tooltip("Specify whether the layer is hooked to an external surface or not. Textures assigned to the editor will be ignored when this option is selected.")]
		[SerializeField]
		public bool isExternalSurface = false;

		[Tooltip("Width of external surface in pixels.")]
		[SerializeField]
		public uint externalSurfaceWidth = 1280;

		[Tooltip("Height of external surface in pixels.")]
		[SerializeField]
		public uint externalSurfaceHeight = 720;

		[Tooltip("")]
		[SerializeField]
		public bool isProtectedSurface = false;

		[Tooltip("Specify source textures if textures are from Asset.")]
		[SerializeField]
		public Texture[] textures = new Texture[] { null, null };

		public GameObject generatedUnderlayMesh = null;
		public MeshRenderer generatedUnderlayMeshRenderer = null;
		public MeshFilter generatedUnderlayMeshFilter = null;

		[Tooltip("When Auto Fallback is enabled, layers with a higher render priority will be rendered as normal layers first.")]
		[SerializeField]
		private uint renderPriority = 0;
		public uint GetRenderPriority() { return renderPriority; }
		public void SetRenderPriority(uint newRenderPriority)
		{
			renderPriority = newRenderPriority;
			isRenderPriorityChanged = true;
			CompositorLayerManager.GetInstance().SubscribeToLayerManager(this);
		}
		public bool isRenderPriorityChanged
		{
			get; private set;
		}

		private GameObject generatedFallbackMesh = null;
		private MeshRenderer generatedFallbackMeshRenderer = null;
		private MeshFilter generatedFallbackMeshFilter = null;

		private GameObject generatedCaptureMesh = null;
		private MeshRenderer generatedCaptureMeshRenderer = null;
		private MeshFilter generatedCaptureMeshFilter = null;

		private WVR_TextureTarget textureTarget = WVR_TextureTarget.WVR_TextureTarget_2D;
		private WVR_TextureFormat textureFormat = WVR_TextureFormat.WVR_TextureFormat_RGBA;
		private WVR_TextureType textureType = WVR_TextureType.WVR_TextureType_UnsignedByte;
		private WVR_TextureOptions textureOptions = WVR_TextureOptions.WVR_TextureOption_None;

		public LayerTextureQueue[] textureQueues { get; private set; }
		private WVR_PoseState_t poseState;
		private WVR_Pose_t layerPoseTransform;
		private WVR_Vector3f_t layerScale;
		private Material texture2DBlitMaterial;

		private IntPtr poseStatePtr;
		private IntPtr layerPoseTransformPtr;
		private IntPtr layerScalePtr;

		private GameObject CompositorLayerPlaceholderPrefabInstance = null;

#pragma warning disable
		private int frameIndex = -1;
#pragma warning restore

		public readonly float angleOfArcLowerLimit = 5f;
		public readonly float angleOfArcUpperLimit = 180f;

		private LayerShape previousLayerShape = LayerShape.Quad;
		public float previousQuadWidth = 1f;
		public float previousQuadHeight = 1f;
		public float previousCylinderHeight = 1f;
		public float previousCylinderArcLength = 1f;
		public float previousCylinderRadius = 1f;
		public float previousAngleOfArc = 180f;
		public Texture[] previousTextures = new Texture[] { null, null };

		private bool[] LRInitStatus = new bool[] { false, false };
		private bool isInitializationComplete = false;
		private bool reinitializationNeeded = false;
		private bool isOnBeforeRenderSubscribed = false, isOnCameraPostRenderSubscribed = false, isOnCameraPreCullSubscribed = false;
		private bool isLayerReadyForSubmit = false;
		private bool isLinear = false;
		private bool isAutoFallbackActive = false;
		private bool placeholderGenerated = false;
		private static bool isSynchronized = false;
		private static RenderThreadSynchronizer synchronizer;
		private Camera hmd;

		private const string LOG_TAG = "Wave_CompositorLayer";

		private static void DEBUG(string log)
		{
#if UNITY_ANDROID
			Log.d(LOG_TAG, log);

#elif UNITY_EDITOR
			Debug.Log(LOG_TAG + ": DEBUG " + log);

#endif
		}

		private static void ERROR(string log)
		{
#if UNITY_ANDROID
			Log.e(LOG_TAG, log);

#elif UNITY_EDITOR
			Debug.Log(LOG_TAG + ": ERROR " + log);

#endif
		}

		#region Compositor Layer Lifecycle

		IntPtr externalAndroidSurfaceObj = IntPtr.Zero;
		WVR_TextureParams_t[] externalAndroidSurfaceTextureParams;
		private WVR_TextureLayout_t externalSurfaceTextureLayout { get; set; }

		private bool CompositorLayerInit()
		{
			if (isInitializationComplete)
			{
				//DEBUG("CompositorLayerInit: Already initialized.");
				return true;
			}

			if (isExternalSurface)
			{
				CompositorLayerRenderThreadSyncObject SetupExternalAndroidSurfaceSyncObjects = new CompositorLayerRenderThreadSyncObject(
					(taskQueue) =>
					{
						lock (taskQueue)
						{
							CompositorLayerRenderThreadTask task = (CompositorLayerRenderThreadTask)taskQueue.Dequeue();

							externalAndroidSurfaceTextureParams = new WVR_TextureParams_t[1];
							externalAndroidSurfaceObj = Interop.WVR_CreateAndroidSurface((int)externalSurfaceWidth, (int)externalSurfaceHeight, isProtectedSurface, externalAndroidSurfaceTextureParams);
							if (externalAndroidSurfaceObj != IntPtr.Zero)
							{
								LRInitStatus[0] = true;
								LRInitStatus[1] = true;
								DEBUG("SetupExternalAndroidSurface handle: " + externalAndroidSurfaceObj.ToString() + ", id: " + externalAndroidSurfaceTextureParams[0].id.ToString());
							}
							else
							{
								DEBUG("externalAndroidSurfaceObj handle not received.");
							}

							taskQueue.Release(task);
						}
					});

				CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(SetupExternalAndroidSurfaceSyncObjects);

				if (textures == null)
				{
					textures = new Texture[2];
				}

				textures[0] = new Texture2D((int)externalSurfaceWidth, (int)externalSurfaceHeight, TextureFormat.RGBA32, false, isLinear);
				textures[1] = new Texture2D((int)externalSurfaceWidth, (int)externalSurfaceHeight, TextureFormat.RGBA32, false, isLinear);

				//Allocate memory to pointers
				poseState = default(WVR_PoseState_t);
				layerPoseTransform = default(WVR_Pose_t);
				layerScale = default(WVR_Vector3f_t);

				poseStatePtr = Marshal.AllocHGlobal(Marshal.SizeOf(poseState));
				layerPoseTransformPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerPoseTransform));
				layerScalePtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerScale));

				DEBUG("CompositorLayerInit Ext Surf");

				return true;
			}
			else
			{
				if (textures == null || textures[0] == null)
				{
					ERROR("CompositorLayerInit: Source Texture not found, abort init.");
					return false;
				}

				if (textures[0] != null && textures[1] == null)
				{
					DEBUG("CompositorLayerInit: Using Left Texture as Right Texture.");
					textures[1] = textures[0];
				}
			}

			if (textureQueues == null)
			{
				DEBUG("CompositorLayerInit: Create new TextureQueues.");
				textureQueues = new LayerTextureQueue[2];
			}

			DEBUG("CompositorLayerInit");

			IntPtr[] LayerTextureHandlers = new IntPtr[2];
			CompositorLayerRenderThreadSyncObject[] ObtainLayerTextureQueueSyncObjects = new CompositorLayerRenderThreadSyncObject[2];

			foreach (Eye eye in Enum.GetValues(typeof(Eye)))
			{
				int eyeIndex = (int)eye;

				uint textureWidth = (uint)textures[eyeIndex].width;
				uint textureHeight = (uint)textures[eyeIndex].height;

				ObtainLayerTextureQueueSyncObjects[eyeIndex] = new CompositorLayerRenderThreadSyncObject(
					(taskQueue) =>
					{
						lock (taskQueue)
						{
							CompositorLayerRenderThreadTask task = (CompositorLayerRenderThreadTask)taskQueue.Dequeue();

							LayerTextureHandlers[eyeIndex] = Interop.WVR_ObtainTextureQueue(textureTarget, textureFormat, textureType, textureWidth, textureHeight, 0);

							uint newTextureQueueLength = Interop.WVR_GetTextureQueueLength(LayerTextureHandlers[eyeIndex]);

							textureQueues[eyeIndex] = new LayerTextureQueue(LayerTextureHandlers[eyeIndex], eye, newTextureQueueLength, textureWidth, textureHeight);
							//DEBUG(eye.ToString() + " eye of layer setup Success. Queue Length: " + newTextureQueueLength);

							LRInitStatus[eyeIndex] = true;
							//DEBUG(eye.ToString() + " LRInitStatus " + LRInitStatus[eyeIndex].ToString());

							taskQueue.Release(task);
						}
					});

				CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(ObtainLayerTextureQueueSyncObjects[eyeIndex]);

				previousLayerShape = layerShape;
				previousQuadWidth = m_QuadWidth;
				previousQuadHeight = m_QuadHeight;
				previousCylinderHeight = m_CylinderHeight;
				previousCylinderArcLength = m_CylinderArcLength;
				previousCylinderRadius = m_CylinderRadius;
				previousAngleOfArc = m_CylinderAngleOfArc;
				previousTextures = textures;

				//Allocate memory to pointers
				poseState = default(WVR_PoseState_t);
				layerPoseTransform = default(WVR_Pose_t);
				layerScale = default(WVR_Vector3f_t);

				poseStatePtr = Marshal.AllocHGlobal(Marshal.SizeOf(poseState));
				layerPoseTransformPtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerPoseTransform));
				layerScalePtr = Marshal.AllocHGlobal(Marshal.SizeOf(layerScale));
			}
			return true;
		}

		private bool GetAvaliableCompositorLayerTexture()
		{
			if (!isInitializationComplete || !isSynchronized) return false;

			if (isExternalSurface) return true; //No need to process texture queues

			if (textureQueues == null) return false;

			foreach (int eye in Enum.GetValues(typeof(Eye)))
			{
				LayerTextureQueue currentLayerTextureQueue = textureQueues[eye];

				if (textures != null) //check for texture params change
				{
					if (TextureParamsChanged())
					{
						//Destroy queues
						DEBUG("GetAvaliableCompositorLayerTexture: Texture changed, need to re-init queues");
						DestroyCompositorLayer();
						reinitializationNeeded = true;
						return false;
					}
				}

				IntPtr currentTextureHandle = textureQueues[eye].GetLayerTextureQueueHandle();
				if (currentTextureHandle == IntPtr.Zero)
				{
					ERROR("GetAvaliableCompositorLayerTexture: Texturehandle not found.");
					return false;
				}

				//DEBUG("GetAvaliableCompositorLayerTexture: Current Texture Handle: " + currentTextureHandle.ToString());

				//Get available texture index
				currentLayerTextureQueue.CurrentAvailableTextureIndex = Interop.WVR_GetAvailableTextureIndex(currentTextureHandle);
				if (currentLayerTextureQueue.CurrentAvailableTextureIndex == -1)
				{
					ERROR("GetAvaliableCompositorLayerTexture: There are no available textures.");
					return false;
				}
				//DEBUG("GetAvaliableCompositorLayerTexture: Available Index: " + currentLayerTextureQueue.CurrentAvailableTextureIndex);

				//Get Texture ID at available index
				bool textureIDUpdated = false;
				WVR_TextureParams_t availableTextureParams = Interop.WVR_GetTexture(currentTextureHandle, currentLayerTextureQueue.CurrentAvailableTextureIndex);
				IntPtr currentTextureID = currentLayerTextureQueue.GetCurrentAvailableTextureID();
				if (currentTextureID == null || currentTextureID != availableTextureParams.id)
				{
					DEBUG("GetAvaliableCompositorLayerTexture: Update Texture ID.");
					currentLayerTextureQueue.SetCurrentAvailableTextureID(availableTextureParams.id);
					textureIDUpdated = true;
				}

				if (currentLayerTextureQueue.GetCurrentAvailableTextureID() == IntPtr.Zero || currentLayerTextureQueue.GetCurrentAvailableTextureID() == null)
				{
					ERROR("GetAvaliableCompositorLayerTexture: Failed to get texture.");
					return false;
				}

				//DEBUG("GetAvaliableCompositorLayerTexture: Available Texture ID: " + currentLayerTextureQueue.GetCurrentAvailableTextureID().ToString());

				//Create external texture
				if (currentLayerTextureQueue.GetCurrentAvailableExternalTexture() == null || textureIDUpdated)
				{
					//DEBUG("SetupCompositorLayerTexture: Create External Texture.")
					currentLayerTextureQueue.SetCurrentAvailableExternalTexture(Texture2D.CreateExternalTexture(textures[eye].width, textures[eye].height, TextureFormat.RGBA32, false, isLinear, currentLayerTextureQueue.GetCurrentAvailableTextureID()));
				}

				if (currentLayerTextureQueue.ExternalTextures[currentLayerTextureQueue.CurrentAvailableTextureIndex] == null)
				{
					ERROR("GetAvaliableCompositorLayerTexture: Create External Texture Failed.");
					return false;
				}
			}

			return true;
		}

		private bool SetCompositorLayerContent()
		{
			if (!isInitializationComplete) return false;

			if (isExternalSurface)
			{
				WVR_Vector2f_t lowerLeftUVCoords = new WVR_Vector2f_t();
				lowerLeftUVCoords.v0 = 0f;
				lowerLeftUVCoords.v1 = 0f;
				WVR_Vector2f_t upperRightUVCoords = new WVR_Vector2f_t();
				upperRightUVCoords.v0 = 1f;
				upperRightUVCoords.v1 = 1f;
				WVR_TextureLayout_t currentTextureLayout = new WVR_TextureLayout_t();
				currentTextureLayout.leftLowUVs = lowerLeftUVCoords;
				currentTextureLayout.rightUpUVs = upperRightUVCoords;
				externalSurfaceTextureLayout = currentTextureLayout;

				return true;
			}

			foreach (int eye in Enum.GetValues(typeof(Eye)))
			{
				LayerTextureQueue currentLayerTextureQueue = textureQueues[eye];

				bool isContentSet = currentLayerTextureQueue.TextureContentSet[currentLayerTextureQueue.CurrentAvailableTextureIndex];

				if (!isDynamicLayer && isContentSet)
				{
					continue;
				}

				//Set Texture Layout
				WVR_Vector2f_t lowerLeftUVCoords = new WVR_Vector2f_t();
				lowerLeftUVCoords.v0 = 0f;
				lowerLeftUVCoords.v1 = 0f;
				WVR_Vector2f_t upperRightUVCoords = new WVR_Vector2f_t();
				upperRightUVCoords.v0 = 1f;
				upperRightUVCoords.v1 = 1f;
				WVR_TextureLayout_t currentTextureLayout = new WVR_TextureLayout_t();
				currentTextureLayout.leftLowUVs = lowerLeftUVCoords;
				currentTextureLayout.rightUpUVs = upperRightUVCoords;
				currentLayerTextureQueue.textureLayout = currentTextureLayout;

				//Blit and copy texture
				RenderTexture srcTexture = textures[eye] as RenderTexture;
				int msaaSamples = 1;
				if (srcTexture != null)
				{
					msaaSamples = srcTexture.antiAliasing;
				}

				int currentTextureWidth = currentLayerTextureQueue.GetCurrentAvailableExternalTexture().width;
				int currentTextureHeight = currentLayerTextureQueue.GetCurrentAvailableExternalTexture().height;
				Material currentBlitMat = texture2DBlitMaterial;

				RenderTextureDescriptor rtDescriptor = new RenderTextureDescriptor(currentTextureWidth, currentTextureHeight, RenderTextureFormat.ARGB32, 0);
				rtDescriptor.msaaSamples = msaaSamples;
				rtDescriptor.autoGenerateMips = false;
				rtDescriptor.useMipMap = false;
				rtDescriptor.sRGB = false;

				RenderTexture blitTempRT = RenderTexture.GetTemporary(rtDescriptor);
				if (!blitTempRT.IsCreated())
				{
					blitTempRT.Create();
				}
				blitTempRT.DiscardContents();

				Texture dstTexture = currentLayerTextureQueue.GetCurrentAvailableExternalTexture();
				Graphics.Blit(textures[eye], blitTempRT, currentBlitMat);
#if UNITY_2020_3_OR_NEWER
				Graphics.CopyTexture(blitTempRT, 0, 0, 0, 0, blitTempRT.width, blitTempRT.height, dstTexture, 0, 0, 0, 0); //Master Texture Level workaround
#else
				Graphics.CopyTexture(blitTempRT, 0, 0, dstTexture, 0, 0);
#endif

				//DEBUG("Blit and CopyTexture complete.");

				if (blitTempRT != null)
				{
					RenderTexture.ReleaseTemporary(blitTempRT);
				}

				currentLayerTextureQueue.TextureContentSet[currentLayerTextureQueue.CurrentAvailableTextureIndex] = true;
			}
			return true;
		}

		private void GetCompositorLayerPose() //Call at onBeforeRender
		{
			//check if overlay is child of hmd transform i.e. head-lock
			Transform currentTransform = transform;
			bool isHeadLock = false;
			textureOptions = WVR_TextureOptions.WVR_TextureOption_None;
			while (currentTransform != null)
			{
				if (currentTransform == hmd.transform) //overlay is child of hmd transform
				{
					textureOptions = WVR_TextureOptions.WVR_TextureOption_HeadLocked;
					isHeadLock = true;
					break;
				}
				currentTransform = currentTransform.parent;
			}

			WVR_Vector3f_t layerPosition = new WVR_Vector3f_t();
			WVR_Quatf_t layerRotation = new WVR_Quatf_t();

			if (isHeadLock)
			{
				//DEBUG("GetCompositorLayerPose: Using Head-lock Pose");

				//Calculate Layer Rotation and Position relative to Camera
				Vector3 relativePosition = hmd.transform.InverseTransformPoint(transform.position);
				Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation) * transform.rotation;

				layerPosition = UnityToWVRConversionHelper.GetWVRVector(relativePosition);
				layerRotation = UnityToWVRConversionHelper.GetWVRQuaternion(relativeRotation);
			}
			else
			{
				//DEBUG("GetCompositorLayerPose: Using World-lock Pose");
				layerPosition = UnityToWVRConversionHelper.GetWVRVector(transform.position);
				layerRotation = UnityToWVRConversionHelper.GetWVRQuaternion(transform.rotation);

				//Get worldToCameraMatrix
				Matrix4x4 worldToCameraMatrix_Unity = hmd.worldToCameraMatrix;
				//DEBUG("GetCompositorLayerPose: Unity world to camera matrix (Pre TRS): \n" + worldToCameraMatrix_Unity.ToString());
				Matrix4x4 flipZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
				worldToCameraMatrix_Unity *= flipZ;
				WVR_Matrix4f_t worldToCameraMatrix_WVR = RigidTransform.ToWVRMatrix(worldToCameraMatrix_Unity.inverse, false); //already GL convention
				poseState.IsValidPose = true;
				poseState.PoseMatrix = worldToCameraMatrix_WVR;
				//DEBUG("GetCompositorLayerPose: Unity world to camera matrix: \n" + RigidTransform.toMatrix44(poseState.PoseMatrix, false).ToString());
			}

			layerPoseTransform.position = layerPosition;
			layerPoseTransform.rotation = layerRotation;

			//DEBUG("GetCompositorLayerPose: Layer Position: " + layerPosition.v0 + ", " + layerPosition.v1 + ", " + layerPosition.v2);
			//DEBUG("GetCompositorLayerPose: Layer Rotation: " + layerRotation.x + ", " + layerRotation.y + ", " + layerRotation.z + ", " + layerRotation.w);

			if (layerShape == LayerShape.Quad)
			{
				layerScale.v0 = m_QuadWidth;
				layerScale.v1 = m_QuadHeight;
			}
			else if (layerShape == LayerShape.Cylinder)
			{
				layerScale.v0 = m_CylinderArcLength;
				layerScale.v1 = m_CylinderHeight;
				layerScale.v2 = m_CylinderRadius;
			}

			//DEBUG("GetCompositorLayerPose: Layer Scale: " + layerScale.v0 + ", " + layerScale.v1 + ", " + layerScale.v2);
		}

		private void SubmitCompositorLayer() //Call at onBeforeRender
		{
			if (!isInitializationComplete) return;

			WVR_LayerSetParams_t compositorLayerParams = new WVR_LayerSetParams_t();

			//Structure To Pointer
			Marshal.StructureToPtr(poseState, poseStatePtr, true);
			Marshal.StructureToPtr(layerPoseTransform, layerPoseTransformPtr, true);
			Marshal.StructureToPtr(layerScale, layerScalePtr, true);

			//DEBUG("SubmitCompositorLayer: Set Params for " + Eye.Left.ToString() + " eye");
			compositorLayerParams.textureL = AssignCompositorLayerParams(Eye.Left);

			//DEBUG("SubmitCompositorLayer: Set Params for " + Eye.Right.ToString() + " eye");
			compositorLayerParams.textureR = AssignCompositorLayerParams(Eye.Right);

			//DEBUG("SubmitCompositorLayer: Left Eye: Texture ID: " + compositorLayerParams.textureL.id.ToString());
			//DEBUG("SubmitCompositorLayer: Right Eye: Texture ID: " + compositorLayerParams.textureR.id.ToString());

			//DEBUG("SubmitCompositorLayer: Submitting layer");

			CompositorLayerRenderThreadSyncObject SubmitCompositionLayerSyncObject = new CompositorLayerRenderThreadSyncObject(
					(taskQueue) =>
					{
						lock (taskQueue)
						{
							CompositorLayerRenderThreadTask task = (CompositorLayerRenderThreadTask)taskQueue.Dequeue();

							if (isInitializationComplete && isSynchronized)
							{
								WVR_SubmitError submitResult = Interop.WVR_SubmitCompositionLayer(new WVR_LayerSetParams_t[] { task.LayerParams });

								if (submitResult == WVR_SubmitError.WVR_SubmitError_None)
								{
									//DEBUG("SubmitCompositorLayer: Layer Submitted Successfully");
								}
								else
								{
									DEBUG("SubmitCompositorLayer: Failed with error " + submitResult.ToString());
									DestroyCompositorLayer();
									reinitializationNeeded = true;
									DEBUG("SubmitCompositorLayer: Destroying layer");
								}
							}

							taskQueue.Release(task);
						}
					});

			CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(SubmitCompositionLayerSyncObject, compositorLayerParams);
		}

		public delegate void OnDestroyCompositorLayer();
		public event OnDestroyCompositorLayer OnDestroyCompositorLayerDelegate = null;

		private void DestroyCompositorLayer()
		{
			if (!isInitializationComplete || textureQueues == null)
			{
				DEBUG("DestroyCompositorLayer: Layer already destroyed/not initialized.");
				return;
			}

			//Free allocated memory
			Marshal.FreeHGlobal(poseStatePtr);
			Marshal.FreeHGlobal(layerPoseTransformPtr);
			Marshal.FreeHGlobal(layerScalePtr);

			if (isExternalSurface)
			{
				Destroy(textures[0]);
				Destroy(textures[1]);

				CompositorLayerRenderThreadSyncObject ReleaseLayerTextureQueueSyncObjects = new CompositorLayerRenderThreadSyncObject(
						(taskQueue) =>
						{
							lock (taskQueue)
							{
								CompositorLayerRenderThreadTask task = (CompositorLayerRenderThreadTask)taskQueue.Dequeue();

								Interop.WVR_DeleteAndroidSurface();
								externalAndroidSurfaceObj = IntPtr.Zero;
								externalAndroidSurfaceTextureParams = null;

								LRInitStatus[0] = false;
								LRInitStatus[1] = false;

								taskQueue.Release(task);
							}
						});

					CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(ReleaseLayerTextureQueueSyncObjects);

					Log.d(LOG_TAG, "DestroyCompositorLayer Ext Suf");
			}
			else
			{
				CompositorLayerRenderThreadSyncObject[] ReleaseLayerTextureQueueSyncObjects = new CompositorLayerRenderThreadSyncObject[2];

				foreach (int eye in Enum.GetValues(typeof(Eye))) //Release Texture Queue for each eye
				{
					foreach (Texture externalTexture in textureQueues[eye].ExternalTextures)
					{
						DEBUG("DestroyCompositionLayer: External textures");
						if (externalTexture != null) Destroy(externalTexture);
					}

					ReleaseLayerTextureQueueSyncObjects[eye] = new CompositorLayerRenderThreadSyncObject(
						(taskQueue) =>
						{
							lock (taskQueue)
							{
								CompositorLayerRenderThreadTask task = (CompositorLayerRenderThreadTask)taskQueue.Dequeue();

								if (textureQueues != null && textureQueues[eye] != null)
								{
									Interop.WVR_ReleaseTextureQueue(textureQueues[eye].GetLayerTextureQueueHandle());
								}

								LRInitStatus[eye] = false;

								textureQueues[eye] = null;

								taskQueue.Release(task);
							}
						});

					CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(ReleaseLayerTextureQueueSyncObjects[eye]);

					DEBUG("DestroyCompositorLayer: " + eye.ToString() + " eye of layer released.");
				}
			}

			isLayerReadyForSubmit = false;
			isInitializationComplete = false;

			//Destroy fallback mesh
			if (generatedFallbackMeshFilter != null && generatedFallbackMeshFilter.mesh != null)
			{
				DEBUG("DestroyCompositionLayer: generatedFallbackMeshFilter");
				Destroy(generatedFallbackMeshFilter.mesh);
				generatedFallbackMeshFilter = null;
			}
			if (generatedFallbackMeshRenderer != null && generatedFallbackMeshRenderer.material != null)
			{
				DEBUG("DestroyCompositionLayer: generatedFallbackMeshRenderer");
				Destroy(generatedFallbackMeshRenderer.material);
				generatedFallbackMeshRenderer = null;
			}
			if (generatedFallbackMesh != null)
			{
				Destroy(generatedFallbackMesh);
				generatedFallbackMesh = null;
			}

			//Destroy underlay mesh
			if (generatedUnderlayMeshFilter != null && generatedUnderlayMeshFilter.mesh != null)
			{
				DEBUG("DestroyCompositionLayer: generatedUnderlayMeshFilter");
				Destroy(generatedUnderlayMeshFilter.mesh);
			}
			if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
			{
				DEBUG("DestroyCompositionLayer: generatedUnderlayMeshRenderer");
				Destroy(generatedUnderlayMeshRenderer.material);
				generatedUnderlayMeshRenderer = null;
			}
			if (generatedUnderlayMesh != null)
			{
				Destroy(generatedUnderlayMesh);
				generatedUnderlayMesh = null;
			}

			//Destroy capture mesh
			if (generatedCaptureMeshFilter != null && generatedCaptureMeshFilter.mesh != null)
			{
				DEBUG("DestroyCompositionLayer: generatedCaptureMeshFilter");
				Destroy(generatedCaptureMeshFilter.mesh);
			}
			if (generatedCaptureMeshRenderer != null && generatedCaptureMeshRenderer.material != null)
			{
				DEBUG("DestroyCompositionLayer: generatedCaptureMeshRenderer");
				Destroy(generatedCaptureMeshRenderer.material);
				generatedCaptureMeshRenderer = null;
			}
			if (generatedCaptureMesh != null)
			{
				Destroy(generatedCaptureMesh);
				generatedCaptureMesh = null;
			}

			OnDestroyCompositorLayerDelegate?.Invoke();
		}

		private WVR_LayerParams_t AssignCompositorLayerParams(Eye eye)
		{
			int eyeIndex = (int)eye;

			WVR_LayerParams_t compositorTextureParams = new WVR_LayerParams_t();

			compositorTextureParams.eye = (WVR_Eye)eye;
			compositorTextureParams.id = (isExternalSurface? externalAndroidSurfaceTextureParams[0].id : textureQueues[eyeIndex].GetCurrentAvailableTextureID());
			compositorTextureParams.target = (isExternalSurface ? WVR_TextureTarget.WVR_TextureTarget_2D_EXTERNAL : textureTarget);
			compositorTextureParams.layout = (isExternalSurface ? externalSurfaceTextureLayout : textureQueues[eyeIndex].textureLayout);
			compositorTextureParams.opts = textureOptions;
			compositorTextureParams.shape = (WVR_TextureShape)layerShape;
			compositorTextureParams.type = (WVR_TextureLayerType)layerType;
			compositorTextureParams.compositionDepth = compositionDepth;
			compositorTextureParams.poseTransform = layerPoseTransformPtr;
			compositorTextureParams.scale = layerScalePtr;
			compositorTextureParams.width = (isExternalSurface ? externalSurfaceWidth : (uint)textureQueues[eyeIndex].GetCurrentAvailableExternalTexture().width);
			compositorTextureParams.height = (isExternalSurface ? externalSurfaceHeight : (uint)textureQueues[eyeIndex].GetCurrentAvailableExternalTexture().height);
			compositorTextureParams.depth = IntPtr.Zero;
			compositorTextureParams.projectionMatrix = IntPtr.Zero;

			if (textureOptions == WVR_TextureOptions.WVR_TextureOption_None)
			{
				compositorTextureParams.pose = poseStatePtr;
			}

			return compositorTextureParams;
		}

		private bool GetGraphicsReadyStat()
		{
			bool isReady = false;
			string STATE_TAG = "DisplayProviderGraphicReady";

			XRDisplaySubsystem displaySubsystem = Wave.XR.Utils.DisplaySubsystem;
			if (displaySubsystem != null)
			{
				float graphicReadyStat = -1f;
				UnityEngine.XR.Provider.XRStats.TryGetStat(displaySubsystem, STATE_TAG, out graphicReadyStat);

				isReady = Convert.ToBoolean(graphicReadyStat);

				//DEBUG("isReady: " + isReady + " graphicReadyStat: " + graphicReadyStat);
			}

			return isReady;
		}

		private void ActivatePlaceholder()
		{
			if (Debug.isDebugBuild && !placeholderGenerated)
			{
				if (CompositorLayerManager.GetInstance().MaxLayerCount() == 0)//Platform does not support multi-layer. Show placeholder instead if in development build
				{
					DEBUG("Generate Placeholder");
					CompositorLayerPlaceholderPrefabInstance = Instantiate((GameObject)Resources.Load("Prefabs/CompositorLayerDebugPlaceholder", typeof(GameObject)));
					CompositorLayerPlaceholderPrefabInstance.name = "CompositorLayerDebugPlaceholder";
					CompositorLayerPlaceholderPrefabInstance.transform.SetParent(this.transform);
					CompositorLayerPlaceholderPrefabInstance.transform.position = this.transform.position;
					CompositorLayerPlaceholderPrefabInstance.transform.rotation = this.transform.rotation;
					CompositorLayerPlaceholderPrefabInstance.transform.localScale = this.transform.localScale;

					Text placeholderText = CompositorLayerPlaceholderPrefabInstance.transform.GetChild(0).Find("Text").GetComponent<Text>();

					placeholderText.text = placeholderText.text.Replace("{REASON}", "Device does not support Multi-Layer");

					placeholderGenerated = true;
				}
				else if (CompositorLayerManager.GetInstance().MaxLayerCount() <= CompositorLayerManager.GetInstance().CurrentLayerCount())//Do not draw layer as limit is reached. Show placeholder instead if in development build 
				{
					DEBUG("Generate Placeholder");
					CompositorLayerPlaceholderPrefabInstance = Instantiate((GameObject)Resources.Load("Prefabs/CompositorLayerDebugPlaceholder", typeof(GameObject)));
					CompositorLayerPlaceholderPrefabInstance.name = "CompositorLayerDebugPlaceholder";
					CompositorLayerPlaceholderPrefabInstance.transform.SetParent(this.transform);
					CompositorLayerPlaceholderPrefabInstance.transform.position = this.transform.position;
					CompositorLayerPlaceholderPrefabInstance.transform.rotation = this.transform.rotation;
					CompositorLayerPlaceholderPrefabInstance.transform.localScale = this.transform.localScale;

					Text placeholderText = CompositorLayerPlaceholderPrefabInstance.transform.GetChild(0).Find("Text").GetComponent<Text>();

					placeholderText.text = placeholderText.text.Replace("{REASON}", "Max number of layers exceeded");

					placeholderGenerated = true;
				}
			}
			else if (placeholderGenerated && CompositorLayerPlaceholderPrefabInstance != null)
			{
				DEBUG("Placeholder already generated, activating.");
				CompositorLayerPlaceholderPrefabInstance.SetActive(true);
			}
		}

		public bool RenderAsLayer() 
		{
			if (placeholderGenerated && CompositorLayerPlaceholderPrefabInstance != null)
			{
				CompositorLayerPlaceholderPrefabInstance.SetActive(false);
			}

			if (isAutoFallbackActive)
			{
				generatedFallbackMesh.SetActive(false);
				isAutoFallbackActive = false;
			}

			isRenderPriorityChanged = false;
			
			if (layerType == LayerType.Underlay)
			{
				if (!UnderlayMeshIsValid()) //Underlay Mesh needs to be generated
				{
					UnderlayMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if Underlay Mesh is present but needs to be updated
				{
					UnderlayMeshUpdate();
				}
				generatedUnderlayMesh.SetActive(true);
			}

			WaveXRSettings waveXRSettingsInstance = WaveXRSettings.GetInstance();
			if (waveXRSettingsInstance != null && waveXRSettingsInstance.allowSpectatorCamera)
			{
				if (!CaptureMeshIsValid())
				{
					CaptureMeshGeneration();
				}
				else if (LayerDimensionsChanged())
				{
					CaptureMeshUpdate();
				}
				generatedCaptureMesh.SetActive(true);

				if (!isOnCameraPostRenderSubscribed)
				{
					Camera.onPostRender += OnCameraPostRender;
					isOnCameraPostRenderSubscribed = true;
				}

				if (!isOnCameraPreCullSubscribed)
				{
					Camera.onPreCull += OnCameraPreCull;
					isOnCameraPreCullSubscribed = true;
				}
			}

			UpdatePreviousLayerDimensions();

			return CompositorLayerInit();
		}

		public void RenderInGame()
		{
			WaveXRSettings waveXRSettingsInstance = WaveXRSettings.GetInstance();
			if (waveXRSettingsInstance != null && waveXRSettingsInstance.enableAutoFallbackForMultiLayerProperty)//Use fallback
			{
				if (!isAutoFallbackActive)
				{
					//Activate auto fallback
					if (!FallbackMeshIsValid())
					{
						AutoFallbackMeshGeneration();
					}
					else if (LayerDimensionsChanged())
					{
						AutoFallbackMeshUpdate();
					}
					generatedFallbackMesh.SetActive(true);
					isAutoFallbackActive = true;
				}
			}
			else //Use placeholder
			{
				ActivatePlaceholder();
			}

			if (generatedUnderlayMesh != null)
			{
				generatedUnderlayMesh.SetActive(false);
			}

			if (generatedCaptureMesh != null)
			{
				generatedCaptureMesh.SetActive(false);
			}

			if (isOnCameraPostRenderSubscribed)
			{
				Camera.onPostRender -= OnCameraPostRender;
				isOnCameraPostRenderSubscribed = false;
			}

			if (isOnCameraPreCullSubscribed)
			{
				Camera.onPreCull -= OnCameraPreCull;
				isOnCameraPreCullSubscribed = false;
			}

			UpdatePreviousLayerDimensions();

			isRenderPriorityChanged = false;
		}

		public void TerminateLayer()
		{
			DestroyCompositorLayer();

			if (placeholderGenerated && CompositorLayerPlaceholderPrefabInstance != null)
			{
				CompositorLayerPlaceholderPrefabInstance.SetActive(false);
			}

			if (isAutoFallbackActive)
			{
				if (generatedFallbackMesh != null) generatedFallbackMesh.SetActive(false);
				isAutoFallbackActive = false;
			}
		}

		public bool TextureParamsChanged()
		{
			if (previousTextures[0] != textures[0] || previousTextures[1] != textures[1])
			{
				return true;
			}

			return false;
		}

		public void UpdatedPreviousTextureParams()
		{
			previousTextures[0] = textures[0];
			previousTextures[1] = textures[1];
		}

		public bool LayerDimensionsChanged()
		{
			bool isChanged = false;

			if (layerShape == LayerShape.Cylinder)
			{
				if (previousAngleOfArc != m_CylinderAngleOfArc ||
					previousCylinderArcLength != m_CylinderArcLength ||
					previousCylinderHeight != m_CylinderHeight ||
					previousCylinderRadius != m_CylinderRadius)
				{
					isChanged = true;
				}
			}
			else if (layerShape == LayerShape.Quad)
			{
				if (previousQuadWidth != m_QuadWidth ||
					previousQuadHeight != m_QuadHeight)
				{
					isChanged = true;
				}
			}

			if (previousLayerShape != layerShape)
			{
				isChanged = true;
			}

			return isChanged;
		}

		public void UpdatePreviousLayerDimensions()
		{
			if (layerShape == LayerShape.Cylinder)
			{
				previousAngleOfArc = m_CylinderAngleOfArc;
				previousCylinderArcLength = m_CylinderArcLength;
				previousCylinderHeight = m_CylinderHeight;
				previousCylinderRadius = m_CylinderRadius;
			}
			else if (layerShape == LayerShape.Quad)
			{
				previousQuadWidth = m_QuadWidth;
				previousQuadHeight = m_QuadHeight;
			}
			previousLayerShape = layerShape;
		}

		#region Quad Runtime Parameter Change
		/// <summary>
		/// Use this function to update the width of a Quad Layer.
		/// </summary>
		/// <param name="inWidth"></param>
		/// <returns></returns>
		public bool SetQuadLayerWidth(float inWidth)
		{
			if (inWidth <= 0)
			{
				return false;
			}

			m_QuadWidth = inWidth;

			return true;
		}

		/// <summary>
		/// Use this function to update the height of a Quad Layer.
		/// </summary>
		/// <param name="inHeight"></param>
		/// <returns></returns>
		public bool SetQuadLayerHeight(float inHeight)
		{
			if (inHeight <= 0)
			{
				return false;
			}

			m_QuadHeight = inHeight;

			return true;
		}
		#endregion

		#region Cylinder Runtime Parameter Change
		/// <summary>
		/// Use this function to update the radius and arc angle of a Cylinder Layer. 
		/// A new arc length will be calculated automatically.
		/// </summary>
		/// <param name="inRadius"></param>
		/// <param name="inArcAngle"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerRadiusAndArcAngle(float inRadius, float inArcAngle)
		{
			//Check if radius is valid
			if (inRadius <= 0)
			{
				return false;
			}

			//Check if angle of arc is valid
			if (inArcAngle < angleOfArcLowerLimit || inArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//Check if new arc length is valid
			float newArcLength = CylinderParameterHelper.RadiusAndDegAngleOfArcToArcLength(inArcAngle, inRadius);
			if (newArcLength <= 0)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = newArcLength;
			m_CylinderRadius = inRadius;
			m_CylinderAngleOfArc = inArcAngle;

			return true;
		}

		/// <summary>
		/// Use this function to update the radius and arc length of a Cylinder Layer. 
		/// A new arc angle will be calculated automatically.
		/// </summary>
		/// <param name="inRadius"></param>
		/// <param name="inArcLength"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerRadiusAndArcLength(float inRadius, float inArcLength)
		{
			//Check if radius is valid
			if (inRadius <= 0)
			{
				return false;
			}

			//Check if arc length is valid
			if (inArcLength <= 0)
			{
				return false;
			}

			//Check if new arc angle is valid
			float newArcAngle = CylinderParameterHelper.RadiusAndArcLengthToDegAngleOfArc(inArcLength, inRadius);
			if (newArcAngle < angleOfArcLowerLimit || newArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = inArcLength;
			m_CylinderRadius = inRadius;
			m_CylinderAngleOfArc = newArcAngle;

			return true;
		}

		/// <summary>
		/// Use this function to update the arc angle and arc length of a Cylinder Layer. 
		/// A new radius will be calculated automatically.
		/// </summary>
		/// <param name="inArcAngle"></param>
		/// <param name="inArcLength"></param>
		/// <returns>True if the parameters are valid and successfully updated.</returns>
		public bool SetCylinderLayerArcAngleAndArcLength(float inArcAngle, float inArcLength)
		{
			//Check if arc angle is valid
			if (inArcAngle < angleOfArcLowerLimit || inArcAngle > angleOfArcUpperLimit)
			{
				return false;
			}

			//Check if arc length is valid
			if (inArcLength <= 0)
			{
				return false;
			}

			//Check if new radius is valid
			float newRadius = CylinderParameterHelper.ArcLengthAndDegAngleOfArcToRadius(inArcLength, inArcAngle);
			if (newRadius <= 0)
			{
				return false;
			}

			//All parameters are valid, assign to layer
			m_CylinderArcLength = inArcLength;
			m_CylinderRadius = newRadius;
			m_CylinderAngleOfArc = inArcAngle;

			return true;

		}

		/// <summary>
		/// Use this function to update the height of a Cylinder Layer.
		/// </summary>
		/// <param name="inHeight"></param>
		/// <returns></returns>
		public bool SetCylinderLayerHeight(float inHeight)
		{
			if (inHeight <=0)
			{
				return false;
			}

			m_CylinderHeight = inHeight;

			return true;
		}

		#endregion

#if UNITY_EDITOR
		public CylinderLayerParamAdjustmentMode CurrentAdjustmentMode()
		{
			if (previousCylinderArcLength != m_CylinderArcLength)
			{
				return CylinderLayerParamAdjustmentMode.ArcLength;
			}
			else if (previousAngleOfArc != m_CylinderAngleOfArc)
			{
				return CylinderLayerParamAdjustmentMode.ArcAngle;
			}
			else
			{
				return CylinderLayerParamAdjustmentMode.Radius;
			}
		}
#endif

		public void ChangeBlitShadermode(BlitShaderMode shaderMode, bool enable)
		{
			if (texture2DBlitMaterial == null || useCustomTextureBlitShaderPath) return;

			switch (shaderMode)
			{
				case BlitShaderMode.LINEAR_TO_SRGB_COLOR:
					if (enable)
					{
						texture2DBlitMaterial.EnableKeyword("LINEAR_TO_SRGB_COLOR");
					}
					else
					{
						texture2DBlitMaterial.DisableKeyword("LINEAR_TO_SRGB_COLOR");
					}
					break;
				case BlitShaderMode.LINEAR_TO_SRGB_ALPHA:
					if (enable)
					{
						texture2DBlitMaterial.EnableKeyword("LINEAR_TO_SRGB_ALPHA");
					}
					else
					{
						texture2DBlitMaterial.DisableKeyword("LINEAR_TO_SRGB_ALPHA");
					}
					break;
				default:
					break;
			}
		}

		public IntPtr GetExternalSurfaceObj()
		{
			return externalAndroidSurfaceObj;
		}
		#endregion

#region Monobehavior Lifecycle
		private void Awake()
		{
			//Create blit mat
			if (useCustomTextureBlitShaderPath && !string.IsNullOrEmpty(customTextureBlitShaderPath)) //Check if custom shader should be used and if a shader path is provided
			{
				Shader targetCustomShader = Shader.Find(customTextureBlitShaderPath);

				if (targetCustomShader != null) //Use custom shader if shader is found
				{
					texture2DBlitMaterial = new Material(targetCustomShader);
				}
				else //Fallback to default shader
				{
					texture2DBlitMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/Texture2DBlitShader"));
				}
			}
			else //Use default shader
			{
				texture2DBlitMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/Texture2DBlitShader"));
			}

			//Create render thread synchornizer
			if (synchronizer == null) synchronizer = new RenderThreadSynchronizer();

			ColorSpace currentActiveColorSpace = QualitySettings.activeColorSpace;
			if (currentActiveColorSpace == ColorSpace.Linear)
			{
				isLinear = true;
			}
			else
			{
				isLinear = false;
			}
		}

		private void OnEnable()
		{
			hmd = Camera.main;

			CompositorLayerManager.GetInstance().SubscribeToLayerManager(this);
			if (!isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender += OnBeforeRender;
				isOnBeforeRenderSubscribed = true;
			}
		}

		private void OnDisable()
		{
			if (isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}
			CompositorLayerManager.GetInstance().UnsubscribeFromLayerManager(this);
		}

		private void OnDestroy()
		{
			if (isOnBeforeRenderSubscribed)
			{
				Application.onBeforeRender -= OnBeforeRender;
				isOnBeforeRenderSubscribed = false;
			}
			//CompositorLayerManager.GetInstance().UnsubscribeFromLayerManager(this);
		}

		private void LateUpdate()
		{
			isLayerReadyForSubmit = false;

			if (!GetGraphicsReadyStat()) return;

			if (!isInitializationComplete) //Layer not marked as initialized
			{
				if (LRInitStatus[0] && LRInitStatus[1]) //Both Left and Right are initialized
				{
					reinitializationNeeded = false;
					isInitializationComplete = true;
					isSynchronized = false;
				}
				else if (reinitializationNeeded) //Layer is still active but needs to be reinitialized
				{
					CompositorLayerInit();
					return;
				}
				else
				{
					return; //do not continue to run if initialization is not yet complete
				}
			}

			if (!isSynchronized)
			{
				DEBUG("CompositorLayer: Sync");
				if (synchronizer != null)
				{
					synchronizer.sync();
					isSynchronized = true;
				}
			}

			if (isAutoFallbackActive) //Do not submit when auto fallback is active
			{
				//Check if auto fallback mesh needs to be updated
				if (!FallbackMeshIsValid()) //fallback Mesh needs to be generated
				{
					AutoFallbackMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if fallback Mesh is present but needs to be updated
				{
					AutoFallbackMeshUpdate();
				}

				//Handle possible lossy scale change
				if (generatedFallbackMesh.transform.lossyScale != Vector3.one)
				{
					generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
				}

				return;
			}

			if ((CompositorLayerPlaceholderPrefabInstance != null) && CompositorLayerPlaceholderPrefabInstance.activeSelf) //Do not submit when placeholder is active
			{
				return;
			}

			if (GetAvaliableCompositorLayerTexture())
			{
				//DEBUG("Compositor Layer Texture Get");
				if (SetCompositorLayerContent())
				{
					//DEBUG("Compositor Layer Content Set");
					isLayerReadyForSubmit = true;
				}
			}

			if (layerType == LayerType.Underlay)
			{
				if (!UnderlayMeshIsValid()) //Underlay Mesh needs to be generated
				{
					UnderlayMeshGeneration();
				}
				else if (LayerDimensionsChanged()) //if Underlay Mesh is present but needs to be updated
				{
					UnderlayMeshUpdate();
				}

				//Handle possible lossy scale change
				if (generatedUnderlayMesh.transform.lossyScale != Vector3.one)
				{
					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
				}

				generatedUnderlayMesh.SetActive(true);
			}

			WaveXRSettings waveXRSettingsInstance = WaveXRSettings.GetInstance();
			if (waveXRSettingsInstance != null && waveXRSettingsInstance.allowSpectatorCamera)
			{
				if (!CaptureMeshIsValid())
				{
					CaptureMeshGeneration();
				}
				else if (LayerDimensionsChanged())
				{
					CaptureMeshUpdate();
				}

				//Handle possible lossy scale change
				if (generatedCaptureMesh.transform.lossyScale != Vector3.one)
				{
					generatedCaptureMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
				}

				generatedCaptureMesh.SetActive(true);

				if (!isOnCameraPostRenderSubscribed)
				{
					Camera.onPostRender += OnCameraPostRender;
					isOnCameraPostRenderSubscribed = true;
				}
			}

			UpdatePreviousLayerDimensions();

			//DEBUG("Frame Index: " + ++frameIndex);
		}

		private void OnApplicationPause(bool isPaused)
		{
			DEBUG("Compositor Layer Lifecycle isPaused: " + isPaused);
			if (isPaused) //Destroy
			{
				if (isOnBeforeRenderSubscribed)
				{
					Application.onBeforeRender -= OnBeforeRender;
					isOnBeforeRenderSubscribed = false;
				}
				CompositorLayerManager.GetInstance().UnsubscribeFromLayerManager(this);
			}
			else //Initialize
			{
				CompositorLayerManager.GetInstance().SubscribeToLayerManager(this);
				if (!isOnBeforeRenderSubscribed)
				{
					Application.onBeforeRender += OnBeforeRender;
					isOnBeforeRenderSubscribed = true;
				}
			}
		}

		#region Handle Capture Mesh
		private void OnCameraPreCull(Camera currentCamera)
		{
			if (currentCamera == Camera.main || currentCamera.stereoTargetEye == StereoTargetEyeMask.Both)
			{
				WaveXRSettings waveXRSettingsInstance = WaveXRSettings.GetInstance();
				if (waveXRSettingsInstance != null && waveXRSettingsInstance.allowSpectatorCamera)
				{
					//Disable capture mesh for main camera or stereo camera
					if (generatedCaptureMesh != null) generatedCaptureMesh.SetActive(false);

					//Activate underlay mesh if necessary
					if (layerType == LayerType.Underlay && generatedUnderlayMesh != null)
					{
						generatedUnderlayMesh.SetActive(true);
					}
				}
			}
			else
			{
				//Deactive Underlay mesh for spectator/other cameras if necessary
				if (layerType == LayerType.Underlay && generatedUnderlayMesh != null)
				{
					generatedUnderlayMesh.SetActive(false);
				}
			}
		}

		private void OnCameraPostRender(Camera currentCamera)
		{
			WaveXRSettings waveXRSettingsInstance = WaveXRSettings.GetInstance();
			if (waveXRSettingsInstance != null && waveXRSettingsInstance.allowSpectatorCamera)
			{
				//Active capture mesh for other cameras
				if (!isAutoFallbackActive && generatedCaptureMesh != null) generatedCaptureMesh.SetActive(true);

				//Deactivate underlay mesh if necessary
				if (generatedUnderlayMesh != null)
				{
					generatedUnderlayMesh.SetActive(false);
				}
			}
		}

		#endregion

		private void OnBeforeRender()
		{
			if (!isInitializationComplete)
			{
				ERROR("Compositor Layer Lifecycle OnBeforeRender: Layer not initialized.");
				return;
			}
			else if (!isLayerReadyForSubmit)
			{
				if (!isAutoFallbackActive) ERROR("Compositor Layer Lifecycle OnBeforeRender: Layer not ready for submit.");
				return;
			}

			//Get Pose and submit
			GetCompositorLayerPose();
			SubmitCompositorLayer();

			isLayerReadyForSubmit = false; //reset flag after submit
		}

		#endregion

		#region Fallback mesh
		public void AutoFallbackMeshGeneration()
		{ 
			if (generatedFallbackMeshFilter != null && generatedFallbackMeshFilter.mesh != null)
			{
				Destroy(generatedFallbackMeshFilter.mesh);
			}
			if (generatedFallbackMeshRenderer != null && generatedFallbackMeshRenderer.material != null)
			{
				Destroy(generatedFallbackMeshRenderer.material);
			}
			if (generatedFallbackMesh != null) Destroy(generatedFallbackMesh);

			Mesh generatedMesh = null;

			switch (layerShape)
			{
				case LayerShape.Quad:
					generatedMesh = MeshGenerationHelper.GenerateQuadMesh(this, MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight));
					break;
				case LayerShape.Cylinder:
					generatedMesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight));
					break;
			}

			generatedFallbackMesh = new GameObject();
			generatedFallbackMesh.SetActive(false);

			generatedFallbackMesh.name = FallbackMeshName;
			generatedFallbackMesh.transform.SetParent(gameObject.transform);
			generatedFallbackMesh.transform.localPosition = Vector3.zero;
			generatedFallbackMesh.transform.localRotation = Quaternion.identity;

			generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

			generatedFallbackMeshRenderer = generatedFallbackMesh.AddComponent<MeshRenderer>();
			generatedFallbackMeshFilter = generatedFallbackMesh.AddComponent<MeshFilter>();

			generatedFallbackMeshFilter.mesh = generatedMesh;

			Material fallBackMat = new Material(Shader.Find("Unlit/Transparent"));
			fallBackMat.mainTexture = textures[0];
			generatedFallbackMeshRenderer.material = fallBackMat;
		}

		public void AutoFallbackMeshUpdate()
		{
			if (generatedFallbackMesh == null || generatedFallbackMeshRenderer == null || generatedFallbackMeshFilter == null)
			{
				return;
			}

			Mesh generatedMesh = null;

			switch (layerShape)
			{
				case LayerShape.Quad:
					generatedMesh = MeshGenerationHelper.GenerateQuadMesh(this, MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight));
					break;
				case LayerShape.Cylinder:
					generatedMesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight));
					break;
			}

			generatedFallbackMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
			Destroy(generatedFallbackMeshFilter.mesh);
			generatedFallbackMeshFilter.mesh = generatedMesh;
			generatedFallbackMeshRenderer.material.mainTexture = textures[0];
		}

		public bool FallbackMeshIsValid()
		{
			if (generatedFallbackMesh == null || generatedFallbackMeshRenderer == null || generatedFallbackMeshFilter == null)
			{
				return false;
			}
			else if (generatedFallbackMeshFilter.mesh == null || generatedFallbackMeshRenderer.material == null)
			{
				return false;
			}
			return true;
		}
		#endregion

		#region Underlay Mesh
		public void UnderlayMeshGeneration()
		{
			if (generatedUnderlayMeshFilter != null && generatedUnderlayMeshFilter.mesh != null)
			{
				Destroy(generatedUnderlayMeshFilter.mesh);
			}
			if (generatedUnderlayMeshRenderer != null && generatedUnderlayMeshRenderer.material != null)
			{
				Destroy(generatedUnderlayMeshRenderer.material);
			}
			if (generatedUnderlayMesh != null) Destroy(generatedUnderlayMesh);

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = CylinderUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();

					if (useCustomUnderlayAlphaShaderPath && !string.IsNullOrEmpty(customUnderlayAlphaShaderPath)) //Check if custom shader should be used and if a shader path is provided
					{
						Shader targetCustomShader = Shader.Find(customUnderlayAlphaShaderPath);

						if (targetCustomShader != null) //Use custom shader if shader is found
						{
							generatedUnderlayMeshRenderer.sharedMaterial = new Material(targetCustomShader);
						}
						else //Fallback to default shader
						{
							generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
						}
					}
					else //Use default shader
					{
						generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
					}
						
					generatedUnderlayMeshRenderer.material.mainTexture = textures[0];

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = QuadUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();

					if (useCustomUnderlayAlphaShaderPath && !string.IsNullOrEmpty(customUnderlayAlphaShaderPath)) //Check if custom shader should be used and if a shader path is provided
					{
						Shader targetCustomShader = Shader.Find(customUnderlayAlphaShaderPath);

						if (targetCustomShader != null) //Use custom shader if shader is found
						{
							generatedUnderlayMeshRenderer.sharedMaterial = new Material(targetCustomShader);
						}
						else //Fallback to default shader
						{
							generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
						}
					}
					else //Use default shader
					{
						generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
					}

#if UNITY_EDITOR
					generatedUnderlayMeshRenderer.sharedMaterial.mainTexture = textures[0];
#elif UNITY_ANDROID
					generatedUnderlayMeshRenderer.material.mainTexture = textures[0];
#endif

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(this, quadVertices);
					break;
			}
		}

		public void UnderlayMeshUpdate()
		{
			if (generatedUnderlayMesh == null || generatedUnderlayMeshRenderer == null || generatedUnderlayMeshFilter == null)
			{
				return;
			}

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Generate Mesh
					Destroy(generatedUnderlayMeshFilter.mesh);
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Generate Mesh
					Destroy(generatedUnderlayMeshFilter.mesh);
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(this, quadVertices);
					break;
			}

			generatedUnderlayMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
		}

		public bool UnderlayMeshIsValid()
		{
			if (generatedUnderlayMesh == null || generatedUnderlayMeshRenderer == null || generatedUnderlayMeshFilter == null)
			{
				return false;
			}
			else if (generatedUnderlayMeshFilter.mesh == null || generatedUnderlayMeshRenderer.material == null)
			{
				return false;
			}

			return true;
		}
		#endregion

		#region Capture Mesh
		public void CaptureMeshGeneration()
		{
			if (generatedCaptureMeshFilter != null && generatedCaptureMeshFilter.mesh != null)
			{
				Destroy(generatedCaptureMeshFilter.mesh);
			}
			if (generatedCaptureMeshRenderer != null && generatedCaptureMeshRenderer.material != null)
			{
				Destroy(generatedCaptureMeshRenderer.material);
			}
			if (generatedCaptureMesh != null) Destroy(generatedCaptureMesh);

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Add components to Game Object
					generatedCaptureMesh = new GameObject();
					generatedCaptureMesh.name = CaptureMeshName;
					generatedCaptureMesh.transform.SetParent(transform);
					generatedCaptureMesh.transform.localPosition = Vector3.zero;
					generatedCaptureMesh.transform.localRotation = Quaternion.identity;

					generatedCaptureMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedCaptureMeshRenderer = generatedCaptureMesh.AddComponent<MeshRenderer>();
					generatedCaptureMeshFilter = generatedCaptureMesh.AddComponent<MeshFilter>();
					generatedCaptureMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
					generatedCaptureMeshRenderer.material.mainTexture = textures[0];

					//Generate Mesh
					generatedCaptureMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Add components to Game Object
					generatedCaptureMesh = new GameObject();
					generatedCaptureMesh.name = CaptureMeshName;
					generatedCaptureMesh.transform.SetParent(transform);
					generatedCaptureMesh.transform.localPosition = Vector3.zero;
					generatedCaptureMesh.transform.localRotation = Quaternion.identity;

					generatedCaptureMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);

					generatedCaptureMeshRenderer = generatedCaptureMesh.AddComponent<MeshRenderer>();
					generatedCaptureMeshFilter = generatedCaptureMesh.AddComponent<MeshFilter>();
					generatedCaptureMeshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
					generatedCaptureMeshRenderer.material.mainTexture = textures[0];

					//Generate Mesh
					generatedCaptureMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(this, quadVertices);
					break;
			}
		}

		public void CaptureMeshUpdate()
		{
			if (generatedCaptureMesh == null || generatedCaptureMeshRenderer == null || generatedCaptureMeshFilter == null)
			{
				return;
			}

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(m_CylinderAngleOfArc, m_CylinderRadius, m_CylinderHeight);

					//Generate Mesh
					Destroy(generatedCaptureMeshFilter.mesh);
					generatedCaptureMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(m_CylinderAngleOfArc, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = MeshGenerationHelper.GenerateQuadVertex(m_QuadWidth, m_QuadHeight);

					//Generate Mesh
					Destroy(generatedCaptureMeshFilter.mesh);
					generatedCaptureMeshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(this, quadVertices);
					break;
			}

			generatedCaptureMesh.transform.localScale = GetNormalizedLocalScale(transform, Vector3.one);
		}

		public bool CaptureMeshIsValid()
		{
			if (generatedCaptureMesh == null || generatedCaptureMeshRenderer == null || generatedCaptureMeshFilter == null)
			{
				return false;
			}
			else if (generatedCaptureMeshFilter.mesh == null || generatedCaptureMeshRenderer.material == null)
			{
				return false;
			}

			return true;
		}


		#endregion


		public Vector3 GetNormalizedLocalScale(Transform targetTransform, Vector3 targetGlobalScale) //Return the local scale needed to make it match to the target global scale
		{
			Vector3 normalizedLocalScale = new Vector3(targetGlobalScale.x / targetTransform.lossyScale.x, targetGlobalScale.y / targetTransform.lossyScale.y, targetGlobalScale.z / targetTransform.lossyScale.z);

			return normalizedLocalScale;
		}

		#region Enum Definitions
		public enum LayerType
		{
			Overlay = 1,
			Underlay = 2,
		}

		public enum LayerShape
		{
			Quad = 0,
			Cylinder = 1,
		}

		public enum Eye
		{
			Left = 0,
			Right = 1,
		}

#if UNITY_EDITOR
		public enum CylinderLayerParamAdjustmentMode
		{
			Radius = 0,
			ArcLength = 1,
			ArcAngle = 2,
		}

		public enum CylinderLayerParamLockMode
		{
			ArcLength = 0,
			ArcAngle = 1,
		}
#endif

		public enum BlitShaderMode
		{
			LINEAR_TO_SRGB_COLOR = 0,
			LINEAR_TO_SRGB_ALPHA = 1,
		}
#endregion

		#region Helper Classes
		public class LayerTextureQueue
		{
			private Eye LayerEye;
			private IntPtr LayerTextureQueueHandle;
			private uint LayerTextureQueueLength;

			public int CurrentAvailableTextureIndex { get; set; }
			public uint LayerTextureWidth, LayerTextureHeight;
			public IntPtr[] TextureIDs;
			public Texture[] ExternalTextures;
			public bool[] TextureContentSet;
			public WVR_TextureLayout_t textureLayout { get; set; }

			public LayerTextureQueue(IntPtr textureQueue, Eye eye, uint textureQueueLength, uint textureWidth, uint textureHeight)
			{
				LayerEye = eye;
				LayerTextureQueueHandle = textureQueue;
				LayerTextureQueueLength = textureQueueLength;
				LayerTextureWidth = textureWidth;
				LayerTextureHeight = textureHeight;
				TextureIDs = new IntPtr[textureQueueLength];
				ExternalTextures = new Texture[textureQueueLength];
				TextureContentSet = new bool[textureQueueLength];

				for (int i = 0; i < textureQueueLength; i++)
				{
					TextureContentSet[i] = false;
				}

				//DEBUG("LayerTextureQueue: TextureQueue is created succesfully for " + eye.ToString());
			}

			public IntPtr GetLayerTextureQueueHandle()
			{
				if (LayerTextureQueueHandle == IntPtr.Zero)
				{
					DEBUG("GetTextureQueueHandle: TextureQueueHandle not found.");
					return IntPtr.Zero;
				}

				return LayerTextureQueueHandle;
			}

			public IntPtr GetCurrentAvailableTextureID()
			{
				if (CurrentAvailableTextureIndex < 0 || CurrentAvailableTextureIndex > LayerTextureQueueLength - 1)
				{
					return IntPtr.Zero;
				}
				return TextureIDs[CurrentAvailableTextureIndex];
			}

			public void SetCurrentAvailableTextureID(IntPtr newTextureID)
			{
				if (CurrentAvailableTextureIndex < 0 || CurrentAvailableTextureIndex > LayerTextureQueueLength - 1)
				{
					return;
				}
				TextureIDs[CurrentAvailableTextureIndex] = newTextureID;
			}

			public Texture GetCurrentAvailableExternalTexture()
			{
				if (CurrentAvailableTextureIndex < 0 || CurrentAvailableTextureIndex > LayerTextureQueueLength - 1)
				{
					return null;
				}
				return ExternalTextures[CurrentAvailableTextureIndex];
			}

			public void SetCurrentAvailableExternalTexture(Texture newExternalTexture)
			{
				if (CurrentAvailableTextureIndex < 0 || CurrentAvailableTextureIndex > LayerTextureQueueLength - 1)
				{
					return;
				}
				ExternalTextures[CurrentAvailableTextureIndex] = newExternalTexture;
			}
		}

		private class CompositorLayerRenderThreadTask : Task
		{
			public WVR_LayerSetParams_t LayerParams;

			public CompositorLayerRenderThreadTask() { }

			public static void IssueObtainTextureQueueEvent(CompositorLayerRenderThreadSyncObject syncObject)
			{
				PreAllocatedQueue taskQueue = syncObject.Queue;
				lock (taskQueue)
				{
					CompositorLayerRenderThreadTask task = taskQueue.Obtain<CompositorLayerRenderThreadTask>();
					taskQueue.Enqueue(task);
				}
				syncObject.IssueEvent();
			}

			public static void IssueObtainTextureQueueEvent(CompositorLayerRenderThreadSyncObject syncObject, WVR_LayerSetParams_t newLayerParams)
			{
				PreAllocatedQueue taskQueue = syncObject.Queue;
				lock (taskQueue)
				{
					CompositorLayerRenderThreadTask task = taskQueue.Obtain<CompositorLayerRenderThreadTask>();

					task.LayerParams = newLayerParams;

					taskQueue.Enqueue(task);
				}
				syncObject.IssueEvent();
			}
		}

		private class RenderThreadSynchronizer
		{
			RenderTexture mutable = new RenderTexture(1, 1, 0);
			public RenderThreadSynchronizer()
			{
				mutable.useMipMap = false;
				mutable.Create();
			}

			public void sync()
			{
				var originalLogType = Application.GetStackTraceLogType(LogType.Error);
				Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);

				// Sync
				mutable.GetNativeTexturePtr();

				Application.SetStackTraceLogType(LogType.Error, originalLogType);
			}
		}

		private class UnityToWVRConversionHelper
		{
			public static WVR_Quatf_t GetWVRQuaternion(Quaternion rot)
			{
				WVR_Quatf_t qua = new WVR_Quatf_t();
				qua.x = rot.x;
				qua.y = rot.y;
				qua.z = -rot.z;
				qua.w = -rot.w;
				return qua;
			}

			public static WVR_Vector3f_t GetWVRVector(Vector3 pos)
			{
				WVR_Vector3f_t vec = new WVR_Vector3f_t();
				vec.v0 = pos.x;
				vec.v1 = pos.y;
				vec.v2 = -pos.z;
				return vec;
			}

			public static WVR_Quatf_t GetWVRQuaternion_NoConversion(Quaternion rot)
			{
				WVR_Quatf_t qua = new WVR_Quatf_t();
				qua.x = rot.x;
				qua.y = rot.y;
				qua.z = rot.z;
				qua.w = rot.w;
				return qua;
			}

			public static WVR_Vector3f_t GetWVRVector_NoConversion(Vector3 pos)
			{
				WVR_Vector3f_t vec = new WVR_Vector3f_t();
				vec.v0 = pos.x;
				vec.v1 = pos.y;
				vec.v2 = pos.z;
				return vec;
			}
		}

		public static class MeshGenerationHelper
		{
			public static Vector3[] GenerateQuadVertex(float quadWidth, float quadHeight)
			{
				Vector3[] vertices = new Vector3[4]; //Four corners

				vertices[0] = new Vector3(-quadWidth / 2, -quadHeight / 2, 0); //Bottom Left
				vertices[1] = new Vector3(quadWidth / 2, -quadHeight / 2, 0); //Bottom Right
				vertices[2] = new Vector3(-quadWidth / 2, quadHeight / 2, 0); //Top Left
				vertices[3] = new Vector3(quadWidth / 2, quadHeight / 2, 0); //Top Right

				return vertices;
			}

			public static Mesh GenerateQuadMesh(CompositorLayer target, Vector3[] vertices)
			{
				Mesh quadMesh = new Mesh();
				quadMesh.vertices = vertices;

				//Create array that represents vertices of the triangles
				int[] triangles = new int[6];
				triangles[0] = 0;
				triangles[1] = 2;
				triangles[2] = 1;

				triangles[3] = 1;
				triangles[4] = 2;
				triangles[5] = 3;

				quadMesh.triangles = triangles;
				Vector2[] uv = new Vector2[vertices.Length];
				Vector4[] tangents = new Vector4[vertices.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < 2; x++, i++)
					{
						uv[i] = new Vector2((float)x, (float)y);
						tangents[i] = tangent;
					}
				}
				quadMesh.uv = uv;
				quadMesh.tangents = tangents;
				quadMesh.RecalculateNormals();

				return quadMesh;
			}

			public static Vector3[] GenerateCylinderVertex(float cylinderAngleOfArc, float cylinderRadius, float cylinderHeight)
			{
				float angleUpperLimitDeg = cylinderAngleOfArc / 2; //Degrees
				float angleLowerLimitDeg = -angleUpperLimitDeg; //Degrees

				float angleUpperLimitRad = angleUpperLimitDeg * Mathf.Deg2Rad; //Radians
				float angleLowerLimitRad = angleLowerLimitDeg * Mathf.Deg2Rad; //Radians

				int arcSegments = Mathf.RoundToInt(cylinderAngleOfArc / 5f);

				float anglePerArcSegmentRad = (cylinderAngleOfArc / arcSegments) * Mathf.Deg2Rad;

				Vector3[] vertices = new Vector3[2 * (arcSegments + 1)]; //Top and bottom lines * Vertex count per line

				int vertexCount = 0;
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < arcSegments + 1; j++) //Clockwise
					{
						float currentVertexAngleRad = angleLowerLimitRad + anglePerArcSegmentRad * j;
						float x = cylinderRadius * Mathf.Sin(currentVertexAngleRad);
						float y = 0;
						float z = cylinderRadius * Mathf.Cos(currentVertexAngleRad);

						if (i == 1) //Top
						{
							y += cylinderHeight / 2;

						}
						else //Bottom
						{
							y -= cylinderHeight / 2;
						}

						vertices[vertexCount] = new Vector3(x, y, z);
						vertexCount++;
					}
				}

				return vertices;
			}

			public static Mesh GenerateCylinderMesh(float cylinderAngleOfArc, Vector3[] vertices)
			{
				Mesh cylinderMesh = new Mesh();
				cylinderMesh.vertices = vertices;
				int arcSegments = Mathf.RoundToInt(cylinderAngleOfArc / 5f);

				//Create array that represents vertices of the triangles
				int[] triangles = new int[arcSegments * 6];
				for (int currentTriangleIndex = 0, currentVertexIndex = 0, y = 0; y < 1; y++, currentVertexIndex++)
				{
					for (int x = 0; x < arcSegments; x++, currentTriangleIndex += 6, currentVertexIndex++)
					{
						triangles[currentTriangleIndex] = currentVertexIndex;
						triangles[currentTriangleIndex + 1] = currentVertexIndex + arcSegments + 1;
						triangles[currentTriangleIndex + 2] = currentVertexIndex + 1;

						triangles[currentTriangleIndex + 3] = currentVertexIndex + 1;
						triangles[currentTriangleIndex + 4] = currentVertexIndex + arcSegments + 1;
						triangles[currentTriangleIndex + 5] = currentVertexIndex + arcSegments + 2;
					}
				}
				cylinderMesh.triangles = triangles;
				Vector2[] uv = new Vector2[vertices.Length];
				Vector4[] tangents = new Vector4[vertices.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < arcSegments + 1; x++, i++)
					{
						uv[i] = new Vector2((float)x / arcSegments, (float)y);
						tangents[i] = tangent;
					}
				}
				cylinderMesh.uv = uv;
				cylinderMesh.tangents = tangents;
				cylinderMesh.RecalculateNormals();

				return cylinderMesh;
			}
		}

		public static class CylinderParameterHelper
		{
			public static float RadiusAndDegAngleOfArcToArcLength(float inDegAngleOfArc, float inRadius)
			{
				float arcLength = inRadius * (inDegAngleOfArc * Mathf.Deg2Rad);

				return arcLength;
			}

			public static float RadiusAndArcLengthToDegAngleOfArc(float inArcLength, float inRadius)
			{
				float degAngleOfArc = (inArcLength / inRadius) * Mathf.Rad2Deg;

				return degAngleOfArc;
			}

			public static float ArcLengthAndDegAngleOfArcToRadius(float inArcLength, float inDegAngleOfArc)
			{
				float radius = (inArcLength / (inDegAngleOfArc * Mathf.Deg2Rad));

				return radius;
			}
		}
		#endregion
	}
}
