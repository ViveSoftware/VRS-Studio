using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotScenarioControllerBase : MonoBehaviour
{
    public GameObject InitialPositionAnchorObject;

	public void InitializeRobot()
	{
		RobotAssistantManager.robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(false);
		StartCoroutine(Initialization());
	}

	private IEnumerator Initialization()
	{
        yield return StartCoroutine(RobotAssistantManager.robotAssistantManagerInstance.RobotStartTeleport(InitialPositionAnchorObject.transform.position));
    }
}
