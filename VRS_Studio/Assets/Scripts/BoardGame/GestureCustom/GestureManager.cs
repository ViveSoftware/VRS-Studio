using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using HTC.UnityPlugin.PoseTracker;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using UnityEngine;


public class GestureManager : MonoBehaviour
{
    private static GestureManager _instance;
    public static GestureManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GestureManager>();
            }
            return _instance;
        }
    }

    private JointPose PalmJointPose;
    private JointPose[] JointPose = new JointPose[16]; //Finger joint

    [Header("Finger Angle Conidition")]
    [Range(0, 180)] public float ThumbAngle;
    [Range(0, 180)] public float IndexAngle;
    [Range(0, 180)] public float MiddleAngle;
    [Range(0, 180)] public float RingAngle;
    [Range(0, 180)] public float PinkyAngle;

    [Header("Finger Alter Angle Conidition")]
    [Range(0, 180)] public float AlterThumbAngle;
    [Range(0, 180)] public float AlterIndexAngle;
    [Range(0, 180)] public float AlterMiddleAngle;
    [Range(0, 180)] public float AlterRingAngle;
    [Range(0, 180)] public float AlterPinkyAngle;

    public bool GestureControl(GestureCustom m_Gesture)
    {
        VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.Wrist, out JointPose[15]);

        bool AlterState;
        if (JointPose[15].pose.rot.z > -0.55)
            AlterState = true;
        else
            AlterState = false;        

        if (m_Gesture.Thumb != FingerState.nothing) //thumb
        {
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.ThumbTip, out JointPose[0]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.ThumbDistal, out JointPose[1]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.ThumbProximal, out JointPose[2]);

            float FingerCurl = GetFingerCurl(JointPose[0].pose.pos, JointPose[1].pose.pos, JointPose[2].pose.pos);
            float AlterFingerCurl = GetFingerCurl(JointPose[0].pose.pos, JointPose[2].pose.pos, JointPose[15].pose.pos);

            if (m_Gesture.Thumb == FingerState.close && (FingerCurl > ThumbAngle ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl > AlterThumbAngle && AlterState == true)))
            {

            }
            else if (m_Gesture.Thumb == FingerState.open && ((FingerCurl < ThumbAngle && AlterState == false) || (FingerCurl < ThumbAngle-30 && AlterState == true) ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl < AlterThumbAngle-20 && AlterState == true)))
            {

            }
            else
            {
                return false;
            }
        }

        if (m_Gesture.Index != FingerState.nothing) //index
        {
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.IndexTip, out JointPose[3]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.IndexDistal, out JointPose[4]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.IndexProximal, out JointPose[5]);

            float FingerCurl = GetFingerCurl(JointPose[3].pose.pos, JointPose[4].pose.pos, JointPose[5].pose.pos);
            float AlterFingerCurl = GetFingerCurl(JointPose[3].pose.pos, JointPose[5].pose.pos, JointPose[15].pose.pos);

            if (m_Gesture.Index == FingerState.close && (FingerCurl > IndexAngle ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl > AlterIndexAngle && AlterState == true)))
            {

            }
            else if (m_Gesture.Index == FingerState.open && ((FingerCurl < IndexAngle && AlterState == false) || (FingerCurl < IndexAngle - 30 && AlterState == true) ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl < AlterIndexAngle - 20 && AlterState == true)))
            {

            }
            else
            {
                return false;
            }
        }

        if (m_Gesture.Middle != FingerState.nothing) //Middle
        {
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.MiddleTip, out JointPose[6]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.MiddleDistal, out JointPose[7]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.MiddleProximal, out JointPose[8]);

            float FingerCurl = GetFingerCurl(JointPose[6].pose.pos, JointPose[7].pose.pos, JointPose[8].pose.pos);
            float AlterFingerCurl = GetFingerCurl(JointPose[6].pose.pos, JointPose[8].pose.pos, JointPose[15].pose.pos);

            if (m_Gesture.Middle == FingerState.close && (FingerCurl > MiddleAngle ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl > AlterMiddleAngle && AlterState == true)))
            {

            }
            else if (m_Gesture.Middle == FingerState.open && ((FingerCurl < MiddleAngle && AlterState == false) || (FingerCurl < MiddleAngle - 30 && AlterState == true) ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl < AlterMiddleAngle - 20 && AlterState == true)))
            {

            }
            else
            {
                return false;
            }
        }

        if (m_Gesture.Ring != FingerState.nothing) //Ring
        {
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.RingTip, out JointPose[9]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.RingDistal, out JointPose[10]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.RingProximal, out JointPose[11]);

            float FingerCurl = GetFingerCurl(JointPose[9].pose.pos, JointPose[10].pose.pos, JointPose[11].pose.pos);
            float AlterFingerCurl = GetFingerCurl(JointPose[9].pose.pos, JointPose[11].pose.pos, JointPose[15].pose.pos);

            if (m_Gesture.Ring == FingerState.close && (FingerCurl > RingAngle ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl > AlterRingAngle && AlterState == true)))
            {

            }
            else if (m_Gesture.Ring == FingerState.open && ((FingerCurl < RingAngle && AlterState == false) || (FingerCurl < RingAngle - 30 && AlterState == true) ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl < AlterRingAngle - 20 && AlterState == true)))
            {

            }
            else
            {
                return false;
            }
        }

        if (m_Gesture.Pinky != FingerState.nothing) //Pinky
        {
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.PinkyTip, out JointPose[12]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.PinkyDistal, out JointPose[13]);
            VivePose.TryGetHandJointPose(m_Gesture.m_role, HandJointName.PinkyProximal, out JointPose[14]);

            float FingerCurl = GetFingerCurl(JointPose[12].pose.pos, JointPose[13].pose.pos, JointPose[14].pose.pos);
            float AlterFingerCurl = GetFingerCurl(JointPose[12].pose.pos, JointPose[14].pose.pos, JointPose[15].pose.pos);

            if (m_Gesture.Pinky == FingerState.close && (FingerCurl > PinkyAngle ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl > AlterPinkyAngle && AlterState == true)))
            {

            }
            else if (m_Gesture.Pinky == FingerState.open && ((FingerCurl < PinkyAngle && AlterState == false) || (FingerCurl < PinkyAngle - 30 && AlterState == true) ||
                (m_Gesture.allowBackAlternativeCondition == true && AlterFingerCurl < AlterPinkyAngle - 20 && AlterState == true)))
            {

            }
            else
            {
                return false;
            }
        }
        return true;
    }
    private float GetFingerCurl(Vector3 Tip, Vector3 Mid, Vector3 Root)
    {
        return Vector3.Angle(Mid - Tip, Root - Mid);
    }
}