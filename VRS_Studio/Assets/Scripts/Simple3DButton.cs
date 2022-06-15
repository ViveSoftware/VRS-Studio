using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.Hand;
using Wave.Native;

public class Simple3DButton : MonoBehaviour
{
	public HandInteractable proximityInteractable, selectionInteractable;

	public Animator buttonAnimator;
	public float buttonAnimatorSpeed = 1.5f;

	public AudioSource deselectAudioSrc, pressedAudioSrc, highlightAudioSrc;

	public delegate void OnButtonAnimationCompleteDelegate();
	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_Pressed;
	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_Highlighted;
	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_Unhighlighted;
	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_Deselected;

	public delegate void OnButtonAnimationTimedDelegate();
	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_Pressed;
	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_Highlighted;
	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_Unhighlighted;
	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_Deselected;

	protected string animation_Layer = "Base Layer";
	protected string animation_NormalState = "Base Layer.Button_Normal";
	protected string animation_HighlightState = "Base Layer.Button_Highlight";
	protected string animation_UnhighlightState = "Base Layer.Button_Unhighlight";
	protected string animation_PressState = "Base Layer.Button_Press";
	protected string animation_DeselectState = "Base Layer.Button_Deselect";

	protected int animation_LayerID, animation_NormalStateHash, animation_HighlightStateHash, animation_UnhighlightStateHash, animation_PressStateHash, animation_DeselectStateHash;

	protected ButtonState currentButtonState = ButtonState.Normal;
	protected float smoothTransitionTimeStamp = 0.7f;

	protected readonly string LOG_TAG = "Simple3DButton";

	// Start is called before the first frame update
	protected virtual void Start()
	{
		animation_LayerID = buttonAnimator.GetLayerIndex(animation_Layer);
		animation_NormalStateHash = Animator.StringToHash(animation_NormalState);
		animation_HighlightStateHash = Animator.StringToHash(animation_HighlightState);
		animation_UnhighlightStateHash = Animator.StringToHash(animation_UnhighlightState);
		animation_PressStateHash = Animator.StringToHash(animation_PressState);
		animation_DeselectStateHash = Animator.StringToHash(animation_DeselectState);

		RegisterButtonEvents();
		//selectionInteractable.TouchEndEvent.AddListener(DeselectButton);
	}

	protected virtual void OnDestroy()
	{
		UnregisterButtonEvents();
		//selectionInteractable.TouchEndEvent.RemoveListener(DeselectButton);
	}

	public void HighlightButton()
	{
		if (IsButtonDisabled()) return;
		if (currentButtonState != ButtonState.Normal) return;
		Log.d(LOG_TAG, "Highlight: " + gameObject.name);
		currentButtonState = ButtonState.Highlighted;
		PlayAnimation(animation_HighlightStateHash, animation_HighlightState, OnButtonAnimationCompleteCallback_Highlighted, OnButtonAnimationTimedCallback_Highlighted, smoothTransitionTimeStamp);
		if (highlightAudioSrc != null) highlightAudioSrc.Play();
	}

	public void UnhighlightButton()
	{
		if (IsButtonDisabled()) return;
		if (currentButtonState != ButtonState.Highlighted) return;
		Log.d(LOG_TAG, "Unhighlight: " + gameObject.name);
		PlayAnimation(animation_UnhighlightStateHash, animation_UnhighlightState, OnButtonAnimationCompleteCallback_Unhighlighted, OnButtonAnimationTimedCallback_Unhighlighted, smoothTransitionTimeStamp);
		if (deselectAudioSrc != null) deselectAudioSrc.Play();
		currentButtonState = ButtonState.Normal;
	}

	public void SelectButton()
	{
		if (IsButtonDisabled()) return;
		Log.d(LOG_TAG, "Select: " + gameObject.name);
		currentButtonState = ButtonState.Pressed;
		PlayAnimation(animation_PressStateHash, animation_PressState, OnButtonAnimationCompleteCallback_Pressed, OnButtonAnimationTimedCallback_Pressed, smoothTransitionTimeStamp);
		if (pressedAudioSrc != null) pressedAudioSrc.Play();
	}

	public virtual void DeselectButton()
	{
		if (IsButtonDisabled()) return;
		//if (currentButtonState != ButtonState.Pressed) return;
		Log.d(LOG_TAG, "Deselect: " + gameObject.name);
		PlayAnimation(animation_DeselectStateHash, animation_DeselectState,  OnButtonAnimationCompleteCallback_Deselected, OnButtonAnimationTimedCallback_Deselected, smoothTransitionTimeStamp);
		currentButtonState = ButtonState.Normal;
	}

	public virtual void ResetButton()
	{
		Log.d(LOG_TAG, "Reset: " + gameObject.name);
		PlayAnimation(animation_NormalStateHash);
		currentButtonState = ButtonState.Normal;
	}

	public virtual void DisableButton()
	{
		if (IsButtonDisabled())
		{
			Log.d(LOG_TAG, "Button is already Disabled: " + gameObject.name);
			return;
		}
		Log.d(LOG_TAG, "Disable: " + gameObject.name);
		UnregisterButtonEvents();
		currentButtonState = ButtonState.Disabled;
	}

	public virtual void EnableButton()
	{
		if (!IsButtonDisabled())
		{
			Log.d(LOG_TAG, "Button is already Enabled: " + gameObject.name);
			return;
		}
		Log.d(LOG_TAG, "Enable: " + gameObject.name);
		RegisterButtonEvents();
		currentButtonState = ButtonState.Normal;
	}

	public bool IsButtonDisabled()
	{
		return currentButtonState == ButtonState.Disabled;
	}

	protected void RegisterButtonEvents()
	{
		Log.d(LOG_TAG, "RegisterButtonEvents: " + gameObject.name);
		proximityInteractable.TouchBeginEvent.AddListener(HighlightButton);
		proximityInteractable.TouchEndEvent.AddListener(UnhighlightButton);
		selectionInteractable.TouchBeginEvent.AddListener(SelectButton);
	}

	protected void UnregisterButtonEvents()
	{
		Log.d(LOG_TAG, "UnregisterButtonEvents: " + gameObject.name);
		proximityInteractable.TouchBeginEvent.RemoveListener(HighlightButton);
		proximityInteractable.TouchEndEvent.RemoveListener(UnhighlightButton);
		selectionInteractable.TouchBeginEvent.RemoveListener(SelectButton);
	}

	protected void PlayAnimation(int animationStateHash)
	{
		if (buttonAnimator != null && buttonAnimator.HasState(animation_LayerID, animationStateHash))
		{
			buttonAnimator.Play(animationStateHash);
		}
	}

	protected void PlayAnimation(int animationStateHash, string animationStateName, OnButtonAnimationCompleteDelegate OnCompleteAction, OnButtonAnimationTimedDelegate OnTimedAction, float targetNormalizedTime)
	{
		if (buttonAnimator != null && buttonAnimator.HasState(animation_LayerID, animationStateHash))
		{
			buttonAnimator.Play(animationStateHash);

			StartCoroutine(ButtonAnimationCompletionTracker(animationStateName, OnCompleteAction, OnTimedAction, targetNormalizedTime));
		}
	}

	protected IEnumerator ButtonAnimationCompletionTracker(string animationStateName, OnButtonAnimationCompleteDelegate OnCompleteAction, OnButtonAnimationTimedDelegate OnTimedAction, float targetNormalizedTime)
	{
		while (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName))
		{
			//Log.d(LOG_TAG, gameObject.name + " ButtonAnimationCompletionTracker: Waiting for state: " + animationStateName);
			yield return null;
		}

		float currentNormalizedTime = buttonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		bool isTimedActionInvoked = false;
		while ((currentNormalizedTime) < 1f)
		{
			//Log.d(LOG_TAG, gameObject.name + " ButtonAnimationCompletionTracker: " + animationStateName + " Normalized Time: " + currentNormalizedTime);
			currentNormalizedTime = buttonAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (targetNormalizedTime <= currentNormalizedTime && !isTimedActionInvoked)
			{
				Log.d(LOG_TAG, gameObject.name + " ButtonAnimationCompletionTracker: " + animationStateName + " reach target normalized time " + targetNormalizedTime);
				if (OnTimedAction != null) OnTimedAction.Invoke();

				isTimedActionInvoked = true;
			}
			yield return null;
		}

		Log.d(LOG_TAG, gameObject.name + " ButtonAnimationCompletionTracker: " + animationStateName + " is complete.");
		if (OnCompleteAction != null) OnCompleteAction.Invoke();
	}

	public enum ButtonState
	{
		Disabled,
		Normal,
		Highlighted,
		Pressed,
	}
}
