// /******************************************************************************
//  * File: HandPoseDriver.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace QCHT.Interactions.Hands
{
    public sealed class HandPoseDriver : MonoBehaviour
    {
        [Space]
        public HandModel HandModel;

        private HandPose _handPose;

        public HandPose HandPose
        {
            set
            {
                _handPose = value;
                MapDataToParts();
            }
            get => _handPose;
        }

        private HandPoseMask _handPoseMask;

        public HandPoseMask HandPoseMask
        {
            set
            {
                _handPoseMask = value;
                MapDataToParts();
            }

            get => _handPoseMask;
        }

        [FormerlySerializedAs("palmPart")]
        [Header("Joints")]
        [SerializeField] private HandJointUpdater rootPart;

        [Space]

        // Thumb
        [SerializeField] private HandJointUpdater thumbBase;
        [SerializeField] private HandJointUpdater thumbMiddle;
        [SerializeField] private HandJointUpdater thumbTop;

        [Space]

        // Index
        [SerializeField] private HandJointUpdater indexBase;
        [SerializeField] private HandJointUpdater indexMiddle;
        [SerializeField] private HandJointUpdater indexTop;

        [Space]

        // Middle
        [SerializeField] private HandJointUpdater middleBase;
        [SerializeField] private HandJointUpdater middleMiddle;
        [SerializeField] private HandJointUpdater middleTop;

        [Space]

        // Ring
        [SerializeField] private HandJointUpdater ringBase;
        [SerializeField] private HandJointUpdater ringMiddle;
        [SerializeField] private HandJointUpdater ringTop;

        [Space]

        // Pinky
        [SerializeField] private HandJointUpdater pinkyBase;
        [SerializeField] private HandJointUpdater pinkyMiddle;
        [SerializeField] private HandJointUpdater pinkyTop;

        [SerializeField, CanBeEmpty] private Transform scale;

        public Transform RootTransform => rootPart.transform;

        private float _scaleMultiplier = 1f;
        public float ScaleMultiplier
        {
            get => _scaleMultiplier;
            set => _scaleMultiplier = value;
        }

        #region MonoBehaviour Functions

        private void Start()
        {
            MapDataToParts();
        }

        private void LateUpdate()
        {
            // Update hand scale 
            if (scale)
            {
                scale.localScale = (HandPose ? HandPose.Scale : HandModel.Pose.Scale) * _scaleMultiplier;
            }

            rootPart.UpdateJoint();
            UpdateFingerJoints(FingerId.THUMB);
            UpdateFingerJoints(FingerId.INDEX);
            UpdateFingerJoints(FingerId.MIDDLE);
            UpdateFingerJoints(FingerId.RING);
            UpdateFingerJoints(FingerId.PINKY);
        }

        #endregion

        private void MapDataToParts()
        {
            rootPart.BoneRef = HandModel.Pose.Root;
            MapFingerDataToParts(FingerId.THUMB);
            MapFingerDataToParts(FingerId.INDEX);
            MapFingerDataToParts(FingerId.MIDDLE);
            MapFingerDataToParts(FingerId.RING);
            MapFingerDataToParts(FingerId.PINKY);
        }

        private void MapFingerDataToParts(FingerId id)
        {
            var maskState = GetMaskStateForFinger(id);
            var handPose = maskState == HandPoseMask.MaskState.Required ? HandPose : HandModel.Pose;

            if (handPose == null)
                return;

            var fingerData = GetFingerDataFromPose(id, handPose);
            var (bottom, middle, top) = GetJointsOfAFinger(id);

            if (bottom != null) bottom.BoneRef = fingerData.BaseData;
            if (middle != null) middle.BoneRef = fingerData.MiddleData;
            if (top != null) top.BoneRef = fingerData.TopData;
        }

        private void UpdateFingerJoints(FingerId id)
        {
            var (bottom, middle, top) = GetJointsOfAFinger(id);
            bottom?.UpdateJoint();
            middle?.UpdateJoint();
            top?.UpdateJoint();
        }

        private HandPoseMask.MaskState GetMaskStateForFinger(FingerId id)
        {
            if (HandPoseMask == null)
                return HandPoseMask.MaskState.Free;

            return id switch
            {
                FingerId.THUMB => HandPoseMask.Thumb,
                FingerId.INDEX => HandPoseMask.Index,
                FingerId.MIDDLE => HandPoseMask.Middle,
                FingerId.RING => HandPoseMask.Ring,
                FingerId.PINKY => HandPoseMask.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private FingerData GetFingerDataFromPose(FingerId id, HandPose pose)
        {
            return id switch
            {
                FingerId.THUMB => pose.Thumb,
                FingerId.INDEX => pose.Index,
                FingerId.MIDDLE => pose.Middle,
                FingerId.RING => pose.Ring,
                FingerId.PINKY => pose.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private (IHandJointUpdater, IHandJointUpdater, IHandJointUpdater) GetJointsOfAFinger(FingerId id)
        {
            return id switch
            {
                FingerId.THUMB => (thumbBase, thumbMiddle, thumbTop),
                FingerId.INDEX => (indexBase, indexMiddle, indexTop),
                FingerId.MIDDLE => (middleBase, middleMiddle, middleTop),
                FingerId.RING => (ringBase, ringMiddle, ringTop),
                FingerId.PINKY => (pinkyBase, pinkyMiddle, pinkyTop),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }
    }
}