
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using UnityEngine;
using UnityEngine.UI;

public class InVRDebugger : MonoBehaviour
{
    public Text debugText;
    private ViveRoleProperty leftTrackedHandRole = ViveRoleProperty.New(TrackedHandRole.LeftHand);
    private ViveRoleProperty rightTrackedHandRole = ViveRoleProperty.New(TrackedHandRole.RightHand);

    private void Update()
    {
        debugText.text = "left: " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.Trigger).ToString("0.00000")
            + " " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.CapSenseGrip).ToString("0.00000")
            + " " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.IndexCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.MiddleCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.RingCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(leftTrackedHandRole, ControllerAxis.PinkyCurl).ToString("0.00000")
            + " " + ViveInput.GetPress(leftTrackedHandRole, ControllerButton.Grip)
            + " " + ViveInput.GetPress(leftTrackedHandRole, ControllerButton.GripTouch)
            + "\nright: " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.Trigger).ToString("0.00000")
            + " " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.CapSenseGrip).ToString("0.00000")
            + " " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.IndexCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.MiddleCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.RingCurl).ToString("0.00000")
            + " " + ViveInput.GetAxis(rightTrackedHandRole, ControllerAxis.PinkyCurl).ToString("0.00000")
            + " " + ViveInput.GetPress(rightTrackedHandRole, ControllerButton.Grip)
            + " " + ViveInput.GetPress(rightTrackedHandRole, ControllerButton.GripTouch);
    }
}