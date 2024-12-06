using TMPro;
using UnityEngine;
using UnityEngine.XR;
using VRSStudio.Common.Input;
using VRSStudio.Common;
using Wave.Essence;
using Wave.Native;
using HTC.UnityPlugin.ColliderEvent;
using Wave.Essence.Tracker;
using VRSStudio.Spectator;

namespace VRSStudio.Tracker
{
    public class TrackerIndicator : MonoBehaviour
        , IColliderEventHoverEnterHandler
        , IColliderEventHoverExitHandler
    {
        VRSStudio.Common.Input.Tracker tracker = new Common.Input.Tracker(Common.Input.Tracker.TrackerID.Tracker0, InputUsage.Position | InputUsage.Rotation);
        public GameObject indicator;
        public GameObject selectedTracker;
        public TextMeshPro text;
        /// <summary>
        /// if tracker is in front of Camera within the showFOV angle, show the indicator.
        /// </summary>
        [Range(30, 179)]
        public float showFOV = 40;

        /// <summary>
        /// if tracker is not in front of Camera within the hideFOV angle, hide the indicator.  Could be wider than showFOV.  Let it hard to hide.
        /// </summary>
        [Range(30, 179)]
        public float hideFOV = 70;

        /// <summary>
        /// A timer for show tracker.  If look at the tracker, how long will the tracker show.  
        /// </summary>
        [Range(0.01f, 3)]
        public float timeShow = 0.5f;

        /// <summary>
        /// A timer for hide tracker.  Could be longer than timeShow.  Let it hard to hide.
        /// </summary>
        [Range(0.01f, 3)]
        public float timeHide = 1;

        public Material indicatorMat;
        public Material invalidMat;

        Timer timerShow;
        Timer timerHide;

        public bool IsShow { get; private set; }

        private void Awake()
        {
            timerShow = new Timer(timeShow);
            timerHide = new Timer(timeHide);
        }

        private Transform GetRig()
        {
            if (WaveRig.Instance != null) return WaveRig.Instance.transform;
            else if (Camera.main != null) return Camera.main.transform.parent;
            else return null;
        }

        public void SetDevice(int trackerId, InputDevice dev, string name)
        {
            tracker.trackerId = (Common.Input.Tracker.TrackerID)trackerId;
            tracker.dev = dev;
            if (text) text.text = name.Contains("Tracker") ? "Tracker" : name;
        }

        public int GetTrackerId()
        {
            return (int)tracker.trackerId;
        }

        public InputDevice GetInputDevice()
        {
            return tracker.dev;
        }

        // Update is called once per frame
        void Update()
        {
            //Log.d("TrackerIndicator", "n=" + name + " timerGone=" + timerHide + ", timerShow=" + timerShow);

            if (timerHide.Check())
            {
                indicator.SetActive(false);
                selectedTracker.SetActive(false);
                IsShow = false;
            }

            if (timerShow.Check())
            {
                var tm = TrackerManager.Instance;
                var tid = (TrackerId)(int)tracker.trackerId;
                var role = tm.GetTrackerRole(tid);

                if (role == TrackerRole.Standalone)
                {
                    var ctid = VRSSpectatorManager.Instance.GetCurrentTracker();
                    if (tid == ctid)
                    {
                        indicator.SetActive(false);
                        selectedTracker.SetActive(true);
                    }
                    else
                    {
                        if ((int)ctid == -1)
                        {
                            VRSSpectatorManager.Instance.SetTracker(tid);
                        }
                        else
                        {
                            selectedTracker.SetActive(false);
                            indicator.SetActive(true);
                        }
                    }
                }
                else
                {
                    selectedTracker.SetActive(false);
                    indicator.SetActive(true);
                }

                IsShow = true;
            }

            var rig = GetRig();
            if (!rig) return;
            if (!tracker.dev.isValid)
            {
                ShowIndicator(false, true);
                return;
            }
            InputDeviceTools.UpdateXRTrackingDevice(tracker, InputUsage.Position | InputUsage.Rotation);

            // If already show, the hideFOV will be wider.
            ShowIndicator(IsInFov(IsShow ? hideFOV : showFOV));
        }

        public void Show()
        {
            ShowIndicator(true, true);
        }

        public void Hide()
        {
            ShowIndicator(false, true);
        }

        bool IsInFov(float fov)
        {
            var rig = GetRig();
            if (!rig) return true;

            // Use rig, make tracker pose to world space
            var pos = rig.TransformPoint(tracker.position);
            transform.position = pos;
            transform.rotation = rig.rotation * tracker.rotation;

            var cam = Camera.main;
            if (cam == null) return true;
            var dir = (pos - cam.transform.position).normalized;
            var threshold = fov / 2 * Mathf.Deg2Rad;
            var angle = Mathf.Acos(Vector3.Dot(cam.transform.forward, dir));
            //Log.d("TrackerIndicator", "n=" + name + "angle = " + angle * Mathf.Rad2Deg);
            return angle < threshold;
        }


        public void ShowIndicator(bool show, bool force = false)
        {
            if (show && (!indicator.activeInHierarchy))
            {
                if (!timerShow.IsSet)
                    timerShow.Set();
                if (force)
                    timerShow.Timeout();

                // Cancel timer hide
                timerHide.Reset();
            }
            if (!show && (indicator.activeInHierarchy || force))
            {
                if (!timerHide.IsSet)
                    timerHide.Set();
                if (force)
                    timerHide.Timeout();

                // Cancel timer show
                timerShow.Reset();
            }
        }

        public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
        {
            var tm = TrackerManager.Instance;
            var tid = (TrackerId)(int)tracker.trackerId;
            var role = tm.GetTrackerRole(tid);

            if (role == TrackerRole.Standalone)
            {
                VRSSpectatorManager.Instance.SetTracker(tid);
            }
            else
            {
                var mesh = indicator.GetComponent<MeshRenderer>();
                mesh.material = invalidMat;
            }
        }

        public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
        {
            var mesh = indicator.GetComponent<MeshRenderer>();
            mesh.material = indicatorMat;
        }
    }
}