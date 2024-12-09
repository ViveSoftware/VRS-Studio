using UnityEngine;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.CommonEventVariable;
using Wave.Native;
using System.Collections.Generic;

public class ControllerHandModelHandler : MonoBehaviour
{
    private Animator animator;

    public HandRole m_viveRole = HandRole.RightHand;

    private CommonVariableHandler<bool> isLeftHandHovering = CommonVariable.Get<bool>("LeftHand_isHovering");
    private CommonVariableHandler<bool> isRightHandHovering = CommonVariable.Get<bool>("RightHand_isHovering");

    private CommonVariableHandler<bool> isLeftHandTouching = CommonVariable.Get<bool>("LeftHand_isTouching");
    private CommonVariableHandler<bool> isRightHandTouching = CommonVariable.Get<bool>("RightHand_isTouching");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ViveInput.AddPressDown(m_viveRole, ControllerButton.AKey, OnAKeyDownEvent);
        ViveInput.AddPressDown(m_viveRole, ControllerButton.BKey, OnBKeyDownEvent);
        ViveInput.AddPress(m_viveRole, ControllerButton.AKeyTouch, OnAKeyDownEvent);
        ViveInput.AddPress(m_viveRole, ControllerButton.BKeyTouch, OnBKeyDownEvent);
        ViveInput.AddPressUp(m_viveRole, ControllerButton.AKey, OnAKeyUpEvent);
        ViveInput.AddPressUp(m_viveRole, ControllerButton.BKey, OnBKeyUpEvent);
        ViveInput.AddPressUp(m_viveRole, ControllerButton.AKeyTouch, OnAKeyUpEvent);
        ViveInput.AddPressUp(m_viveRole, ControllerButton.BKeyTouch, OnBKeyUpEvent);

        isLeftHandTouching.OnChange += TouchVibration;
        isRightHandTouching.OnChange += TouchVibration;
    }

    private void OnDisable()
    {
        ViveInput.RemovePressDown(m_viveRole, ControllerButton.AKey, OnAKeyDownEvent);
        ViveInput.RemovePressDown(m_viveRole, ControllerButton.BKey, OnBKeyDownEvent);
        ViveInput.RemovePress(m_viveRole, ControllerButton.AKeyTouch, OnAKeyDownEvent);
        ViveInput.RemovePress(m_viveRole, ControllerButton.BKeyTouch, OnBKeyDownEvent);
        ViveInput.RemovePressUp(m_viveRole, ControllerButton.AKey, OnAKeyUpEvent);
        ViveInput.RemovePressUp(m_viveRole, ControllerButton.BKey, OnBKeyUpEvent);
        ViveInput.RemovePressUp(m_viveRole, ControllerButton.AKeyTouch, OnAKeyUpEvent);
        ViveInput.RemovePressUp(m_viveRole, ControllerButton.BKeyTouch, OnBKeyUpEvent);

        isLeftHandTouching.OnChange -= TouchVibration;
        isRightHandTouching.OnChange -= TouchVibration;
    }

    float tempTriggerValue;
    float tempStickValue;
    private void Update()
    {
        //Update trigger touch state
        WVR_Controller_UpdateTriggerTouch(m_viveRole == HandRole.RightHand ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left);

        tempTriggerValue = animator.GetFloat("TriggerValue");

        if (ViveInput.GetTriggerValue(m_viveRole) > 0)
            tempTriggerValue = Mathf.Lerp(tempTriggerValue, ViveInput.GetTriggerValue(m_viveRole), 0.5f);
        else if (isTriggerTouch)
            tempTriggerValue = Mathf.Lerp(tempTriggerValue, 0f, 0.5f);
        else
            tempTriggerValue = Mathf.Lerp(tempTriggerValue, -1f, 0.5f);

        animator.SetFloat("TriggerValue", tempTriggerValue);

        animator.SetFloat("GripValue", ViveInput.GetAxis(m_viveRole, ControllerAxis.CapSenseGrip));
        animator.SetFloat("XAxis", ViveInput.GetAxis(m_viveRole, ControllerAxis.JoystickX));
        animator.SetFloat("YAxis", ViveInput.GetAxis(m_viveRole, ControllerAxis.JoystickY));

        tempStickValue = animator.GetFloat("StickValue");
        if (ViveInput.GetPressDown(m_viveRole, ControllerButton.Joystick))
            tempStickValue = Mathf.Lerp(tempStickValue, 1f, 0.5f);
        else if (ViveInput.GetPress(m_viveRole, ControllerButton.JoystickTouch))
            tempStickValue = Mathf.Lerp(tempStickValue, 0.5f, 0.5f);
        else
            tempStickValue = Mathf.Lerp(tempStickValue, 0f, 0.35f);

        animator.SetFloat("StickValue", tempStickValue);
    }

    private void OnAKeyDownEvent()
    {
        animator.SetTrigger("AKeyDown");
        animator.SetTrigger("XKeyDown");
    }
    private void OnAKeyUpEvent()
    {
        animator.SetTrigger("AKeyUp");
        animator.SetTrigger("XKeyUp");
    }
    private void OnBKeyDownEvent()
    {
        animator.SetTrigger("BKeyDown");
        animator.SetTrigger("YKeyDown");
    }
    private void OnBKeyUpEvent()
    {
        animator.SetTrigger("BKeyUp");
        animator.SetTrigger("YKeyUp");
    }

    private void TouchVibration()
    {
        var isTouching = (m_viveRole == HandRole.RightHand) && isRightHandTouching.CurrentValue
            || (m_viveRole == HandRole.LeftHand) && isLeftHandTouching.CurrentValue;

        if (isTouching) ViveInput.TriggerHapticVibration(m_viveRole, 0.05f);
    }

    #region Check trigger touch by WVR
    bool isTriggerTouch;
    uint inputTypeLeft = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
    uint inputTypeRight = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);

    Dictionary<WVR_DeviceType, WVR_AnalogState_t[]> analogState = new Dictionary<WVR_DeviceType, WVR_AnalogState_t[]>()
    {
        { WVR_DeviceType.WVR_DeviceType_Controller_Right, null },
        { WVR_DeviceType.WVR_DeviceType_Controller_Left, null },
    };

    private void WVR_Controller_UpdateTriggerTouch(WVR_DeviceType dev)
    {
        uint inputType = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
        switch (dev)
        {
            case WVR_DeviceType.WVR_DeviceType_Controller_Left:
                inputType = inputTypeLeft;
                break;
            case WVR_DeviceType.WVR_DeviceType_Controller_Right:
                inputType = inputTypeRight;
                break;
            default:
                return;
        }

        uint buttons = 0, touches = 0;
        int analogCount = Interop.WVR_GetInputTypeCount(dev, WVR_InputType.WVR_InputType_Analog);

        if (analogCount > 0)
        {
            if (analogState[dev] == null || analogState[dev].Length < analogCount)
            {
                analogState[dev] = new WVR_AnalogState_t[analogCount];
            }

            if (Interop.WVR_GetInputDeviceState(dev, inputType, ref buttons, ref touches, analogState[dev], (uint)analogCount))
            {
                int input = 1 << (int)(uint)WVR_InputId.WVR_InputId_17;
                isTriggerTouch = ((touches & input) == input);
            }
        }
    }
    #endregion 

    public void SetBoolTrue(string name) => animator.SetBool(name, true);
    public void SetBoolFalse(string name) => animator.SetBool(name, false);
}