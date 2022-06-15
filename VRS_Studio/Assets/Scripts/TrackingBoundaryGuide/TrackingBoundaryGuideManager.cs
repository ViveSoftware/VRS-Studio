using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

public class TrackingBoundaryGuideManager : MonoBehaviour
{
	public static TrackingBoundaryGuideManager Instance = null;

	public List<HandContainer> ContainerList;
	//public GameObject PrecautionSprite;
	public Text InstructionText;
	public TabGroup InstructionPanelTabGroup;
	public Animator InstructionPanelAnimator;

	public Simple3DButton NextButton;

	public RobotScenarioControllerBase robotAssistantController;

	public bool isGuideRunning { get; private set; }
	public bool isContainerStageOver { get; private set; }
	public bool isTutorialClearedBefore { get { return m_isTutorialClearedBefore; } private set { m_isTutorialClearedBefore = value; } }
	private bool m_isTutorialClearedBefore = false;

	private int currStageIndex = 0; //0 is stage one
	private int currInstructionPanelIndex = 0;
	private Vector3 LeftContainerPos_Pose1 = new Vector3(-0.147f, -0.3f, 0.228f);
	private Vector3 LeftContainerPos_Pose2 = new Vector3(-0.147f, -0.3f, 0.228f);
	private Vector3 RightContainerPos_Pose1 = new Vector3(0.147f, -0.3f, 0.228f);
	private Vector3 RightContainerPos_Pose2 = new Vector3(0.147f, -0.3f, 0.228f);
	private Quaternion LeftContainerRot_Pose1 = Quaternion.Euler(154.609f, -1.525f, 180);
	private Quaternion RightContainerRot_Pose1 = Quaternion.Euler(154.609f, -1.525f, 180);
	private Vector3 NextButton_Pose = new Vector3(0, -0.25f, 0.45f);
	//private Vector3 Barrier_Pose = new Vector3(0, 0.17f, 0.23f);

	private string animation_InstructionPanelEndTrigger = "EndInstructionPanel";

	public const string TutorialClearPrefKey = "TutorialCompleted";

	private string LOG_TAG = "TrackingBoundaryGuideManager";

	// Start is called before the first frame update
	void Start()
	{
		Instance = this;
		currStageIndex = 0;
		isGuideRunning = true;
		isContainerStageOver = false;

		NextButton.gameObject.SetActive(false);

		foreach (HandContainer handContainer in ContainerList)
		{
			handContainer.gameObject.SetActive(false);
		}

		isTutorialClearedBefore = TutorialCompletedBefore();

		GuideStageOneStart();

		if (robotAssistantController != null)
		{
			if (isTutorialClearedBefore) robotAssistantController.InitializeRobot(); //Only do this on reentering the scene
		}
	}

	public static bool TutorialCompletedBefore()
	{
		bool hasKey = PlayerPrefs.HasKey(TutorialClearPrefKey);
		if (hasKey)
		{
			return (PlayerPrefs.GetInt(TutorialClearPrefKey, 0) == 1);
		}
		else
		{
			PlayerPrefs.SetInt(TutorialClearPrefKey, 0);
			PlayerPrefs.Save();
			return false;
		}
	}

	private void OnDestroy()
	{
		//HandInteractionCameraRig.Instance.BoundaryBarrier.SetActive(false);
		isGuideRunning = false;
	}

	// Update is called once per frame
	void Update()
	{
		ContainerList[0].transform.position = VRSStudioCameraRig.Instance.HMD.transform.position + LeftContainerPos_Pose1;
		ContainerList[0].transform.rotation = LeftContainerRot_Pose1;
		ContainerList[1].transform.position = VRSStudioCameraRig.Instance.HMD.transform.position + RightContainerPos_Pose1;
		ContainerList[1].transform.rotation = RightContainerRot_Pose1;
		NextButton.transform.position = VRSStudioCameraRig.Instance.HMD.transform.position + NextButton_Pose;
	}

	bool guideStageOneStart_Completed = false;
	private void GuideStageOneStart()
	{
		if (guideStageOneStart_Completed) return;

		currInstructionPanelIndex = 0;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 1
		ContainerList[currStageIndex].gameObject.SetActive(true);
		guideStageOneStart_Completed = true;
	}

	bool guideStageOneEnd_Completed = false;
	public void GuideStageOneEnd()
	{
		if (guideStageOneEnd_Completed) return;

		ContainerList[currStageIndex].gameObject.SetActive(false);
		currStageIndex++;
		GuideStageTwoStart();

		guideStageOneEnd_Completed = true;
	}

	bool guideStageOTwoStart_Completed = false;
	private void GuideStageTwoStart()
	{
		if (guideStageOTwoStart_Completed) return;

		currInstructionPanelIndex = 1;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 2
		ContainerList[currStageIndex].gameObject.SetActive(true);

		guideStageOTwoStart_Completed = true;
	}

	bool guideStageTwoEnd_Completed = false;
	public void GuideStageTwoEnd()
	{
		if (guideStageTwoEnd_Completed) return;

		ContainerList[currStageIndex].gameObject.SetActive(false);
		currInstructionPanelIndex = 2;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 3

		NextButton.gameObject.SetActive(true);
		NextButton.OnButtonAnimationCompleteCallback_Pressed += DisableNextButton;
		NextButton.OnButtonAnimationCompleteCallback_Pressed += GuideStageThreeStart;
		NextButton.DeselectButton();

		isContainerStageOver = true;
		guideStageTwoEnd_Completed = true;
	}

	bool guideStageThreeStart_Completed = false;
	public void GuideStageThreeStart()
	{
		if (guideStageThreeStart_Completed) return;

		NextButton.OnButtonAnimationCompleteCallback_Pressed -= GuideStageThreeStart;
		CustomHandStateManager.ChangeCustomHandStateManagerState(true);
		CustomHandStateManager.Instance.HandDistanceStateChangeCallback += GuideStageThreeTrigger;
		currInstructionPanelIndex = 3;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 4
		guideStageThreeStart_Completed = true;
	}

	bool GuideStageThreeTriggered = false;
	public void GuideStageThreeTrigger(CustomHandState state)
	{
		if (state.handDistanceState == CustomHandState.HandStateFlags.DistanceWarning && !GuideStageThreeTriggered)
		{
			GuideStageThreeTriggered = true;
			CustomHandStateManager.Instance.HandDistanceStateChangeCallback -= GuideStageThreeTrigger;
			GuideStageThreeEnd();
		}
	}

	bool guideStageThreeEnd_Completed = false;
	public void GuideStageThreeEnd()
	{
		if (guideStageThreeEnd_Completed) return;

		currInstructionPanelIndex = 4;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 5
		NextButton.OnButtonAnimationCompleteCallback_Pressed += GuideStageFourStart;
		NextButton.gameObject.SetActive(true);
		NextButton.DeselectButton();
		guideStageThreeEnd_Completed = true;
	}

	bool guideStageFourStart_Completed = false;
	public void GuideStageFourStart()
	{
		if (guideStageFourStart_Completed) return;

		currInstructionPanelIndex = 5;
		NextButton.OnButtonAnimationCompleteCallback_Pressed -= GuideStageFourStart;
		CustomHandStateManager.Instance.HandBoundaryStateChangeCallback += GuideStageFourTrigger;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 6

		guideStageFourStart_Completed = true;
	}

	bool GuideStageFourTriggered = false;
	public void GuideStageFourTrigger(CustomHandState state)
	{
		if (state.handBoundaryState == CustomHandState.HandStateFlags.BoundaryWarning && !GuideStageFourTriggered)
		{
			GuideStageFourTriggered = true;
			CustomHandStateManager.Instance.HandBoundaryStateChangeCallback -= GuideStageFourTrigger;
			GuideStageFourEnd();
		}
	}

	bool guideStageFourEnd_Completed = false;
	public void GuideStageFourEnd()
	{
		if (guideStageFourEnd_Completed) return;

		currInstructionPanelIndex = 6;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 7
		NextButton.OnButtonAnimationCompleteCallback_Pressed += NextButton.DeselectButton;
		//NextButton.OnButtonAnimationCompleteCallback_Pressed -= DisableNextButton;
		NextButton.gameObject.SetActive(true);
		NextButton.DeselectButton();
		NextButton.OnButtonAnimationCompleteCallback_Pressed += GuideStageFiveStart;

		guideStageFourEnd_Completed = true;
	}

	bool guideStageFiveStart_Completed = false;
	public void GuideStageFiveStart()
	{
		if (guideStageFiveStart_Completed) return;

		CustomHandStateManager.ChangeCustomHandStateManagerState(false);
		NextButton.OnButtonAnimationCompleteCallback_Pressed -= GuideStageFiveStart;
		currInstructionPanelIndex = 7;
		InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 8
		NextButton.OnButtonAnimationCompleteCallback_Pressed -= NextButton.DeselectButton;
		//NextButton.OnButtonAnimationCompleteCallback_Pressed += DisableNextButton;
		//NextButton.OnButtonAnimationCompleteCallback_Pressed += GuideStageFiveEnd;
		EndOfGuide();

		guideStageFiveStart_Completed = true;
	}

	//bool guideStageFiveEnd_Completed = false;
	//public void GuideStageFiveEnd()
	//{
	//	if (guideStageFiveEnd_Completed) return;

	//	currInstructionPanelIndex = 8;
	//	InstructionPanelTabGroup.tabButtons[currInstructionPanelIndex].Select(); //Tab 9
	//	EndOfGuide();
	//	NextButton.OnButtonAnimationCompleteCallback_Pressed -= DisableNextButton;
	//	NextButton.OnButtonAnimationCompleteCallback_Pressed -= GuideStageFiveEnd;

	//	guideStageFiveEnd_Completed = true;
	//}

	private void EndOfGuide()
	{
		if (!isGuideRunning) return;

		isGuideRunning = false;
		PlayerPrefs.SetInt(TutorialClearPrefKey, 1);
		PlayerPrefs.Save();
		isTutorialClearedBefore = true;

		NavMenuManager.Instance.NavMenuSummonCallback += PlayRollUpAnimation;
		NavMenuManager.Instance.BeginMenuTutorial();
	}

	private void PlayRollUpAnimation(bool menuActive)
	{
		if (menuActive)
		{
			InstructionPanelAnimator.SetTrigger(animation_InstructionPanelEndTrigger);
			NavMenuManager.Instance.NavMenuSummonCallback -= PlayRollUpAnimation;
		}
	}

	private void DisableNextButton()
	{
		NextButton.gameObject.SetActive(false);
	}
}
