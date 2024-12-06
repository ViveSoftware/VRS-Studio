using UnityEngine;
using Wave.Essence;

public class UpdateTextPose : MonoBehaviour
{
    [SerializeField]
    private GameObject text;
    private Transform cameraTrans = null;

    void Update()
    {
        if (cameraTrans == null)
        {
            if (WaveRig.Instance)
            {
                cameraTrans = WaveRig.Instance.transform;
            }
            else if (Camera.main)
            {
                cameraTrans = Camera.main.transform;
            }
        }
        if (cameraTrans == null) { return; }

        if (text)
        {
            Vector3 targetPosition = new Vector3(cameraTrans.position.x, text.transform.position.y, cameraTrans.position.z);
            text.transform.LookAt(targetPosition);
            text.transform.Rotate(0, 180, 0);
        }
    }
}
