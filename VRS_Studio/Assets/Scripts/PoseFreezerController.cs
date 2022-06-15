using HTC.UnityPlugin.PoseTracker;
using UnityEngine;

public class PoseFreezerController : MonoBehaviour
{
    public GameObject poseFreezer;

    public void EnablePoseFreezer()
    {
        var pf = poseFreezer.GetComponent<PoseFreezer>();
        pf.enabled = true;
    }

    public void DisablePoseFreezer()
    {
        var pf = poseFreezer.GetComponent<PoseFreezer>();
        pf.enabled = false;
    }
}