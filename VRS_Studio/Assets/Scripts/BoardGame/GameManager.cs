using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    PokerManager PokerManager { get { return PokerManager.instance; } }
    CatchManager CatchManager { get { return CatchManager.instance; } }

    [HideInInspector] public GameMode gameMode = GameMode.Init;
    [HideInInspector] public GameState gameState = GameState.Invalid;
    [HideInInspector] public ResultState gameResult = ResultState.Invalid;

    public StartButton[] startButton; //0:dice 1:poker

    public GameObject[] Panel;
    public Text textDiceModeInstrcution, textPokerModeInstrcution;

    [Header("Dice Mode")]
    public GameObject[] Dice;
    Vector3[] InitDicePosition;
    bool isRobotTossing = false;
    public float RobotDicePoint;
    public GameObject RobotDicePointPanel;
    public Text textRobotDicePoint, textPlayerDicePoint;

    [Header("Poker Mode")]
    public Transform CardDeck;
    GameObject[] Card;
    GameObject[] RobotCard, PlayerCard;
    public Transform RobotCardArea, PlayerCardArea, OutCardArea;
    public Transform RobotPlayedArea;
    public GameObject RobotPlayCard, PlayerPlayCard;
    public bool PlayerPlayState = false;

    [Header("Feedback")]
    public Transform resultObj;
    public GameObject[] resultObjs;
    public PlayableDirector[] resultPlayableDirectors;
    public AudioSource AudioPlayer = null;
    public AudioClip[] VoiceClips;

    float time = 0;

    public enum GameMode
    {
        Init = 0,
        DiceMode = 1,
        PokerMode = 2
    }
    public enum GameState
    {
        Invalid,
        Start,
        Wait,
    }
    public enum ResultState
    {
        Invalid = 0,
        Win = 1,
        Lose = 2,
        Tie = 3
    }
    private void Awake()
    {
        InitDicePosition = new Vector3[Dice.Length];
        for (int a = 0; a < Dice.Length; a++)
            InitDicePosition[a] = Dice[a].transform.position;

        //the deck of cards        
        ResetCardDeck();
    }

    private void Update()
    {
        //selecting the mode
        if (gameMode == GameMode.Init)
        {
            if (startButton[0].GameStart())
            {
                ModeSelect(1);
                resultObj.position = new Vector3(-0.85f, 1.2f, 0.55f);
                resultObj.eulerAngles = new Vector3(0, -65f, 0);
                startButton[0].gameObject.SetActive(false);
            }
            else if (startButton[1].GameStart())
            {
                ModeSelect(2);
                resultObj.position = new Vector3(0.9f, 1.2f, 0.35f);
                resultObj.eulerAngles = new Vector3(0, 65f, 0);
                startButton[1].gameObject.SetActive(false);
            }
        }
        //toss dice mode
        else if (gameMode == GameMode.DiceMode)
        {
            //can change poker mode in the state
            if (!startButton[1].gameObject.activeSelf)
            {
                startButton[1].Restart();
                startButton[1].gameObject.SetActive(true);
            }
            if (startButton[1].GameStart())
            {
                //reset dice mode
                RespwanDice(false);
                RobotDicePoint = 0;
                textRobotDicePoint.text = "0";
                textPlayerDicePoint.text = "0";
                RobotDicePointPanel.SetActive(false);

                ModeSelect(2);
                resultObj.position = new Vector3(0.9f, 1.2f, 0.35f);
                resultObj.eulerAngles = new Vector3(0, 65f, 0);
                startButton[1].gameObject.SetActive(false);
                for (int a = 0; a < 3; a++) resultObjs[a].SetActive(false);
            }
            //robot will toss and change player's time
            if (gameState == GameState.Invalid)
            {
                RobotAssistantManager.Instance.ForceStopReaction();
                StartCoroutine(DynamicPanelHint("Waiting for robot toss the dices", 1, 0));
                if (time >= 0)
                {
                    time -= Time.deltaTime;
                    isRobotTossing = false;
                    return;
                }

                if (isRobotTossing == false)
                {
                    RobotAssistantManager.Instance.SetRobotPosition(new Vector3(-0.85f, 1f, 0.55f));

                    for (int a = 0; a < Dice.Length; a++)
                    {
                        Dice[a].transform.DOMove(new Vector3(InitDicePosition[a].x, InitDicePosition[a].y + 0.25f, InitDicePosition[a].z), 0.5f).SetDelay(2f);
                        Dice[a].transform.DOLocalRotate(new Vector3(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f)), 0.5f).SetDelay(2f).SetLoops(0);
                    }
                    Invoke("RobotTossDice", 3f);
                    isRobotTossing = true;
                }

                if (RobotDicePoint <= 0)
                    return;
                else
                {
                    RobotAssistantManager.Instance.SetRobotPosition(new Vector3(-1.175f, 1f, 0.7f));

                    textRobotDicePoint.text = RobotDicePoint.ToString();
                    RespwanDice(true);
                    gameState = GameState.Start;
                }
            }
            //start game and check win or loss
            else if (gameState == GameState.Start)
            {
                switch (gameResult)
                {
                    case ResultState.Invalid:
                        StartCoroutine(DynamicPanelHint("Using any hand to catch and toss the dices to win robot's point", 1, 0));
                        return;
                    case ResultState.Win:
                        resultObjs[0].SetActive(true);
                        resultPlayableDirectors[0].Play();
                        if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                        AudioPlayer.PlayOneShot(VoiceClips[0]);

                        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Scare);

                        break;
                    case ResultState.Lose:
                        resultObjs[1].SetActive(true);
                        resultPlayableDirectors[1].Play();
                        if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                        AudioPlayer.PlayOneShot(VoiceClips[1]);

                        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Clap);

                        break;
                    case ResultState.Tie:
                        resultObjs[2].SetActive(true);
                        resultPlayableDirectors[2].Play();

                        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.ScratchHead);

                        break;
                }
                time = 2.5f;
                gameState = GameState.Wait;
            }
            //after robot reaction to restart
            else if (gameState == GameState.Wait)
            {
                StartCoroutine(DynamicPanelHint("Ready to next game", 1, 0));
                if (time >= 0)
                {
                    time -= Time.deltaTime;
                    return;
                }
                else
                {
                    //restore warn panel
                    Panel[1].SetActive(true);
                    Panel[3].SetActive(false);

                    RobotAssistantManager.Instance.ForceStopReaction();

                    for (int a = 0; a < 3; a++) resultObjs[a].SetActive(false);

                    RespwanDice(false);
                    RobotDicePoint = 0;
                    textRobotDicePoint.text = "0";
                    textPlayerDicePoint.text = "0";

                    time = 0.5f;
                    gameState = GameState.Invalid;
                    gameResult = ResultState.Invalid;
                }
            }
        }
        //high point with poker
        else if (gameMode == GameMode.PokerMode)
        {
            //can change dice mode in the state
            if (!startButton[0].gameObject.activeSelf)
            {
                startButton[0].Restart();
                startButton[0].gameObject.SetActive(true);
            }
            if (startButton[0].GameStart())
            {
                //reset poker mode
                ResetCardDeck();
                CardDeck.DOLocalMove(new Vector3(0.55f, 0.96f, 0.55f), 0.25f);

                ModeSelect(1);
                resultObj.position = new Vector3(-0.85f, 1.2f, 0.55f);
                resultObj.eulerAngles = new Vector3(0, -65f, 0);
                startButton[0].gameObject.SetActive(false);
                for (int a = 0; a < 3; a++) resultObjs[a].SetActive(false);
            }
            //ready to start poker game
            if (gameState == GameState.Invalid)
            {
                //left hand to pick the poker
                if (Vector3.Distance(PokerManager.Left_index.position, PlayerCardArea.position) < 0.1f && PokerManager.CardHolder.GetChild(0).childCount <= 0)
                {
                    PlayerCardArea.localScale = Vector3.Lerp(PlayerCardArea.localScale, new Vector3(1.1f, 1.1f, 1.1f), 0.5f);
                    if (Vector3.Distance(PokerManager.Left_index.position, PokerManager.Left_thumb.position) < 0.05f)
                    {
                        PlayerCardArea.localScale = new Vector3(1, 1, 1);
                        for (int a = 0; a < 5; a++) PlayerCard[a].transform.parent = PokerManager.Left_palm;
                    }
                }
                //take it back
                else if (PokerManager.Left_palm.childCount > 0 && Vector3.Distance(PokerManager.Left_index.position, PlayerCardArea.position) > 0.2f)
                {
                    for (int a = 0; a < 5; a++) PlayerCard[a].transform.parent = PokerManager.CardHolder.GetChild(0);
                    PokerManager.UpdateCardCount(true);

                    SortRobotCard();
                    time = 2.0f;
                    gameState = GameState.Wait;
                }
                else
                {
                    PlayerCardArea.localScale = Vector3.Lerp(PlayerCardArea.localScale, new Vector3(1, 1, 1), 0.5f);
                }
            }
            //wait and check player point
            else if (gameState == GameState.Start)
            {
                if (time >= 0)
                {
                    time -= Time.deltaTime;
                    return;
                }
                switch (gameResult)
                {
                    case ResultState.Invalid:
                        StartCoroutine(DynamicPanelHint("Right hand pinch a card to play higher than the robot's point to win", 2, 0));
                        return;
                    case ResultState.Win:
                        resultObjs[0].SetActive(true);
                        resultPlayableDirectors[0].Play();
                        if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                        AudioPlayer.PlayOneShot(VoiceClips[0]);

                        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Scare);

                        break;
                    case ResultState.Lose:
                        resultObjs[1].SetActive(true);
                        resultPlayableDirectors[1].Play();
                        if (AudioPlayer.isPlaying) AudioPlayer.Stop();
                        AudioPlayer.PlayOneShot(VoiceClips[1]);

                        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Clap);

                        break;
                    case ResultState.Tie:
                        resultObjs[2].SetActive(true);
                        resultPlayableDirectors[2].Play();
                        break;
                }
                RobotPlayCard.transform.DOLocalMove(new Vector3(0, 0.05f, 0), 0.25f).SetDelay(0.25f);
                RobotPlayCard.transform.DOLocalRotate(new Vector3(-90, 180, -180), 0.25f).SetLoops(0);
                time = 2.5f;
                gameState = GameState.Wait;
            }
            //wait robot play card
            else if (gameState == GameState.Wait)
            {
                if (time >= 0)
                {
                    time -= Time.deltaTime;
                    return;
                }
                else
                {
                    //restore warn panel
                    Panel[2].SetActive(true);
                    Panel[4].SetActive(false);

                    RobotAssistantManager.Instance.ForceStopReaction();

                    for (int a = 0; a < 3; a++) resultObjs[a].SetActive(false);

                    //old robot, player card to temp outside and clean out
                    if (RobotPlayCard != null)
                    {
                        RobotPlayCard.transform.DOLocalMove(new Vector3(0.15f, 0.001f * RobotPlayedArea.childCount, 0), 0.25f);
                        RobotPlayCard.transform.DOLocalRotate(new Vector3(0, 0, -180), 0.25f).SetLoops(0);
                        RobotPlayCard = null;
                    }
                    if (PlayerPlayCard != null)
                    {
                        PlayerPlayCard.transform.parent = PlayerCardArea;
                        PlayerPlayCard.GetComponent<Rigidbody>().isKinematic = true;
                        PlayerPlayCard.transform.DOLocalMove(new Vector3(0.15f, 0.001f * RobotPlayedArea.childCount, 0), 0.25f);
                        PlayerPlayCard.transform.DOLocalRotate(new Vector3(0, 0, -180), 0.25f).SetLoops(0);
                        PlayerPlayCard = null;
                    }

                    //new round
                    if (RobotCard.Length > 0)
                    {
                        RobotPlayCard = RobotCard[Random.Range(0, RobotCard.Length)];
                        RobotPlayCard.transform.parent = RobotPlayedArea;
                        RobotPlayCard.transform.DOLocalMove(new Vector3(0, 0.001f * RobotPlayedArea.childCount, 0), 0.25f);
                        RobotPlayCard.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.25f).SetLoops(0);
                        SortRobotCard();

                        gameState = GameState.Start;
                        PlayerPlayState = true;
                    }
                    else //redeal
                    {
                        RobotCard = new GameObject[5];
                        CardDeck.DOLocalMove(new Vector3(0.55f, 0.96f, 0.55f), 0.25f);
                        for (int a = 0; a < 5; a++)
                        {
                            RobotPlayedArea.GetChild(0).parent = OutCardArea;
                            PlayerCard[a].transform.parent = OutCardArea;
                            PlayerCard[a].GetComponent<Rigidbody>().isKinematic = true;
                        }
                        for (int a = 0; a < OutCardArea.childCount; a++)
                        {
                            OutCardArea.GetChild(a).DOLocalMove(new Vector3(0, 0.001f * a, 0), 0.25f);
                            OutCardArea.GetChild(a).DOLocalRotate(new Vector3(0, 0, 180f), 0.25f).SetLoops(0);
                        }
                        Invoke("DealCards", 0.5f);
                        gameState = GameState.Invalid;
                    }
                    gameResult = ResultState.Invalid;
                }
            }
        }
    }
    void ModeSelect(int mode)
    {
        if (mode == 1)
        {
            CatchManager.enabled = true;
            PokerManager.enabled = false;
            RobotDicePointPanel.SetActive(true);
            RobotAssistantManager.Instance.SetRobotPosition(new Vector3(-1.175f, 1.15f, 0.7f));

            gameMode = GameMode.DiceMode;
            time = 3f;
        }
        else if (mode == 2)
        {
            CatchManager.enabled = false;
            PokerManager.enabled = true;
            RobotAssistantManager.Instance.SetRobotPosition(new Vector3(1.35f, 1.15f, 0.55f), DealCards);

            gameMode = GameMode.PokerMode;
        }
        gameState = GameState.Invalid;
        for (int a = 0; a < 3; a++) Panel[a].SetActive(false);
        Panel[mode].SetActive(true);
    }
    private void RespwanDice(bool isAllowCatable)
    {
        for (int a = 0; a < Dice.Length; a++)
        {
            Dice[a].transform.DOMove(InitDicePosition[a], 0.25f);
            Dice[a].transform.DORotate(new Vector3(0, 0, 0), 0.25f);
            Dice[a].GetComponent<Rigidbody>().isKinematic = true;
            Dice[a].GetComponent<SmallObjectProperty>().canCauclateCD = 20;
            Dice[a].GetComponent<SmallObjectProperty>().canCauclate = false;
            Dice[a].GetComponent<SmallObjectProperty>().AllowCatchable = isAllowCatable;
        }
    }
    private void RobotTossDice()
    {
        RobotAssistantManager.Instance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Clap);

        for (int a = 0; a < Dice.Length; a++)
        {
            Dice[a].GetComponent<Rigidbody>().isKinematic = false;
            Dice[a].GetComponent<Rigidbody>().AddForce(Vector3.down, ForceMode.Impulse);
            Dice[a].GetComponent<Rigidbody>().AddTorque(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
            Dice[a].GetComponent<SmallObjectProperty>().canCauclateCD = 2.5f;
        }
    }
    public void CompareDicePoint(int PlayerPoint)
    {
        if (gameState != GameState.Start) return;

        textPlayerDicePoint.text = PlayerPoint.ToString();
        if (PlayerPoint > RobotDicePoint) gameResult = ResultState.Win;
        else if (PlayerPoint < RobotDicePoint) gameResult = ResultState.Lose;
        else if (PlayerPoint == RobotDicePoint) gameResult = ResultState.Tie;
    }

    private void ResetCardDeck()
    {
        DOTween.CompleteAll();
        //get card back not catching in change mode
        if (RobotCard != null)
        {
            int count = RobotPlayedArea.childCount;
            for (int a = 0; a < count; a++) RobotPlayedArea.GetChild(0).parent = OutCardArea;
            for (int a = 0; a < RobotCard.Length; a++) if (RobotCard[a] != null) RobotCard[a].transform.parent = OutCardArea;
            for (int a = 0; a < 5; a++) if (PlayerCard[a] != null) PlayerCard[a].transform.parent = OutCardArea;
            RobotPlayCard = null;
            PlayerCard = null;
        }
        //out card area back to carddeck
        if (OutCardArea.childCount > 0)
        {
            int count = OutCardArea.childCount;
            for (int a = 0; a < count; a++) OutCardArea.GetChild(0).parent = CardDeck;
        }

        Card = new GameObject[CardDeck.childCount];
        //random sort
        for (int a = 0; a < CardDeck.childCount; a++) CardDeck.GetChild(a).SetSiblingIndex(Random.Range(0, CardDeck.childCount));

        for (int a = 0; a < CardDeck.childCount; a++)
        {
            Card[a] = CardDeck.GetChild(a).gameObject;
            CardDeck.GetChild(a).localPosition = new Vector3(0, 0.0005f * a, 0);
            CardDeck.GetChild(a).localEulerAngles = new Vector3(0, 0, 0);
        }
    }
    private void DealCards()
    {
        if (CardDeck.childCount < 10) ResetCardDeck();

        StartCoroutine(DynamicPanelHint("Waitting for dealing the cards", 2, 0));
        StartCoroutine(DynamicPanelHint("Left hand pinch to take them back and keep cardholder on", 2, 2));

        RobotCard = new GameObject[5];
        PlayerCard = new GameObject[5];

        for (int a = 0; a < 5; a++)
        {
            RobotCard[a] = Card[Card.Length - 2 * (a + 1)];
            PlayerCard[a] = Card[Card.Length - (2 * a + 1)];
            Card[Card.Length - 2 * (a + 1)].transform.parent = RobotCardArea;
            Card[Card.Length - (2 * a + 1)].transform.parent = PlayerCardArea;

            RobotCard[a].transform.DOLocalMove(new Vector3(0, 0.001f * a, -0.001f * a), 0.25f).SetDelay(0.25f * a);
            RobotCard[a].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.25f).SetLoops(0).SetDelay(0.25f * a);
            PlayerCard[a].transform.DOLocalMove(new Vector3(0, 0.001f * a, 0), 0.25f).SetDelay(0.25f * a);
            PlayerCard[a].transform.DOLocalRotate(new Vector3(0, 0, 0), 0.25f).SetLoops(0).SetDelay(0.25f * a);
            StartCoroutine(PlayDelayedClip(VoiceClips[2], 0.25f * a));
        }
        CardDeck.DOLocalMove(new Vector3(0.75f, 0.96f, 0.35f), 0.5f).SetLoops(0).SetDelay(1.5f);

        //update carddeck after dealing the cards
        Card = new GameObject[CardDeck.childCount];
        for (int a = 0; a < CardDeck.childCount; a++) Card[a] = CardDeck.GetChild(a).gameObject;
    }
    private void SortRobotCard()
    {
        int cardcount = RobotCardArea.childCount;

        RobotCard = new GameObject[cardcount];
        for (int a = 0; a < cardcount; a++) RobotCard[a] = RobotCardArea.GetChild(a).gameObject;
        //correct robot poker rotation
        for (int a = 0; a < cardcount; a++)
            RobotCard[a].transform.DOLocalRotate(new Vector3(0, -20f + (10f * a), 0), 0.5f).SetLoops(0);
    }
    public void ComparePokerPoint()
    {
        time = 2f;
        if (gameState != GameState.Start) return;

        if (PlayerPlayCard.GetComponent<PokerProperty>().Number > RobotPlayCard.GetComponent<PokerProperty>().Number)
            gameResult = ResultState.Win;
        else if (PlayerPlayCard.GetComponent<PokerProperty>().Number == RobotPlayCard.GetComponent<PokerProperty>().Number)
        {
            if (PlayerPlayCard.GetComponent<PokerProperty>().Suit > RobotPlayCard.GetComponent<PokerProperty>().Suit)
                gameResult = ResultState.Win;
            else gameResult = ResultState.Lose;
        }
        else gameResult = ResultState.Lose;
    }

    private IEnumerator DynamicPanelHint(string hint, int mode, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (mode == 1) textDiceModeInstrcution.text = hint;
        else if (mode == 2) textPokerModeInstrcution.text = hint;
        yield return null;
    }
    private IEnumerator PlayDelayedClip(AudioClip _clip, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        AudioPlayer.PlayOneShot(_clip);
    }
}