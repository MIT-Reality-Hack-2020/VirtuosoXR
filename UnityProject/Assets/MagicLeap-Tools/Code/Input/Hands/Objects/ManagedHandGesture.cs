// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;
using MagicLeapTools;

//Enums:
public enum IntentPose { Relaxed, Pinching, Grasping, Pointing }

[System.Serializable]
public class ManagedHandGesture
{
    //Events:
    /// <summary>
    /// A verified keypose change that will not always match the KeyPose of the hand.  This is useful for maintaining the action of the user's hand and will strive to maintain grasping poses regardless of variations.
    /// </summary>
    public event Action<ManagedHand, MLHandKeyPose> OnVerifiedGestureChanged;
    /// <summary>
    /// A change in intent of the hand.  This buckets a few keyposes together so an overall intent can be identified.
    /// </summary>
    public event Action<ManagedHand, IntentPose> OnIntentChanged;
    /// <summary>
    /// The raw keypose change.
    /// </summary>
    public event Action<ManagedHand, MLHandKeyPose> OnKeyPoseChanged;

    //Public Properties:
    public IntentPose Intent
    {
        get;
        private set;
    }

    public InteractionPoint Pinch
    {
        get;
        private set;
    }

    public InteractionPoint Grasp
    {
        get;
        private set;
    }

    public InteractionPoint Point
    {
        get;
        private set;
    }

    /// <summary>
    /// A verified keypose that will not always match the KeyPose of the hand.  This is useful for maintaining the action of the user's hand and will strive to maintain grasping poses regardless of variations.
    /// </summary>
    public MLHandKeyPose VerifiedGesture
    {
        get;
        private set;
    }

    //Private Methods:
    private ManagedHand _managedHand;
    private MLHandKeyPose _proposedKeyPose = MLHandKeyPose.NoPose;
    private float _keyPoseChangedTime;
    private float _keyPoseStabailityDuration = .08f;
    private bool _collapsed;
    private float _dynamicReleaseDistance = 0.00762f;
    private Pose _interactionPointOffset;
    private InteractionState _currentInteractionState;
    private Vector3 _pinchAbsolutePositionOffset = new Vector3(-0.03f, -0.1f, 0.04f);
    private Vector3 _pinchAbsoluteRotationOffset = new Vector3(57.2f, -44.6f, -7.9f);
    private float _pinchRelativePositionDistance = 0.0889f;
    private Vector3 _pinchRelativeRotationOffset = new Vector3(57.2f, 0, -7.9f);
    private bool _pinchIsRelative;
    private bool _pinchTransitioning;
    private float _pinchTransitionStartTime;
    private float _pinchTransitionMaxDuration = .5f;
    private Vector3 _pinchArrivalPositionVelocity;
    private Quaternion _pinchArrivalRotationVelocity;
    private float _pinchTransitionTime = .1f;
    private float _maxGraspRadius = 0.1143f;

    //Constructors:
    public ManagedHandGesture(ManagedHand managedHand)
    {
        //sets:
        _managedHand = managedHand;
        Pinch = new InteractionPoint(_managedHand);
        Grasp = new InteractionPoint(_managedHand);
        Point = new InteractionPoint(_managedHand);

        _managedHand.Hand.OnKeyPoseBegin += HandleKeyposeChanged;
    }

    //Public Methods:
    public void Update()
    {
        if (!_managedHand.Visible)
        {
            return;
        }

        //pinch rotation offset mirror:
        Vector3 rotationOffset = _pinchAbsoluteRotationOffset;
        if (_managedHand.Hand.HandType == MLHandType.Left)
        {
            rotationOffset.y *= -1;
        }

        //holders:
        Vector3 pinchPosition = Vector3.zero;
        Quaternion pinchRotation = Quaternion.identity;

        //pinch interaction point radius:
        if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
        {
            Pinch.radius = Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered);
        }

        if (_managedHand.Skeleton.Thumb.Tip.Visible) //absolute placement:
        {
            //are we swapping modes?
            if (_pinchIsRelative)
            {
                _pinchIsRelative = false;
                _pinchTransitioning = true;
                _pinchTransitionStartTime = Time.realtimeSinceStartup;
            }

            pinchPosition = _managedHand.Skeleton.Thumb.Tip.positionFiltered;
            pinchRotation = TransformUtilities.RotateQuaternion(_managedHand.Skeleton.Rotation, rotationOffset);

            //gather offset distance:
            if (_managedHand.Skeleton.Index.Knuckle.Visible && _managedHand.Skeleton.Thumb.Knuckle.Visible)
            {
                Vector3 mcpMidpoint = Vector3.Lerp(_managedHand.Skeleton.Index.Knuckle.positionFiltered, _managedHand.Skeleton.Thumb.Knuckle.positionFiltered, .5f);
                _pinchRelativePositionDistance = Vector3.Distance(mcpMidpoint, pinchPosition);
            }
        }
        else //relative placement:
        {
            //are we swapping modes?
            if (!_pinchIsRelative)
            {
                _pinchIsRelative = true;
                _pinchTransitioning = true;
                _pinchTransitionStartTime = Time.realtimeSinceStartup;
            }

            //place between available mcps:
            if (_managedHand.Skeleton.Index.Knuckle.Visible && _managedHand.Skeleton.Thumb.Knuckle.Visible)
            {
                pinchPosition = Vector3.Lerp(_managedHand.Skeleton.Index.Knuckle.positionFiltered, _managedHand.Skeleton.Thumb.Knuckle.positionFiltered, .5f);

                //rotate:
                pinchRotation = TransformUtilities.RotateQuaternion(_managedHand.Skeleton.Rotation, _pinchRelativeRotationOffset);

                //move out along rotation forward:
                pinchPosition += pinchRotation * Vector3.forward * _pinchRelativePositionDistance;
            }
            else
            {
                //just use previous:
                pinchPosition = Pinch.position;
                pinchRotation = Pinch.rotation;
            }
        }

        //sticky release reduction:
        if (_collapsed)
        {
            if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
            {
                //if starting to release, start using a point between the thumb and index tips:
                if (Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered) > _dynamicReleaseDistance)
                {
                    pinchPosition = Vector3.Lerp(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered, .3f);
                }
            }
        }

        //apply pinch pose - to avoid jumps when relative placement is used we smooth until close enough:
        if (_pinchTransitioning)
        {
            //position:
            Pinch.position = Vector3.SmoothDamp(Pinch.position, pinchPosition, ref _pinchArrivalPositionVelocity, _pinchTransitionTime);
            float positionDelta = Vector3.Distance(Pinch.position, pinchPosition);

            //rotation:
            Pinch.rotation = MotionUtilities.SmoothDamp(Pinch.rotation, pinchRotation, ref _pinchArrivalRotationVelocity, _pinchTransitionTime);
            float rotationDelta = Quaternion.Angle(Pinch.rotation, pinchRotation);

            //close enough to hand off?
            if (positionDelta < .001f && rotationDelta < 5)
            {
                _pinchTransitioning = false;
            }

            //taking too long?
            if (Time.realtimeSinceStartup - _pinchTransitionStartTime > _pinchTransitionMaxDuration)
            {
                _pinchTransitioning = false;
            }
        }
        else
        {
            Pinch.position = pinchPosition;
            Pinch.rotation = pinchRotation;
        }

        //grasp interaction point:
        Bounds graspBounds = CalculateGraspBounds
            (
            _managedHand.Skeleton.Thumb.Knuckle,
            _managedHand.Skeleton.Thumb.Joint,
            _managedHand.Skeleton.Thumb.Tip,
            _managedHand.Skeleton.Index.Knuckle,
            _managedHand.Skeleton.Index.Joint,
            _managedHand.Skeleton.Index.Tip,
            _managedHand.Skeleton.Middle.Knuckle,
            _managedHand.Skeleton.Middle.Joint,
            _managedHand.Skeleton.Middle.Tip
            );
        Grasp.position = _managedHand.Skeleton.Position;
        //when points are being initially found they can be wildly off and this could cause a massively large volume:
        Grasp.radius = Mathf.Min(graspBounds.size.magnitude, _maxGraspRadius); 
        Grasp.rotation = _managedHand.Skeleton.Rotation;

        //intent updated:
        if (_currentInteractionState != null)
        {
            _currentInteractionState.FireUpdate();
        }

        //keypose change proposed:
        if (_managedHand.Hand.KeyPose != VerifiedGesture && _managedHand.Hand.KeyPose != _proposedKeyPose)
        {
            //queue a new proposed change to keypose:
            _proposedKeyPose = _managedHand.Hand.KeyPose;
            _keyPoseChangedTime = Time.realtimeSinceStartup;
        }
        
        //keypose change acceptance:
        if (_managedHand.Hand.KeyPose != VerifiedGesture && Time.realtimeSinceStartup - _keyPoseChangedTime > _keyPoseStabailityDuration)
        {
            //reset:
            Point.active = false;
            Pinch.active = false;
            Grasp.active = false;
            
            if (_collapsed)
            {
                //intent end:
                if (_managedHand.Hand.KeyPose == MLHandKeyPose.C || _managedHand.Hand.KeyPose == MLHandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandKeyPose.L || _managedHand.Hand.KeyPose == MLHandKeyPose.Finger)
                {
                    if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
                    {
                        //dynamic release:
                        if (Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered) > _dynamicReleaseDistance)
                        {
                            //end intent:
                            _collapsed = false;
                            _currentInteractionState.FireEnd();
                            _currentInteractionState = null;

                            //accept keypose change:
                            VerifiedGesture = _managedHand.Hand.KeyPose;
                            _proposedKeyPose = _managedHand.Hand.KeyPose;
                            OnVerifiedGestureChanged?.Invoke(_managedHand, VerifiedGesture);

                            if (_managedHand.Hand.KeyPose == MLHandKeyPose.Finger || _managedHand.Hand.KeyPose == MLHandKeyPose.L)
                            {
                                Intent = IntentPose.Pointing;
                                OnIntentChanged?.Invoke(_managedHand, Intent);
                            }
                            else if (_managedHand.Hand.KeyPose == MLHandKeyPose.C || _managedHand.Hand.KeyPose == MLHandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandKeyPose.Thumb)
                            {
                                Intent = IntentPose.Relaxed;
                                OnIntentChanged?.Invoke(_managedHand, Intent);
                            }
                        }
                    }
                }
            }
            else
            {
                //intent begin:
                if (_managedHand.Hand.KeyPose == MLHandKeyPose.Pinch || _managedHand.Hand.KeyPose == MLHandKeyPose.Ok || _managedHand.Hand.KeyPose == MLHandKeyPose.Fist)
                {
                    _collapsed = true;
                    
                    if (_managedHand.Hand.KeyPose == MLHandKeyPose.Pinch || _managedHand.Hand.KeyPose == MLHandKeyPose.Ok)
                    {
                        Intent = IntentPose.Pinching;
                        Pinch.active = true;
                        _currentInteractionState = Pinch.Touch;
                        _currentInteractionState.FireBegin();
                        OnIntentChanged?.Invoke(_managedHand, Intent);
                    }
                    else if (_managedHand.Hand.KeyPose == MLHandKeyPose.Fist)
                    {
                        Intent = IntentPose.Grasping;
                        Grasp.active = true;
                        _currentInteractionState = Grasp.Touch;
                        _currentInteractionState.FireBegin();
                        OnIntentChanged?.Invoke(_managedHand, Intent);
                    }
                }

                if (_managedHand.Hand.KeyPose == MLHandKeyPose.Finger || _managedHand.Hand.KeyPose == MLHandKeyPose.L)
                {
                    Intent = IntentPose.Pointing;
                    Point.active = true;
                    OnIntentChanged?.Invoke(_managedHand, Intent);
                }
                else if (_managedHand.Hand.KeyPose == MLHandKeyPose.C || _managedHand.Hand.KeyPose == MLHandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandKeyPose.Thumb)
                {
                    Intent = IntentPose.Relaxed;
                    OnIntentChanged?.Invoke(_managedHand, Intent);
                }

                //accept keypose change:
                VerifiedGesture = _managedHand.Hand.KeyPose;
                _proposedKeyPose = _managedHand.Hand.KeyPose;
                OnVerifiedGestureChanged?.Invoke(_managedHand, VerifiedGesture);
            }
        }
    }

    //Event Handlers:
    private void HandleKeyposeChanged(MLHandKeyPose keyPose)
    {
        OnKeyPoseChanged?.Invoke(_managedHand, keyPose);
    }

    //Private Methods:
    private Bounds CalculateGraspBounds(params ManagedKeypoint[] points)
    {
        Bounds graspBounds = new Bounds();
        graspBounds.center = _managedHand.Skeleton.Position;

        foreach (var item in points)
        {
            if (item.Visible)
            {
                graspBounds.Encapsulate(item.positionFiltered);
            }
        }

        return graspBounds;
    }
}