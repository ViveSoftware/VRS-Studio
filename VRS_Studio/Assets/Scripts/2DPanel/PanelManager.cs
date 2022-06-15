using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject TouchPanel, FrontPanel;
    public RobotScenarioControllerBase robotAssistantController;
    private Vector3 Panel_OffsetFromHMD = new Vector3(0f, -0.15f, 0.45f);

    void Start()
    {
        if (VRSStudioCameraRig.Instance != null)
        {
            var pos = VRSStudioCameraRig.Instance.HMD.transform.position + Panel_OffsetFromHMD;
            TouchPanel.transform.position += pos;
            FrontPanel.transform.position += pos;
            robotAssistantController.transform.position += pos;

            if (robotAssistantController != null)
            {
                robotAssistantController.InitializeRobot();
            }
        }
    }
}
