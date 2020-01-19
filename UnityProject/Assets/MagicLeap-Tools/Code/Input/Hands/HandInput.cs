// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

namespace MagicLeapTools
{
    public class HandInput : MonoBehaviour
    {
        //Events:
        public static event Action OnReady;

        //Public Properties:
        public static bool Ready
        {
            get;
            private set;
        }

        public static ManagedHand Right
        {
            get
            {
                if (_right == null)
                {
                    NotReadyError();
                }
                return _right;
            }
        }

        public static ManagedHand Left
        {
            get
            {
                if (_left == null)
                {
                    NotReadyError();
                }
                return _left;
            }
        }

        //Public Variables:
        [Header("Experimental")]
        [Tooltip("Allows a palm collider for pushing content around when the hand is fully open. Experimental for now since grabbing for something can incur a hit from the palm collider which knocks it away.")]
        public bool palmCollisions;

        //Private Variables:
        private static ManagedHand _right;
        private static ManagedHand _left;

        //Init:
        private void Start()
        {
            //turn on inputs:
            if (!MLHands.IsStarted)
            {
                if (!MLHands.Start().IsOk)
                {
                    enabled = false;
                }
                else
                {
                    MLHands.KeyPoseManager.SetKeyPointsFilterLevel(MLKeyPointFilterLevel.Smoothed);
                }
            }

            //setup hand tracking:
            List<MLHandKeyPose> handPoses = new List<MLHandKeyPose>();
            handPoses.Add(MLHandKeyPose.Finger);
            handPoses.Add(MLHandKeyPose.Pinch);
            handPoses.Add(MLHandKeyPose.Fist);
            handPoses.Add(MLHandKeyPose.Thumb);
            handPoses.Add(MLHandKeyPose.L);
            handPoses.Add(MLHandKeyPose.OpenHand);
            handPoses.Add(MLHandKeyPose.Ok);
            handPoses.Add(MLHandKeyPose.C);
            handPoses.Add(MLHandKeyPose.NoPose);
            MLHands.KeyPoseManager.EnableKeyPoses(handPoses.ToArray(), true, false);

            _right = new ManagedHand(MLHands.Right, this);
            _left = new ManagedHand(MLHands.Left, this);

            //ready:
            Ready = true;
            OnReady?.Invoke();
        }

        private void OnDestroy()
        {
            //turn off inputs:
            if (MLInput.IsStarted)
            {
                MLInput.Stop();
            }
        }
        
        //Loops:
        private void Update()
        {
            //avoid MLHands start failures:
            if (!Ready)
            {
                return;
            }

            //process hands:
            _right.Update();
            _left.Update();
        }

        //Private Methods:
        private static void NotReadyError()
        {
            Debug.LogError("Hand input not ready. Check 'Ready' property or subscribe to OnReady event before accessing.");
        }
    }
}