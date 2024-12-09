using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class ViveCloneColliderEventCaster : ColliderEventCaster, IViveRoleComponent
{
    [SerializeField]
    private ViveRoleProperty m_viveRole = ViveRoleProperty.New(HandRole.RightHand);
    [SerializeField]
    [CustomOrderedEnum]
    private ControllerButton m_buttonTrigger = ControllerButton.Trigger;
    [SerializeField]
    [CustomOrderedEnum]
    private ControllerButton m_buttonPadOrStick = ControllerButton.Pad;
    [SerializeField]
    [CustomOrderedEnum]
    private ControllerButton m_buttonGripOrHandTrigger = ControllerButton.Grip;
    [SerializeField]
    [CustomOrderedEnum]
    private ControllerButton m_buttonFunctionKey = ControllerButton.Menu;
    [SerializeField]
    [FlagsFromEnum(typeof(ControllerButton))]
    private ulong m_additionalButtons = 0ul;
    [SerializeField]
    private ScrollType m_scrollType = ScrollType.Auto;
    [SerializeField]
    private Vector2 m_scrollDeltaScale = new Vector2(1f, -1f);

    public ViveRoleProperty viveRole { get { return m_viveRole; } }
    public ScrollType scrollType { get { return m_scrollType; } set { m_scrollType = value; } }
    public Vector2 scrollDeltaScale { get { return m_scrollDeltaScale; } set { m_scrollDeltaScale = value; } }
#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        FilterOutAssignedButton();
    }
#endif
    protected void FilterOutAssignedButton()
    {
        EnumUtils.SetFlag(ref m_additionalButtons, (int)m_buttonTrigger, false);
        EnumUtils.SetFlag(ref m_additionalButtons, (int)m_buttonPadOrStick, false);
        EnumUtils.SetFlag(ref m_additionalButtons, (int)m_buttonFunctionKey, false);
        EnumUtils.SetFlag(ref m_additionalButtons, (int)m_buttonGripOrHandTrigger, false);
    }

    protected virtual void Start()
    {
        buttonEventDataList.Add(new ViveCloneColliderButtonEventData(this, m_buttonTrigger, ColliderButtonEventData.InputButton.Trigger));
        if (m_buttonPadOrStick != ControllerButton.None) { buttonEventDataList.Add(new ViveCloneColliderButtonEventData(this, m_buttonPadOrStick, ColliderButtonEventData.InputButton.PadOrStick)); }
        if (m_buttonGripOrHandTrigger != ControllerButton.None) { buttonEventDataList.Add(new ViveCloneColliderButtonEventData(this, m_buttonGripOrHandTrigger, ColliderButtonEventData.InputButton.GripOrHandTrigger)); }
        if (m_buttonFunctionKey != ControllerButton.None) { buttonEventDataList.Add(new ViveCloneColliderButtonEventData(this, m_buttonFunctionKey, ColliderButtonEventData.InputButton.FunctionKey)); }

        FilterOutAssignedButton();

        var eventBtn = ColliderButtonEventData.InputButton.GripOrHandTrigger + 1;
        var addBtns = m_additionalButtons;
        for (ControllerButton btn = 0; addBtns > 0u; ++btn, addBtns >>= 1)
        {
            if ((addBtns & 1u) == 0u) { continue; }

            buttonEventDataList.Add(new ViveCloneColliderButtonEventData(this, btn, eventBtn++));
        }

        axisEventDataList.Add(new ViveCloneColliderPadAxisEventData(this));
        axisEventDataList.Add(new ViveCloneColliderTriggerAxisEventData(this));
    }
}

public class ViveCloneColliderButtonEventData : ColliderButtonEventData
{
    public ViveCloneColliderEventCaster viveEventCaster { get; private set; }
    public ControllerButton viveButton { get; private set; }

    public ViveRoleProperty viveRole { get { return viveEventCaster.viveRole; } }

    private int updateFrame = -1;
    private bool prevValue;
    private bool currValue;

    public ViveCloneColliderButtonEventData(ViveCloneColliderEventCaster eventCaster, ControllerButton viveButton, InputButton button) : base(eventCaster, button)
    {
        this.viveEventCaster = eventCaster;
        this.viveButton = viveButton;
    }

    private void UpdateValue()
    {
        var frameCount = Time.frameCount;
        if (updateFrame == frameCount) { return; }
        updateFrame = frameCount;
        prevValue = currValue;
        if (VivePose.IsValid(viveRole))
        {
            currValue = ViveInput.GetPressEx(viveRole.roleType, viveRole.roleValue, viveButton);
        }
        else
        {
            currValue = prevValue;
        }
    }

    public override bool GetPress() { UpdateValue(); return currValue; }

    public override bool GetPressDown() { UpdateValue(); return !prevValue && currValue; }

    public override bool GetPressUp() { UpdateValue(); return prevValue && !currValue; }
}

public class ViveCloneColliderTriggerAxisEventData : ColliderAxisEventData
{
    public ViveCloneColliderEventCaster viveEventCaster { get; private set; }

    public ViveRoleProperty viveRole { get { return viveEventCaster.viveRole; } }

    private int updateFrame = -1;
    private float prevValue;
    private float currValue;

    public ViveCloneColliderTriggerAxisEventData(ViveCloneColliderEventCaster eventCaster) : base(eventCaster, Dim.D1, InputAxis.Trigger1D)
    {
        viveEventCaster = eventCaster;
    }

    private void UpdateValue()
    {
        var frameCount = Time.frameCount;
        if (updateFrame == frameCount) { return; }
        updateFrame = frameCount;
        prevValue = currValue;
        if (VivePose.IsValid(viveRole))
        {
            currValue = ViveInput.GetTriggerValueEx(viveRole.roleType, viveRole.roleValue, false);
        }
        else
        {
            currValue = prevValue;
        }
    }

    public override Vector4 GetDelta()
    {
        UpdateValue();
        return new Vector4(currValue - prevValue, 0f);
    }
}

public class ViveCloneColliderPadAxisEventData : ColliderAxisEventData
{
    public ViveCloneColliderEventCaster viveEventCaster { get; private set; }

    public ViveRoleProperty viveRole { get { return viveEventCaster.viveRole; } }

    private int updateFrame = -1;
    private Vector2 prevValue;
    private Vector2 currValue;

    public ViveCloneColliderPadAxisEventData(ViveCloneColliderEventCaster eventCaster) : base(eventCaster, Dim.D2, InputAxis.Scroll2D)
    {
        viveEventCaster = eventCaster;
    }

    private void UpdateValue()
    {
        var frameCount = Time.frameCount;
        if (updateFrame == frameCount) { return; }
        updateFrame = frameCount;
        prevValue = currValue;
        if (VivePose.IsValid(viveRole))
        {
            currValue = ViveInput.GetScrollDelta(viveRole, viveEventCaster.scrollType, viveEventCaster.scrollDeltaScale);
        }
        else
        {
            currValue = prevValue;
        }
    }

    public override Vector4 GetDelta() { UpdateValue(); return currValue; }
}