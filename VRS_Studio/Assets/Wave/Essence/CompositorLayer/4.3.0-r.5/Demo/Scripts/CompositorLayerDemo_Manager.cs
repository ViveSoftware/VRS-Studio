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
using Wave.Essence.CompositorLayer;

public class CompositorLayerDemo_Manager : MonoBehaviour
{
	public Camera hmd, srcRTCamera;
	public Texture texture512, texture1024;
	public List<Transform> worldLockAnchorList;
	public Material RTContentMaterial;
	public RenderTexture srcRTCameraRenderTexture;
	public int srcCameraRTDimension = 1024;
	public Transform layerContentAnchor;

	protected const string LOG_TAG = "CompositorLayerDemo";

	// Start is called before the first frame update
	protected virtual void Start()
    {
        if (hmd == null)
		{
			hmd = Camera.main;
		}
	}

	public void SetHeadLock()
	{
		layerContentAnchor.SetParent(hmd.transform);
	}

	public void SetWorldLock(int anchorIndex)
	{
		Transform anchorTransform = worldLockAnchorList[anchorIndex];

		layerContentAnchor.SetParent(null);
		layerContentAnchor.SetPositionAndRotation(anchorTransform.position, anchorTransform.rotation);
	}

	public void SetSrcCameraRT(CompositorLayer RTLayer)
	{
		if (srcRTCamera.targetTexture != null) srcRTCamera.targetTexture.Release();

		srcRTCamera.orthographicSize = 0.5f; //Scale 1/2
		srcRTCamera.aspect = 1f; //1:1 Aspect Ratio

		if (srcRTCamera.targetTexture == null)
		{
			srcRTCameraRenderTexture = new RenderTexture(srcCameraRTDimension, srcCameraRTDimension, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			srcRTCameraRenderTexture.hideFlags = HideFlags.DontSave;
			srcRTCameraRenderTexture.useMipMap = false;
			srcRTCameraRenderTexture.filterMode = FilterMode.Trilinear;
			srcRTCameraRenderTexture.anisoLevel = 4;
			srcRTCameraRenderTexture.autoGenerateMips = false;

			srcRTCameraRenderTexture.Create();

			srcRTCamera.targetTexture = srcRTCameraRenderTexture;
		}

		RTLayer.textures[0] = srcRTCameraRenderTexture;

		RTContentMaterial.mainTexture = srcRTCameraRenderTexture;
	}
}
