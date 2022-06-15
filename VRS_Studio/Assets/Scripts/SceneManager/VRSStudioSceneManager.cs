using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wave.Native;

public class VRSStudioSceneManager : MonoBehaviour
{
	public static VRSStudioSceneManager Instance = null;
	public string DemoAppEnvironmentScenePath, NavMenuScenePath, EntranceContentScenePath, TutorialContentScenePath, RobotAssistantScenePath;

	public delegate void SceneManager_OnLoadNewContentScene();
	public event SceneManager_OnLoadNewContentScene OnLoadNewContentScene;

	private string currentContentScenePath = null;

	private string LOG_TAG = "HandDemoSceneManager";

	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		if (DemoAppEnvironmentScenePath != null) SceneManager.LoadSceneAsync(DemoAppEnvironmentScenePath, LoadSceneMode.Additive);
		if (RobotAssistantScenePath != null) SceneManager.LoadSceneAsync(RobotAssistantScenePath, LoadSceneMode.Additive);
	}

	public void LoadNavMenuScene()
	{
		if (NavMenuScenePath != null) SceneManager.LoadSceneAsync(NavMenuScenePath, LoadSceneMode.Additive);
	}

	public void InitialSceneLoadSequence()
	{
		if (!TrackingBoundaryGuideManager.TutorialCompletedBefore() && TutorialContentScenePath != null)
		{
			SwitchContentScene(TutorialContentScenePath);
		}
		else
		{
			SwitchContentScene(EntranceContentScenePath);
		}
	}

	public void SwitchContentScene(string TargetContentScenePath, bool EnableEnvironment)
	{
		if (VRSStudioEnvController.Instance.IsEnvActive() != EnableEnvironment)
		{
			VRSStudioEnvController.Instance.SetEnvActive(EnableEnvironment);
		}

		if (currentContentScenePath != null)
		{
			Log.d(LOG_TAG, "Unloading Scene: " + currentContentScenePath);
			SceneManager.UnloadSceneAsync(currentContentScenePath);
		}
		if (TargetContentScenePath != null)
		{
			Log.d(LOG_TAG, "SwitchContentScene: " + TargetContentScenePath);
			SceneManager.LoadSceneAsync(TargetContentScenePath, LoadSceneMode.Additive);
			currentContentScenePath = TargetContentScenePath;
			if (OnLoadNewContentScene != null) OnLoadNewContentScene.Invoke();
		}
	}

	public void SwitchContentScene(string TargetContentScenePath) //Will enable env if not enabled
	{
		if (currentContentScenePath != null)
		{
			Log.d(LOG_TAG, "Unloading Scene: " + currentContentScenePath);
			SceneManager.UnloadSceneAsync(currentContentScenePath);
		}
		if (TargetContentScenePath != null)
		{
			Log.d(LOG_TAG, "SwitchContentScene: " + TargetContentScenePath);
			if (!VRSStudioEnvController.Instance.IsEnvActive())
			{
				VRSStudioEnvController.Instance.SetEnvActive(true);
			}
			SceneManager.LoadSceneAsync(TargetContentScenePath, LoadSceneMode.Additive);
			currentContentScenePath = TargetContentScenePath;
			if (OnLoadNewContentScene != null) OnLoadNewContentScene.Invoke();
		}
	}

	public string CurrentContentScenePath()
	{
		return currentContentScenePath;
	}
}
