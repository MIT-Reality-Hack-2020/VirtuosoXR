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
using MagicLeapTools;
using UnityEngine.XR.MagicLeap;

public class SimpleHandPointerExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public PlaceInFront contentRoot;

    //Private Variables:
    private float _thumbsUpVerificationTime = .5f;

    //Init:
    private void Awake()
    {
        //hooks:
        HandInput.OnReady += HandleHandInputReady;
    }

    //Event Handlers:
    private void HandleHandInputReady()
    {
        //hooks:
        HandInput.Right.Gesture.OnKeyPoseChanged += HandleKeyPoseChanged;
        HandInput.Left.Gesture.OnKeyPoseChanged += HandleKeyPoseChanged;
    }

    private void HandleKeyPoseChanged(ManagedHand hand, MLHandKeyPose pose)
    {
        //both hands holding thumbs?
        if (HandInput.Left.Hand.KeyPose == MLHandKeyPose.Thumb && HandInput.Right.Hand.KeyPose == MLHandKeyPose.Thumb)
        {
            StartCoroutine("ThumbsUpReset");
        }
        else
        {
            StopCoroutine("ThumbsUpReset");
        }
    }

    //Coroutines:
    private IEnumerator ThumbsUpReset()
    {
        yield return new WaitForSeconds(_thumbsUpVerificationTime);
        contentRoot.Place();
    }
#endif
}