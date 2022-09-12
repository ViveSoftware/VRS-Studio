using UnityEngine;
using DG.Tweening;

public class PokerProperty : MonoBehaviour
{
    GameManager GameManager { get { return GameManager.instance; } }

    Rigidbody rb;

    public enum SuitType
    {
        Clubs = 0,
        Diamonds = 1,
        Hearts = 2,
        Spades = 3
    }
    public SuitType Suit;
    public int Number;

    [HideInInspector] public Transform Card;
    [HideInInspector] public GameObject disstancecheck;

    [HideInInspector] public float SelectDistance;
    [HideInInspector] public bool SelectTrigger = false;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        Card = this.transform.GetChild(0);

        GameObject empty = new GameObject();
        disstancecheck = Instantiate(empty, transform.position, Quaternion.identity);
        disstancecheck.name = "disstancecheck_" + Number.ToString() + Suit.ToString();
        disstancecheck.transform.parent = this.transform;
        disstancecheck.transform.localPosition = new Vector3(0.0035f, 0, 0.015f);
        Destroy(empty);
    }

    private void Update()
    {
        //highlight for card be selected
        if (SelectTrigger == true)
            Card.localScale = Vector3.Lerp(Card.localScale, new Vector3(1.1f, 1.1f, 1.1f), 0.3f);
        else
            Card.localScale = Vector3.Lerp(Card.localScale, new Vector3(1f,1f,1f), 0.3f);
    }

    private void OnCollisionEnter(Collision other)
    {
        //prevent outside
        if (other.gameObject.name == "OutArea")
        {
            GameManager.Panel[2].SetActive(false);
            GameManager.Panel[4].SetActive(true);
            rb.isKinematic = true;
            transform.parent = GameManager.PlayerCardArea;
            transform.DOLocalMove(new Vector3(0f, 0, 0), 0.25f);
            transform.DOLocalRotate(new Vector3(0, 0, -180), 0.25f).SetLoops(0);
        }
    }

    public void Throw()
    {
        SelectTrigger = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        transform.parent = null;
    }
}