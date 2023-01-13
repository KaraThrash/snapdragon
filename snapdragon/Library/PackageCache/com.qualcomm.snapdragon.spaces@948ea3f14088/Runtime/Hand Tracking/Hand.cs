/******************************************************************************
 * File: Hand.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class Hand
    {
        private XRTrackedHand _trackedHand;
        private bool _isLeft;

        public XRTrackedHand TrackedHand => _trackedHand;
        public bool IsLeft => _isLeft;
        public Pose[] Joints;
        public int Gesture;
        public float GestureRatio;
        public float FlipRatio;

        public Hand(XRTrackedHand trackedHand, bool isLeft) {
            _trackedHand = trackedHand;
            _isLeft = isLeft;
        }

        public void UpdateTrackedHandPoseAndTrackingState(Pose pose, TrackingState state) {
            _trackedHand.UpdatePoseAndTrackedState(pose, state);
        }
    }
}