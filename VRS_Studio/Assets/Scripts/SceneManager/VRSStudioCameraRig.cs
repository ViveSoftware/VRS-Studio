using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSStudioCameraRig : MonoBehaviour
{
    public static VRSStudioCameraRig Instance;

    public GameObject HMD, LeftHand, RightHand, BoundaryBarrier_L, BoundaryBarrier_R;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (HMD == null)
        {
            HMD = Camera.main.gameObject;
        }

        Wave.Native.Interop.WVR_SetAMCMode(Wave.Native.WVR_AMCMode.Force_PMC);
    }
}
