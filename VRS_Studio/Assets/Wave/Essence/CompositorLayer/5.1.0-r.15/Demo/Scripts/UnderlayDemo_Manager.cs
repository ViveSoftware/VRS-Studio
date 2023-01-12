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

public class UnderlayDemo_Manager : CompositorLayerDemo_Manager
{
	public Transform underlayContentStatic, underlayContentDynamic, underlayAlphaPanelStatic, underlayAlphaPanelDynamic;
	public CompositorLayer cameraRTUnderlay;

	protected override void Start()
    {
		base.Start();
		SetSrcCameraRT(cameraRTUnderlay);
    }

	public void ToggleAlphaPanel(bool enable)
	{
		underlayAlphaPanelStatic.gameObject.SetActive(enable);
		underlayAlphaPanelDynamic.gameObject.SetActive(enable);
	}
}
