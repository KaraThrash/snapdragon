/******************************************************************************
 * File: SpacesQCHTSDK.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if QCHT_UNITY_CORE
using System.Linq;
using QCHT.Core;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed class SpacesQCHTSDK : QCHTSDK
    {
        public override void UpdateData() {
            UpdateQCHTHand(true);
            UpdateQCHTHand(false);
        }

        private void UpdateQCHTHand(bool isLeft) {
            SpacesHand spacesHand = null;
            if (SpacesHandManager.instance != null) {
                spacesHand = isLeft ? SpacesHandManager.instance.LeftHand : SpacesHandManager.instance.RightHand;
            }

            QCHTHand hand = _data.GetHand(isLeft);

            if (spacesHand == null) {
                hand.IsDetected = false;
                hand.gesture = GestureId.UNKNOWN;
                hand.gestureRatio = 0f;
                hand.flipRatio = 0;
                return;
            }

            hand.IsDetected = spacesHand.trackingState == TrackingState.Tracking;
            var handRotation = hand.rotations;
            var handPosition = hand.points;

            var baseOrientation = Quaternion.AngleAxis(90f, Vector3.right);
            var adjustedRotations = spacesHand.RawJoints.Select(pose => pose.rotation * baseOrientation).ToArray();

            for (var i = 0; i < (int) QCHTPointId.POINT_COUNT; i++) {
                var _rotationLookup = QCHTJointLookupTables.RotationLookup[i];
                var _pointsLookup = QCHTJointLookupTables.PointsLookup[i];
                var rotation = adjustedRotations[(int)_rotationLookup[0]];
                handRotation[i] = _rotationLookup.Length > 1 ? Quaternion.Inverse(adjustedRotations[(int)_rotationLookup[1]]) * rotation : rotation;
                handPosition[i] = spacesHand.RawJoints[(int)_pointsLookup].position;
            }

            hand.gesture = (GestureId) spacesHand.RawGesture;
            // TODO: Implementation of gesture ratio calculation
            hand.gestureRatio = Mathf.Clamp01(spacesHand.RawGestureRatio);
            // TODO: Implementation of flip ratio calculation
            hand.flipRatio = Mathf.Clamp(spacesHand.RawFlipRatio, -1f, 1f);
        }
    }
}
#endif