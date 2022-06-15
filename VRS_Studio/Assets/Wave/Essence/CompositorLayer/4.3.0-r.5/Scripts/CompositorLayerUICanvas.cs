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
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Wave.Essence.CompositorLayer
{
	[RequireComponent(typeof(Canvas))]
	public class CompositorLayerUICanvas : MonoBehaviour
	{
		private Canvas sourceCanvas;
		private RectTransform sourceCanvasRectTransform;
		private Graphic[] graphicComponents;

		private Camera canvasRenderCamera;
		private RenderTexture canvasRenderTexture;

		private CompositorLayer canvasCompositorLayer;

		[Tooltip("Maximum render texture dimension. e.g. If maxRenderTextureSize is 1024, the render texture dimensions of a canvas with an Aspect Ratio of 2:1 will be 1024 x 512.")]
		[SerializeField]
		public uint maxRenderTextureSize = 1024;

		[Tooltip("Overlays render on top of all in-game objects.\nUnderlays can be occluded by in-game objects but may introduce alpha blending issues with transparent objects.")]
		[SerializeField]
		public CompositorLayer.LayerType layerType = CompositorLayer.LayerType.Underlay;

		[Tooltip("Background color of the camera for rendering the Canvas to the render texture target.\nChanging this option will affect the tint of the final image of the Canvas if no background gameobject is assigned.")]
		[SerializeField]
		public Color cameraBGColor = Color.clear;

		[Tooltip("GameObject that contains a UI Component and will be used as the background of the Canvas.\nWhen succesfully assigned, the area which the background UI component covers will no longer be affected by the background color of the camera.")]
		[SerializeField]
		public List<GameObject> backgroundGO = null;

		[Tooltip("Enable this option if transparent UI elements are rendering darker than expected in an overall sense.\nNote that enabling this option will consume more resources.")]
		[SerializeField]
		public bool enableAlphaBlendingCorrection = false;

		[Tooltip("Specify Layer Composition Depth.")]
		[SerializeField]
		public uint compositionDepth = 0;

		[Tooltip("When Auto Fallback is enabled, layers with a higher render priority will be rendered as normal layers first.")]
		[SerializeField]
		private uint renderPriority = 0;
		public uint GetRenderPriority() { return renderPriority; }
		public void SetRenderPriority(uint newRenderPriority)
		{
			renderPriority = newRenderPriority;
			canvasCompositorLayer.SetRenderPriority(renderPriority);
		}

		private void Start()
		{
			sourceCanvas = GetComponent<Canvas>();
			sourceCanvasRectTransform = sourceCanvas.GetComponent<RectTransform>();

			UpdateUIElementBlendMode();

			//Calulate Aspect Ratio of the Canvas
			float canvasRectWidth = sourceCanvasRectTransform.rect.width;
			float canvasRectHeight = sourceCanvasRectTransform.rect.height;

			float canvasAspectRatio_X = 1, canvasAspectRatio_Y = 1;

			if (canvasRectWidth > canvasRectHeight)
			{
				canvasAspectRatio_X = canvasRectWidth / canvasRectHeight;
			}
			else if (canvasRectWidth < canvasRectHeight)
			{
				canvasAspectRatio_Y = canvasRectHeight / canvasRectWidth;
			}

			//Create Render Texture
			int renderTextureWidth = Mathf.CeilToInt(maxRenderTextureSize * canvasAspectRatio_X);
			int renderTextureHeight = Mathf.CeilToInt(maxRenderTextureSize * canvasAspectRatio_Y);

			canvasRenderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			canvasRenderTexture.useMipMap = false;
			canvasRenderTexture.filterMode = FilterMode.Bilinear;
			canvasRenderTexture.autoGenerateMips = false;

			canvasRenderTexture.Create();

			//Create Canvas Rendering Camera
			GameObject canvasRenderCameraGO = new GameObject(name + "_CanvasRenderCamera");
			canvasRenderCameraGO.transform.SetParent(transform, false);

			canvasRenderCamera = canvasRenderCameraGO.AddComponent<Camera>();
			canvasRenderCamera.stereoTargetEye = StereoTargetEyeMask.None;
			canvasRenderCamera.transform.position = transform.position - transform.forward; //1m away from canvas
			canvasRenderCamera.orthographic = true;
			canvasRenderCamera.enabled = false;
			canvasRenderCamera.targetTexture = canvasRenderTexture;
			canvasRenderCamera.cullingMask = 1 << gameObject.layer;
			canvasRenderCamera.clearFlags = CameraClearFlags.SolidColor;
			canvasRenderCamera.backgroundColor = cameraBGColor;

			float widthWithScale = canvasRectWidth * sourceCanvasRectTransform.localScale.x;
			float heightWithScale = canvasRectHeight * sourceCanvasRectTransform.localScale.y;

			canvasRenderCamera.orthographicSize = 0.5f * heightWithScale;

			canvasRenderCamera.nearClipPlane = 0.99f;
			canvasRenderCamera.farClipPlane = 1.01f;

			//Create Compositor Layer Component
			GameObject canvasCompositorLayerGO = new GameObject(name + "_CanvasCompositorLayer");
			canvasCompositorLayerGO.transform.SetParent(transform, false);
			canvasCompositorLayerGO.transform.localPosition = Vector3.zero;
			canvasCompositorLayerGO.transform.localRotation = Quaternion.identity;
			canvasCompositorLayerGO.transform.localScale = Vector3.one;

			canvasCompositorLayer = canvasCompositorLayerGO.AddComponent<CompositorLayer>();
			canvasCompositorLayer.isDynamicLayer = true;
			canvasCompositorLayer.textures[0] = canvasRenderTexture;
			canvasCompositorLayer.layerShape = CompositorLayer.LayerShape.Quad;
			canvasCompositorLayer.layerType = layerType;
			canvasCompositorLayer.quadHeight = heightWithScale;
			canvasCompositorLayer.quadWidth = widthWithScale;
			canvasCompositorLayer.compositionDepth = compositionDepth;
			canvasCompositorLayer.SetRenderPriority(renderPriority);
			if (enableAlphaBlendingCorrection && layerType == CompositorLayer.LayerType.Underlay) 
			{ 
				canvasCompositorLayer.ChangeBlitShadermode(CompositorLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, true); 
			}
			else
			{
				canvasCompositorLayer.ChangeBlitShadermode(CompositorLayer.BlitShaderMode.LINEAR_TO_SRGB_ALPHA, false);
			}
		}

		private void Update()
		{
			canvasRenderCamera.Render();
		}

		private void OnDestroy()
		{
			canvasRenderTexture.Release();
			Destroy(canvasRenderTexture);
		}

		private void OnEnable()
		{
			if (canvasRenderCamera)
			{
				canvasRenderCamera.enabled = true;
			}
		}

		private void OnDisable()
		{
			if (canvasRenderCamera)
			{
				canvasRenderCamera.enabled = false;
			}
		}

		public void ReplaceUIMaterials()
		{
			sourceCanvas = GetComponent<Canvas>();
			graphicComponents = sourceCanvas.GetComponentsInChildren<Graphic>();
			
			Material underlayCanvasUIMat = new Material(Shader.Find("Wave/Essence/CompositorLayerUICanvas/MultiLayerCanvasUI"));

			foreach (Graphic graphicComponent in graphicComponents)
			{
				if (backgroundGO != null && backgroundGO.Contains(graphicComponent.gameObject))
				{
					graphicComponent.material = new Material(Shader.Find("Wave/Essence/CompositorLayerUICanvas/MultiLayerCanvasUI")); //Seperate material instance for background
				}
				else
				{
					graphicComponent.material = underlayCanvasUIMat;
				}
			}
		}

		public void UpdateUIElementBlendMode()
		{
			sourceCanvas = GetComponent<Canvas>();
			graphicComponents = sourceCanvas.GetComponentsInChildren<Graphic>();

			foreach (Graphic graphicComponent in graphicComponents)
			{
				if (backgroundGO != null && backgroundGO.Contains(graphicComponent.gameObject))
				{
					SetUIShaderBlendMode(graphicComponent.material, UIShaderBlendMode.Background);
				}
				else
				{
					SetUIShaderBlendMode(graphicComponent.material, UIShaderBlendMode.Others);
				}
			}
		}

		public void SetUIShaderBlendMode(Material canvasUIMaterial, UIShaderBlendMode blendMode = UIShaderBlendMode.Others)
		{
			switch (blendMode)
			{
				case UIShaderBlendMode.Background: //Discard camera background color and alpha values
					canvasUIMaterial.SetInt("_SrcColBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstColBlendMode", (int)BlendMode.Zero);
					canvasUIMaterial.SetInt("_SrcAlpBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstAlpBlendMode", (int)BlendMode.Zero);

					break;

				case UIShaderBlendMode.Others: //Nornmal transparency blending
				default:
					canvasUIMaterial.SetInt("_SrcColBlendMode", (int)BlendMode.SrcAlpha);
					canvasUIMaterial.SetInt("_DstColBlendMode", (int)BlendMode.OneMinusSrcAlpha);
					canvasUIMaterial.SetInt("_SrcAlpBlendMode", (int)BlendMode.One);
					canvasUIMaterial.SetInt("_DstAlpBlendMode", (int)BlendMode.OneMinusSrcAlpha);

					break;
			}
		}

		public enum UIShaderBlendMode
		{
			Background,
			Others,
		}
	}
}
