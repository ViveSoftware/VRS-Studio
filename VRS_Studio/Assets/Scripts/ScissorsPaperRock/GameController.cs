using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static ScissorsPaperRock;

public class GameController : MonoBehaviour
{
    public ViveRoleProperty rightTrackedHandRole = ViveRoleProperty.New();
    public ViveRoleProperty leftTrackedHandRole = ViveRoleProperty.New();
    public StartButton startButton;
    public ScissorsPaperRock scissorsPaperRock;
    public GameObject textPanel;
    public GameObject[] resultTypes;
    public Material mat;
    public Animator gestureAnimator;
    public GameObject animatorObj;
    public AudioSource AudioPlayer = null;
    public AudioClip[] VoiceClips;
    public PlayableDirector[] playableDirectors;
    public GameObject[] counterObjs;
    public PlayableDirector[] resultPlayableDirectors;
    public GameObject[] resultObjs;
    private float time = -1.0f;
    private int counter = -1;
    private int invalidCount = 0;
    private ResultState gameResult = ResultState.Invalid;
    private GameState gameState = GameState.Init;

    public enum GameState
    {
        Invalid,
        Init,
        Start,
        End,
        Countdown,
        Wait,
    }

    private void OnEnable()
    {
        for (int i = 0; i < playableDirectors.Length; i++)
        {
            playableDirectors[i].played += OnPlayableDirectorPlayed;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < playableDirectors.Length; i++)
        {
            playableDirectors[i].played -= OnPlayableDirectorPlayed;
        }
    }

    void Update()
    {
        if (gameState == GameState.Init)
        {
            if (startButton.GameStart())
            {
                textPanel.SetActive(false);
                resultTypes[0].SetActive(false);
                startButton.gameObject.SetActive(false);
                gameState = GameState.Countdown;

                time = 0.5f;
                counter = 3;
                invalidCount = 0;
            }
        }
        else if (gameState == GameState.Countdown)
        {
            if (time >= 0)
            {
                time -= Time.deltaTime;
            }
            else if (counter == -1)
            {
                gameState = GameState.Start;
            }
            else
            {
                if (counter == 3)
                {
                    animatorObj.SetActive(true);
                    counterObjs[3].SetActive(true);
                    playableDirectors[3].Play();
                }
                else if (counter == 2)
                {
                    playableDirectors[3].Stop();
                    counterObjs[3].SetActive(false);
                    counterObjs[2].SetActive(true);
                    playableDirectors[2].Play();
                }
                else if (counter == 1)
                {
                    playableDirectors[2].Stop();
                    counterObjs[2].SetActive(false);
                    counterObjs[1].SetActive(true);
                    playableDirectors[1].Play();
                }
                else if (counter == 0)
                {
                    playableDirectors[1].Stop();
                    counterObjs[1].SetActive(false);
                    counterObjs[0].SetActive(true);
                    playableDirectors[0].Play();
                }

                counter--;
            }
        }
        else if (gameState == GameState.Start)
        {
            if (time >= 0)
            {
                time -= Time.deltaTime;
                return;
            }

            var right = scissorsPaperRock.DetectGesture(rightTrackedHandRole);
            var left = scissorsPaperRock.DetectGesture(leftTrackedHandRole);
            if (right != GestureType.Invalid && left != GestureType.Invalid)
            {
                gameResult = ResultState.SingleHand;
            }
            else if (right != GestureType.Invalid)
            {
                gameResult = scissorsPaperRock.RandomGesture(rightTrackedHandRole);
            }
            else if (left != GestureType.Invalid)
            {
                gameResult = scissorsPaperRock.RandomGesture(leftTrackedHandRole);
            }
            else
            {
                gameResult = ResultState.Invalid;
            }

            if (invalidCount < 3 && gameResult == ResultState.Invalid)
            {
                invalidCount++;
                time = 0.33f;
            }
            else
            {
                switch (gameResult)
                {
                    case ResultState.Invalid:
                        DisplayMessage(false, true);
                        break;
                    case ResultState.Lose:
                        ClearTextPanel();
                        resultObjs[1].SetActive(true);
                        resultPlayableDirectors[1].Play();
                        RobotAssistantManager.robotAssistantManagerInstance.TriggerReaction(RobotAssistantManager.ReactionAnimationIndex.Happy);
                        break;
                    case ResultState.Win:
                        ClearTextPanel();
                        resultObjs[0].SetActive(true);
                        resultPlayableDirectors[0].Play();
                        RobotAssistantManager.robotAssistantManagerInstance.TriggerReaction(RobotAssistantManager.ReactionAnimationIndex.Angry);
                        break;
                    case ResultState.Tie:
                        ClearTextPanel();
                        resultObjs[2].SetActive(true);
                        resultPlayableDirectors[2].Play();
                        break;
                    case ResultState.SingleHand:
                        DisplayMessage(true, false);
                        break;
                    default:
                        DisplayMessage(false, true);
                        break;
                }

                gameState = GameState.Wait;
                time = 2.5f;
            }
        }
        else if (gameState == GameState.End)
        {
            RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();

            animatorObj.SetActive(false);

            startButton.gameObject.SetActive(true);
            startButton.Restart();
            gameState = GameState.Init;
        }
        else if (gameState == GameState.Wait)
        {
            if (time >= 0)
            {
                time -= Time.deltaTime;
            }
            else if (gameResult == ResultState.Win || gameResult == ResultState.Lose)
            {
                resultObjs[0].SetActive(false);
                resultObjs[1].SetActive(false);
                gameState = GameState.End;
            }
            else
            {
                RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();

                ClearTextPanel();
                resultObjs[2].SetActive(false);
                gameState = GameState.Countdown;
                time = 0.5f;
                counter = 3;
                invalidCount = 0;
            }
        }
    }

    private void DisplayMessage(bool t1, bool t2)
    {
        textPanel.SetActive(true);
        resultTypes[1].SetActive(t1);
        resultTypes[2].SetActive(t2);
    }

    private void ClearTextPanel()
    {
        textPanel.SetActive(false);
        resultTypes[0].SetActive(false);
        resultTypes[1].SetActive(false);
        resultTypes[2].SetActive(false);
    }

    void OnPlayableDirectorPlayed(PlayableDirector director)
    {
        if (playableDirectors[3] == director)
        {
            gestureAnimator.SetTrigger("StartShaking");
            time = 0.7f;
        }
        else if (playableDirectors[2] == director)
        {
            time = 0.7f;
        }
        else if (playableDirectors[1] == director)
        {
            time = 0.6f;
        }
        else if (playableDirectors[0] == director)
        {
            time = 0.6f;
        }
    }
}
