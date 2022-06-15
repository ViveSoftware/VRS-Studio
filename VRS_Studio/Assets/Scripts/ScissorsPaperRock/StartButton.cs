using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.Playables;

public class StartButton : MonoBehaviour
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    public PlayableDirector idleTimeline;
    public PlayableDirector highlightTimeline;
    public PlayableDirector dragTimeline;
    public GameObject drag;
    public GameObject[] arrows;
    private bool isHover = false;
    private bool gameStart = false;
    private ViveRoleProperty role;

    private void OnEnable()
    {
        dragTimeline.stopped += OnPlayableDirectorStopped;
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        isHover = true;
        dragTimeline.time = 0;
        dragTimeline.Evaluate();
        highlightTimeline.Play();

        ViveColliderEventCaster caster;
        if (eventData.TryGetEventCaster(out caster))
        {
            role = caster.viveRole;
        }
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        isHover = false;

        if (dragTimeline.time > 0.8f)
        {
            ShowHideArrows(false);
            dragTimeline.Play();
        }
        else
        {
            ShowHideArrows(true);
            dragTimeline.time = 0;
            dragTimeline.Evaluate();
        }
    }

    private void Update()
    {
        if (isHover)
        {
            idleTimeline.Stop();
            var indexTip = default(JointPose);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexTip, out indexTip);
            var value = indexTip.pose.pos.x - drag.transform.position.x;

            if (value > 0.25f)
            {
                ShowHideArrows(false);
            }

            dragTimeline.time = dragTimeline.duration * value;
            dragTimeline.Evaluate();
        }
        else if (!isHover && dragTimeline.time == 0)
        {
            ShowHideArrows(true);
            idleTimeline.Play();
        }
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (dragTimeline == director)
        {
            gameStart = true;
        }
    }

    private void OnDisable()
    {
        dragTimeline.stopped -= OnPlayableDirectorStopped;
    }

    public bool GameStart()
    {
        return gameStart;
    }

    public void Restart()
    {
        gameStart = false;
        dragTimeline.time = 0;
        dragTimeline.Evaluate();
    }

    private void ShowHideArrows(bool enable)
    {
        arrows[0].SetActive(enable);
        arrows[1].SetActive(enable);
        arrows[2].SetActive(enable);
    }
}
