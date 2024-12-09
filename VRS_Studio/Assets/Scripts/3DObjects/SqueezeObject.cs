using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SqueezeObject : MonoBehaviour
{
    public Transform balloon;
    public BasicGrabbable grabbedObj;
    public PlayableDirector playableDirector;

    private ViveRoleProperty rightHandRole = ViveRoleProperty.New(HandRole.RightHand);
    private ViveRoleProperty leftHandRole = ViveRoleProperty.New(HandRole.LeftHand);

    private Vector3 balloon_orig_pos;
    private Quaternion balloon_orig_rot;

    private bool[] leftGestures = new bool[6];
    private bool[] rightGestures = new bool[6];

    private float time = 0.25f;
    private float reset = 1f;
    private IVRModuleDeviceState current;
    private Vector3 posOffset;
    private Vector3 rotOffset;

    private BalloonState balloonState = BalloonState.Idle;

    public enum BalloonState
    {
        Invalid,
        Idle,
        Grabbed,
        Released,
        Reset,
        Waiting,
        Exploded,
    }

    private void OnEnable()
    {
        playableDirector.stopped += OnPlayableDirectorStopped;
    }

    private void OnDisable()
    {
        playableDirector.stopped -= OnPlayableDirectorStopped;
    }

    private void Start()
    {
        balloon_orig_pos = balloon.position;
        balloon_orig_rot = balloon.rotation;

        for (int i = 0; i < 6; i++)
        {
            leftGestures[i] = false;
            rightGestures[i] = false;
        }
    }

    void Update()
    {
        var rightDeviceState = VRModule.GetCurrentDeviceState(rightHandRole.GetDeviceIndex());
        var leftDeviceState = VRModule.GetCurrentDeviceState(leftHandRole.GetDeviceIndex());

        if (!rightDeviceState.isPoseValid && !leftDeviceState.isPoseValid) return;

        if (balloonState == BalloonState.Idle)
        {
            playableDirector.time = 0;
            playableDirector.Evaluate();

            ViveColliderButtonEventData viveEventData;

            if (grabbedObj.isGrabbed && grabbedObj.grabbedEvent.TryGetViveButtonEventData(out viveEventData))
            {
                switch (viveEventData.viveRole.ToRole<HandRole>())
                {
                    case HandRole.RightHand:
                        current = rightDeviceState;
                        posOffset = new Vector3(0.025f, 0f, 0.015f);
                        rotOffset = new Vector3(0f, 110f, 0f);
                        break;
                    case HandRole.LeftHand:
                        current = leftDeviceState;
                        posOffset = new Vector3(-0.025f, 0f, 0.015f);
                        rotOffset = new Vector3(0f, -110f, 0f);
                        break;
                }

                balloonState = BalloonState.Grabbed;
            }

            if (balloon.position.y < -1f)
            {
                reset -= Time.deltaTime;
                if (reset < 0f)
                {
                    balloonState = BalloonState.Reset;
                }
            }
        }
        else if (balloonState == BalloonState.Grabbed)
        {
            if (!grabbedObj.isGrabbed) balloonState = BalloonState.Released;

            var pose = grabbedObj.currentGrabber.grabberOrigin * new RigidPose(posOffset, Quaternion.Euler(rotOffset));
            balloon.position = pose.pos;
            balloon.rotation = pose.rot;

            var gripAxis = current.GetAxisValue(VRModuleRawAxis.CapSenseGrip);

            if (gripAxis == 1f)
            {
                time -= Time.deltaTime;

                if (time < 0f)
                {
                    playableDirector.Play();
                    balloonState = BalloonState.Waiting;
                }
            }
            else
            {
                playableDirector.time = 0.716f * gripAxis;
                playableDirector.Evaluate();
                time = 0.25f;
            }
        }
        else if (balloonState == BalloonState.Released)
        {
            balloonState = BalloonState.Idle;
            time = 0.25f;
        }
        else if (balloonState == BalloonState.Reset)
        {
            reset = 1f;
            playableDirector.time = 0;
            playableDirector.Evaluate();
            balloon.position = balloon_orig_pos;
            balloon.rotation = balloon_orig_rot;
            balloon.GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            balloon.GetComponentInChildren<Rigidbody>().angularVelocity = Vector3.zero;
            balloonState = BalloonState.Idle;
        }
        else if (balloonState == BalloonState.Exploded)
        {
            if (!grabbedObj.isGrabbed)
            {
                reset -= Time.deltaTime;
                if (reset < 0f)
                {
                    balloonState = BalloonState.Reset;
                }
            }
        }
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        balloonState = BalloonState.Exploded;
        reset = 1f;
        time = 0.25f;
    }
}
