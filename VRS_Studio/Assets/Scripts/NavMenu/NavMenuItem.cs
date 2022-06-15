using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Wave.Essence.Hand;
using Wave.Native;

public class NavMenuItem : Simple3DButton
{
	public GameObject MenuLayer;
	public UnityEvent NavMenuSelectionCallbackOnTimedAnimationForSmoothTransition, NavMenuSelectionCallbackOnAnimationComplete, NavMenuSelectionCallbackOnSelection;

	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_SwitchUp;
	public event OnButtonAnimationCompleteDelegate OnButtonAnimationCompleteCallback_SwitchDown;

	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_SwitchUp;
	public event OnButtonAnimationTimedDelegate OnButtonAnimationTimedCallback_SwitchDown;

	public bool isPartOfMainMenu = false;
	[HideInInspector]
	public bool menuSwitchDownInvoker = false, navMenuCloseInvoker = false;

	private bool isInvoked_AnimationReachedTargetTimeAction = false, isInvoked_AnimationCompleteAction = false, isInvoked_ImmediateAction = false;

	private string animation_SwitchDown = "Base Layer.Button_SwitchDown";
	private string animation_SwitchUp = "Base Layer.Button_SwitchUp";
	private string animation_MenuUIOpen = "Base Layer.Button_MenuUIOpen";
	private string animation_MenuUIClose = "Base Layer.Button_MenuUIClose";

	private int animation_SwitchDownHash, animation_SwitchUpHash, animation_MenuUIOpenHash, animation_MenuUICloseHash;

	new readonly string LOG_TAG = "NavMenuItem";

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		animation_SwitchDownHash = Animator.StringToHash(animation_SwitchDown);
		animation_SwitchUpHash = Animator.StringToHash(animation_SwitchUp);
		animation_MenuUIOpenHash = Animator.StringToHash(animation_MenuUIOpen);
		animation_MenuUICloseHash = Animator.StringToHash(animation_MenuUIClose);

		buttonAnimator.speed = buttonAnimatorSpeed;
		if (MenuLayer == null)
		{
			MenuLayer = transform.parent.gameObject;
		}
		RegisterNavMenuItemEvents();
		OnButtonAnimationTimedCallback_SwitchUp += NavMenuManager.Instance.SwitchUpObjectCompletionCounterIncrement;
		OnButtonAnimationTimedCallback_SwitchDown += NavMenuManager.Instance.SwitchDownObjectCompletionCounterIncrement;
		//OnButtonAnimationCompleteCallback_SwitchUp += NavMenuManager.Instance.SwitchUpObjectCompletionCounterIncrement;
		//OnButtonAnimationCompleteCallback_SwitchDown += NavMenuManager.Instance.SwitchDownObjectCompletionCounterIncrement;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnregisterNavMenuItemEvents();
		OnButtonAnimationTimedCallback_SwitchUp -= NavMenuManager.Instance.SwitchUpObjectCompletionCounterIncrement;
		OnButtonAnimationTimedCallback_SwitchDown -= NavMenuManager.Instance.SwitchDownObjectCompletionCounterIncrement;
		//OnButtonAnimationCompleteCallback_SwitchUp -= NavMenuManager.Instance.SwitchUpObjectCompletionCounterIncrement;
		//OnButtonAnimationCompleteCallback_SwitchDown -= NavMenuManager.Instance.SwitchDownObjectCompletionCounterIncrement;
	}

	public void NavMenuOpen(GameObject targetLayer)
	{
		if (MenuLayer == targetLayer)
		{
			Log.d(LOG_TAG, "NavMenuOpen: " + gameObject.name);
			PlayAnimation(animation_MenuUIOpenHash);
			buttonAnimator.SetTrigger("MenuOpen");
		}
	}

	public void NavMenuClose(GameObject targetLayer)
	{
		if (MenuLayer == targetLayer)
		{
			Log.d(LOG_TAG, "NavMenuClose: " + gameObject.name);
			PlayAnimation(animation_MenuUICloseHash);
		}
	}

	public void NavMenuSwitchLayer_Up(GameObject targetLayer)
	{
		if (MenuLayer == targetLayer)
		{
			Log.d(LOG_TAG, "NavMenuSwitchLayer_Up: " + gameObject.name);
			PlayAnimation(animation_SwitchUpHash, animation_SwitchUp, OnButtonAnimationCompleteCallback_SwitchUp, OnButtonAnimationTimedCallback_SwitchUp, smoothTransitionTimeStamp);
			buttonAnimator.SetTrigger("SwitchUp");
		}
	}

	public void NavMenuSwitchLayer_Down(GameObject targetLayer)
	{
		if (MenuLayer == targetLayer)
		{
			Log.d(LOG_TAG, "NavMenuSwitchLayer_Down: " + gameObject.name);
			PlayAnimation(animation_SwitchDownHash, animation_SwitchDown, OnButtonAnimationCompleteCallback_SwitchDown, OnButtonAnimationTimedCallback_SwitchDown, smoothTransitionTimeStamp);
		}
	}

	private void OnNavMenuSelection_AnimationCompleteAction()
	{
		if (!isInvoked_AnimationCompleteAction)
		{
			Log.d(LOG_TAG, "OnNavMenuSelection_AnimationCompleteAction: " + gameObject.name);
			if (NavMenuSelectionCallbackOnAnimationComplete != null) NavMenuSelectionCallbackOnAnimationComplete.Invoke();
			isInvoked_AnimationCompleteAction = true;
		}
	}

	private void OnNavMenuSelection_AnimationReachTargetTimeAction()
	{
		if (!isInvoked_AnimationReachedTargetTimeAction)
		{
			Log.d(LOG_TAG, "OnNavMenuSelection_AnimationReachTargetTimeAction: " + gameObject.name);
			if (NavMenuSelectionCallbackOnTimedAnimationForSmoothTransition != null) NavMenuSelectionCallbackOnTimedAnimationForSmoothTransition.Invoke();
			isInvoked_AnimationReachedTargetTimeAction = true;
		}
	}

	private void OnNavMenuSelection_ImmediateAction()
	{
		Log.d(LOG_TAG, "OnNavMenuSelection_ImmediateAction: " + gameObject.name + " isInvoked: " + isInvoked_ImmediateAction);
		if (!isInvoked_ImmediateAction)
		{
			Log.d(LOG_TAG, "OnNavMenuSelection_ImmediateAction: " + gameObject.name);
			if (NavMenuSelectionCallbackOnSelection != null) NavMenuSelectionCallbackOnSelection.Invoke();
			isInvoked_ImmediateAction = true;
		}
	}

	private void OnNavMenuDeselection()
	{
		Log.d(LOG_TAG, "OnNavMenuDeselection: " + gameObject.name);
		isInvoked_ImmediateAction = false;
		isInvoked_AnimationCompleteAction = false;
		isInvoked_AnimationReachedTargetTimeAction = false;
	}

	public override void ResetButton()
	{
		base.ResetButton();
		OnNavMenuDeselection();
	}

	public override void DeselectButton()
	{
		if (IsButtonDisabled()) return;
		base.DeselectButton();
		OnNavMenuDeselection();
	}

	public override void DisableButton()
	{
		if (IsButtonDisabled())
		{
			Log.d(LOG_TAG, "Button is already Disabled: " + gameObject.name);
			return;
		}
		base.DisableButton();
		UnregisterNavMenuItemEvents();
		OnNavMenuDeselection();
	}

	public override void EnableButton()
	{
		if (!IsButtonDisabled())
		{
			Log.d(LOG_TAG, "Button is already Enabled: " + gameObject.name);
			return;
		}
		base.EnableButton();
		RegisterNavMenuItemEvents();
	}

	private void RegisterNavMenuItemEvents()
	{
		Log.d(LOG_TAG, "RegisterNavMenuItemEvents: " + gameObject.name);
		OnButtonAnimationCompleteCallback_Pressed += OnNavMenuSelection_AnimationCompleteAction;
		OnButtonAnimationTimedCallback_Pressed += OnNavMenuSelection_AnimationReachTargetTimeAction;
		selectionInteractable.TouchBeginEvent.AddListener(OnNavMenuSelection_ImmediateAction);
		selectionInteractable.TouchEndEvent.AddListener(OnNavMenuDeselection);
	}

	private void UnregisterNavMenuItemEvents()
	{
		Log.d(LOG_TAG, "UnregisterNavMenuItemEvents: " + gameObject.name);
		OnButtonAnimationCompleteCallback_Pressed -= OnNavMenuSelection_AnimationCompleteAction;
		OnButtonAnimationTimedCallback_Pressed -= OnNavMenuSelection_AnimationReachTargetTimeAction;
		selectionInteractable.TouchBeginEvent.RemoveListener(OnNavMenuSelection_ImmediateAction);
		selectionInteractable.TouchEndEvent.RemoveListener(OnNavMenuDeselection);
	}

	//// Update is called once per frame
	//void Update()
	//{
	//    if (currentButtonState != ButtonState.Disabled)
	//    {
	//        if (proximityInteractable.isTouched) //Interactor is in proximity of menu item
	//        {
	//            //Log.d(LOG_TAG, "proximityInteractable.isTouched: " + gameObject.name);
	//            if (selectionInteractable.isTouched) //Menu item is selected by interactor
	//            {
	//                //Log.d(LOG_TAG, "selectionInteractable.isTouched: " + gameObject.name);
	//                if (currentButtonState != ButtonState.Pressed) //State: Highlighted/Deselected -> Selected
	//                {
	//                    Log.d(LOG_TAG, "Pressed: " + gameObject.name);
	//                    currentButtonState = ButtonState.Pressed;

	//                    //Play press animation and switch to pressed state
	//                    PlayAnimation(animation_PressStateHash);
	//                    pressedAudioSrc.Play();
	//                    //if (!isPartOfMainMenu) NavMenuManager.Instance.currSelectedNavMenuItem = this;

	//                    if (!isInvoked)
	//                    {
	//                        if (NavMenuSelectionCallback != null) NavMenuSelectionCallback.Invoke();
	//                        isInvoked = true;
	//                    }
	//                }
	//            }
	//            else
	//            {
	//                if (currentButtonState == ButtonState.Normal) //State: Normal -> Highlighted
	//                {
	//                    //Only play highlight animation when deselected
	//                    currentButtonState = ButtonState.Highlighted;
	//                    PlayAnimation(animation_HighlightStateHash);
	//                    highlightAudioSrc.Play();

	//                    Log.d(LOG_TAG, "Highlighted: " + gameObject.name);
	//                }
	//            }
	//        }
	//        else
	//        {
	//            if (currentButtonState != ButtonState.Normal)
	//            {
	//                if (currentButtonState == ButtonState.Pressed)//State: Selected -> Deselected
	//                {
	//                    //Play deselection animation and switch to deselected state
	//                    PlayAnimation(animation_DeselectStateHash);
	//                    deselectAudioSrc.Play();
	//                    isInvoked = false;
	//                    Log.d(LOG_TAG, "Deselected: " + gameObject.name);
	//                }
	//                else if (currentButtonState == ButtonState.Highlighted)
	//                {
	//                    PlayAnimation(animation_UnhighlightStateHash);
	//                    Log.d(LOG_TAG, "Unhighlight: " + gameObject.name);
	//                }

	//                currentButtonState = ButtonState.Normal;
	//            }
	//        }
	//    }
	//}

	public void TriggerScenarioChange(string destScenarioScenePath)
	{
		navMenuCloseInvoker = true;
		NavMenuManager.Instance.HandleScenarioChangeRequest(destScenarioScenePath);
	}

	public void TriggerMoveToNextMenuLayer(int layerIndex)
	{
		menuSwitchDownInvoker = true;
		NavMenuManager.Instance.MoveToNextMenuLayer(layerIndex);
	}
}
