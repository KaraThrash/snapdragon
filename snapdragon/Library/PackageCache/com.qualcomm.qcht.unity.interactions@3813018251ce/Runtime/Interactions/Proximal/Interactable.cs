// /******************************************************************************
//  * File: Interactable.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Linq;
using QCHT.Core;
using UnityEngine;
using UnityEngine.Events;

namespace QCHT.Interactions.Proximal
{
    public sealed class Interactable : MonoBehaviour, IProximalEnterHandler, IProximalExitHandler,
        IBeginProximalGrabHandler, IEndProximalGrabHandler, IProximalGrabHandler, IBeginDoubleGrabHandler,
        IDoubleGrabHandler
    {
        [Header("Object"), SerializeField]
        private bool disableCollisionsWhenInteracting = true;

        [Header("Scale"), SerializeField, MinMax(0f, 10f)]
        private Vector2 maxScaleMultiplier = new Vector2(.5f, 2f);

        [Header("Callbacks"), Space]
        public UnityEvent OnProximalEnterEvent = new UnityEvent();
        public UnityEvent OnProximalExitEvent = new UnityEvent();
        public UnityEvent OnProximalGrabEvent = new UnityEvent();
        public UnityEvent OnProximalReleaseEvent = new UnityEvent();
        
        private Pose _leftPoseOffset = Pose.identity;
        private Pose _rightPoseOffset = Pose.identity;
        private float _startMagnitude;
        private Vector3 _startScale;

        private SnapData _currentSnapData;
        private InteractionData _currentInteractionData;

        private Rigidbody _rigidbody;
        private Collider[] _colliders; // Collider that are not trigger

        public bool IsGrabbed => _currentInteractionData != null;
        
        private readonly float _grabbingSpeed = 15f;

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true).Where(col => !col.isTrigger).ToArray();
        }

        public void OnDisable()
        {
            ReleaseGrab();
        }

        #region Proximal Events

        public void OnProximalEnter(InteractionData eventData)
        {
            OnProximalEnterEvent.Invoke();
        }

        public void OnProximalExit(InteractionData eventData)
        {
            OnProximalExitEvent.Invoke();
        }

        public void OnBeginGrab(InteractionData eventData)
        {
            if (eventData.Data is {Type: InteractableType.Snap})
            {
                // Release snapping when already grabbed
                if (_currentInteractionData != eventData)
                    ReleaseGrab();

                var snapData = eventData.Data?.SnapData;

                try
                {
                    var data = snapData?.First(x => x.HandPose && x.HandPose.Type == eventData.Controller.Type);
                    if (data != null)
                    {
                        var pose = data.HandPose;
                        pose.Type = eventData.Controller.Type;
                        eventData.Controller.Driver.HandPose = pose;
                        eventData.Controller.Driver.HandPoseMask = data.HandPoseMask;
                        _currentSnapData = data;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else
            {
                var t = transform;
                ref var poseOffset = ref eventData.Controller.Type == HandType.Left
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;
                poseOffset.position = eventData.Controller.Driver.RootTransform.InverseTransformPoint(t.position);
                poseOffset.rotation =
                    Quaternion.Inverse(eventData.Controller.Driver.RootTransform.rotation) * t.rotation;
            }

            if (disableCollisionsWhenInteracting)
                ToggleObjectCollision(false);

            OnProximalGrabEvent.Invoke();
            _currentInteractionData = eventData;
        }

        public void OnGrab(InteractionData eventData)
        {
            if (eventData.Data is {Type: InteractableType.Snap})
            {
                if (_currentSnapData == null)
                    return;

                var pose = _currentSnapData.HandPose;
                pose.Root.Position = eventData.Controller.Driver.RootTransform.localPosition;
                pose.Root.Rotation = eventData.Controller.Driver.RootTransform.localRotation;

                var t = transform;
                /* t.rotation = Quaternion.Slerp(t.rotation,
                                             eventData.Controller.Driver.RootTransform.rotation * Quaternion.Inverse(Quaternion.Euler(_currentSnapData.LocalOffsetRotation)),
                                             Time.deltaTime * _grabbingSpeed);
                 t.position = Vector3.Lerp(t.position,
                                             eventData.Controller.Driver.RootTransform.position - t.rotation * _currentSnapData.LocalOffsetPosition,
                                             Time.deltaTime * _grabbingSpeed);*/

                t.rotation = eventData.Controller.Driver.RootTransform.rotation *
                             Quaternion.Inverse(Quaternion.Euler(_currentSnapData.LocalOffsetRotation));
                t.position = eventData.Controller.Driver.RootTransform.position -
                             t.rotation * _currentSnapData.LocalOffsetPosition;
            }
            else
            {
                var t = transform;

                ref var poseOffset = ref eventData.Controller.Type == HandType.Left
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;

                var rRot = eventData.Controller.Driver.RootTransform.rotation;
                t.rotation = Quaternion.Slerp(t.rotation, rRot * poseOffset.rotation, Time.deltaTime * _grabbingSpeed);
                t.position = Vector3.Lerp(t.position,
                    eventData.Controller.Driver.RootTransform.position + rRot * poseOffset.position,
                    Time.deltaTime * _grabbingSpeed);
            }
        }

        public void OnEndGrab(InteractionData eventData)
        {
            if (disableCollisionsWhenInteracting)
                ToggleObjectCollision(true);
            
            _currentInteractionData.ProximalGrab = null;
            _currentInteractionData.Controller.OnEndInteraction();
            _currentInteractionData = null;
            _currentSnapData = null;

            if (eventData.Controller)
            {
                eventData.Controller.Driver.HandPose = null;
                eventData.Controller.Driver.HandPoseMask = null;
                ref var poseOffset = ref eventData.Controller.Type == HandType.Left
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;
                poseOffset = Pose.identity;
            }

            OnProximalReleaseEvent.Invoke();
        }

        public void OnBeginDoubleGrab(DoubleInteractionData eventData)
        {
            _startMagnitude = Vector3.Distance(eventData.LeftData.Controller.Driver.RootTransform.position,
                eventData.RightData.Controller.Driver.RootTransform.position);
            _startScale = transform.localScale;

            if (eventData.LeftData.Data is {Type: InteractableType.Snap})
            {
                var grabData = _currentInteractionData.Controller.Type == HandType.Left
                    ? eventData.LeftData
                    : eventData.RightData;
                OnBeginGrab(grabData);
            }
        }

        public void OnDoubleGrab(DoubleInteractionData eventData)
        {
            if (eventData.LeftData.Data is {Type: InteractableType.Snap})
            {
                // Do nothing
            }
            else
            {
                var t = transform;
                var lPos = eventData.LeftData.Controller.Driver.RootTransform.position;
                var rPos = eventData.RightData.Controller.Driver.RootTransform.position;
                var lrRot = eventData.LeftData.Controller.Driver.RootTransform.rotation;
                var leftPos = lPos + lrRot * _leftPoseOffset.position;
                var rrRot = eventData.RightData.Controller.Driver.RootTransform.rotation;
                var rightPos = rPos + rrRot * _rightPoseOffset.position;
                var m = Vector3.Distance(lPos, rPos);
                t.position = Vector3.Lerp(leftPos, rightPos, .5f);
                t.localScale = _startScale *
                               Mathf.Clamp(m / _startMagnitude, maxScaleMultiplier.x, maxScaleMultiplier.y);
            }
        }

        #endregion

        public void ReleaseGrab()
        {
            if (_currentInteractionData == null)
                return;
            
            OnEndGrab(_currentInteractionData);
        }

        private void ToggleObjectCollision(bool enable)
        {
            if (_rigidbody)
                _rigidbody.isKinematic = !enable;

            foreach (var col in _colliders)
            {
                col.isTrigger = !enable;
            }
        }
    }
}