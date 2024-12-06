using HTC.UnityPlugin.PoseTracker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using VRSStudio.Common;
using VRSStudio.Common.Input;
using VRSStudio.Tracker;
using Wave.Essence;
using Wave.Essence.BodyTracking;
using Wave.Essence.Tracker;
using Wave.Native;
using static Wave.Essence.BodyTracking.HumanoidTracking;

namespace VRSStudio.Avatar
{
    public class VRSBodyTrackingManager : MonoBehaviour
    {
        static string TAG = "AvatarManager";

        public static VRSBodyTrackingManager Instance { get; private set; }

        #region Inspector
        [SerializeField]
        private GameObject displayAvatar;
        [SerializeField]
        private GameObject calibAvatar;
        [SerializeField]
        private HumanoidTracking trackAvatar;

        [SerializeField]
        private SkinnedMeshRenderer body;
        [SerializeField]
        private SkinnedMeshRenderer hair;
        [SerializeField]
        private SkinnedMeshRenderer face;

        [Header("Left Body Settings")]
        [SerializeField]
        private Transform leftUpperLeg;
        [SerializeField]
        private Transform leftLowerLeg;
        [SerializeField]
        private Transform leftFoot;

        [Header("Right Body Settings")]
        [SerializeField]
        private Transform rightUpperLeg;
        [SerializeField]
        private Transform rightLowerLeg;
        [SerializeField]
        private Transform rightFoot;

        // BodyTracking start/stop
        [SerializeField]
        private bool debugBT = false;
        #endregion

        private static readonly InputUsage usagesCtrl = InputUsage.Position | InputUsage.IsTracked | InputUsage.PrimaryButton;
        private static Controller ctrlL = new Controller(true, usagesCtrl);
        private static Controller ctrlR = new Controller(false, usagesCtrl);

        private bool isTracking = false;
        private Vector3 initFootPos = Vector3.zero;
        private bool isAvatarUpdated = false;
        private float calibCamHeight = 0f;
        private List<Component> trackingComponents = new List<Component>();
        private IEnumerator coroutine;

        bool GetCameraYawPose(Transform target, out Vector3 pos, out Quaternion rot)
        {
            if (target == null)
            {
                pos = Vector3.zero;
                rot = Quaternion.identity;
                return false;
            }
            var euler = target.rotation.eulerAngles;
            var forwardDir = Quaternion.Euler(0, euler.y, 0);
            pos = target.position;
            rot = forwardDir;
            return true;
        }

        // Return world position and rotation
        bool GetCameraYawPose(out Vector3 pos, out Quaternion rot)
        {
            if (Camera.main != null)
                return GetCameraYawPose(Camera.main.transform, out pos, out rot);
            else
                return GetCameraYawPose(null, out pos, out rot);
        }

        //Transform htOriginalParent;
        //Pose htOriginalTransform = new Pose();
        Bounds boundsBodyOrigin;

        IEnumerator StartTracking()
        {
            Log.d(TAG, "BeginTracking()");
            Transform rig = null;
            if (rig == null && WaveRig.Instance != null)
                rig = WaveRig.Instance.transform;
            else
                yield break;

            var trackingMode = GetTrackingMode();
            if (trackingMode != TrackingMode.Arm && VRSTrackerManager.Instance)
            {
                var roles = GetRolesByMode(trackingMode);
                VRSTrackerManager.Instance.GetAllTrackerIdByRole(roles, out var tids);
                VRSTrackerManager.Instance.RequestTrackers(tids, OnReallocateTracker);
            }

            bool hasMainCam = false;
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsInTutorial())
            {
                yield return new WaitUntil(() => TutorialManager.Instance.AllowCalibrate());
                trackAvatar.Tracking = trackingMode;
                Log.d(TAG, $"TrackingMode set to {trackAvatar.Tracking}");
                trackAvatar.AvatarOffset = rig.transform;

                displayAvatar.SetActive(false);

                boundsBodyOrigin = body.localBounds;
                // pos and rot are in world coord.
                hasMainCam = GetCameraYawPose(out Vector3 pos, out Quaternion rot);
                if (hasMainCam)
                {
                    var pos_front = pos + rot * Vector3.forward * 1.5f;
                    var rot_180 = Quaternion.Euler(0, 180, 0) * rot;

                    calibAvatar.SetActive(true);
                    calibAvatar.transform.position = new Vector3(pos_front.x, rig.position.y, pos_front.z);
                    calibAvatar.transform.rotation = rot_180;
                }

                yield return new WaitUntil(() => TutorialManager.Instance.IsArmStretched());
                trackAvatar.BeginCalibration();
				yield return new WaitForSeconds(2.5f);
				trackAvatar.BeginTracking();
				isTracking = true;
			}
			else
            {
                trackAvatar.Tracking = trackingMode;
                Log.d(TAG, $"TrackingMode set to {trackAvatar.Tracking}");
                trackAvatar.AvatarOffset = rig.transform;
				trackAvatar.BeginCalibration();

				displayAvatar.SetActive(false);

                boundsBodyOrigin = body.localBounds;
                // pos and rot are in world coord.
                hasMainCam = GetCameraYawPose(out Vector3 pos, out Quaternion rot);
                if (hasMainCam)
                {
                    var pos_front = pos + rot * Vector3.forward * 1.5f;
                    var rot_180 = Quaternion.Euler(0, 180, 0) * rot;

                    calibAvatar.SetActive(true);
                    calibAvatar.transform.position = new Vector3(pos_front.x, rig.position.y, pos_front.z);
                    calibAvatar.transform.rotation = rot_180;
                }

				yield return new WaitForSeconds(2.5f);
				trackAvatar.BeginTracking();
				isTracking = true;
			}

			if (hasMainCam)
                calibAvatar.SetActive(false);

            if (isTracking)
            {
                StartCoroutine(WaitAvatarAndRigUpdate());
            }
        }

        // If tracker is used on other feature.  Stop tracking.
        private void OnReallocateTracker(List<TrackerId> rolesWillBeReallocated)
        {
            StopTracking();
        }

        public void StopTracking()
        {
            if (!isTracking) return;
            Log.d(TAG, "StopTracking()");
            StopCoroutine("StartTracking");
            trackAvatar.StopTracking();

            body.localBounds = boundsBodyOrigin;
            hair.localBounds = boundsBodyOrigin;
            face.localBounds = boundsBodyOrigin;

            displayAvatar.SetActive(true);
            calibAvatar.SetActive(false);
            isTracking = false;
            isAvatarUpdated = false;
            EnableTrackingComponents(false);

            var trackingMode = GetTrackingMode();
            if (trackingMode != TrackingMode.Arm && VRSTrackerManager.Instance)
                VRSTrackerManager.Instance.FreeAllTrackers(OnReallocateTracker);
        }

        public bool IsCalibrating()
        {
            return calibAvatar.activeSelf;
        }

        readonly Timer timerTracking = new Timer(0.75f);
        readonly Timer timerCheckMode = new Timer(0.50f);

        List<InputDevice> inputDevices = new List<InputDevice>();

        TrackingMode availableTrackingMode = TrackingMode.Arm;

        private void UpdateInput()
        {
            if (!ctrlR.dev.isValid) InputDeviceTools.GetController(ctrlR, inputDevices);
            if (ctrlR.dev.isValid) InputDeviceTools.UpdateController(ctrlR);
            if (!ctrlL.dev.isValid) InputDeviceTools.GetController(ctrlL, inputDevices);
            if (ctrlL.dev.isValid) InputDeviceTools.UpdateController(ctrlL);
        }

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                if (leftFoot && rightFoot)
                {
                    initFootPos = (leftFoot.position + rightFoot.position) / 2;
                }

                if (trackAvatar && WaveRig.Instance)
                {
                    FindComponent(trackAvatar.transform, typeof(PoseEaser));
                    FindComponent(trackAvatar.transform, typeof(PoseStablizer));
                    FindComponent(trackAvatar.transform, typeof(PoseTracker));
                    EnableTrackingComponents(false);
                }
            }
        }

        private void FindComponent(Transform transform, Type type)
        {
            Component component = trackAvatar.GetComponent(type);
            if (component != null)
            {
                trackingComponents.Add(component);
            }
        }

        private void EnableTrackingComponents(bool enable)
        {
            foreach (Component component in trackingComponents)
            {
                Type type = component.GetType();
                if (type == typeof(PoseEaser))
                {
                    PoseEaser poseEaser = component as PoseEaser;
                    if (poseEaser != null)
                        poseEaser.enabled = enable;
                }
                else if (type == typeof(PoseStablizer))
                {
                    PoseStablizer poseStablizer = component as PoseStablizer;
                    if (poseStablizer != null)
                        poseStablizer.enabled = enable;
                }
                else if (type == typeof(PoseTracker))
                {
                    PoseTracker poseTracker = component as PoseTracker;
                    if (poseTracker != null)
                        poseTracker.enabled = enable;
                }
            }
        }

        private void Update()
        {
            UpdateInput();

            if (debugBT)
            {
                debugBT = false;
                if (isTracking)
                    StopTracking();
                else
                {
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                    }
                    coroutine = StartTracking();
                    StartCoroutine(coroutine);
                }
            }

			// Log.d(TAG, $"L {btnPriL.IsPressed} R {btnPriR.IsPressed}");

			if (ctrlL.btnPri.IsPressed && ctrlR.btnPri.IsPressed && TutorialManager.Instance.AllowCalibrate())
            {
                if (timerTracking.IsSet)
                {
					if (timerTracking.IsPaused)
                    { }
                    else if (timerTracking.Check())
                    {
						if (isTracking)
                            StopTracking();
                        else
                        {
                            if (coroutine != null)
                            {
                                StopCoroutine(coroutine);
                            }
							coroutine = StartTracking();
                            StartCoroutine(coroutine);
                        }
                    }
                }
                else
                {
                    timerTracking.Set();
                }
            }
            else
            {
                if (timerTracking.IsSet)
                    timerTracking.Reset();
            }

            var hasCamera = GetCameraYawPose(out Vector3 pos, out Quaternion rot);
            if (isTracking && hasCamera)
            {
                // Update AABB of avatar's components
                var center = Quaternion.Inverse(body.transform.rotation) * (pos - body.transform.position);
                if (body != null)
                {
                    var b = body.localBounds;
                    b.center = center;
                    b.size = Vector3.one * 2.5f;
                    body.localBounds = b;
                }
                if (hair != null)
                {
                    var b = hair.localBounds;
                    b.center = center;
                    b.size = Vector3.one + Vector3.up * 0.5f;
                    hair.localBounds = b;
                }
                if (face != null)
                {
                    var b = face.localBounds;
                    b.center = center;
                    b.size = Vector3.one + Vector3.forward * 0.5f;
                    face.localBounds = b;
                }
            }

            if (IsSquatSimulationNeeded())
            {
                float camHeight = Camera.main.transform.position.y - WaveRig.Instance.transform.position.y;
                if (camHeight < calibCamHeight)
                {
                    float maxThreshold = calibCamHeight * 1.00f;
                    float minThreshold = calibCamHeight * 0.60f;
                    int upperLegRot = Mathf.RoundToInt(MappingValue(camHeight, minThreshold, maxThreshold, -130, 0));
                    int lowerLegRot = Mathf.RoundToInt(MappingValue(camHeight, minThreshold, maxThreshold, 130, 0));
                    if (leftUpperLeg)
                    {
                        leftUpperLeg.transform.localRotation = Quaternion.Euler(upperLegRot, 0, 0);
                    }
                    if (rightUpperLeg)
                    {
                        rightUpperLeg.transform.localRotation = Quaternion.Euler(upperLegRot, 0, 0);
                    }
                    if (leftLowerLeg)
                    {
                        leftLowerLeg.transform.localRotation = Quaternion.Euler(lowerLegRot, 0, 0);
                    }
                    if (rightLowerLeg)
                    {
                        rightLowerLeg.transform.localRotation = Quaternion.Euler(lowerLegRot, 0, 0);
                    }
                }
            }

            if (!isTracking && timerCheckMode.Check())
            {
                availableTrackingMode = GetTrackingMode();
            }
        }

        private IEnumerator WaitAvatarAndRigUpdate()
        {
            yield return new WaitUntil(() => IsAvatarAndRigUpdate() == true);
            calibCamHeight = Camera.main.transform.position.y - WaveRig.Instance.transform.position.y;
            EnableTrackingComponents(true);
            isAvatarUpdated = true;
        }

        private bool IsAvatarAndRigUpdate()
        {
            if (leftFoot && rightFoot)
            {
                Vector3 avatarFootPos = (leftFoot.position + rightFoot.position) / 2;
                return avatarFootPos.y != initFootPos.y;
            }
            return false;
        }

        private bool IsSquatSimulationNeeded()
        {
            var trackingMode = GetTrackingMode();
            return isTracking && trackingMode == TrackingMode.Arm && isAvatarUpdated && WaveRig.Instance != null && Camera.main.transform != null;
        }

        private float MappingValue(float currentValue, float minValue, float maxValue, float leftValue, float rightValue)
        {
            // Ensure the current value is within the min and max bounds
            float clampedValue = Mathf.Clamp(currentValue, minValue, maxValue);

            // Normalize the current value within the original range
            float normalizedValue = (clampedValue - minValue) / (maxValue - minValue);

            // Map the normalized value to the new range
            float mappedValue = leftValue + (normalizedValue * (rightValue - leftValue));

            // Ensure the mapped value is within the left and right bounds
            float clampedMappedValue = Mathf.Clamp(mappedValue, Mathf.Min(leftValue, rightValue), Mathf.Max(leftValue, rightValue));

            return clampedMappedValue;
        }

        public TrackingMode GetAvailableTrackingMode()
        {
            return availableTrackingMode;
        }

        public void OnDrawGizmosSelected()
        {
            var bounds = face.localBounds;
            Gizmos.matrix = face.transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
        }

        readonly List<TrackerRole> rolesForArm = new List<TrackerRole>();
        readonly List<TrackerRole> rolesForUpper = new List<TrackerRole>() { TrackerRole.Waist };
        readonly List<TrackerRole> rolesForFull1 = new List<TrackerRole>() { TrackerRole.Waist, TrackerRole.Ankle_Left, TrackerRole.Ankle_Right };
        readonly List<TrackerRole> rolesForFull2 = new List<TrackerRole>() { TrackerRole.Waist, TrackerRole.Knee_Left, TrackerRole.Knee_Right, TrackerRole.Ankle_Left, TrackerRole.Ankle_Right };

        public List<TrackerRole> GetRolesByMode(TrackingMode mode)
        {
            switch (mode)
            {
                default:
                case TrackingMode.Arm:
                    return rolesForArm;
                case TrackingMode.UpperBody:
                    return rolesForUpper;
                case TrackingMode.FullBody:
                    return rolesForFull1;
                case TrackingMode.UpperBodyAndLeg:
                    return rolesForFull2;
            }
        }

        // No need to care controller and hand.  VRSStudio will always have it.
        public TrackingMode GetTrackingMode()
        {
            var tm = VRSTrackerManager.Instance;
            if (tm == null) return TrackingMode.Arm;

            if (tm.IsAllRoleAvailable(rolesForFull2))
                return TrackingMode.UpperBodyAndLeg;  //3
            if (tm.IsAllRoleAvailable(rolesForFull1))
                return TrackingMode.FullBody;  // 2
            if (tm.IsAllRoleAvailable(rolesForUpper))
                return TrackingMode.UpperBody;  // 1
            return TrackingMode.Arm;  // 0
        }

        public void StartBodyTracking()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartTracking();
            StartCoroutine(coroutine);
        }

        public bool IsTracking()
        {
            return isTracking;
        }
    }
}
