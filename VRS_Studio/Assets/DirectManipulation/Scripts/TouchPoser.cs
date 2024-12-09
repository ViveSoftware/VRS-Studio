using HTC.UnityPlugin.CommonEventVariable;
using HTC.UnityPlugin.Pointer3D;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using UnityEngine;

public class TouchPoser : MonoBehaviour
{
    [SerializeField]
    private bool isRightHand;

    [SerializeField]
    private bool isController;

    [SerializeField]
    [Tooltip("The visual model tracks the transform.")]
    private Transform _targetTransform;

    [SerializeField]
    [Tooltip("The touch origin tracks the transform.")]
    private Transform _pointTransform;

    [SerializeField]
    [Tooltip("The wrist scale align with the transform.")]
    [ConditionalHide("isController", true, true)]
    private Transform _wristJoint;

    [SerializeField]
    [ConditionalHide("isController", true, true)]
    private GameObject touchPointerPrefabs;
    [SerializeField]
    [ConditionalHide("isController", true)]
    private TouchRaycaster touchRaycaster;
    private CanvasRaycastMethod touchCanvasMethod;
    private PhysicsRaycastMethod touchPhysicsMethod;

    private CommonVariableHandler<bool> isLeftHandTouching = CommonVariable.Get<bool>("LeftHand_isTouching");
    private CommonVariableHandler<bool> isLeftHandHovering = CommonVariable.Get<bool>("LeftHand_isHovering");
    private CommonVariableHandler<bool> isRightHandTouching = CommonVariable.Get<bool>("RightHand_isTouching");
    private CommonVariableHandler<bool> isRightHandHovering = CommonVariable.Get<bool>("RightHand_isHovering");
    private CommonVariableHandler<bool> isTouchPanelMoving = CommonVariable.Get<bool>("Common_CurrentPanelMovingState");

    private void Awake()
    {
        if (!isController)
            _wristJoint = transform.GetChild(1);
    }

    private void Start()
    {
        if (touchRaycaster == null && !isController)
        {
            GameObject touchPointer = Instantiate(touchPointerPrefabs);
            touchPointer.transform.parent = GameObject.Find("VROrigin").transform;
            touchPointer.transform.localPosition = Vector3.zero;
            touchPointer.transform.localRotation = Quaternion.identity;
            touchRaycaster = touchPointer.GetComponentInChildren<TouchRaycaster>();
        }
        touchCanvasMethod = touchRaycaster.GetComponent<CanvasRaycastMethod>();
        touchPhysicsMethod = touchRaycaster.GetComponent<PhysicsRaycastMethod>();
    }

    private void OnEnable()
    {
        if (touchRaycaster)
            touchRaycaster.enabled = true;
    }

    private void OnDisable()
    {
        if (touchRaycaster)
            touchRaycaster.enabled = false;

        if (!isRightHand)
        {
            isLeftHandHovering.SetValue(false);
            isLeftHandTouching.SetValue(false);
        }
        else
        {
            isRightHandHovering.SetValue(false);
            isRightHandTouching.SetValue(false);
        }
    }

    private void Update()
    {
        touchCanvasMethod.enabled = touchPhysicsMethod.enabled = !isTouchPanelMoving.CurrentValue;

        if (_wristJoint != null)
            _wristJoint.localScale = _targetTransform.localScale;

        if (!isRightHand)
        {
            isLeftHandHovering.SetValue(touchRaycaster.FirstRaycastResult().gameObject);
            isLeftHandTouching.SetValue(touchRaycaster.FirstRaycastResult().gameObject &&
                touchRaycaster.CurrentFrameHitRange < touchRaycaster.mouseButtonLeftRange);
        }
        else
        {
            isRightHandHovering.SetValue(touchRaycaster.FirstRaycastResult().gameObject);
            isRightHandTouching.SetValue(touchRaycaster.FirstRaycastResult().gameObject &&
                touchRaycaster.CurrentFrameHitRange < touchRaycaster.mouseButtonLeftRange);
        }
    }

    private void FixedUpdate()
    {
        bool isTouching = isRightHand ? isRightHandTouching.CurrentValue : isLeftHandTouching.CurrentValue;

        if (!isTouching)
        {
            var origin = default(Vector3);
            var direction = default(Vector3);
            bool isVaild;

#if UNITY_EDITOR
            if (!isController)
            {
                isVaild = true;
            }
            else
            {
                if (isRightHand) isVaild = ViveInput.GetTriggerValue(HandRole.RightHand) < 0.5f;
                else isVaild = ViveInput.GetTriggerValue(HandRole.LeftHand) < 0.5f;
            }
#else
            if (!isController)
            {
                if (isRightHand) isVaild = WaveHandTrackingSubmodule.TryGetRightPinchRay(out origin, out direction);
                else isVaild = WaveHandTrackingSubmodule.TryGetLeftPinchRay(out origin, out direction);
            }
            else
            {
                if (isRightHand) isVaild = ViveInput.GetTriggerValue(HandRole.RightHand) < 0.5f;
                else isVaild = ViveInput.GetTriggerValue(HandRole.LeftHand) < 0.5f;
            }
#endif

            if (!isController)
                touchRaycaster.enabled = isVaild;
            else
            {
                if (touchRaycaster.FirstRaycastResult().gameObject == null)                
                    touchRaycaster.enabled = isVaild;                
                else
                    touchRaycaster.enabled = true;
            }
        }

        MoveUsingTransform(isTouching);
        RotateUsingTransform();
    }

    private void MoveUsingTransform(bool isTouching)
    {
        if (!isTouching)
        {
            if (!isController)
                transform.position = Vector3.Lerp(transform.position, _targetTransform.position, 0.95f);
            else
            {
                touchRaycaster.transform.parent.position = _pointTransform.position;
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, 0.95f);                
            }
        }
        else
        {
            Vector3 positionDelta = _targetTransform.position - _pointTransform.position;
            Vector3 targetPosePosition = touchRaycaster.BreakPoints[1] + positionDelta;
            transform.position = Vector3.Lerp(transform.position, targetPosePosition, 0.5f);
        }
    }

    private void RotateUsingTransform()
    {
        if (!isController)
            transform.localEulerAngles = _targetTransform.localEulerAngles + new Vector3(180, 0, 180);
        else
            transform.localEulerAngles = Vector3.zero;
    }
}