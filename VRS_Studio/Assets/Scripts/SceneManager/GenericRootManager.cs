using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericRootManager : MonoBehaviour
{
    public Vector3 Root_OffsetFromHMD = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        UpdateRootPosition();
    }

    public void UpdateRootPosition()
	{
        if (VRSStudioCameraRig.Instance != null)
        {
            transform.position = VRSStudioCameraRig.Instance.HMD.transform.position + Root_OffsetFromHMD;
        }
    }
}
