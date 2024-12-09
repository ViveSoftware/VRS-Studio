using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wave.Essence.Hand.NearInteraction;
using Wave.Native;

public class CustomHandStateManager : MonoBehaviour
{
	public static CustomHandStateManager Instance;
	private GameObject hmd;

	//public LineRenderer debugline;
	//public GameObject pt1, pt2;

	public delegate void HandDistanceStateChangeDelegate(CustomHandState state);
	public event HandDistanceStateChangeDelegate HandDistanceStateChangeCallback;

	public delegate void HandBoundaryStateChangeDelegate(CustomHandState state);
	public event HandBoundaryStateChangeDelegate HandBoundaryStateChangeCallback;

	private Material BoundaryMaterial_L, BoundaryMaterial_R;
	private float distance_HMD_LH, distance_HMD_RH;
	private bool boundaryWarningActivated_LH = false;
	private bool boundaryWarningActivated_RH = false;

	private const float handProximityWarningDistance = 0.03f;
	private const float boundaryWarningDistance = 0.3f;

	private string LOG_TAG = "CustomHandStateManager";

	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		hmd = VRSStudioCameraRig.Instance.HMD;

		BoundaryMaterial_L = VRSStudioCameraRig.Instance.BoundaryBarrier_L.GetComponent<Renderer>().material;
		BoundaryMaterial_R = VRSStudioCameraRig.Instance.BoundaryBarrier_R.GetComponent<Renderer>().material;

		BoundaryMaterial_L.SetFloat("_hitDistance", boundaryWarningDistance);
		BoundaryMaterial_R.SetFloat("_hitDistance", boundaryWarningDistance);
		//BoundaryMaterial_L.SetFloat("_hitDistance", 0.29f);
		//BoundaryMaterial_R.SetFloat("_hitDistance", 0.29f);

		//Log.d(LOG_TAG, "Camera found: " + (hmd == null));

		enabled = false;
	}

	private void OnEnable()
	{
		HandBoundaryStateChangeCallback += BoundaryBarrierHandler;
	}

	private void OnDisable()
	{
		HandBoundaryStateChangeCallback -= BoundaryBarrierHandler;

		//Reset Boundary Barriers
		VRSStudioCameraRig.Instance.BoundaryBarrier_L.SetActive(false);
		VRSStudioCameraRig.Instance.BoundaryBarrier_R.SetActive(false);

		//Reset colors
		CustomHandState.LeftHandState.customHandMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.originalHandGraAColor);
		CustomHandState.LeftHandState.customHandMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.originalHandGraBColor);

		CustomHandState.RightHandState.customHandMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.originalHandGraAColor);
		CustomHandState.RightHandState.customHandMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.originalHandGraBColor);
	}

	// Update is called once per frame
	void Update()
	{
		if (CustomHandState.LeftHandState != null && CustomHandState.RightHandState != null && hmd != null)
		{
			Vector3 leftHandCenterPos = CustomHandState.LeftHandState.transform.position;
			Vector3 rightHandCenterPos = CustomHandState.RightHandState.transform.position;
			Vector3 leftHandWristPos = CustomHandState.LeftHandState.WristJoint.position;
			Vector3 rightHandWristPos = CustomHandState.RightHandState.WristJoint.position;
			Material LHMaterial = CustomHandState.LeftHandState.customHandMaterial;
			Material RHMaterial = CustomHandState.RightHandState.customHandMaterial;

			BoundaryMaterial_L.SetVector("_Hand_pos", leftHandWristPos);
			BoundaryMaterial_R.SetVector("_Hand_pos", rightHandWristPos);

			//Handle hand distance warning
			//Approximate the distance between hands
			//1. Estimate the closest point on LH to RH center pose
			Vector3 closestPoint_LHRB_RHCenter = CustomHandState.LeftHandState.handMainRigidBody.ClosestPointOnBounds(rightHandCenterPos);
			//2. Estimate the closest point on RH to previously estimated LH bound point
			Vector3 closestPoint_RHRB_LHRB = CustomHandState.RightHandState.handMainRigidBody.ClosestPointOnBounds(closestPoint_LHRB_RHCenter);

			//debugline.SetPosition(0, closestPoint_LHRB_RHCenter);
			//debugline.SetPosition(1, closestPoint_RHRB_LHRB);

			//pt1.transform.position = leftHandWristPos;
			//pt2.transform.position = rightHandWristPos;

			//3. Obtain estimated distance between hands
			float estimatedHandDistance = Vector3.Distance(closestPoint_LHRB_RHCenter, closestPoint_RHRB_LHRB);
			Log.d(LOG_TAG, "estimatedHandDistance: " + estimatedHandDistance);
			if (estimatedHandDistance <= handProximityWarningDistance)
			{
				if (LHMaterial != null && CustomHandState.LeftHandState.handDistanceState == CustomHandState.HandStateFlags.Normal)
				{
					LHMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.warningHandColor);
					LHMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.warningHandColor);
					CustomHandState.LeftHandState.handDistanceState = CustomHandState.HandStateFlags.DistanceWarning;
					if (HandDistanceStateChangeCallback != null) HandDistanceStateChangeCallback.Invoke(CustomHandState.LeftHandState);
				}
				if (RHMaterial != null && CustomHandState.RightHandState.handDistanceState == CustomHandState.HandStateFlags.Normal)
				{
					RHMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.warningHandColor);
					RHMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.warningHandColor);
					CustomHandState.RightHandState.handDistanceState = CustomHandState.HandStateFlags.DistanceWarning;
					if (HandDistanceStateChangeCallback != null) HandDistanceStateChangeCallback.Invoke(CustomHandState.RightHandState);
				}
			}
			else
			{
				if (LHMaterial != null && CustomHandState.LeftHandState.handDistanceState == CustomHandState.HandStateFlags.DistanceWarning)
				{
					LHMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.originalHandGraAColor);
					LHMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.originalHandGraBColor);
					CustomHandState.LeftHandState.handDistanceState = CustomHandState.HandStateFlags.Normal;
					if (HandDistanceStateChangeCallback != null) HandDistanceStateChangeCallback.Invoke(CustomHandState.LeftHandState);
				}
				if (RHMaterial != null && CustomHandState.RightHandState.handDistanceState == CustomHandState.HandStateFlags.DistanceWarning)
				{
					RHMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.originalHandGraAColor);
					RHMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.originalHandGraBColor);
					CustomHandState.RightHandState.handDistanceState = CustomHandState.HandStateFlags.Normal;
					if (HandDistanceStateChangeCallback != null) HandDistanceStateChangeCallback.Invoke(CustomHandState.RightHandState);
				}
			}

			//Handle Boundary Warning
			//Left Hand
			distance_HMD_LH = Vector3.Distance(leftHandWristPos, hmd.transform.position);
			Log.d(LOG_TAG, "distance_HMD_LH: " + distance_HMD_LH);
			if (distance_HMD_LH <= boundaryWarningDistance)
			{
				if (LHMaterial != null && CustomHandState.LeftHandState.handBoundaryState == CustomHandState.HandStateFlags.Normal)
				{
					LHMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.warningHandColor);
					LHMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.warningHandColor);
					CustomHandState.LeftHandState.handBoundaryState = CustomHandState.HandStateFlags.BoundaryWarning;
					boundaryWarningActivated_LH = true;
					if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.LeftHandState);
				}
			}
			else
			{
				if (LHMaterial != null && CustomHandState.LeftHandState.handBoundaryState == CustomHandState.HandStateFlags.BoundaryWarning)
				{
					LHMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.originalHandGraAColor);
					LHMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.originalHandGraBColor);
					CustomHandState.LeftHandState.handBoundaryState = CustomHandState.HandStateFlags.Normal;
					boundaryWarningActivated_LH = false;
					if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.LeftHandState);
				}
			}
			//Right Hand
			distance_HMD_RH = Vector3.Distance(rightHandWristPos, hmd.transform.position);
			Log.d(LOG_TAG, "distance_HMD_RH: " + distance_HMD_RH);
			if (distance_HMD_RH <= boundaryWarningDistance)
			{
				if (RHMaterial != null && CustomHandState.RightHandState.handBoundaryState == CustomHandState.HandStateFlags.Normal)
				{
					RHMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.warningHandColor);
					RHMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.warningHandColor);
					CustomHandState.RightHandState.handBoundaryState = CustomHandState.HandStateFlags.BoundaryWarning;
					boundaryWarningActivated_RH = true;
					if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.RightHandState);
				}
			}
			else
			{
				if (RHMaterial != null && CustomHandState.RightHandState.handBoundaryState == CustomHandState.HandStateFlags.BoundaryWarning)
				{
					RHMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.originalHandGraAColor);
					RHMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.originalHandGraBColor);
					CustomHandState.RightHandState.handBoundaryState = CustomHandState.HandStateFlags.Normal;
					boundaryWarningActivated_RH = false;
					if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.RightHandState);
				}
			}

			if (!VIUHand.Get(true).isTracked && RHMaterial != null && CustomHandState.LeftHandState.handBoundaryState == CustomHandState.HandStateFlags.BoundaryWarning) //If left hand pose is no longer valid
			{
				//Disable boundary object and reset boundary state
				LHMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.originalHandGraAColor);
				LHMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.originalHandGraBColor);
				CustomHandState.LeftHandState.handBoundaryState = CustomHandState.HandStateFlags.Normal;
				boundaryWarningActivated_LH = false;
				if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.LeftHandState);
			}

			if (!VIUHand.Get(false).isTracked && RHMaterial != null && CustomHandState.RightHandState.handBoundaryState == CustomHandState.HandStateFlags.BoundaryWarning) //If right hand pose is no longer valid
			{
				//Disable boundary object and reset boundary state
				RHMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.originalHandGraAColor);
				RHMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.originalHandGraBColor);
				CustomHandState.RightHandState.handBoundaryState = CustomHandState.HandStateFlags.Normal;
				boundaryWarningActivated_RH = false;
				if (HandBoundaryStateChangeCallback != null) HandBoundaryStateChangeCallback.Invoke(CustomHandState.RightHandState);
			}
		}
	}

	private void BoundaryBarrierHandler(CustomHandState customHandState)
	{
		//if (!(TrackingBoundaryGuideManager.Instance != null && !TrackingBoundaryGuideManager.Instance.isContainerStageOver))
		//{
			if (boundaryWarningActivated_LH)
			{
				VRSStudioCameraRig.Instance.BoundaryBarrier_L.SetActive(true);
			}
			else
			{
				VRSStudioCameraRig.Instance.BoundaryBarrier_L.SetActive(false);
			}

			if (boundaryWarningActivated_RH)
			{
				VRSStudioCameraRig.Instance.BoundaryBarrier_R.SetActive(true);
			}
			else
			{
				VRSStudioCameraRig.Instance.BoundaryBarrier_R.SetActive(false);
			}
		//}
	}

	public static void ChangeCustomHandStateManagerState(bool state)
	{
		if (Instance != null)
		{
			Instance.enabled = state;
		}
	}
}
