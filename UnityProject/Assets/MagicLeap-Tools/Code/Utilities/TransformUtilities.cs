// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    public static class TransformUtilities
    {
        //Private Variables:
        private static float _nearClipBuffer = .05f;
        private static Camera _mainCamera;

        //Private Properties:
        private static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        private static Plane CameraPlane
        {
            get
            {
                return new Plane(MainCamera.transform.forward, MainCamera.transform.position + MainCamera.transform.forward * (MainCamera.nearClipPlane + _nearClipBuffer));
            }
        }

        //Public Methods:
        public static bool InsideClipPlane(Vector3 location)
        {
            return !CameraPlane.GetSide(location);
        }

        public static Vector3 LocationOnClipPlane(Vector3 location)
        {
            return CameraPlane.ClosestPointOnPlane(location);
        }

        public static float DistanceInsideClipPlane(Vector3 location)
        {
            return Vector3.Distance(LocationOnClipPlane(location), location);
        }

        public static Vector3 RelativeOffset(Vector3 basePosition, Quaternion baseRotation, Vector3 offset)
        {
            Matrix4x4 trs = Matrix4x4.TRS(basePosition, baseRotation, Vector3.one);
            return trs.MultiplyPoint3x4(offset);
        }

        public static Vector3 InverseRelativeOffset(Vector3 basePosition, Quaternion baseRotation, Vector3 position)
        {
            Matrix4x4 trs = Matrix4x4.TRS(basePosition, baseRotation, Vector3.one);
            return trs.inverse.MultiplyPoint3x4(position);
        }

        public static Quaternion RotateQuaternion(Quaternion rotation, Vector3 amount)
        {
            return Quaternion.AngleAxis(amount.x, rotation * Vector3.right) *
                Quaternion.AngleAxis(amount.y, rotation * Vector3.up) *
                Quaternion.AngleAxis(amount.z, rotation * Vector3.forward) *
                rotation;
        }
    }
}