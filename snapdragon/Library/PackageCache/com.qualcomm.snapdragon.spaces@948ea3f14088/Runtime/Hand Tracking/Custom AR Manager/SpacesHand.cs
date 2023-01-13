/******************************************************************************
 * File: SpacesHand.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    public sealed partial class SpacesHand : ARTrackable<XRTrackedHand, SpacesHand>
    {
        private bool _isLeft;
        private Pose[] _rawJoints = Enumerable.Repeat(new Pose(), (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT).ToArray();
        private readonly Pose[] _emptyJoints = Enumerable.Repeat(new Pose(), (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT).ToArray();
        private int _gesture;
        private float _gestureRatio;
        private float _flipRatio;
        private float _scale = 1f;

        private Transform _mainHeadTransform;

        internal Pose[] RawJoints => trackingState != TrackingState.None ? _rawJoints : _emptyJoints;
        internal int RawGesture => trackingState != TrackingState.None ? _gesture : -1;
        internal float RawGestureRatio => trackingState != TrackingState.None ? _gestureRatio : 0f;
        internal float RawFlipRatio => trackingState != TrackingState.None ? _flipRatio : 0f;
        internal float RawScale => trackingState != TrackingState.None ? _scale : 1f;

        public Pose Pose => sessionRelativeData.pose;
        public bool IsLeft => _isLeft;
        public Joint[] Joints {
            get {
                if (_mainHeadTransform == null) {
                    _mainHeadTransform = FindObjectOfType<ARPoseDriver>().transform;
                }
                return _rawJoints.Select((pose, index) => new Joint((JointType) index, new Pose(_mainHeadTransform.TransformPoint(pose.position), pose.rotation))).ToArray();
            }
        }

        public Gesture CurrentGesture => new Gesture((GestureType) RawGesture, RawGestureRatio, RawFlipRatio);

        internal void UpdateHandData(XRHandTrackingSubsystem subsystem) {
            _isLeft = subsystem.LeftHand.trackableId == trackableId;

            subsystem.GetJoints(trackableId, ref _rawJoints);
            if (_rawJoints.Length != (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT) {
                Debug.LogError("Underlying Hand Tracking subsystem returned the wrong number of joints! Adjusting returned array");
                _rawJoints = _emptyJoints.Select((pose, index) => index <= _rawJoints.Length ? _rawJoints[index] : pose).ToArray();
            }

            subsystem.GetGestureData(trackableId, ref _gesture, ref _gestureRatio, ref _flipRatio);
        }
    }
}