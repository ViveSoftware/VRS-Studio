using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    public Animator animator;
    public bool isDefaultPressed = false;

    private void Update()
    {
        if (isDefaultPressed)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Normal"))
            {
                animator.Play("Highlighted");
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted"))
            {
                animator.Play("Pressed");
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Pressed"))
            {
                isDefaultPressed = false;
            }
        }
    }

    public void Reset()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Pressed"))
        {
            animator.SetTrigger("Normal");
            animator.Rebind();
        }
    }

    public void ResetToNormal()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted"))
        {
            animator.SetTrigger("Normal");
        }
    }
}
