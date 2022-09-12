using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.PoseTracker;
using HTC.UnityPlugin.Utility;

namespace HTC.UnityPlugin.Vive
{
    [CreateAssetMenu(fileName = "CustomGesture",
                 menuName = "VIU/Hand Gesture")]
    public class GestureCustom : ScriptableObject
    {
        public string Name = "Default";
                
        public HandRole m_role;

        //finger state condition
        public FingerState Thumb;
        public FingerState Index;
        public FingerState Middle;
        public FingerState Ring;
        public FingerState Pinky;

        //allow finger back alternative condition
        public bool allowBackAlternativeCondition;
    }

    public enum FingerState
    {
        nothing = 0,
        close = 1,
        open = 2
    }

}