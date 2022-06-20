using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Essence;

public class PoseDelayerDebugger : MonoBehaviour
{
    public List<RigidbodyPoseDelayer> delayers;
    public Text debugDisplay;

    private float poseSpeedFactorModifier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        foreach (RigidbodyPoseDelayer poseDelayer in delayers)
        {
            poseDelayer.poseSpeedFactor = poseSpeedFactorModifier;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool modified = false;
        
        if (WXRDevice.ButtonPress(Wave.Native.WVR_DeviceType.WVR_DeviceType_Controller_Right, Wave.Native.WVR_InputId.WVR_InputId_Alias1_B))
        {
            poseSpeedFactorModifier = Mathf.Clamp(poseSpeedFactorModifier + 0.1f, 0.1f, 10f);
            foreach (RigidbodyPoseDelayer poseDelayer in delayers)
            {
                poseDelayer.poseSpeedFactor = poseSpeedFactorModifier;
            }
        }

        if (WXRDevice.ButtonPress(Wave.Native.WVR_DeviceType.WVR_DeviceType_Controller_Right, Wave.Native.WVR_InputId.WVR_InputId_Alias1_A))
        {
            poseSpeedFactorModifier = Mathf.Clamp(poseSpeedFactorModifier - 0.1f, 0.1f, 10f); ;
            foreach (RigidbodyPoseDelayer poseDelayer in delayers)
            {
                poseDelayer.poseSpeedFactor = poseSpeedFactorModifier;
            }
        }

        debugDisplay.text = "Pose change speed modifier: " + poseSpeedFactorModifier;
    }
}
