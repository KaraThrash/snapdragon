// /******************************************************************************
//  * File: QCHTPhysicsRaycaster.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Distal
{
    /// <summary>
    /// Mainly based on the Unity PhysicsRaycaster this class allows to perform raycasting using the event system.
    /// Instead raycasting physics ray through the camera center, here it uses transform for raycast.
    /// The origin of raycast is the self transform position it-self and the direction is the local Z axis.
    /// </summary>
    public sealed class QCHTPhysicsRaycaster : BaseRaycaster
    {
        public static QCHTPhysicsRaycaster LeftHandRaycaster;
        public static QCHTPhysicsRaycaster RightHandRaycaster;

        private Camera _eventCamera;

        [Tooltip("Determines the hand side")]
        public bool IsLeftHand;

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        [FormerlySerializedAs("m_EventMask")]
        [Tooltip("Layer mask used to filter events."), SerializeField]
        private LayerMask eventMask;

        /// <summary>
        /// The max distance of raycasting allowed.
        /// </summary>
        [Tooltip("The max distance of raycasting allowed."), SerializeField]
        private float maxRayDistance;

        [Tooltip("Ray transform origin")]
        [SerializeField] public Transform rayOrigin;

        private int _lastMaxRayIntersections;

        protected override void Awake()
        {
            base.Awake();

            if (IsLeftHand)
                LeftHandRaycaster = this;
            else
                RightHandRaycaster = this;
        }

        public override Camera eventCamera
        {
            get
            {
                if (_eventCamera == null)
                    _eventCamera = GetComponent<Camera>();
                return _eventCamera ?? Camera.main;
            }
        }

        public LayerMask EventMask
        {
            get => eventMask;
            set => eventMask = value;
        }

        public float MaxRayDistance
        {
            get => maxRayDistance;
            set => maxRayDistance = value;
        }

        public int FinalEventMask => eventCamera != null ? eventCamera.cullingMask & eventMask : (int) eventMask;

        private Ray _mRay;

        public Ray Ray
        {
            get
            {
                var t = rayOrigin ? rayOrigin : transform;
                _mRay.origin = t.position;
                _mRay.direction = t.forward;
                return _mRay;
            }
        }

        private RaycastHit[] _mHits;

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!(eventData is QCHTPointerEventData qchtEventData) || qchtEventData.isLeftHand != IsLeftHand)
                return;

            qchtEventData.worldSpaceRay = Ray;

            _mHits = Physics.RaycastAll(qchtEventData.worldSpaceRay, maxRayDistance, FinalEventMask);

            var hitCount = _mHits.Length;

            if (hitCount == 0)
                return;

            if (hitCount > 1)
                System.Array.Sort(_mHits, 0, hitCount, RaycastHitComparer.s_instance);

            for (var b = 0; b < hitCount; ++b)
            {
                var result = new RaycastResult
                {
                    gameObject = _mHits[b].collider.gameObject,
                    module = this,
                    distance = _mHits[b].distance,
                    worldPosition = _mHits[b].point,
                    worldNormal = _mHits[b].normal,
                    screenPosition = eventData.position,
                    displayIndex = 0,
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                };

                resultAppendList.Add(result);
            }
        }

        private sealed class RaycastHitComparer : IComparer<RaycastHit>
        {
            public static readonly RaycastHitComparer s_instance = new RaycastHitComparer();

            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(QCHTPhysicsRaycaster))]
    public class QCHTPhysicsRaycasterEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void CustomGizmoWhenSelectedInHierarchy(Transform transform, GizmoType gizmoType)
        {
            var raycaster = transform.GetComponent<QCHTPhysicsRaycaster>();
            if (!raycaster)
                return;

            var ray = raycaster.Ray;
            Debug.DrawRay(ray.origin, ray.direction * raycaster.MaxRayDistance, Color.magenta);
        }
    }
#endif
}