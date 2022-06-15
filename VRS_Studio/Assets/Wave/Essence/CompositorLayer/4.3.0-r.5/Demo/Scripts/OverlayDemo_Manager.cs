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
using Wave.Native;

public class OverlayDemo_Manager : CompositorLayerDemo_Manager
{
	public Transform overlayContentStatic, overlayContentDynamic;
	public CompositorLayer cameraRTOverlay, staticOverlay;

	public Material EyeBufferContentUnlit;
	public TextureDisplayArray[] LRTextureDisplays;

	[System.Serializable]
	public class TextureDisplayArray
	{
		public MeshRenderer[] TextureDisplays;
	}

	protected override void Start()
    {
		base.Start();
		SetSrcCameraRT(cameraRTOverlay);

		for (int i=0; i< LRTextureDisplays.Length; i++)
		{
			for (int j = 0; j < LRTextureDisplays[i].TextureDisplays.Length; j++)
				LRTextureDisplays[i].TextureDisplays[j].material = new Material(EyeBufferContentUnlit);
		}
    }

	private void Update()
	{
		if (staticOverlay.textureQueues != null)
		{
			for (int i=0; i<staticOverlay.textureQueues.Length; i++)
			{
				for (int j = 0; j < staticOverlay.textureQueues[i].ExternalTextures.Length; j++)
				{
					if (staticOverlay.textureQueues[i].TextureContentSet[j])
					{
						LRTextureDisplays[i].TextureDisplays[j].material.mainTexture = staticOverlay.textureQueues[i].ExternalTextures[j];
						//Log.d(LOG_TAG, "Display texture i: " + i + " j: " + j);
					}
				}
			}
		}
	}

	public void ChangeTextureSize()
	{
		if (staticOverlay.textures[0].width == texture1024.width)
		{
			staticOverlay.textures[0] = texture512;
			staticOverlay.textures[1] = texture512;
			Log.d(LOG_TAG, "ChangeTextureSize original 1024");
		}
		else if (staticOverlay.textures[0].width == texture512.width)
		{
			staticOverlay.textures[0] = texture1024;
			staticOverlay.textures[1] = texture1024;
			Log.d(LOG_TAG, "ChangeTextureSize original 512");
		}
		Log.d(LOG_TAG, "ChangeTextureSize L to width: " + staticOverlay.textures[0].width + " height: " + staticOverlay.textures[0].height);
		Log.d(LOG_TAG, "ChangeTextureSize R to width: " + staticOverlay.textures[1].width + " height: " + staticOverlay.textures[1].height);
	}
}
