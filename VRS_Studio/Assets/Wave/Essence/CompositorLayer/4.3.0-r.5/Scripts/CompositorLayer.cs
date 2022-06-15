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

		[Tooltip("Specify Compositor Layer Type.")]
		[SerializeField]
		public LayerType layerType = LayerType.Overlay;

		[Tooltip("Specify Layer Composition Depth.")]
		[SerializeField]
		public uint compositionDepth = 0;

		[Tooltip("Specify Layer Shape.")]
		[SerializeField]
		public LayerShape layerShape = LayerShape.Quad;

		[Tooltip("Width of a Quad Layer")]
		[SerializeField]
		[Min(0.001f)]
		public float quadWidth = 1f;

		[Tooltip("Height of a Quad Layer")]
		[SerializeField]
		[Min(0.001f)]
		public float quadHeight = 1f;

		[Tooltip("Height of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		public float cylinderHeight = 1f;

		[Tooltip("Arc Length of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		public float cylinderArcLength = 1f;

		[Tooltip("Radius of Cylinder Layer")]
		[SerializeField]
		[Min(0.001f)]
		public float cylinderRadius = 1f;

		[Tooltip("Central angle of arc of Cylinder Layer")]
		[SerializeField]
		[Range(5f, 180f)]
		public float angleOfArc = 180f;

		[Tooltip("Cylinder Layer parameter to be locked when changing the radius.")]
		[SerializeField]
		public CylinderLayerParamLockMode lockMode = CylinderLayerParamLockMode.ArcLength;

		[Tooltip("Specify whether Layer needs to be updated each frame or not.")]
		[SerializeField]
		public bool isDynamicLayer = false;

		[Tooltip("Specify source textures if textures are from Asset.")]
		[SerializeField]
		public Texture[] textures = new Texture[] { null, null };

		[SerializeField]
		public GameObject generatedUnderlayMesh = null;
		private MeshRenderer generatedUnderlayMeshRenderer = null;
		private MeshFilter generatedUnderlayMeshFilter = null;

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

		[SerializeField]
		public GameObject generatedFallbackMesh = null;
		private MeshRenderer generatedFallbackMeshRenderer = null;
		private MeshFilter generatedFallbackMeshFilter = null;

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

		private int frameIndex = -1;

		public readonly float angleOfArcLowerLimit = 5f;
		public readonly float angleOfArcUpperLimit = 180f;

		public float previous_quadWidth = 1f;
		public float previous_quadHeight = 1f;
		public float previous_cylinderHeight = 1f;
		public float previous_cylinderArcLength = 1f;
		public float previous_cylinderRadius = 1f;
		public float previous_angleOfArc = 180f;
		public Texture[] previous_textures = new Texture[] { null, null };

		private bool[] LRInitStatus = new bool[] { false, false };
		private bool isInitializationComplete = false;
		private bool reinitializationNeeded = false;
		private bool isOnBeforeRenderSubscribed = false;
		private bool isLayerReadyForSubmit = false;
		private bool isLinear = false;
		private bool isAutoFallbackActive = false;
		private bool placeholderGenerated = false;
		private static bool isSynchronized = false;
		private static RenderThreadSynchronizer synchronizer;
		private Camera hmd;

		private const string LOG_TAG = "Wave_CompositorLayer";

		#region Compositor Layer Lifecycle

		private bool CompositorLayerInit()
		{
			if (isInitializationComplete)
			{
				//Log.d(LOG_TAG, "CompositorLayerInit: Already initialized.");
				return true;
			}

			if (textures == null || textures[0] == null)
			{
				Log.e(LOG_TAG, "CompositorLayerInit: Source Texture not found, abort init.");
				return false;
			}

			if (textures[0] != null && textures[1] == null)
			{
				Log.d(LOG_TAG, "CompositorLayerInit: Using Left Texture as Right Texture.");
				textures[1] = textures[0];
			}

			if (textureQueues == null)
			{
				Log.d(LOG_TAG, "CompositorLayerInit: Create new TextureQueues.");
				textureQueues = new LayerTextureQueue[2];
			}

			Log.d(LOG_TAG, "CompositorLayerInit");

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
							//Log.d(LOG_TAG, eye.ToString() + " eye of layer setup Success. Queue Length: " + newTextureQueueLength);

							LRInitStatus[eyeIndex] = true;
							//Log.d(LOG_TAG, eye.ToString() + " LRInitStatus " + LRInitStatus[eyeIndex].ToString());

							taskQueue.Release(task);
						}
					});

				CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(ObtainLayerTextureQueueSyncObjects[eyeIndex]);

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
			if (!isInitializationComplete || !isSynchronized || textureQueues == null) return false;

			foreach (int eye in Enum.GetValues(typeof(Eye)))
			{
				LayerTextureQueue currentLayerTextureQueue = textureQueues[eye];

				if (textures != null) //check for texture size change
				{
					uint textureWidth = (uint)textures[eye].width;
					uint textureHeight = (uint)textures[eye].height;

					if (currentLayerTextureQueue.LayerTextureWidth != textureWidth || currentLayerTextureQueue.LayerTextureHeight != textureHeight || ParamsChanged())
					{
						//Destroy queues
						Log.d(LOG_TAG, "GetAvaliableCompositorLayerTexture: Texture size changed, need to re-init queues");
						DestroyCompositorLayer();
						reinitializationNeeded = true;
						return false;
					}
				}

				IntPtr currentTextureHandle = textureQueues[eye].GetLayerTextureQueueHandle();
				if (currentTextureHandle == IntPtr.Zero)
				{
					Log.e(LOG_TAG, "GetAvaliableCompositorLayerTexture: Texturehandle not found.");
					return false;
				}

				//Log.d(LOG_TAG, "GetAvaliableCompositorLayerTexture: Current Texture Handle: " + currentTextureHandle.ToString());

				//Get available texture index
				currentLayerTextureQueue.CurrentAvailableTextureIndex = Interop.WVR_GetAvailableTextureIndex(currentTextureHandle);
				if (currentLayerTextureQueue.CurrentAvailableTextureIndex == -1)
				{
					Log.e(LOG_TAG, "GetAvaliableCompositorLayerTexture: There are no available textures.");
					return false;
				}
				//Log.d(LOG_TAG, "GetAvaliableCompositorLayerTexture: Available Index: " + currentLayerTextureQueue.CurrentAvailableTextureIndex);

				//Get Texture ID at available index
				bool textureIDUpdated = false;
				WVR_TextureParams_t availableTextureParams = Interop.WVR_GetTexture(currentTextureHandle, currentLayerTextureQueue.CurrentAvailableTextureIndex);
				IntPtr currentTextureID = currentLayerTextureQueue.GetCurrentAvailableTextureID();
				if (currentTextureID == null || currentTextureID != availableTextureParams.id)
				{
					Log.d(LOG_TAG, "GetAvaliableCompositorLayerTexture: Update Texture ID.");
					currentLayerTextureQueue.SetCurrentAvailableTextureID(availableTextureParams.id);
					textureIDUpdated = true;
				}

				if (currentLayerTextureQueue.GetCurrentAvailableTextureID() == IntPtr.Zero || currentLayerTextureQueue.GetCurrentAvailableTextureID() == null)
				{
					Log.e(LOG_TAG, "GetAvaliableCompositorLayerTexture: Failed to get texture.");
					return false;
				}

				//Log.d(LOG_TAG, "GetAvaliableCompositorLayerTexture: Available Texture ID: " + currentLayerTextureQueue.GetCurrentAvailableTextureID().ToString());

				//Create external texture
				if (currentLayerTextureQueue.GetCurrentAvailableExternalTexture() == null || textureIDUpdated)
				{
					//Log.d(LOG_TAG, "SetupCompositorLayerTexture: Create External Texture.")
					currentLayerTextureQueue.SetCurrentAvailableExternalTexture(Texture2D.CreateExternalTexture(textures[eye].width, textures[eye].height, TextureFormat.RGBA32, false, isLinear, currentLayerTextureQueue.GetCurrentAvailableTextureID()));
				}

				if (currentLayerTextureQueue.ExternalTextures[currentLayerTextureQueue.CurrentAvailableTextureIndex] == null)
				{
					Log.e(LOG_TAG, "GetAvaliableCompositorLayerTexture: Create External Texture Failed.");
					return false;
				}
			}

			return true;
		}

		private bool SetCompositorLayerContent()
		{
			if (!isInitializationComplete) return false;

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
				Graphics.CopyTexture(blitTempRT, 0, 0, dstTexture, 0, 0);

				//Log.d(LOG_TAG, "Blit and CopyTexture complete.");

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
				Log.d(LOG_TAG, "GetCompositorLayerPose: Using Head-lock Pose");

				//Calculate Layer Rotation and Position relative to Camera
				Vector3 relativePosition = hmd.transform.InverseTransformPoint(transform.position);
				Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation) * transform.rotation;

				layerPosition = UnityToWVRConversionHelper.GetWVRVector(relativePosition);
				layerRotation = UnityToWVRConversionHelper.GetWVRQuaternion(relativeRotation);
			}
			else
			{
				Log.d(LOG_TAG, "GetCompositorLayerPose: Using World-lock Pose");
				layerPosition = UnityToWVRConversionHelper.GetWVRVector(transform.position);
				layerRotation = UnityToWVRConversionHelper.GetWVRQuaternion(transform.rotation);

				//Get worldToCameraMatrix
				Matrix4x4 worldToCameraMatrix_Unity = hmd.worldToCameraMatrix;
				//Log.d(LOG_TAG, "GetCompositorLayerPose: Unity world to camera matrix (Pre TRS): \n" + worldToCameraMatrix_Unity.ToString());
				Matrix4x4 flipZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
				worldToCameraMatrix_Unity *= flipZ;
				WVR_Matrix4f_t worldToCameraMatrix_WVR = RigidTransform.ToWVRMatrix(worldToCameraMatrix_Unity.inverse, false); //already GL convention
				poseState.IsValidPose = true;
				poseState.PoseMatrix = worldToCameraMatrix_WVR;
				//Log.d(LOG_TAG, "GetCompositorLayerPose: Unity world to camera matrix: \n" + RigidTransform.toMatrix44(poseState.PoseMatrix, false).ToString());
			}

			layerPoseTransform.position = layerPosition;
			layerPoseTransform.rotation = layerRotation;

			//Log.d(LOG_TAG, "GetCompositorLayerPose: Layer Position: " + layerPosition.v0 + ", " + layerPosition.v1 + ", " + layerPosition.v2);
			//Log.d(LOG_TAG, "GetCompositorLayerPose: Layer Rotation: " + layerRotation.x + ", " + layerRotation.y + ", " + layerRotation.z + ", " + layerRotation.w);

			if (layerShape == LayerShape.Quad)
			{
				layerScale.v0 = quadWidth;
				layerScale.v1 = quadHeight;
			}
			else if (layerShape == LayerShape.Cylinder)
			{
				layerScale.v0 = cylinderArcLength;
				layerScale.v1 = cylinderHeight;
				layerScale.v2 = cylinderRadius;
			}

			//Log.d(LOG_TAG, "GetCompositorLayerPose: Layer Scale: " + layerScale.v0 + ", " + layerScale.v1 + ", " + layerScale.v2);
		}

		private void SubmitCompositorLayer() //Call at onBeforeRender
		{
			if (!isInitializationComplete) return;

			WVR_LayerSetParams_t compositorLayerParams = new WVR_LayerSetParams_t();

			//Structure To Pointer
			Marshal.StructureToPtr(poseState, poseStatePtr, true);
			Marshal.StructureToPtr(layerPoseTransform, layerPoseTransformPtr, true);
			Marshal.StructureToPtr(layerScale, layerScalePtr, true);

			//Log.d(LOG_TAG, "SubmitCompositorLayer: Set Params for " + Eye.Left.ToString() + " eye");
			compositorLayerParams.textureL = AssignCompositorLayerParams(Eye.Left);

			//Log.d(LOG_TAG, "SubmitCompositorLayer: Set Params for " + Eye.Right.ToString() + " eye");
			compositorLayerParams.textureR = AssignCompositorLayerParams(Eye.Right);

			//Log.d(LOG_TAG, "SubmitCompositorLayer: Left Eye: Texture ID: " + compositorLayerParams.textureL.id.ToString());
			//Log.d(LOG_TAG, "SubmitCompositorLayer: Right Eye: Texture ID: " + compositorLayerParams.textureR.id.ToString());

			Log.d(LOG_TAG, "SubmitCompositorLayer: Submitting layer");

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
									Log.d(LOG_TAG, "SubmitCompositorLayer: Layer Submitted Successfully");
								}
								else
								{
									Log.d(LOG_TAG, "SubmitCompositorLayer: Failed with error " + submitResult.ToString());
									DestroyCompositorLayer();
									reinitializationNeeded = true;
									Log.d(LOG_TAG, "SubmitCompositorLayer: Destroying layer");
								}
							}

							taskQueue.Release(task);
						}
					});

			CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(SubmitCompositionLayerSyncObject, compositorLayerParams);
		}

		private void DestroyCompositorLayer()
		{
			if (!isInitializationComplete && textureQueues == null)
			{
				Log.d(LOG_TAG, "DestroyCompositorLayer: Layer already destroyed/not initialized.");
				return;
			}

			//Free allocated memory
			Marshal.FreeHGlobal(poseStatePtr);
			Marshal.FreeHGlobal(layerPoseTransformPtr);
			Marshal.FreeHGlobal(layerScalePtr);

			CompositorLayerRenderThreadSyncObject[] ReleaseLayerTextureQueueSyncObjects = new CompositorLayerRenderThreadSyncObject[2];

			foreach (int eye in Enum.GetValues(typeof(Eye))) //Release Texture Queue for each eye
			{
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

							taskQueue.Release(task);
						}
					});

				CompositorLayerRenderThreadTask.IssueObtainTextureQueueEvent(ReleaseLayerTextureQueueSyncObjects[eye]);

				Log.d(LOG_TAG, "DestroyCompositorLayer: " + eye.ToString() + " eye of layer released.");
			}

			isLayerReadyForSubmit = false;
			isInitializationComplete = false;
			textureQueues = null;
		}

		private WVR_LayerParams_t AssignCompositorLayerParams(Eye eye)
		{
			int eyeIndex = (int)eye;

			WVR_LayerParams_t compositorTextureParams = new WVR_LayerParams_t();

			compositorTextureParams.eye = (WVR_Eye)eye;
			compositorTextureParams.id = textureQueues[eyeIndex].GetCurrentAvailableTextureID();
			compositorTextureParams.target = textureTarget;
			compositorTextureParams.layout = textureQueues[eyeIndex].textureLayout;
			compositorTextureParams.opts = textureOptions;
			compositorTextureParams.shape = (WVR_TextureShape)layerShape;
			compositorTextureParams.type = (WVR_TextureLayerType)layerType;
			compositorTextureParams.compositionDepth = compositionDepth;
			compositorTextureParams.poseTransform = layerPoseTransformPtr;
			compositorTextureParams.scale = layerScalePtr;
			compositorTextureParams.width = (uint)textureQueues[eyeIndex].GetCurrentAvailableExternalTexture().width;
			compositorTextureParams.height = (uint)textureQueues[eyeIndex].GetCurrentAvailableExternalTexture().height;
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

				//Log.d(LOG_TAG, "isReady: " + isReady + " graphicReadyStat: " + graphicReadyStat);
			}

			return isReady;
		}

		private void ActivatePlaceholder()
		{
			if (Debug.isDebugBuild && !placeholderGenerated)
			{
				if (CompositorLayerManager.GetInstance().MaxLayerCount() == 0)//Platform does not support multi-layer. Show placeholder instead if in development build
				{
					Log.d(LOG_TAG, "Generate Placeholder");
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
					Log.d(LOG_TAG, "Generate Placeholder");
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
				Log.d(LOG_TAG, "Placeholder already generated, activating.");
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

			//if Underlay Mesh is present but needs to be reconstructed
			if (layerType == LayerType.Underlay)
			{
				if (!UnderlayMeshIsValid()) UnderlayMeshGeneration();
				else generatedUnderlayMesh.SetActive(true);
			}

			return CompositorLayerInit();
		}

		public void RenderInGame()
		{
			if (WaveXRSettings.GetInstance().enableAutoFallbackForMultiLayerProperty) //Use fallback
			{
				if (!isAutoFallbackActive)
				{
					//Activate auto fallback
					if (!FallbackMeshIsValid() || ParamsChanged())
					{
						AutoFallbackMeshGeneration();
					}
					generatedFallbackMesh.SetActive(true);
					isAutoFallbackActive = true;
				}
			}
			else //Use placeholder
			{
				ActivatePlaceholder();
			}

			if (layerType == LayerType.Underlay && generatedUnderlayMesh != null)
			{
				generatedUnderlayMesh.SetActive(false);
			}

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
				generatedFallbackMesh.SetActive(false);
				isAutoFallbackActive = false;
			}
		}

		public bool ParamsChanged()
		{
			if (previous_textures[0] != textures[0] || previous_textures[1] != textures[1])
			{
				previous_textures[0] = textures[0];
				previous_textures[1] = textures[1];
				return true;
			}

			if (layerShape == LayerShape.Cylinder)
			{
				if (previous_angleOfArc != angleOfArc ||
					previous_cylinderArcLength != cylinderArcLength ||
					previous_cylinderHeight != cylinderHeight ||
					previous_cylinderRadius != cylinderRadius)
				{
					previous_angleOfArc = angleOfArc;
					previous_cylinderArcLength = cylinderArcLength;
					previous_cylinderHeight = cylinderHeight;
					previous_cylinderRadius = cylinderRadius;
					return true;
				}
				return false;
			}
			else if (layerShape == LayerShape.Quad)
			{
				if (previous_quadWidth != quadWidth ||
					previous_quadHeight != quadHeight)
				{
					previous_quadWidth = quadWidth;
					previous_quadHeight = quadHeight;
					return true;
				}
				return false;
			}

			return false;
		}

		public CylinderLayerParamAdjustmentMode CurrentAdjustmentMode()
		{
			if (previous_cylinderArcLength != cylinderArcLength)
			{
				return CylinderLayerParamAdjustmentMode.ArcLength;
			}
			else if (previous_angleOfArc != angleOfArc)
			{
				return CylinderLayerParamAdjustmentMode.ArcAngle;
			}
			else
			{
				return CylinderLayerParamAdjustmentMode.Radius;
			}
		}

		public void ChangeBlitShadermode(BlitShaderMode shaderMode, bool enable)
		{
			if (texture2DBlitMaterial == null) return;

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
		#endregion

		#region Monobehavior Lifecycle
		private void Awake()
		{
			//Create blit mat
			texture2DBlitMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/Texture2DBlitShader"));

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
				Log.d(LOG_TAG, "CompositorLayer: Sync");
				if (synchronizer != null)
				{
					synchronizer.sync();
					isSynchronized = true;
				}
			}

			if (isAutoFallbackActive || ((CompositorLayerPlaceholderPrefabInstance != null) && CompositorLayerPlaceholderPrefabInstance.activeSelf)) //Do not submit when auto fallback or placeholder is active
			{
				return;
			}

			if (GetAvaliableCompositorLayerTexture())
			{
				//Log.d(LOG_TAG, "Compositor Layer Texture Get");
				if (SetCompositorLayerContent())
				{
					//Log.d(LOG_TAG, "Compositor Layer Content Set");
					isLayerReadyForSubmit = true;
				}
			}

			//Log.d(LOG_TAG, "Frame Index: " + ++frameIndex);
		}

		//private void OnApplicationFocus(bool hasFocus)
		//{
		//	Log.d(LOG_TAG, "Compositor Layer Lifecycle hasFocus: " + hasFocus);
		//}

		private void OnApplicationPause(bool isPaused)
		{
			Log.d(LOG_TAG, "Compositor Layer Lifecycle isPaused: " + isPaused);
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

		private void OnBeforeRender()
		{
			if (!isInitializationComplete)
			{
				Log.d(LOG_TAG, "Compositor Layer Lifecycle OnBeforeRender: Layer not initialized.");
				return;
			}
			else if (!isLayerReadyForSubmit)
			{
				Log.d(LOG_TAG, "Compositor Layer Lifecycle OnBeforeRender: Layer not ready for submit.");
				return;
			}

			//Get Pose and submit
			GetCompositorLayerPose();
			SubmitCompositorLayer();

			isLayerReadyForSubmit = false; //reset flag after submit
		}

		public void AutoFallbackMeshGeneration()
		{
			Mesh generatedMesh = null;

			switch(layerShape)
			{
				case LayerShape.Quad:
					generatedMesh = MeshGenerationHelper.GenerateQuadMesh(this, MeshGenerationHelper.GenerateQuadVertex(this, this.quadWidth, this.quadHeight));
					break;
				case LayerShape.Cylinder:
					generatedMesh = MeshGenerationHelper.GenerateCylinderMesh(this, MeshGenerationHelper.GenerateCylinderVertex(this, this.cylinderRadius, this.cylinderHeight));
					break;
			}

#if UNITY_EDITOR
			if (generatedUnderlayMesh != null) DestroyImmediate(generatedFallbackMesh);
#elif UNITY_ANDROID
			if (generatedUnderlayMesh != null) Destroy(generatedFallbackMesh);
#endif


			generatedFallbackMesh = new GameObject();
			generatedFallbackMesh.SetActive(false);

			generatedFallbackMesh.name = FallbackMeshName;
			generatedFallbackMesh.transform.SetParent(gameObject.transform);
			generatedFallbackMesh.transform.localPosition = Vector3.zero;
			generatedFallbackMesh.transform.localRotation = Quaternion.identity;

			generatedFallbackMesh.transform.localScale = new Vector3(1, 1, 1);

			generatedFallbackMeshRenderer = generatedFallbackMesh.AddComponent<MeshRenderer>();
			generatedFallbackMeshFilter = generatedFallbackMesh.AddComponent<MeshFilter>();

			generatedFallbackMeshFilter.mesh = generatedMesh;

			Material fallBackMat = new Material(Shader.Find("Unlit/Transparent"));
			fallBackMat.mainTexture = textures[0];
			generatedFallbackMeshRenderer.material = fallBackMat;
		}

		public bool FallbackMeshIsValid()
		{
			if (generatedFallbackMesh == null || generatedFallbackMeshRenderer == null || generatedFallbackMeshFilter == null)
			{
				return false;
			}
#if UNITY_EDITOR
			else if (generatedFallbackMeshFilter.sharedMesh == null || generatedFallbackMeshRenderer.sharedMaterial == null)
			{
				return false;
			}
#elif UNITY_ANDROID
			else if (generatedFallbackMeshFilter.mesh == null || generatedFallbackMeshRenderer.material == null)
			{
				return false;
			}
#endif
			return true;
		}

		public void UnderlayMeshGeneration()
		{
#if UNITY_EDITOR
			if (generatedUnderlayMesh != null) DestroyImmediate(generatedUnderlayMesh);
#elif UNITY_ANDROID
			if (generatedUnderlayMesh != null) Destroy(generatedUnderlayMesh);
#endif

			switch (layerShape)
			{
				case LayerShape.Cylinder:
					//Generate vertices
					Vector3[] cylinderVertices = MeshGenerationHelper.GenerateCylinderVertex(this, cylinderRadius, cylinderHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = CylinderUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = new Vector3(1f, 1f, 1f);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();
					generatedUnderlayMeshRenderer.sharedMaterial = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
					generatedUnderlayMeshRenderer.material.mainTexture = textures[0];

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = MeshGenerationHelper.GenerateCylinderMesh(this, cylinderVertices);
					break;
				case LayerShape.Quad:
				default:
					//Generate vertices
					Vector3[] quadVertices = CompositorLayer.MeshGenerationHelper.GenerateQuadVertex(this, quadWidth, quadHeight);

					//Add components to Game Object
					generatedUnderlayMesh = new GameObject();
					generatedUnderlayMesh.name = CompositorLayer.QuadUnderlayMeshName;
					generatedUnderlayMesh.transform.SetParent(transform);
					generatedUnderlayMesh.transform.localPosition = Vector3.zero;
					generatedUnderlayMesh.transform.localRotation = Quaternion.identity;

					generatedUnderlayMesh.transform.localScale = new Vector3(1, 1, 1);

					generatedUnderlayMeshRenderer = generatedUnderlayMesh.AddComponent<MeshRenderer>();
					generatedUnderlayMeshFilter = generatedUnderlayMesh.AddComponent<MeshFilter>();
					generatedUnderlayMeshRenderer.material = new Material(Shader.Find("Wave/Essence/CompositorLayer/UnderlayAlphaZero"));
					generatedUnderlayMeshRenderer.material.mainTexture = textures[0];

					//Generate Mesh
					generatedUnderlayMeshFilter.mesh = CompositorLayer.MeshGenerationHelper.GenerateQuadMesh(this, quadVertices);
					break;
			}
		}

		public bool UnderlayMeshIsValid()
		{
			if (generatedUnderlayMesh == null || generatedUnderlayMeshRenderer == null || generatedUnderlayMeshFilter == null)
			{
				return false;
			}
#if UNITY_EDITOR
			else if (generatedUnderlayMeshFilter.sharedMesh == null || generatedUnderlayMeshRenderer.sharedMaterial == null)
			{
				return false;
			}
#elif UNITY_ANDROID
			else if (generatedUnderlayMeshFilter.mesh == null || generatedUnderlayMeshRenderer.material == null)
			{
				return false;
			}
#endif

			return true;
		}

#endregion

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

				//Log.d(LOG_TAG, "LayerTextureQueue: TextureQueue is created succesfully for " + eye.ToString());
			}

			public IntPtr GetLayerTextureQueueHandle()
			{
				if (LayerTextureQueueHandle == null)
				{
					Log.d(LOG_TAG, "GetTextureQueueHandle: TextureQueueHandle not found.");
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
			public static Vector3[] GenerateQuadVertex(CompositorLayer target, float quadWidth, float quadHeight)
			{
				Vector3[] vertices = new Vector3[4]; //Four corners

				vertices[0] = new Vector3(target.transform.localPosition.x - quadWidth / 2, target.transform.localPosition.y - quadHeight / 2, target.transform.localPosition.z); //Bottom Left
				vertices[1] = new Vector3(target.transform.localPosition.x + quadWidth / 2, target.transform.localPosition.y - quadHeight / 2, target.transform.localPosition.z); //Bottom Right
				vertices[2] = new Vector3(target.transform.localPosition.x - quadWidth / 2, target.transform.localPosition.y + quadHeight / 2, target.transform.localPosition.z); //Top Left
				vertices[3] = new Vector3(target.transform.localPosition.x + quadWidth / 2, target.transform.localPosition.y + quadHeight / 2, target.transform.localPosition.z); //Top Right

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

			public static Vector3[] GenerateCylinderVertex(CompositorLayer target, float cylinderRadius, float cylinderHeight)
			{
				float angleUpperLimitDeg = target.angleOfArc / 2; //Degrees
				float angleLowerLimitDeg = -angleUpperLimitDeg; //Degrees

				float angleUpperLimitRad = angleUpperLimitDeg * Mathf.Deg2Rad; //Radians
				float angleLowerLimitRad = angleLowerLimitDeg * Mathf.Deg2Rad; //Radians

				int arcSegments = Mathf.RoundToInt(target.angleOfArc / 5f);

				float anglePerArcSegmentRad = (target.angleOfArc / arcSegments) * Mathf.Deg2Rad;

				Vector3[] vertices = new Vector3[2 * (arcSegments + 1)]; //Top and bottom lines * Vertex count per line

				int vertexCount = 0;
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < arcSegments + 1; j++) //Clockwise
					{
						float currentVertexAngleRad = angleLowerLimitRad + anglePerArcSegmentRad * j;
						float x = cylinderRadius * Mathf.Sin(currentVertexAngleRad);
						float y = target.transform.localPosition.y;
						float z = cylinderRadius * Mathf.Cos(currentVertexAngleRad);

						if (i == 1) //Top
						{
							y = y + cylinderHeight / 2;

						}
						else //Bottom
						{
							y = y - cylinderHeight / 2;
						}

						vertices[vertexCount] = new Vector3(x, y, z);
						vertexCount++;
					}
				}

				return vertices;
			}

			public static Mesh GenerateCylinderMesh(CompositorLayer target, Vector3[] vertices)
			{
				Mesh cylinderMesh = new Mesh();
				cylinderMesh.vertices = vertices;
				int arcSegments = Mathf.RoundToInt(target.angleOfArc / 5f);

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
			public static float RadiusAndDegThetaToArcLength(float inDegTheta, float inRadius)
			{
				float arcLength = 0;

				arcLength = inRadius * (inDegTheta * Mathf.Deg2Rad);

				return arcLength;
			}

			public static float RadiusAndArcLengthToDegTheta(float inArcLength, float inRadius)
			{
				float theta = 0;

				theta = (inArcLength / inRadius) * Mathf.Rad2Deg;

				return theta;
			}

			public static float ArcLengthAndDegThetaToRadius(float inArcLength, float inDegTheta)
			{
				float radius = 0;

				radius = (inArcLength / (inDegTheta * Mathf.Deg2Rad));

				return radius;
			}

			public static bool ArcLengthValidityCheck(ref float inArcLength, float inRadius, float thetaLowerLimit, float thetaUpperLimit)
			{
				bool isValid = true;

				if (inArcLength <= 0)
				{
					inArcLength = RadiusAndDegThetaToArcLength(thetaLowerLimit, inRadius);
					isValid = false;
					return isValid;
				}

				float degThetaResult = RadiusAndArcLengthToDegTheta(inArcLength, inRadius);

				if (degThetaResult < thetaLowerLimit)
				{
					inArcLength = RadiusAndDegThetaToArcLength(thetaLowerLimit, inRadius);
					isValid = false;
				}
				else if (degThetaResult > thetaUpperLimit)
				{
					inArcLength = RadiusAndDegThetaToArcLength(thetaUpperLimit, inRadius);
					isValid = false;
				}

				return isValid;
			}

			public static bool RadiusValidityCheck(float inArcLength, ref float inRadius, float thetaLowerLimit, float thetaUpperLimit, CylinderLayerParamLockMode lockMode)
			{
				bool isValid = true;

				if (inRadius <= 0)
				{
					inRadius = ArcLengthAndDegThetaToRadius(inArcLength, thetaUpperLimit);
					isValid = false;
					return isValid;
				}

				float degThetaResult = RadiusAndArcLengthToDegTheta(inArcLength, inRadius);

				if (degThetaResult < thetaLowerLimit)
				{
					if (lockMode == CylinderLayerParamLockMode.ArcAngle) //Angle locked, increase arc length
					{
						ArcLengthValidityCheck(ref inArcLength, inRadius, thetaLowerLimit, thetaUpperLimit);
						inRadius = ArcLengthAndDegThetaToRadius(inArcLength, thetaLowerLimit);
					}
					else if (lockMode == CylinderLayerParamLockMode.ArcLength) //ArcLength Locked, keep angle at min
					{
						inRadius = ArcLengthAndDegThetaToRadius(inArcLength, thetaLowerLimit);
					}
					isValid = false;
				}
				else if (degThetaResult > thetaUpperLimit)
				{
					if (lockMode == CylinderLayerParamLockMode.ArcAngle) //Angle locked, decrease arc length
					{
						ArcLengthValidityCheck(ref inArcLength, inRadius, thetaLowerLimit, thetaUpperLimit);
						inRadius = ArcLengthAndDegThetaToRadius(inArcLength, thetaUpperLimit);
					}
					else if (lockMode == CylinderLayerParamLockMode.ArcLength) //ArcLength Locked, keep angle at max
					{
						inRadius = ArcLengthAndDegThetaToRadius(inArcLength, thetaUpperLimit);
					}
					isValid = false;
				}

				return isValid;
			}

		}
#endregion
	}
}
