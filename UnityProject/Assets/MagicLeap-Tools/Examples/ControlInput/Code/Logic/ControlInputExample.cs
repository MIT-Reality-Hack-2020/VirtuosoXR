// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using MagicLeapTools;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

public class ControlInputExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public ControlInput controlInput;
    public Text status;
    public Text events;

    //Private Variables:
    private readonly int _maxEvents = 20;
    private List<string> _events = new List<string>();

    //Init:
    private void Awake()
    {
        //control events:
        controlInput.events.OnControlConnected.AddListener(HandleControlConnected);
        controlInput.events.OnControlDisconnected.AddListener(HandleControlDisconnected);

        //trigger events:
        controlInput.events.OnTriggerMove.AddListener(HandleTriggerMove);
        controlInput.events.OnTriggerPressBegan.AddListener(HandleTriggerPressBegan);
        controlInput.events.OnTriggerDown.AddListener(HandleTriggerDown);
        controlInput.events.OnTriggerHold.AddListener(HandleTriggerHold);
        controlInput.events.OnTriggerUp.AddListener(HandleTriggerUp);
        controlInput.events.OnTriggerPressEnded.AddListener(HandleTriggerReleased);
        controlInput.events.OnDoubleTrigger.AddListener(HandleDoubleTrigger);

        //bumper events:
        controlInput.events.OnBumperDown.AddListener(HandleBumperDown);
        controlInput.events.OnBumperHold.AddListener(HandleBumperHold);
        controlInput.events.OnBumperUp.AddListener(HandleBumperUp);
        controlInput.events.OnDoubleBumper.AddListener(HandleDoubleBumper);

        //home events:
        controlInput.events.OnHomeButtonTap.AddListener(HandleHomeTap);
        controlInput.events.OnDoubleHome.AddListener(HandleDoubleHome);

        //touch events:
        controlInput.events.OnForceTouchDown.AddListener(HandleForceTouchDown);
        controlInput.events.OnForceTouchUp.AddListener(HandleForceTouchUp);
        controlInput.events.OnSwipe.AddListener(HandleSwipe);
        controlInput.events.OnTapped.AddListener(HandleTapped);
        controlInput.events.OnTouchBeganMoving.AddListener(HandleTouchBeganMoving);
        controlInput.events.OnTouchDown.AddListener(HandleTouchDown);
        controlInput.events.OnTouchUp.AddListener(HandleTouchUp);
        controlInput.events.OnTouchHold.AddListener(HandleTouchHold);
        controlInput.events.OnTouchMove.AddListener(HandleTouchMove);
        controlInput.events.OnTouchRadialMove.AddListener(HandleTouchRadialMove);
    }

    //Loops:
    private void Update()
    {
        //control properties:
        status.text = "Control Connected: " + controlInput.Connected + "\n";
        status.text += "Control Position: " + controlInput.Position + "\n";
        status.text += "Control Orientation: " + controlInput.Orientation.eulerAngles + "\n";

        //trigger properties:
        status.text += "Trigger Down: " + controlInput.Trigger + "\n";
        status.text += "Trigger Value: " + controlInput.TriggerValue + "\n";

        //bumper properties:
        status.text += "Bumper Down: " + controlInput.Bumper + "\n";

        //touch properties:
        status.text += "Force Touch: " + controlInput.ForceTouch + "\n";
        status.text += "Touch Active: " + controlInput.Touch + "\n";
        status.text += "Touch Moved: " + controlInput.TouchMoved + "\n";
        status.text += "Touch Radial Delta: " + controlInput.TouchRadialDelta + "\n";
        status.text += "Touch: " + controlInput.TouchValue + "\n";
    }

    //Event Handlers:
    private void HandleControlConnected()
    {
        AddEvent("Control Connected");
    }

    private void HandleControlDisconnected()
    {
        AddEvent("Control Disconnected");
    }

    private void HandleTriggerPressBegan()
    {
        AddEvent("Trigger Began Moving");
    }

    private void HandleTriggerMove(float value)
    {
        AddEvent("Trigger Moved " + value);
    }

    private void HandleTriggerDown()
    {
        AddEvent("Trigger Down");
    }

    private void HandleTriggerHold()
    {
        AddEvent("Trigger Hold");
    }

    private void HandleTriggerUp()
    {
        AddEvent("Trigger Up");
    }

    private void HandleTriggerReleased()
    {
        AddEvent("Trigger Fully Released");
    }

    private void HandleDoubleTrigger()
    {
        AddEvent("Trigger Double Pull");
    }

    private void HandleBumperDown()
    {
        AddEvent("Bumper Down");
    }

    private void HandleBumperHold()
    {
        AddEvent("Bumper Hold");
    }

    private void HandleBumperUp()
    {
        AddEvent("Bumper Up");
    }

    private void HandleDoubleBumper()
    {
        AddEvent("Bumper Double Press");
    }

    private void HandleHomeTap()
    {
        AddEvent("Home Tap");
    }

    private void HandleDoubleHome()
    {
        AddEvent("Home Double Press");
    }

    private void HandleTouchHold()
    {
        AddEvent("Touch Hold");
    }

    private void HandleForceTouchDown()
    {
        AddEvent("Force Touch Down");
    }

    private void HandleForceTouchUp()
    {
        AddEvent("Force Touch Up");
    }

    private void HandleSwipe(MLInputControllerTouchpadGestureDirection value)
    {
        AddEvent("Swipe " + value);
    }

    private void HandleTapped(MLInputControllerTouchpadGestureDirection value)
    {
        AddEvent("Tap " + value);
    }

    private void HandleTouchBeganMoving()
    {
        AddEvent("Touch Began Moving");
    }

    private void HandleTouchDown(Vector4 value)
    {
        AddEvent("Touch Down " + value);
    }

    private void HandleTouchMove(Vector4 value)
    {
        AddEvent("Touch Move " + value);
    }

    private void HandleTouchRadialMove(float value)
    {
        AddEvent("Touch Radial Move " + value);
    }

    private void HandleTouchUp(Vector4 value)
    {
        AddEvent("Touch Touch Up " + value);
    }

    //Private Methods:
    private void AddEvent(string newEvent)
    {
        _events.Add(newEvent);

        //too many events?
        if (_events.Count > _maxEvents)
        {
            _events.RemoveAt(0);
        }

        //display events:
        events.text = string.Join(Environment.NewLine, _events.ToArray());
    }
#endif
}