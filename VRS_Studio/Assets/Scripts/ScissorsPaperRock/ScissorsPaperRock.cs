using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using VRSStudio.Utils;

public class ScissorsPaperRock : MonoBehaviour
{
    public Animator gestureAnimator;
    public AudioSource AudioPlayer = null;
    public AudioClip[] VoiceClips;
    private ViveRoleProperty hmdRole = ViveRoleProperty.New(DeviceRole.Hmd);
    private JointPose[] joints = new JointPose[20];
    private int oppGesture = -1;

    public enum GestureType
    {
        Invalid,
        Scissors,
        Paper,
        Rock,
    }

    public enum ResultState
    {
        Invalid,
        Win,
        Lose,
        Tie,
        SingleHand,
    }



    public GestureType DetectGesture(ViveRoleProperty role)
    {
        if (!VRModule.GetCurrentDeviceState(role.GetDeviceIndex()).isPoseValid) return GestureType.Invalid;

        var hmd = VRModule.GetCurrentDeviceState(hmdRole.GetDeviceIndex());
        JointPose wrist;
        VivePose.TryGetHandJointPose(role, HandJointName.Wrist, out wrist);
        var dist = hmd.position.y - wrist.pose.pos.y;
        if (dist > 0.5f) return GestureType.Invalid;

        var handState = GetHandState(role);

        if (IsRockGesture(handState)) // rock
        {
            return GestureType.Rock;
        }
        else if (IsPaperGesture(handState)) // paper
        {
            return GestureType.Paper;
        }
        else if (IsScissorsGesture(handState)) // scissors
        {
            return GestureType.Scissors;
        }

        return GestureType.Invalid;
    }

    public ResultState RandomGesture(ViveRoleProperty role)
    {
        var rand = Random.Range(0, 2);
        oppGesture = rand;

        var handState = GetHandState(role);
        var handStateAlt = GetHandStateAlt(role);

        if (IsRockGesture(handState)) // rock
        {
            if (oppGesture == 0)
            {
                gestureAnimator.SetTrigger("ThrowingScissors");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[0]);
                return ResultState.Win;
            }
            else if (oppGesture == 1)
            {
                gestureAnimator.SetTrigger("ThrowingPaper");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[1]);
                return ResultState.Lose;
            }
            else if (oppGesture == 2)
            {
                gestureAnimator.SetTrigger("ThrowingRock");
                return ResultState.Tie;
            }
        }
        else if (IsPaperGesture(handState)) // paper
        {
            if (oppGesture == 0)
            {
                gestureAnimator.SetTrigger("ThrowingScissors");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[1]);
                return ResultState.Lose;
            }
            else if (oppGesture == 1)
            {
                gestureAnimator.SetTrigger("ThrowingPaper");
                return ResultState.Tie;
            }
            else if (oppGesture == 2)
            {
                gestureAnimator.SetTrigger("ThrowingRock");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[0]);
                return ResultState.Win;
            }
        }
        else if (IsScissorsGesture(handState)) // scissors
        {
            if (oppGesture == 0)
            {
                gestureAnimator.SetTrigger("ThrowingScissors");
                return ResultState.Tie;
            }
            else if (oppGesture == 1)
            {
                gestureAnimator.SetTrigger("ThrowingPaper");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[0]);
                return ResultState.Win;
            }
            else if (oppGesture == 2)
            {
                gestureAnimator.SetTrigger("ThrowingRock");
                if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                AudioPlayer.PlayOneShot(VoiceClips[1]);
                return ResultState.Lose;
            }
        }

        return ResultState.Invalid;
    }

    public HandState GetHandState(ViveRoleProperty role)
    {
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbProximal, out joints[1]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbDistal, out joints[2]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbTip, out joints[3]);

        VivePose.TryGetHandJointPose(role, HandJointName.IndexProximal, out joints[4]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexIntermediate, out joints[5]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexDistal, out joints[6]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexTip, out joints[7]);

        VivePose.TryGetHandJointPose(role, HandJointName.MiddleProximal, out joints[8]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleIntermediate, out joints[9]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleDistal, out joints[10]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleTip, out joints[11]);

        VivePose.TryGetHandJointPose(role, HandJointName.RingProximal, out joints[12]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingIntermediate, out joints[13]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingDistal, out joints[14]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingTip, out joints[15]);

        VivePose.TryGetHandJointPose(role, HandJointName.PinkyProximal, out joints[16]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyIntermediate, out joints[17]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyDistal, out joints[18]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyTip, out joints[19]);

        var thumbV1 = (joints[2].pose.pos - joints[1].pose.pos).normalized;
        var thumbV2 = (joints[3].pose.pos - joints[1].pose.pos).normalized;
        var indexV1 = (joints[5].pose.pos - joints[4].pose.pos).normalized;
        var indexV2 = (joints[7].pose.pos - joints[4].pose.pos).normalized;
        var middleV1 = (joints[9].pose.pos - joints[8].pose.pos).normalized;
        var middleV2 = (joints[11].pose.pos - joints[8].pose.pos).normalized;
        var ringV1 = (joints[13].pose.pos - joints[12].pose.pos).normalized;
        var ringV2 = (joints[15].pose.pos - joints[12].pose.pos).normalized;
        var pinkyV1 = (joints[17].pose.pos - joints[16].pose.pos).normalized;
        var pinkyV2 = (joints[19].pose.pos - joints[16].pose.pos).normalized;

        HandState handState = new HandState();
        handState.thumb = GetAngle(thumbV1, thumbV2, Vector3.Cross(thumbV1, thumbV2));
        handState.index = GetAngle(indexV1, indexV2, Vector3.Cross(indexV1, indexV2));
        handState.middle = GetAngle(middleV1, middleV2, Vector3.Cross(middleV1, middleV2));
        handState.ring = GetAngle(ringV1, ringV2, Vector3.Cross(ringV1, ringV2));
        handState.pinky = GetAngle(pinkyV1, pinkyV2, Vector3.Cross(pinkyV1, pinkyV2));

        return handState;
    }

    public HandState GetHandStateAlt(ViveRoleProperty role)
    {
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbProximal, out joints[1]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbDistal, out joints[2]);
        VivePose.TryGetHandJointPose(role, HandJointName.ThumbTip, out joints[3]);

        VivePose.TryGetHandJointPose(role, HandJointName.IndexProximal, out joints[4]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexIntermediate, out joints[5]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexDistal, out joints[6]);
        VivePose.TryGetHandJointPose(role, HandJointName.IndexTip, out joints[7]);

        VivePose.TryGetHandJointPose(role, HandJointName.MiddleProximal, out joints[8]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleIntermediate, out joints[9]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleDistal, out joints[10]);
        VivePose.TryGetHandJointPose(role, HandJointName.MiddleTip, out joints[11]);

        VivePose.TryGetHandJointPose(role, HandJointName.RingProximal, out joints[12]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingIntermediate, out joints[13]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingDistal, out joints[14]);
        VivePose.TryGetHandJointPose(role, HandJointName.RingTip, out joints[15]);

        VivePose.TryGetHandJointPose(role, HandJointName.PinkyProximal, out joints[16]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyIntermediate, out joints[17]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyDistal, out joints[18]);
        VivePose.TryGetHandJointPose(role, HandJointName.PinkyTip, out joints[19]);

        HandState handState = new HandState();
        handState.thumb = GetAngleAlt(joints[1].pose.pos, joints[2].pose.pos, joints[3].pose.pos);
        handState.index = GetAngleAlt(joints[4].pose.pos, joints[5].pose.pos, joints[7].pose.pos);
        handState.middle = GetAngleAlt(joints[8].pose.pos, joints[9].pose.pos, joints[11].pose.pos);
        handState.ring = GetAngleAlt(joints[12].pose.pos, joints[13].pose.pos, joints[15].pose.pos);
        handState.pinky = GetAngleAlt(joints[16].pose.pos, joints[17].pose.pos, joints[19].pose.pos);

        return handState;
    }

    private float GetAngle(Vector3 v1, Vector3 v2, Vector3 n)
    {
        var angle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)),
                                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        return angle;
    }

    private float GetAngleAlt(Vector3 root, Vector3 node1, Vector3 top)
    {
        var angle = Vector3.Angle(node1 - root, top - node1);

        return angle;
    }

    private bool IsScissorsGesture(HandState state)
    {
        if (state.thumb < 5f) return false;
        if (state.index > 5f) return false;
        if (state.middle > 5f) return false;
        if (state.ring < 20f) return false;
        if (state.pinky < 10f) return false;

        return true;
    }

    private bool IsPaperGesture(HandState state)
    {
        if (state.thumb > 2f) return false;
        if (state.index > 5f) return false;
        if (state.middle > 5f) return false;
        if (state.ring > 5f) return false;
        if (state.pinky > 5f) return false;

        return true;
    }

    private bool IsRockGesture(HandState state)
    {
        if (state.thumb < 10f) return false;
        if (state.index < 35f) return false;
        if (state.middle < 35f) return false;
        if (state.ring < 30f) return false;
        if (state.pinky < 25f) return false;

        return true;
    }

    // Leverage Vive Hand Tracking's algorithm
    private bool IsScissorsGestureAlt(HandState state)
    {
        if (state.thumb < 15f) return false;
        if (state.index > 5f) return false;
        if (state.middle > 5f) return false;
        if (state.ring < 75f) return false;
        if (state.pinky < 75f) return false;

        return true;
    }

    private bool IsPaperGestureAlt(HandState state)
    {
        if (state.thumb > 20f) return false;
        if (state.index > 20f) return false;
        if (state.middle > 20f) return false;
        if (state.ring > 20f) return false;
        if (state.pinky > 20f) return false;

        return true;
    }

    private bool IsRockGestureAlt(HandState state)
    {
        if (state.thumb < 15f) return false;
        if (state.index < 75f) return false;
        if (state.middle < 75f) return false;
        if (state.ring < 75f) return false;
        if (state.pinky < 75f) return false;

        return true;
    }
}
