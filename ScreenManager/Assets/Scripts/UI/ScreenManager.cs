using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

// TODO: add no animate option (for example, moving from avatar to settings)
// TODO: add overlay mode, where hiding screen doesn't move and just hides after

public class ScreenManager: SingletonMonoBehaviour<ScreenManager>
{
    public BaseScreen DefaultScreen;
    public ErrorScreen ErrorModal;
    public TitleBar Navigator;

    public BaseScreen CurrentScreen { get; private set; }

    public Dictionary<string, BaseScreen> Screens { get; private set; }

    private Stack<BaseScreen> _navStack;

    protected override void Awake()
    {
        base.Awake();
        
        if (!DefaultScreen || !ErrorModal || !Navigator)
        {
            throw new ArgumentException("ScreenManager: Initialize() failed, DefaultScreen, ErrorModal, Navigator must be set in Inspector.");
        }

        // default active states for ErrorModal and TitleBar
        ErrorModal.gameObject.SetActive(false);
        Navigator.gameObject.SetActive(false);

        Screens = new Dictionary<string, BaseScreen>();
        _navStack = new Stack<BaseScreen>();

        foreach (BaseScreen screen in GetComponentsInChildren<BaseScreen>(true))
        {
            screen.gameObject.SetActive(false);
            screen.Initialize(this);
            Screens[screen.Name] = screen;
        }

        if (!Screens.ContainsKey(DefaultScreen.Name))
        {
            throw new KeyNotFoundException("ScreenManager: Initialize() failed, cannot find screen named '" + DefaultScreen.Name +  "'.");
        }

        NavigateTo(DefaultScreen.Name);
    }

    public bool ResetNavStack()
    {
        if (!CurrentScreen.IsTransitioning)
        {
            _navStack.Clear();

            return true;
        }

        return false;
    }

    public void ShowErrorModal(string titleText, string descText, string buttonText = "Close")
    {
        ErrorModal.ShowError(titleText, descText, buttonText);
    }
    
    public void HideErrorModal()
    {
        ErrorModal.Hide();
    }

    public void NavigateTo(string screenName)
    {
        if (!Screens.ContainsKey(screenName))
        {
            throw new KeyNotFoundException("ScreenManager: NavigateTo(screenName) failed, no screen named '" + screenName + "'.");
        }

        BaseScreen targetScreen = Screens[screenName];

        if (CanTransition(targetScreen, CurrentScreen))
        {
            _navStack.Push(CurrentScreen);

            Transition(targetScreen, CurrentScreen, false);
        }
        else
        {
            Debug.Log("ScreenManager: NavigateTo(" + screenName + ") failed, one or more screens current in transition state.");
        }
    }

    public void NavigateBack()
    {
        if (_navStack.Count > 1)
        {
            BaseScreen targetScreen = _navStack.Peek();

            if (CanTransition(targetScreen, CurrentScreen))
            {
                _navStack.Pop();

                Transition(targetScreen, CurrentScreen, true);
            }
            else
            {
                Debug.Log("ScreenManager: NavigateBack() failed, one or more screens current in transition state.");
            }
        }
    }

    public bool CanTransition(BaseScreen targetScreen, BaseScreen originScreen)
    {
        if (targetScreen != null && originScreen != null)
        {
            return !originScreen.IsTransitioning && !targetScreen.IsTransitioning;
        }

        if (targetScreen != null && originScreen == null)
        {
            return !targetScreen.IsTransitioning;
        }
        
        return false;
    }

    private void Transition(BaseScreen targetScreen, BaseScreen originScreen, bool isReverse)
    {
        CurrentScreen = targetScreen;

        if (originScreen)
        {            
            originScreen.Hide(isReverse);
        }
        
        targetScreen.Show(isReverse);        

        TransitionNavigator(targetScreen, isReverse);
    }

    private void TransitionNavigator(BaseScreen targetScreen, bool isReverse)
    {
        if (targetScreen.HasNavigator)
        {
            Navigator.Show(targetScreen.Name, isReverse);
        }
        else
        {
            Navigator.Hide(isReverse);
        }
    }
}
