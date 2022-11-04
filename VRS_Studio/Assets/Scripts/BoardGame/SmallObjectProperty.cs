using UnityEngine;

public class SmallObjectProperty : MonoBehaviour
{
    CatchManager CatchManager { get { return CatchManager.instance; } }
    GameManager GameManager { get { return GameManager.instance; } }

    Rigidbody rb;

    public Material[] colormat;
    public bool AllowCatchable = false;
    public bool Catched = false;
    public bool inLeftHand = false, inRightHand = false;

    //condition of raady to count point
    public bool canCauclate;
    public float canCauclateCD = 20;

    public AudioSource AudioPlayer;
    public AudioClip VoiceClip;
    private void Awake()
    {
        canCauclate = false;
        rb = this.GetComponent<Rigidbody>();
        AudioPlayer = GetComponent<AudioSource>();

        //prevent robot collider
        Physics.IgnoreLayerCollision(9, 2, true);
    }

    private void Update()
    {
        //dishighlight after catching
        if (Catched == true) GetComponent<MeshRenderer>().material = colormat[0];

        //cauclate point counter
        else if (AllowCatchable == false && GameManager.gameMode == GameManager.GameMode.DiceMode)
        {
            if (canCauclateCD > 0 )
            {
                canCauclate = false;
                canCauclateCD -= Time.deltaTime;
            }
            else if (canCauclateCD <= 0)
            {
                canCauclate = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if (rb.angularVelocity.magnitude > 0.1f)
            rb.angularVelocity *= 0.5f;
        //correct rotate issue after throwing dice
        if (canCauclateCD < 1f && rb.velocity.magnitude >= 0.015f)
        {
            rb.velocity *= 0.5f;
            rb.angularVelocity *= 0.5f;
        }
        else if (rb.velocity.magnitude < 0.015f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "CatchTrigger" && AllowCatchable == true)
        {
            if (other.transform.parent.name == "LeftPalm") inLeftHand = true;
            else if (other.transform.parent.name == "RightPalm") inRightHand = true;

            if (Catched == false)
            {
                rb.isKinematic = true;
                if (other.transform.parent.name == "LeftPalm" && CatchManager.LeftHandCatching == false)
                    GetComponent<MeshRenderer>().material = colormat[1];
                else if (other.transform.parent.name == "RightPalm" && CatchManager.RightHandCatching == false)
                    GetComponent<MeshRenderer>().material = colormat[1];
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "CatchTrigger" && AllowCatchable == true)
        {
            if (other.transform.parent.name == "LeftPalm") inLeftHand = false;
            else if (other.transform.parent.name == "RightPalm") inRightHand = false;
            GetComponent<MeshRenderer>().material = colormat[0];
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //prevent outside
        if (other.gameObject.name == "OutArea")
        {
            GameManager.Panel[1].SetActive(false);
            GameManager.Panel[3].SetActive(true);
        }

        //collsion sfx
        if (other.gameObject.name == "Table")
        {
            canCauclateCD = 2.5f;
            if (AudioPlayer.isPlaying) AudioPlayer.Stop();
            AudioPlayer.PlayOneShot(VoiceClip);
        }
    }

    public void CatchStateCheck()
    {
        if (Catched == true)
        {
            AllowCatchable = false; //only toss once

            inLeftHand = false;
            inRightHand = false;
            rb.isKinematic = false;
            rb.AddForce(Vector3.down, ForceMode.Impulse);
            rb.AddTorque(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
            transform.parent = null;
            Catched = false;
            return;
        }
        else if (Catched == false)
        {
            rb.isKinematic = true;
            Catched = true;
        }
    }
}