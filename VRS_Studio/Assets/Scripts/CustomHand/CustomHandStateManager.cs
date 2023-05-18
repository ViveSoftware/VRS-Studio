using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wave.Essence.Hand.NearInteraction.Extend;
using Wave.Native;

public class CustomHandStateManager : MonoBehaviour
{
	public static CustomHandStateManager Instance;
	private GameObject hmd;

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

		enabled = false;
	}

	private void OnDisable()
	{

		//Reset Boundary Barriers
		VRSStudioCameraRig.Instance.BoundaryBarrier_L.SetActive(false);
		VRSStudioCameraRig.Instance.BoundaryBarrier_R.SetActive(false);

		//Reset colors
		CustomHandState.LeftHandState.customHandMaterial.SetColor("_GraColorA", CustomHandState.LeftHandState.originalHandGraAColor);
		CustomHandState.LeftHandState.customHandMaterial.SetColor("_GraColorB", CustomHandState.LeftHandState.originalHandGraBColor);

		CustomHandState.RightHandState.customHandMaterial.SetColor("_GraColorA", CustomHandState.RightHandState.originalHandGraAColor);
		CustomHandState.RightHandState.customHandMaterial.SetColor("_GraColorB", CustomHandState.RightHandState.originalHandGraBColor);
	}

	private void BoundaryBarrierHandler(CustomHandState customHandState)
	{
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
	}

	public static void ChangeCustomHandStateManagerState(bool state)
	{
		if (Instance != null)
		{
			Instance.enabled = state;
		}
	}
}
