using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.Hand;

public class CustomHandState : MonoBehaviour
{
    public static CustomHandState LeftHandState, RightHandState;

    public SkinnedMeshRenderer customHandMeshRenderer;

    public Material customHandMaterial { get; private set; }
    public Rigidbody handMainRigidBody;
    public Transform WristJoint;

    public Color originalHandGraAColor { get; private set; }
    public Color originalHandGraBColor { get; private set; }
    public Color warningHandColor = Color.red;

    [SerializeField]
    private HandManager.HandType m_type = HandManager.HandType.Right;
    public HandManager.HandType HandType { get { return m_type; } set { m_type = value; } }

    [HideInInspector]
    public HandStateFlags handBoundaryState, handDistanceState;

    // Start is called before the first frame update
    void Start()
    {
        handBoundaryState = HandStateFlags.Normal;
        handBoundaryState = HandStateFlags.Normal;

        if (HandType == HandManager.HandType.Left)
        {
            LeftHandState = this;
        }
        else
        {
            RightHandState = this;
        }

        if (customHandMeshRenderer != null)
        {
            customHandMaterial = customHandMeshRenderer.material;
            if (customHandMaterial != null)
            {
                originalHandGraAColor = customHandMaterial.GetColor("_GraColorA");
                originalHandGraBColor = customHandMaterial.GetColor("_GraColorB");
            }
        }
    }

    public enum HandStateFlags
    {
        Normal = 0,
        BoundaryWarning = 1,
        DistanceWarning = 2,
    }
}
