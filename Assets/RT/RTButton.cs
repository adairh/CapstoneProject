using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Based on code from Xarbrough: https://answers.unity.com/questions/1226851/addlistener-to-onpointerdown-of-button-instead-of.html

// Button that raises onDown event when OnPointerDown is called.
[AddComponentMenu("RT/RTButton")]
public class RTButton : Button
{
    // Event delegate triggered on mouse or touch down.
    [SerializeField] private ButtonDownEvent _onDown = new();

    [SerializeField] private ButtonUpEvent _onUp = new();

    protected RTButton()
    {
    }

    public ButtonDownEvent onDown
    {
        get => _onDown;
        set => _onDown = value;
    }

    public ButtonUpEvent onUp
    {
        get => _onUp;
        set => _onUp = value;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onDown.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _onUp.Invoke();
    }

    [Serializable]
    public class ButtonDownEvent : UnityEvent
    {
    }

    [Serializable]
    public class ButtonUpEvent : UnityEvent
    {
    }
}