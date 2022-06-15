using Microsoft.CognitiveServices.Speech;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Wave.VoiceCommand;

public class RobotAssistantEntranceSequence : MonoBehaviour
{
    public RobotAssistantManager robotAssistantManagerInstance = null;
    private VoiceCommandManager voiceCommandManager = null;

    private string tutorialIntroLine = "Hello there! It seems that this is your first time here, let's warm ourselves up for a bit.";
    private string replayIntroLine = "Hey, you are back! Open the menu to try out different experiences.";

    // Start is called before the first frame update
    void Start()
    {
        if (robotAssistantManagerInstance == null)
        {
            robotAssistantManagerInstance = RobotAssistantManager.robotAssistantManagerInstance;
        }

        voiceCommandManager = VoiceCommandManager.Instance;

        RobotAssistantLoSCaster.RobotAssistantLoS_EnterCallback += PowerOnSequence;
    }

    #region LoS reaction
    private void PowerOnSequence()
    {
        RobotAssistantLoSCaster.RobotAssistantLoS_EnterCallback -= PowerOnSequence;
        StartCoroutine(PowerOnSequenceCoroutine());
    }

    private IEnumerator PowerOnSequenceCoroutine()
    {
        yield return StartCoroutine(robotAssistantManagerInstance.PowerOnRobot());
        robotAssistantManagerInstance.SetRobotPosition(new Vector3(transform.position.x, VRSStudioCameraRig.Instance.HMD.transform.position.y - 0.35f, transform.position.z), PowerOnSequenceComplete);
    }

    private async void PowerOnSequenceComplete()
    {
        VRSStudioSceneManager.Instance.LoadNavMenuScene();

        if (!TrackingBoundaryGuideManager.TutorialCompletedBefore())
        {
            await ToTutorialSequence();
        }
        else
        {
            await ReplaySequence();
        }
    }

    private async Task ToTutorialSequence()
    {
#if !VRSSTUDIO_INTERNAL
        if (VoiceCommandManager.Instance.IsCognitiveServicesInfoValid())
        {
#endif
        await VoiceCommandManager.Instance.ReInitializeSpeechSynthesizer("en-US", "en-US-GuyNeural");

        VoiceCommandManager.Instance.SpeechSynthesizerComponent.SynthesisStarted += OnToTutorialVoiceBegin;

        await VoiceCommandManager.Instance.StartSynthesis(tutorialIntroLine);
#if !VRSSTUDIO_INTERNAL
        }
        else
        {
            StartCoroutine(OnToTutorialVoiceBeginCoroutine());
        }            
#endif
    }

    private void OnToTutorialVoiceBegin(object sender, SpeechSynthesisEventArgs e)
    {
        StartCoroutine(OnToTutorialVoiceBeginCoroutine());
    }

    IEnumerator OnToTutorialVoiceBeginCoroutine()
    {
        robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(true);
        yield return new WaitForSecondsRealtime(1.5f);
        robotAssistantManagerInstance.robotAssistantSpeechBubble.RobotLines = tutorialIntroLine;
        robotAssistantManagerInstance.robotAssistantSpeechBubble.typingInterval = 0.05f;
        yield return StartCoroutine(robotAssistantManagerInstance.robotAssistantSpeechBubble.PlayTypingWordAnim());
        yield return new WaitForSecondsRealtime(1f);
        OnToTutorialVoiceEnd();
    }

    private void OnToTutorialVoiceEnd()
    {
        robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(false);
        robotAssistantManagerInstance.SetRobotPosition(new Vector3(-1.141f, 0.809f, 1.612f), VRSStudioSceneManager.Instance.InitialSceneLoadSequence);
    }

    private async Task ReplaySequence()
    {
        VRSStudioSceneManager.Instance.InitialSceneLoadSequence();

#if !VRSSTUDIO_INTERNAL
        if (VoiceCommandManager.Instance.IsCognitiveServicesInfoValid())
        {
#endif
        await VoiceCommandManager.Instance.ReInitializeSpeechSynthesizer("en-US", "en-US-GuyNeural");

        VoiceCommandManager.Instance.SpeechSynthesizerComponent.SynthesisStarted += ReplayVoiceBegin;

        await VoiceCommandManager.Instance.StartSynthesis(replayIntroLine);
#if !VRSSTUDIO_INTERNAL
        }
        else
        {
            StartCoroutine(ReplayVoiceBeginCoroutine());
        }
#endif
    }

    private void ReplayVoiceBegin(object sender, SpeechSynthesisEventArgs e)
    {
        StartCoroutine(ReplayVoiceBeginCoroutine());
    }

    IEnumerator ReplayVoiceBeginCoroutine()
    {
        robotAssistantManagerInstance.robotAssistantSpeechBubble.TextBoardShowup(true);
        yield return new WaitForSecondsRealtime(1.5f);
        robotAssistantManagerInstance.robotAssistantSpeechBubble.RobotLines = replayIntroLine;
        robotAssistantManagerInstance.robotAssistantSpeechBubble.typingInterval = 0.05f;
        yield return StartCoroutine(robotAssistantManagerInstance.robotAssistantSpeechBubble.PlayTypingWordAnim());
        yield return new WaitForSecondsRealtime(1f);
    }

    #endregion
}
