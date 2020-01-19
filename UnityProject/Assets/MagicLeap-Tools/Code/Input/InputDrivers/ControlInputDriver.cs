// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    [RequireComponent(typeof(ControlInput))]
    public class ControlInputDriver : InputDriver
    {
#if PLATFORM_LUMIN
        //Private Variables:
        ControlInput _controlInput;

        //Init:
        private void Awake()
        {
            _controlInput = GetComponent<ControlInput>();
            Active = _controlInput.Control != null;
        }

        //Flow:
        private void OnEnable()
        {
            //hooks:
            _controlInput.events.OnTapped.AddListener(HandleTouchPad);
            _controlInput.events.OnSwipe.AddListener(HandleTouchPad);
            _controlInput.events.OnTriggerDown.AddListener(HandleTriggerDown);
            _controlInput.events.OnTriggerUp.AddListener(HandleTriggerUp);
            _controlInput.events.OnBumperDown.AddListener(HandleBumperDown);
            _controlInput.events.OnBumperUp.AddListener(HandleBumperUp);
            _controlInput.events.OnForceTouchDown.AddListener(HandleForceTouchDown);
            _controlInput.events.OnForceTouchUp.AddListener(HandleForceTouchUp);
            _controlInput.events.OnTouchRadialMove.AddListener(HandleTouchRadialMove);
            _controlInput.events.OnControlConnected.AddListener(HandleControlConnected);
            _controlInput.events.OnControlDisconnected.AddListener(HandleControlDisconnected);
        }

        private void OnDisable()
        {
            //unhook:
            _controlInput.events.OnTapped.RemoveListener(HandleTouchPad);
            _controlInput.events.OnSwipe.RemoveListener(HandleTouchPad);
            _controlInput.events.OnTriggerDown.RemoveListener(HandleTriggerDown);
            _controlInput.events.OnTriggerUp.RemoveListener(HandleTriggerUp);
            _controlInput.events.OnBumperDown.RemoveListener(HandleBumperDown);
            _controlInput.events.OnBumperUp.RemoveListener(HandleBumperUp);
            _controlInput.events.OnForceTouchDown.RemoveListener(HandleForceTouchDown);
            _controlInput.events.OnForceTouchUp.RemoveListener(HandleForceTouchUp);
            _controlInput.events.OnTouchRadialMove.RemoveListener(HandleTouchRadialMove);
            _controlInput.events.OnControlConnected.RemoveListener(HandleControlConnected);
            _controlInput.events.OnControlDisconnected.RemoveListener(HandleControlDisconnected);
        }

        //Event Handlers:
        private void HandleTriggerDown()
        {
            Fire0Down();
        }

        private void HandleTriggerUp()
        {
            Fire0Up();
        }

        private void HandleBumperDown()
        {
            Fire1Down();
        }

        private void HandleBumperUp()
        {
            Fire1Up();
        }

        private void HandleForceTouchDown()
        {
            Fire2Down();
        }

        private void HandleForceTouchUp()
        {
            Fire2Up();
        }

        private void HandleTouchRadialMove(float angleDelta)
        {
            RadialDrag(angleDelta);
        }

        private void HandleTouchPad(MLInputControllerTouchpadGestureDirection direction)
        {
            switch (direction)
            {
                case MLInputControllerTouchpadGestureDirection.Left:
                    Left();
                    break;

                case MLInputControllerTouchpadGestureDirection.Right:
                    Right();
                    break;

                case MLInputControllerTouchpadGestureDirection.Up:
                    Up();
                    break;

                case MLInputControllerTouchpadGestureDirection.Down:
                    Down();
                    break;
            }
        }

        private void HandleControlConnected()
        {
            Activate();
        }

        private void HandleControlDisconnected()
        {
            Deactivate();
        }
#endif
    }
}