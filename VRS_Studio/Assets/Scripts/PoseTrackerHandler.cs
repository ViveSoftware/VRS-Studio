using HTC.UnityPlugin.PoseTracker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseTrackerHandler : MonoBehaviour
{
    public PoseTracker poseTracker;

    void Start()
    {
        var vrOrigin = GameObject.Find("VROrigin").transform;
        poseTracker.target = vrOrigin;
    }
}
