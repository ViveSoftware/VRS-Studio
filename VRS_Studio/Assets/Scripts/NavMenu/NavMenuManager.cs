using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wave.Essence.Hand;
using Wave.Essence.Hand.NearInteraction;
using Wave.Native;

public class NavMenuManager : MonoBehaviour
{
	public static NavMenuManager Instance;
	public Animator navMenuAnimator, SummoningAnimationAnimator;
	public float navMenuAnimatorSpeed = 3f;
	public AudioSource openMenuAudioSrc;
	public GameObject NavMenu, SummoningAnimation, TutorialSummonGesture;
	public List<GameObject> menuLayers;
	public GameObject mainMenuLayer;
	public NavMenuItem prevMenuButton, closeMenuButton;

	public delegate void NavMenuSummonDelegate(bool menuActive);
	public event NavMenuSummonDelegate NavMenuSummonCallback;

	public delegate void NavMenuLayerTransitionDelegate();
	public event NavMenuLayerTransitionDelegate OnComplete_NavMenuLayerTransition_Up;
	public event NavMenuLayerTransitionDelegate OnComplete_NavMenuLayerTransition_Down;

	private NearHandData leftHandData;

	private const float c_summoningAngleThreshold_thumb = 80f, c_summoningAngleThreshold_index = 70f, c_summoningAngleThreshold_middle = 70f, c_summoningAngleThreshold_ring = 70f, c_summoningAngleThreshold_pinky = 70f;
	private const float c_stopSummonAngleBuffer = 10f;

	private int currActiveMenuIndex = 0;
	private Stack<int> prevActiveMenuIndices = new Stack<int>();

	//For menu summoning
	private bool menuActive = false;
	private bool timerStarted = false;
	private float countdownStartTime = 0f;
	private float currTime = 0f;
	private float timeThreshold = 1f;

	//private Renderer mainMenuSummonAreaRenderer;
	private Vector3 MainMenuOffsetPosition = new Vector3(0.03f, -0.28f, 0.35f);
	private Quaternion MainMenuOffsetRotation = Quaternion.Euler(-45, 0, 0);

	private Vector3 SummonAreaOffsetPosition = new Vector3(-0.15f, -0.2537f, 0.175f);

	private Vector3 AnimationOffsetPosition = new Vector3(0, 0.1f, 0.05f);

	private Vector3 MenuObjectPoolingPosition = new Vector3(0, -1000, 0);

	private bool isRunningMenuTutorial = false;
	private bool tutorialCompletedBeforeCacheVal = false;

	private string animation_Layer = "Base Layer";
	private string animation_OpenMenu = "Base Layer.MenuUI_Open";
	private string animation_CloseMenu = "Base Layer.MenuUI_Close";
	private string animation_OpenedMenu = "Base Layer.MenuUI_Opened";
	private string animation_ClosedMenu = "Base Layer.MenuUI_Closed";
	private string animation_SummoningLogo = "Base Layer.OpeningLogo";

	private int animation_LayerID, animation_OpenStateHash, animation_CloseStateHash, animation_SummoningLogoHash;

	private string LOG_TAG = "NavMenuManager";

	private void Awake()
	{
		Instance = this;

		animation_LayerID = navMenuAnimator.GetLayerIndex(animation_Layer);
		animation_OpenStateHash = Animator.StringToHash(animation_OpenMenu);
		animation_CloseStateHash = Animator.StringToHash(animation_CloseMenu);
		animation_SummoningLogoHash = Animator.StringToHash(animation_SummoningLogo);
	}

	// Start is called before the first frame update
	void Start()
	{
		menuActive = false;
		NavMenu.transform.position = MenuObjectPoolingPosition;

		navMenuAnimator.speed = navMenuAnimatorSpeed;

		tutorialCompletedBeforeCacheVal = TrackingBoundaryGuideManager.TutorialCompletedBefore();
	}

	// Update is called once per frame
	void Update()
	{
		//Visual cue only
		if (isRunningMenuTutorial)
		{
			TutorialSummonGesture.SetActive(true);
			TutorialSummonGesture.transform.position = VRSStudioCameraRig.Instance.HMD.transform.position + SummonAreaOffsetPosition;
		}
		else
		{
			TutorialSummonGesture.SetActive(false);
		}

		bool tutorialCompletionStatus = tutorialCompletedBeforeCacheVal;

		if (TrackingBoundaryGuideManager.Instance != null) //Status should only be false if tutorial is not cleared before
		{
			tutorialCompletionStatus = TrackingBoundaryGuideManager.Instance.isTutorialClearedBefore;
			tutorialCompletedBeforeCacheVal = tutorialCompletionStatus;
		}

		if (tutorialCompletionStatus)
		{
			if (IsMenuSummonHandPose(timerStarted) && !menuActive)
			{
				if (!timerStarted) //Start counting down
				{
					Log.d(LOG_TAG, "Start counting down");
					countdownStartTime = Time.realtimeSinceStartup;
					timerStarted = true;

					SummoningAnimation.SetActive(true);

					PlayAnimation(SummoningAnimationAnimator, animation_SummoningLogoHash);
				}

				if (timerStarted)
				{
					SummoningAnimation.transform.position = VRSStudioCameraRig.Instance.LeftHand.transform.position + AnimationOffsetPosition;

					Vector3 animationDirection = VRSStudioCameraRig.Instance.HMD.transform.position - SummoningAnimation.transform.position;
					Quaternion dirRot = Quaternion.LookRotation(animationDirection, Vector3.up);

					SummoningAnimation.transform.rotation = dirRot;
					currTime = Time.realtimeSinceStartup;
					if (currTime - countdownStartTime > timeThreshold)
					{
						SummoningAnimation.SetActive(false);
						SummonNavMenu();


					}
				}
			}
			else
			{
				if (timerStarted) //Stop timer and reset
				{
					Log.d(LOG_TAG, "Stop timer and reset");
					timerStarted = false;
					countdownStartTime = 0f;
					currTime = 0f;
					SummoningAnimation.SetActive(false);
				}
			}
		}
	}

	public bool IsMenuSummonHandPose(bool isCountingDown)
	{
		bool isCorrectPose = false;

		leftHandData = VIUHand.Get(true);

		if (!leftHandData.isTracked) return isCorrectPose;

		//Get cosine(angle between finger direction and hand forward direction)
		Vector3 leftHandForwardDir = leftHandData.wrist.rotation * Vector3.forward;

		float thumbCurl = Vector3.Dot(leftHandData.thumb.direction.normalized, leftHandData.rotation * Vector3.right); //left hand thumb is on the right hand side when palm is facing downwards
		float indexCurl = Vector3.Dot(leftHandData.index.direction.normalized, leftHandForwardDir);
		float middleCurl = Vector3.Dot(leftHandData.middle.direction.normalized, leftHandForwardDir);
		float ringCurl = Vector3.Dot(leftHandData.ring.direction.normalized, leftHandForwardDir);
		float pinkyCurl = Vector3.Dot(leftHandData.pinky.direction.normalized, leftHandForwardDir);

		float thumbCurl_AngleOfElevation = Mathf.Acos(thumbCurl) * Mathf.Rad2Deg;
		float indexCurl_AngleOfElevation = Mathf.Acos(indexCurl) * Mathf.Rad2Deg;
		float middleCurl_AngleOfElevation = Mathf.Acos(middleCurl) * Mathf.Rad2Deg;
		float ringCurl_AngleOfElevation = Mathf.Acos(ringCurl) * Mathf.Rad2Deg;
		float pinkyCurl_AngleOfElevation = Mathf.Acos(pinkyCurl) * Mathf.Rad2Deg;

		//Debug.LogFormat("NavMenuSummoning\nthumbCurl_AngleOfElevation: {0}\nindexCurl_AngleOfElevation: {1}\nmiddleCurl_AngleOfElevation: {2}\nringCurl_AngleOfElevation: {3}\npinkyCurl_AngleOfElevation: {4}",
		//                thumbCurl_AngleOfElevation, indexCurl_AngleOfElevation, middleCurl_AngleOfElevation, ringCurl_AngleOfElevation, pinkyCurl_AngleOfElevation);

		float summoningAngleThreshold_thumb = c_summoningAngleThreshold_thumb, summoningAngleThreshold_index = c_summoningAngleThreshold_index, summoningAngleThreshold_middle = c_summoningAngleThreshold_middle, summoningAngleThreshold_ring = c_summoningAngleThreshold_ring, summoningAngleThreshold_pinky = c_summoningAngleThreshold_pinky;

		if (isCountingDown)
		{
			summoningAngleThreshold_thumb += c_stopSummonAngleBuffer;
			summoningAngleThreshold_index += c_stopSummonAngleBuffer;
			summoningAngleThreshold_middle += c_stopSummonAngleBuffer; 
			summoningAngleThreshold_ring += c_stopSummonAngleBuffer;
			summoningAngleThreshold_pinky += c_stopSummonAngleBuffer;
		}

		bool isFingerCurlInThreshold = (thumbCurl_AngleOfElevation < summoningAngleThreshold_thumb && thumbCurl_AngleOfElevation > 0f) &&
						(indexCurl_AngleOfElevation < summoningAngleThreshold_index && indexCurl_AngleOfElevation > 0f) &&
						(middleCurl_AngleOfElevation < summoningAngleThreshold_middle && middleCurl_AngleOfElevation > 0f) &&
						(ringCurl_AngleOfElevation < summoningAngleThreshold_ring && ringCurl_AngleOfElevation > 0f) &&
						(pinkyCurl_AngleOfElevation < summoningAngleThreshold_pinky && pinkyCurl_AngleOfElevation > 0f);

		float palmNormal_AngleFromUp = Mathf.Acos(Vector3.Dot(Vector3.up, leftHandData.wrist.rotation * Vector3.down)) * Mathf.Rad2Deg;
		bool isHandFacingUpwards = palmNormal_AngleFromUp < 65f;

		Vector3 direction_HMDToLeftHand = leftHandData.position - VRSStudioCameraRig.Instance.HMD.transform.position;
		bool isHandLeftOfHMD = (VRSStudioCameraRig.Instance.HMD.transform.InverseTransformDirection(direction_HMDToLeftHand)).x < 0f; //Left of HMD

		isCorrectPose = isFingerCurlInThreshold && isHandFacingUpwards && isHandLeftOfHMD;

		return isCorrectPose;
	}

	public void SummonNavMenu()
	{
		ResetMenuToInitialStatus();
		NavMenu.transform.position = MainMenuOffsetPosition + VRSStudioCameraRig.Instance.HMD.transform.position;
		NavMenu.transform.rotation = MainMenuOffsetRotation;

		closeMenuButton.ResetButton();

		closeMenuButton.NavMenuOpen(mainMenuLayer);
		PlayAnimation(navMenuAnimator, animation_OpenStateHash);
		openMenuAudioSrc.Play();

		menuActive = true;

		if (NavMenuSummonCallback != null) NavMenuSummonCallback.Invoke(menuActive);

		Log.d(LOG_TAG, "SummonNavMenu, Pos: " + NavMenu.transform.position);
	}

	public void ResetMenuToInitialStatus()
	{
		for (int i = 0; i < menuLayers.Count; i++)
		{
			NavMenuItem[] navMenuItemsToBeDeactiviated = menuLayers[i].transform.GetComponentsInChildren<NavMenuItem>();
			menuLayers[i].transform.localPosition = MenuObjectPoolingPosition;
			menuLayers[i].SetActive(true);

			foreach (NavMenuItem menuItem in navMenuItemsToBeDeactiviated)
			{
				menuItem.DisableButton();
			}
		}

		prevActiveMenuIndices.Clear();
		currActiveMenuIndex = 0;
		menuLayers[currActiveMenuIndex].transform.localPosition = Vector3.zero;
		NavMenuItem[] navMenuItemsToBeActiviated = menuLayers[currActiveMenuIndex].transform.GetComponentsInChildren<NavMenuItem>();
		foreach (NavMenuItem menuItem in navMenuItemsToBeActiviated)
		{
			menuItem.EnableButton();
			menuItem.ResetButton();
			menuItem.NavMenuOpen(menuLayers[currActiveMenuIndex]);
		}

		prevMenuButton.DisableButton();
	}

	int switchDownObjCount, switchUpObjCount, switchDownObjCount_Completed, switchUpObjCount_Completed;

	public void MoveToPrevMenuLayer()
	{
		Log.d(LOG_TAG, "MoveToPrevMenuLayer");
		MoveToMenuLayer(prevActiveMenuIndices.Pop(), false);
	}

	public void MoveToNextMenuLayer(int layerIndex)
	{
		Log.d(LOG_TAG, "MoveToNextMenuLayer");
		MoveToMenuLayer(layerIndex, true);
	}

	public void MoveToMenuLayer(int layerIndex, bool appendToStack)
	{
		Log.d(LOG_TAG, "MoveToMenuLayer: " + layerIndex);
		if (layerIndex < menuLayers.Count)
		{
			switchDownObjCount = switchUpObjCount = switchDownObjCount_Completed = switchUpObjCount_Completed = 0;
			bool isCurrentActiveLayer = false, isLayerToBeActivated = (layerIndex == currActiveMenuIndex);

			//Disable layers other than the target
			for (int i = 0; i < menuLayers.Count; i++)
			{
				Log.d(LOG_TAG, "Processing layer: " + i);
				isCurrentActiveLayer = (i == currActiveMenuIndex);

				if (isCurrentActiveLayer && isLayerToBeActivated)
				{
					Log.d(LOG_TAG, "Layer does not need to be processed.");
					continue; //Does not need to be deactivated
				}

				NavMenuItem[] navMenuItemsToBeDeactiviated = menuLayers[i].transform.GetComponentsInChildren<NavMenuItem>();
				if (isCurrentActiveLayer)
				{
					switchDownObjCount = navMenuItemsToBeDeactiviated.Length;
					Log.d(LOG_TAG, "switchDownObjCount: " + switchDownObjCount);
					StartCoroutine(NavMenuItemSwitchDownCompletionChecker(menuLayers[i], menuLayers[layerIndex]));
				}
				else //move already non active layer to pooling location just in case
				{
					menuLayers[i].transform.localPosition = MenuObjectPoolingPosition;
				}

				foreach (NavMenuItem menuItem in navMenuItemsToBeDeactiviated)
				{
					menuItem.DisableButton();
					if (isCurrentActiveLayer)
					{
						if (!menuItem.menuSwitchDownInvoker)
						{
							menuItem.NavMenuSwitchLayer_Down(menuLayers[i]); //start switch down animation for currently active items
						}
						else //Switch down and press animation conflict workaround
						{
							switchDownObjCount--;
							menuItem.menuSwitchDownInvoker = false; //Reset flag
						}
					}
				}
			}

			if (appendToStack) prevActiveMenuIndices.Push(currActiveMenuIndex);
			currActiveMenuIndex = layerIndex;

			if (prevActiveMenuIndices.Count == 0) //No prev menu to go back to
			{
				prevMenuButton.DisableButton();
				prevMenuButton.NavMenuClose(mainMenuLayer);
			}
			else if (prevMenuButton.IsButtonDisabled())
			{
				prevMenuButton.EnableButton();
				prevMenuButton.ResetButton();
				prevMenuButton.NavMenuOpen(mainMenuLayer);
			}
		}
		else
		{
			Log.d(LOG_TAG, "Layer Index is out of bounds: " + layerIndex);
		}
	}

	public void SwitchDownObjectCompletionCounterIncrement()
	{
		switchDownObjCount_Completed++;
		Log.d(LOG_TAG, "SwitchDownObjectCompletionCounterIncrement: " + switchDownObjCount_Completed);
	}

	public void SwitchUpObjectCompletionCounterIncrement()
	{
		switchUpObjCount_Completed++;
		Log.d(LOG_TAG, "SwitchUpObjectCompletionCounterIncrement: " + switchUpObjCount_Completed);
	}

	IEnumerator NavMenuItemSwitchDownCompletionChecker(GameObject switchDownTargetLayer, GameObject switchUpTargetLayer)
	{
		bool isSwitchDownCompleted = false;
		Log.d(LOG_TAG, "Start Coroutine NavMenuItemSwitchDownCompletionChecker, Target: " + switchDownTargetLayer.name);
		while (!isSwitchDownCompleted)
		{
			if (switchDownObjCount == switchDownObjCount_Completed && switchDownObjCount_Completed != 0)
			{
				Log.d(LOG_TAG, "NavMenuItemSwitchDownCompletionChecker: SwitchDownCompleted " + switchDownTargetLayer.name);
				isSwitchDownCompleted = true;
				switchDownObjCount = switchDownObjCount_Completed = 0; //Reset switch down counter values just in case
				switchDownTargetLayer.transform.localPosition = MenuObjectPoolingPosition; //move deactivated layer to pooling location

				switchUpTargetLayer.transform.localPosition = Vector3.zero; //move activated layer to menu location
				NavMenuItem[] navMenuItemsToBeActiviated = switchUpTargetLayer.transform.GetComponentsInChildren<NavMenuItem>();
				switchUpObjCount = navMenuItemsToBeActiviated.Length;
				Log.d(LOG_TAG, "switchUpObjCount: " + switchDownObjCount);
				StartCoroutine(NavMenuItemSwitchUpCompletionChecker(switchUpTargetLayer));

				foreach (NavMenuItem menuItem in navMenuItemsToBeActiviated)
				{
					menuItem.NavMenuSwitchLayer_Up(switchUpTargetLayer); //start switch up animation for items to be activated
				}
			}
			else
			{
				yield return null;
			}            
		}
	}

	IEnumerator NavMenuItemSwitchUpCompletionChecker(GameObject switchUpTargetLayer)
	{
		bool isSwitchUpCompleted = false;
		Log.d(LOG_TAG, "Start Coroutine NavMenuItemSwitchUpCompletionChecker, Target: " + switchUpTargetLayer.name);
		while (!isSwitchUpCompleted)
		{
			if (switchUpObjCount == switchUpObjCount_Completed && switchUpObjCount_Completed != 0)
			{
				Log.d(LOG_TAG, "NavMenuItemSwitchUpCompletionChecker: switchUpObjCount: " + switchUpObjCount);
				Log.d(LOG_TAG, "NavMenuItemSwitchUpCompletionChecker: SwitchUpCompleted " + switchUpTargetLayer.name);
				isSwitchUpCompleted = true;
				switchUpObjCount = switchUpObjCount_Completed = 0; //Reset values just in case

				NavMenuItem[] navMenuItemsToBeActiviated = switchUpTargetLayer.transform.GetComponentsInChildren<NavMenuItem>();

				foreach (NavMenuItem menuItem in navMenuItemsToBeActiviated)
				{
					menuItem.EnableButton();
					menuItem.ResetButton();
				}
			}
			else
			{
				yield return null;
			}
		}
	}

	public void CloseNavMenu()
	{
		Log.d(LOG_TAG, "CloseNavMenu");
		NavMenuItem[] navMenuItemsCurrentlyActive = menuLayers[currActiveMenuIndex].transform.GetComponentsInChildren<NavMenuItem>();
		foreach (NavMenuItem menuItem in navMenuItemsCurrentlyActive)
		{
			if (!menuItem.navMenuCloseInvoker) menuItem.NavMenuClose(menuLayers[currActiveMenuIndex]);
			else menuItem.navMenuCloseInvoker = false;
		}

		if (!prevMenuButton.IsButtonDisabled())
		{
			prevMenuButton.DisableButton();
			prevMenuButton.NavMenuClose(mainMenuLayer);
		}
		closeMenuButton.NavMenuClose(mainMenuLayer);
		PlayAnimation(navMenuAnimator, animation_CloseStateHash);
		if (NavMenuSummonCallback != null) NavMenuSummonCallback.Invoke(menuActive);
		StartCoroutine(CloseNavMenuCoroutine());
	}

	IEnumerator CloseNavMenuCoroutine()
	{
		while (menuActive)
		{
			if (!navMenuAnimator.IsInTransition(animation_LayerID) && navMenuAnimator.GetCurrentAnimatorStateInfo(animation_LayerID).IsName(animation_ClosedMenu))
			{
				//Close Animation Finished playing
				menuActive = false;
				NavMenu.transform.position = MenuObjectPoolingPosition;
			}
			else
			{
				yield return null;
			}
		}
		
	}

	public void HandleScenarioChangeRequest(string targetScenario)
	{
		Log.d(LOG_TAG, "HandleScenarioChangeRequest: " + targetScenario);
		CloseNavMenu();
		VRSStudioSceneManager.Instance.SwitchContentScene(targetScenario);
	}

	public void BeginMenuTutorial()
	{
		isRunningMenuTutorial = true;
		NavMenuSummonCallback += EndMenuTutorial;
		StartCoroutine(meshRendererFlash(TutorialSummonGesture.GetComponent<MeshRenderer>(), 0, 0.4f, 1f));
	}

	public void EndMenuTutorial(bool result)
	{
		if (result && isRunningMenuTutorial)
		{
			isRunningMenuTutorial = false;
			NavMenuSummonCallback -= EndMenuTutorial;
		}
	}

	public IEnumerator meshRendererFlash(MeshRenderer meshRenderer, float minAlphaValue, float maxAlphaValue, float interval)
	{
		float elapsedTime = 0;
		
		while (isRunningMenuTutorial)
		{
			elapsedTime += Time.deltaTime;
			float newAlpha = 0;
			if (elapsedTime < interval/2)
			{
				newAlpha = Mathf.Lerp(minAlphaValue, maxAlphaValue, elapsedTime / (interval/2));
			}
			else if (elapsedTime >= interval/2 && elapsedTime <= interval)
			{
				newAlpha = Mathf.Lerp(maxAlphaValue, minAlphaValue, (elapsedTime - (interval / 2)) / (interval/2));
			}
			else if (elapsedTime > interval)
			{
				elapsedTime = 0; //reset timer
			}

			
			meshRenderer.material.color = new Color(meshRenderer.material.color.r, meshRenderer.material.color.g, meshRenderer.material.color.b, newAlpha);
			yield return null;
		}
	}

	private void PlayAnimation(Animator animator, int animationStateHash)
	{
		if (animator != null && animator.HasState(animation_LayerID, animationStateHash))
		{
			animator.Play(animationStateHash);
		}
	}
}
