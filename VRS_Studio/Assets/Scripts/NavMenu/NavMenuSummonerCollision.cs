using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.Hand;

public class NavMenuSummonerCollision : MonoBehaviour
{
    [HideInInspector]
    public Vector3 palmPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider collisionData)
    {
        if (collisionData.attachedRigidbody != null && InteractionHub.GetInteractor(collisionData.attachedRigidbody, out BaseInteractor value))
        {
            HandInteractor interactor = value.GetComponent<HandInteractor>();

            if (interactor.HandType == HandManager.HandType.Left && interactor.Joint == HandManager.HandJoint.Palm)
            {
                palmPosition = interactor.transform.position;
            }
        }
    }
}
