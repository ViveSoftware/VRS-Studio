using UnityEngine;
using HTC.UnityPlugin.Vive;
using DG.Tweening;

public class CatchManager : MonoBehaviour
{
    private static CatchManager _instance;
    public static CatchManager instance
    {
        get
        {
            if (_instance == null)  {   _instance = FindObjectOfType<CatchManager>();} return _instance;
        }
    }
    GestureManager GestureManager { get { return GestureManager.instance; } }
    GameManager GameManager { get { return GameManager.instance; } }

    [Header("Left Hand")]
    public bool LeftHandCatching = false;
    [SerializeField]    Transform LeftPalm;
    [SerializeField]    GestureCustom LeftHand_CatchStone;
    [SerializeField]    GestureCustom LeftHand_CatchOpen;

    [Header("Right Hand")]
    public bool RightHandCatching = false;
    [SerializeField]    Transform RightPalm;
    [SerializeField]    GestureCustom RightHand_CatchStone;
    [SerializeField]    GestureCustom RightHand_CatchOpen;
    
    GameObject[] Dice;
    Vector3[] DiceInHand = new Vector3[4];

    float preventTimer = 0;

    private void Awake()
    {
        Dice = GameManager.Dice;
        DiceInHand[0] = new Vector3(-0.01f, -0.02f, -0.01f);
        DiceInHand[1] = new Vector3(0.01f, -0.02f, -0.01f);
        DiceInHand[2] = new Vector3(-0.01f, -0.02f, 0.01f);
        DiceInHand[3] = new Vector3(0.01f, -0.02f, 0.01f);
    }
    void Update()
    {
        if (GestureManager.GestureControl(LeftHand_CatchStone)&& LeftHandCatching == false)
        {
            for (int a = 0; a < Dice.Length; a++)
            {
                if (Dice[a].GetComponent<SmallObjectProperty>().Catched == false && Dice[a].GetComponent<SmallObjectProperty>().inLeftHand == true)
                {
                    Dice[a].transform.parent = LeftPalm;
                    Dice[a].transform.DOLocalMove(DiceInHand[a], 0.1f);
                    Dice[a].transform.localEulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                    Dice[a].GetComponent<SmallObjectProperty>().CatchStateCheck();
                }
            }
            LeftHandCatching = true;
            preventTimer = 0.1f;
        }
        else if (GestureManager.GestureControl(RightHand_CatchStone) && RightHandCatching == false)
        {
            for (int a = 0; a < Dice.Length; a++)
            {
                if (Dice[a].GetComponent<SmallObjectProperty>().Catched == false && Dice[a].GetComponent<SmallObjectProperty>().inRightHand == true)
                {
                    Dice[a].transform.parent = RightPalm;
                    Dice[a].transform.DOLocalMove(DiceInHand[a], 0.1f);
                    Dice[a].transform.localEulerAngles = new Vector3(Random.Range(0f,360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                    Dice[a].GetComponent<SmallObjectProperty>().CatchStateCheck();
                }
            }
            RightHandCatching = true;
            preventTimer = 0.1f;
        }
        
        if(preventTimer > 0)
        {
            preventTimer -= Time.deltaTime;
            return;
        }

        if (GestureManager.GestureControl(LeftHand_CatchOpen) && LeftHandCatching == true)
        {
            for (int a = 0; a < Dice.Length; a++)
            {
                if (Dice[a].GetComponent<SmallObjectProperty>().Catched == true && Dice[a].GetComponent<SmallObjectProperty>().inLeftHand == true)
                    Dice[a].GetComponent<SmallObjectProperty>().CatchStateCheck();
            }
            LeftHandCatching = false;
        }
        else if (GestureManager.GestureControl(RightHand_CatchOpen) && RightHandCatching == true)
        {
            for (int a = 0; a < Dice.Length; a++)
            {
                if (Dice[a].GetComponent<SmallObjectProperty>().Catched == true && Dice[a].GetComponent<SmallObjectProperty>().inRightHand == true)
                    Dice[a].GetComponent<SmallObjectProperty>().CatchStateCheck();
            }
            RightHandCatching = false;
        }
    }
}