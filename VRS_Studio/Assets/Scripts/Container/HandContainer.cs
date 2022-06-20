using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wave.Essence.Hand;
using Wave.Native;

public class HandContainer : MonoBehaviour
{
    public List<GameObject> ColliderList = new List<GameObject>();
    public float timeThreshold = 1f;
    private bool SendEventPeriodically = false;

    public float originalGradientLevel = 0.742f;
    public Color EmptyContainerColorA = new Color(64, 80, 190);
    public Color EmptyContainerColorB = new Color(242, 124, 184);
    public Color CountdownContainerColorA = new Color(64, 190, 127);
    public Color CountdownContainerColorB = new Color(242, 235, 124);
    //public Color FulfilledContainerColor = Color.green;

    public Renderer ContainerRenderer = null;

    public UnityEvent ContainerFullfilledCallback;

    public AudioSource matchAudio, countdownAudio;
    private bool isCountdownAudioPlayed = false;

    private bool timerStarted = false;
    private float countdownStartTime = 0f;
    private float currTime = 0f;
    private bool prevStatus = false;
    private bool containerFulfilled = false;


    private string LOG_TAG = "HandContainer";

    // Start is called before the first frame update
    void Start()
    {
        ContainerReset();
    }

    // Update is called once per frame
    void Update()
    {
        bool containerMatched = isContainerMatched();

        if (containerMatched)
        {
            if (!timerStarted && !containerFulfilled)
            {
                // start timer
                countdownStartTime = Time.realtimeSinceStartup;
                timerStarted = true;
                containerFulfilled = false;
                matchAudio.Play();
            }

            if (timerStarted)
            {
                currTime = Time.realtimeSinceStartup;
                float passedTime = currTime - countdownStartTime;

                float currentLerpValue = Mathf.Lerp(0, 1, passedTime/timeThreshold);
                ContainerCountdown(currentLerpValue);

                if (passedTime > timeThreshold)
                {
                    //container fulfilled
                    ContainerFullfilled();
                    if (!SendEventPeriodically)
                    {
                        containerFulfilled = true;
                        timerStarted = false;
                    }
                }
            }
        }
        else
        {
            //reset timer and container
            timerStarted = false;
            containerFulfilled = false;
            countdownStartTime = 0f;
            currTime = 0f;
            ContainerReset();
        }

        if (containerMatched != prevStatus)
        {
            Log.w(LOG_TAG, "Container status changed", true);
            prevStatus = containerMatched;
        }
    }

    private bool isContainerMatched()
    {
        int matchScore = 0;
        var len = ColliderList.Count;

        for (int i = 0; i < len; i++)
        {
            GameObject colliderGO = ColliderList[i];
            HandInteractable colliderHadnInteractable = colliderGO.GetComponent<HandInteractable>();

            if (colliderHadnInteractable == null) return false;
            if (colliderHadnInteractable.isTouched)
            {
                matchScore++;
            }
        }

        if (matchScore < 3)
		{
            return false;
		}

        return true;
    }

    public void ContainerFullfilled()
    {
        //ContainerRenderer.material.color = ApplyAlpha(FulfilledContainerColor, ContainerAlpha);
        ContainerFullfilledCallback.Invoke();
        Log.d(LOG_TAG, "ContainerFullfilled");
    }

    public void ContainerCountdown(float lerpValue)
    {
        ContainerRenderer.material.SetColor("_GraColorA", CountdownContainerColorA);
        ContainerRenderer.material.SetColor("_GraColorB", CountdownContainerColorB);
        ContainerRenderer.material.SetFloat("_Gradient_Level", lerpValue);

        if (!matchAudio.isPlaying && !countdownAudio.isPlaying && !isCountdownAudioPlayed)
        {
            countdownAudio.Play();
            isCountdownAudioPlayed = true;
        }

        Log.d(LOG_TAG, "ContainerCountdown");
    }

    public void ContainerReset()
    {
        ContainerRenderer.material.SetColor("_GraColorA", EmptyContainerColorA);
        ContainerRenderer.material.SetColor("_GraColorB", EmptyContainerColorB);
        ContainerRenderer.material.SetFloat("_Gradient_Level", originalGradientLevel);

        if (countdownAudio.isPlaying)
        {
            countdownAudio.Stop();
        }

        isCountdownAudioPlayed = false;
    }

    //private Color ApplyAlpha(Color inColor, float inAlpha)
    //{
    //    return new Color(inColor.r, inColor.g, inColor.b, inAlpha);
    //}
}
