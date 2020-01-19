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
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    //Public Enums:
    public enum SurfaceType { None, Floor, Seat, Table, Underside, Wall, Ceiling }

    public class SurfaceDetails : MonoBehaviour
    {
        //Events:
        [System.Serializable]
        public class Events
        {
            /// <summary>
            /// Thrown once the floor has been found or updated.
            /// </summary>
            public FloatEvent OnFloorFound;
            /// <summary>
            /// Thrown once the ceiling has been found or updated.
            /// </summary>
            public FloatEvent OnCeilingFound;
        }

        //Public Variables:
        public Events events;

        //Public Properties:
        public static bool FloorFound
        {
            get;
            private set;
        }

        public static bool CeilingFound
        {
            get;
            private set;
        }

        public static float FloorHeight
        {
            get;
            private set;
        }

        public static float CeilingHeight
        {
            get;
            private set;
        }

        //Private Variables:
        private static Transform _mainCamera;
        private static float _wallThreshold = .65f;
        private static float _minimumSeatHeight = 0.4064f;
        private static float _minimumTableHeight = 0.6604f;
        private static float _undersideHeight = 1.2192f;
        private static bool _initialized;
        private static Vector3 _planesQueryBoundsExtents = new Vector3(6, 6, 6);
        private static int _detectionInterval = 3;
        private static float _deltaThreshold = 0.0762f;

        //Init:
        private void Start()
        {
            Initialize();

            //start planes:
            MLWorldPlanes.Start();
            GetPlanes();
        }

        //Public Methods:
        public static SurfaceType Analyze(RaycastHit hit)
        {
            Initialize();

            //determine surface:
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (Mathf.Abs(dot) <= _wallThreshold)
            {
                return SurfaceType.Wall;
            }
            else
            {
                if (Mathf.Sign(dot) == 1)
                {
                    //status:
                    float floorDistance = Mathf.Abs(hit.point.y - FloorHeight);
                    float headDistance = Mathf.Abs(_mainCamera.position.y - FloorHeight);

                    if (headDistance < _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumSeatHeight)
                    {
                        return SurfaceType.Seat;
                    }

                    return SurfaceType.Floor;
                }
                else
                {
                    if (hit.point.y <= FloorHeight + _undersideHeight)
                    {
                        return SurfaceType.Underside;
                    }

                    return SurfaceType.Ceiling;
                }
            }
        }

        //Private Methods:
        private void GetPlanes()
        {
            MLWorldPlanesQueryParams query = new MLWorldPlanesQueryParams();
            query.MaxResults = 50;
            query.MinPlaneArea = .5f;
            query.Flags = MLWorldPlanesQueryFlags.SemanticWall | MLWorldPlanesQueryFlags.SemanticFloor | MLWorldPlanesQueryFlags.SemanticCeiling;
            query.BoundsCenter = _mainCamera.position;
            query.BoundsRotation = _mainCamera.rotation;
            query.BoundsExtents = _planesQueryBoundsExtents;
            MLWorldPlanes.GetPlanes(query, HandlePlanes);
        }

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            //sets:
            CeilingHeight = float.MinValue;
            FloorHeight = float.MaxValue;

            //refs:
            _mainCamera = Camera.main.transform;

            _initialized = true;
        }

        //Event Handlers:
        private void HandlePlanes(MLResult result, MLWorldPlane[] planes, MLWorldPlaneBoundaries[] boundaries)
        {
            //sets:
            int ceilingPlaneCount = 0;
            float averageCeilingHeight = 0;
            int floorPlaneCount = 0;
            float averageFloorHeight = 0;
            float highestFloor = float.MinValue;
            List<float> ceilingPlanes = new List<float>();
            List<float> floorPlanes = new List<float>();

            //iterate found planes:
            for (int i = 0; i < planes.Length; i++)
            {
                switch ((MLWorldPlanesQueryFlags)planes[i].Flags)
                {
                    case MLWorldPlanesQueryFlags.Horizontal | MLWorldPlanesQueryFlags.SemanticCeiling:
                        if (planes[i].Center.y > _mainCamera.transform.position.y)
                        {
                            ceilingPlanes.Add(planes[i].Center.y);
                        }
                        break;

                    case MLWorldPlanesQueryFlags.Horizontal | MLWorldPlanesQueryFlags.SemanticFloor:
                        if (planes[i].Center.y < _mainCamera.transform.position.y)
                        {
                            //find highest floor to avoid floor planes that may be beneath the physical floor:
                            if (planes[i].Center.y > highestFloor)
                            {
                                highestFloor = planes[i].Center.y;
                                floorPlanes.Add(planes[i].Center.y);
                            }
                        }
                        break;
                }
            }

            //iterate ceiling planes:
            if (ceilingPlanes.Count > 0)
            {
                for (int i = 0; i < ceilingPlanes.Count; i++)
                {
                    ceilingPlaneCount++;
                    averageCeilingHeight += ceilingPlanes[i];
                }

                //average ceiling:
                averageCeilingHeight /= ceilingPlaneCount;
            }
            else
            {
                averageCeilingHeight = float.MaxValue;
                CeilingFound = false;
            }

            //iterate floor planes to only use floor planes in the physcial room:
            for (int i = 0; i < floorPlanes.Count; i++)
            {
                if (highestFloor - floorPlanes[i] < _deltaThreshold)
                {
                    floorPlaneCount++;
                    averageFloorHeight += floorPlanes[i];
                }
            }

            //average floor:
            averageFloorHeight /= floorPlaneCount;

            //handle:
            if (Mathf.Abs(averageCeilingHeight - CeilingHeight) > _deltaThreshold)
            {
                if (!CeilingFound)
                {
                    CeilingFound = true;
                    events.OnCeilingFound?.Invoke(CeilingHeight);
                }
                CeilingHeight = averageCeilingHeight;
            }

            if (Mathf.Abs(averageFloorHeight - FloorHeight) > _deltaThreshold)
            {
                if (!FloorFound)
                {
                    FloorFound = true;
                    events.OnFloorFound?.Invoke(FloorHeight);
                }
                FloorHeight = averageFloorHeight;
            }

            //repeat:
            Invoke("GetPlanes", _detectionInterval);
        }
    }
}