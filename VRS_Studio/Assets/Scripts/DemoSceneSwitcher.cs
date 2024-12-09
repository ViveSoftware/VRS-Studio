using HTC.UnityPlugin.ColliderEvent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSceneSwitcher : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField] bool enter = false;
    [SerializeField] bool leave = false;

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData) { }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData) { }
}
