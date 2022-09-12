using UnityEngine;
using System;
using DG.Tweening;

public class PokerManager : MonoBehaviour
{
    private static PokerManager _instance;
    public static PokerManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PokerManager>();
            }
            return _instance;
        }
    }
    GameManager GameManager { get { return GameManager.instance; } }

    GameObject[] Card;
    public Transform CardHolder;
    private bool CardHolderSwitch;
    private bool CardHolderTracking;    
    [SerializeField] private Transform CardHolderPoseTracker;

    public Transform Left_index, Left_thumb, Left_palm, Right_index, Right_thumb, Right_palm, RightCardPivot;

    private GameObject SelectedCard;
    private int BeforeSelectedIndex;
    private float[] CardHandDistance;
    private bool Picking = false;

    private bool LeftPinchState(float distance)
    {
        if (Vector3.Distance(Left_index.position, Left_thumb.position) < distance) return true;
        return false;
    }
    private bool RightPinchState(float distance, bool open)
    {
        if (Vector3.Distance(Right_index.position, Right_thumb.position) < distance && open == false) return true;
        else if (Vector3.Distance(Right_index.position, Right_thumb.position) > distance && open == true) return true;
        return false;
    }
    //for saving sorting card before releasing
    public GameObject VirtualCard;

    private void Awake()
    {
        CardHolderTracking = true;
    }
    public void UpdateCardCount(bool autoSort)
    {
        int cardcount = CardHolder.GetChild(0).childCount;

        Card = new GameObject[cardcount];
        CardHandDistance = new float[cardcount];
        for (int a = 0; a < cardcount; a++)
            Card[a] = CardHolder.GetChild(0).GetChild(a).gameObject;

        SortCard(cardcount, autoSort);
    }
    private void SortCard(int cardcount, bool autoSort)
    {
        //autosort
        if (autoSort == true)
        {
            GameObject tempCard;
            for (int a = 0; a < cardcount - 1; a++)
            {
                for (int b = 0; b < cardcount - 1; b++)
                {
                    if (Card[b].GetComponent<PokerProperty>().Number > Card[b + 1].GetComponent<PokerProperty>().Number)
                    {
                        tempCard = Card[b];
                        Card[b] = Card[b + 1];
                        Card[b + 1] = tempCard;
                    }
                    else if (Card[b].GetComponent<PokerProperty>().Number == Card[b + 1].GetComponent<PokerProperty>().Number)
                    {
                        if (Card[b].GetComponent<PokerProperty>().Suit > Card[b + 1].GetComponent<PokerProperty>().Suit)
                        {
                            tempCard = Card[b];
                            Card[b] = Card[b + 1];
                            Card[b + 1] = tempCard;
                        }
                    }
                    Card[b].transform.SetSiblingIndex(b);
                    Card[b + 1].transform.SetSiblingIndex(b + 1);
                }
            }
        }

        //correct poker position/rotation
        for (int a = 0; a < cardcount; a++)
        {
            Card[a].transform.DOLocalMove(new Vector3(0, -0.001f * a, 0), 0.5f);
            Card[a].transform.DOLocalRotate(new Vector3(0, -10f * a, 0), 0.5f).SetLoops(0);
        }

        //correct cardholder position/rotation
        CardHolder.GetChild(0).DOLocalRotate(new Vector3(0, 5f * (cardcount - 1), 0), 0.5f).SetLoops(0);
    }

    public void PickupCard(bool incardholder)
    {
        if (SelectedCard != null)
        {
            if (incardholder == true)
            {
                for (int a = 0; a < Card.Length; a++)
                {
                    if (Card[a] == SelectedCard)
                    {
                        Card[a] = VirtualCard;
                        BeforeSelectedIndex = a;
                        VirtualCard.GetComponent<PokerProperty>().Card.GetComponent<MeshRenderer>().enabled = true;
                        VirtualCard.transform.parent = SelectedCard.transform.parent;
                        VirtualCard.transform.SetSiblingIndex(a);
                        SelectedCard.transform.parent = RightCardPivot;
                        break;
                    }
                }
            }
            Picking = true;
            SelectedCard.transform.DOLocalMove(new Vector3(-0.03f, -0.0225f, 0.08f), 0.25f);
            SelectedCard.transform.DOLocalRotate(new Vector3(180, -20, -90f), 0.25f).SetLoops(0);
        }
    }

    private void FixedUpdate()
    {
        RightCardPivot.position = (Right_index.position + Right_thumb.position) / 2;
        RightCardPivot.rotation = Right_palm.rotation;
        //two finger pinch of left hand to show cardholder
        if (LeftPinchState(0.05f))
        {
            CardHolderSwitch = true;
            CardHolder.localScale = Vector3.Lerp(CardHolder.localScale, new Vector3(1, 1, 1), 0.3f);
        }
        else
        {
            CardHolderSwitch = false;
            CardHolder.localScale = Vector3.Lerp(CardHolder.localScale, new Vector3(0, 0, 0), 0.3f);
        }

        //smooth move to palm tracker
        if (CardHolderTracking == true || CardHolderSwitch == false)
        {
            CardHolder.position = Vector3.Lerp(CardHolder.position, CardHolderPoseTracker.position, 0.2f);
            CardHolder.LookAt(Camera.main.transform);
            CardHolder.eulerAngles = new Vector3(CardHolder.eulerAngles.x - 75f, CardHolder.eulerAngles.y, CardHolder.eulerAngles.z);
        }
    }

    private void Update()
    {
        if (CardHolderSwitch == true)
        {
            //select card
            if (Picking == false && GameManager.PlayerPlayState == true)
            {
                if (Vector3.Distance(RightCardPivot.position, CardHolder.position) < 0.16f)
                {
                    CardHolderTracking = false;
                    //record distancecheck
                    for (int a = 0; a < Card.Length; a++)
                    {
                        if (RightPinchState(0.03f, true))
                        {
                            CardHandDistance[a] = Vector3.Distance(RightCardPivot.position, Card[a].GetComponent<PokerProperty>().disstancecheck.transform.position);
                            Card[a].GetComponent<PokerProperty>().SelectDistance = CardHandDistance[a];
                        }

                        if (a == Card.Length - 1)
                            Array.Sort(CardHandDistance);
                    }

                    //after distancecheck to highlight
                    for (int a = 0; a < Card.Length; a++)
                    {
                        if (RightPinchState(0.03f, true))
                        {
                            if (Card[a].GetComponent<PokerProperty>().SelectDistance == CardHandDistance[0])
                            {
                                SelectedCard = Card[a];
                                Card[a].GetComponent<PokerProperty>().SelectTrigger = true;
                            }
                            else
                            {
                                if (SelectedCard == Card[a])
                                    SelectedCard = null;
                                Card[a].GetComponent<PokerProperty>().SelectTrigger = false;
                            }
                        }
                        else
                            Card[a].GetComponent<PokerProperty>().SelectTrigger = false;
                    }

                    //right pinch to draw card
                    if (RightPinchState(0.03f, false))
                        PickupCard(true);
                }
                else
                {
                    CardHolderTracking = true;
                    for (int a = 0; a < Card.Length; a++)
                        Card[a].GetComponent<PokerProperty>().SelectTrigger = false;
                }
            }
            else //recover highlight
            {
                for (int a = 0; a < Card.Length; a++)
                    Card[a].GetComponent<PokerProperty>().SelectTrigger = false;
            }
        }

        if (Picking == true)
        {
            //release card form right pinch
            if (RightPinchState(0.05f, true))
            {
                SelectedCard.transform.DOComplete();
                //play card
                if (CardHolderTracking == true || CardHolderSwitch == false)
                {
                    VirtualCard.transform.parent = CardHolder;
                    VirtualCard.GetComponent<PokerProperty>().Card.GetComponent<MeshRenderer>().enabled = false;
                    SelectedCard.GetComponent<PokerProperty>().Throw();
                    GameManager.PlayerPlayCard = SelectedCard;
                    GameManager.PlayerPlayState = false;
                    SelectedCard = null;
                    Picking = false;
                    UpdateCardCount(false);
                    GameManager.ComparePokerPoint();
                }
                //withdraw
                else if (CardHolderTracking == false)
                {
                    Picking = false;
                    for (int a = 0; a < Card.Length; a++)
                    {
                        if (Card[a] == VirtualCard)
                        {
                            Card[a] = SelectedCard;
                            Card[a].transform.parent = VirtualCard.transform.parent;
                            Card[a].transform.SetSiblingIndex(a);
                            Card[a].GetComponent<PokerProperty>().SelectTrigger = false;
                            VirtualCard.transform.parent = CardHolder;
                            VirtualCard.GetComponent<PokerProperty>().Card.GetComponent<MeshRenderer>().enabled = false;
                            break;
                        }
                    }
                    SelectedCard = null;
                    Picking = false;
                    UpdateCardCount(false);
                }
            }
            //before release
            else if (CardHolderSwitch == true && RightPinchState(0.05f, false))
            {
                //preplay sorting                
                if (Vector3.Distance(RightCardPivot.position, CardHolder.position) <= 0.17f)
                {
                    CardHolderTracking = false;
                    VirtualCard.GetComponent<PokerProperty>().Card.GetComponent<MeshRenderer>().enabled = true;

                    //sorting card condition of distance
                    if (Vector3.Distance(RightCardPivot.position, CardHolder.position) <= 0.14f)
                    {
                        for (int a = 0; a < Card.Length; a++)
                        {
                            CardHandDistance[a] = Vector3.Distance(RightCardPivot.position, Card[a].GetComponent<PokerProperty>().disstancecheck.transform.position);
                            Card[a].GetComponent<PokerProperty>().SelectDistance = CardHandDistance[a];
                            Array.Sort(CardHandDistance);

                            if (Card[a].GetComponent<PokerProperty>().SelectDistance == CardHandDistance[0])
                            {
                                for (int b = 0; b < Card.Length; b++)
                                {
                                    if (Card[b] == Card[a])
                                    {
                                        VirtualCard.transform.SetSiblingIndex(b);
                                        UpdateCardCount(false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //withdraw card condition of distance
                    else
                    {
                        VirtualCard.transform.SetSiblingIndex(BeforeSelectedIndex);
                        UpdateCardCount(false);
                    }
                }
                //distance of ready to play 
                else if (Vector3.Distance(RightCardPivot.position, CardHolder.position) > 0.16f)
                {
                    CardHolderTracking = true;
                    VirtualCard.GetComponent<PokerProperty>().Card.GetComponent<MeshRenderer>().enabled = false;
                    VirtualCard.transform.SetSiblingIndex(BeforeSelectedIndex);
                    UpdateCardCount(false);
                }
            }
            else
            {
                CardHolderTracking = true;
            }
        }
    }
}