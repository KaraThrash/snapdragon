// /******************************************************************************
//  * File: QCHTHandExtensions.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Core.Extensions
{
    public static class QCHTHandExtensions
    {
        /// <summary>
        /// Returns the position of a given qcht point id in the qcht hand data.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="pointId">Id of the qcht point</param>
        /// <returns>The position of the given qcht point</returns>
        public static Vector3 GetPoint(this QCHTHand hand, QCHTPointId pointId)
        {
            return hand.points[(int) pointId];
        }

        /// <summary>
        /// Returns the rotation of a given qcht point id in the qcht hand data.
        /// </summary>
        /// <param name="hand">Self. qcht hand data.</param>
        /// <param name="pointId">Id of the qcht point</param>
        /// <returns>The rotation of the given qcht point</returns>
        public static Quaternion GetRotation(this QCHTHand hand, QCHTPointId pointId)
        {
            return hand.rotations[(int) pointId];
        }

        /// <summary>
        /// Applies finger transform rotations from a qcht hand without alloc new intermediate data.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="fingerId">Finger id to</param>
        /// <param name="baseRotation">Proximal bone transform</param>
        /// <param name="middleRotation">Intermediate bone transform</param>
        /// <param name="topRotation">Distal bone transform</param>
        public static void AssignRotation(this QCHTHand hand, FingerId fingerId, ref Transform baseRotation,
            ref Transform middleRotation, ref Transform topRotation)
        {
            switch (fingerId)
            {
                case FingerId.THUMB:
                    baseRotation.localRotation = hand.rotations[(int) QCHTPointId.THUMB_PIP];
                    middleRotation.localRotation = hand.rotations[(int) QCHTPointId.THUMB_DIP];
                    topRotation.localRotation = hand.rotations[(int) QCHTPointId.THUMB_TIP];
                    break;

                case FingerId.INDEX:
                    baseRotation.localRotation = hand.rotations[(int) QCHTPointId.INDEX_MCP] *
                                                 hand.rotations[(int) QCHTPointId.INDEX_PIP];
                    middleRotation.localRotation = hand.rotations[(int) QCHTPointId.INDEX_DIP];
                    topRotation.localRotation = hand.rotations[(int) QCHTPointId.INDEX_TIP];
                    break;

                case FingerId.MIDDLE:
                    baseRotation.localRotation = hand.rotations[(int) QCHTPointId.MIDDLE_MCP] *
                                                 hand.rotations[(int) QCHTPointId.MIDDLE_PIP];
                    middleRotation.localRotation = hand.rotations[(int) QCHTPointId.MIDDLE_DIP];
                    topRotation.localRotation = hand.rotations[(int) QCHTPointId.MIDDLE_TIP];
                    break;

                case FingerId.RING:
                    baseRotation.localRotation = hand.rotations[(int) QCHTPointId.RING_MCP] *
                                                 hand.rotations[(int) QCHTPointId.RING_PIP];
                    middleRotation.localRotation = hand.rotations[(int) QCHTPointId.RING_DIP];
                    topRotation.localRotation = hand.rotations[(int) QCHTPointId.RING_TIP];
                    break;

                case FingerId.PINKY:
                    baseRotation.localRotation = hand.rotations[(int) QCHTPointId.PINKY_MCP] *
                                                 hand.rotations[(int) QCHTPointId.PINKY_PIP];
                    middleRotation.localRotation = hand.rotations[(int) QCHTPointId.PINKY_DIP];
                    topRotation.localRotation = hand.rotations[(int) QCHTPointId.PINKY_TIP];
                    break;
            }
        }

        /// <summary>
        /// Converts a local representation of qcht hand data into world space.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="head">Head transform</param>
        /// <returns>The world space converted qcht hand data</returns>
        public static QCHTHand ConvertToWorldSpace(this QCHTHand hand, Transform head)
        {
            if (head == null || hand == null)
                return null;

            var worldHand = new QCHTHand();
            hand.ConvertToWorldSpaceNoAlloc(worldHand, head);
            return worldHand;
        }

        /// <summary>
        /// Converts a local representation of qcht hand data into world space without allocation of the 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="worldHand"> World space hand data to update</param>
        /// <param name="head">Head transform</param>
        public static void ConvertToWorldSpaceNoAlloc(this QCHTHand hand, QCHTHand worldHand, Transform head)
        {
            if (!head || worldHand == null)
                return;

            worldHand.IsDetected = hand.IsDetected;
            worldHand.handId = hand.handId;
            worldHand.IsLeft = hand.IsLeft;
            worldHand.gesture = hand.gesture;
            worldHand.gestureRatio = hand.gestureRatio;
            worldHand.scale = hand.scale;

            for (var i = 0; i < hand.points.Length; i++)
            {
                var localPoint = hand.points[i];
                worldHand.points[i] = head.TransformPoint(localPoint);
            }

            for (var i = 0; i < hand.rotations.Length; i++)
            {
                worldHand.rotations[i] = hand.rotations[i];
            }
        }

        /// <summary>
        /// Returns the gesture position depending on the current gesture of qcht hand data.
        /// It is principally used for determine the pointer position to apply depending on the current gesture. 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The calculated gesture position</returns>
        public static Vector3 GetGesturePosition(this QCHTHand hand)
        {
            if (hand == null)
                return Vector3.zero;

            var thumbPoint = hand.points[(int) QCHTPointId.THUMB_DIP];
            var pinchPoint = thumbPoint.MidPoint(hand.points[(int) QCHTPointId.INDEX_PIP]);

            switch (hand.gesture)
            {
                case GestureId.GRAB:
                    return hand.points[(int) QCHTPointId.INDEX_MCP];
                case GestureId.POINT:
                    return hand.points[(int) QCHTPointId.INDEX_TIP];
                default:
                    return pinchPoint;
            }
        }

        /// <summary>
        /// Returns the wrist center alias the hand root position.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The hand root position</returns>
        public static Vector3 GetWristPosition(this QCHTHand hand)
        {
            return hand.points[(int) QCHTPointId.WRIST_CENTER];
        }

        /// <summary>
        /// Returns the wrist center alias the hand root rotation.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The hand root rotation</returns>
        public static Quaternion GetWristRotation(this QCHTHand hand)
        {
            return hand.rotations[(int) QCHTPointId.WRIST_CENTER];
        }

        /// <summary>
        /// Returns the calculated Palm center position.
        /// The palm center correspond to the center of mass of the hand palm.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The palm center position</returns>
        public static Vector3 GetPalmPosition(this QCHTHand hand)
        {
            var wrist = hand.points[(int) QCHTPointId.PALM_CENTER];
            var middle = hand.points[(int) QCHTPointId.MIDDLE_MCP].MidPoint(hand.points[(int) QCHTPointId.RING_MCP]);
            return wrist.MidPoint(middle);// + hand.GetPalmForward().normalized * 0.025f + hand.GetPalmUp().normalized * 0.015f; 
        }

        /// <summary>
        /// Returns the calculated Palm center rotation.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The palm center rotation</returns>
        public static Quaternion GetPalmRotation(this QCHTHand hand)
        {
            return hand.rotations[(int) QCHTPointId.PALM_CENTER];
        }

        /// <summary>
        /// Returns the normalized up vector from the wrist position.
        /// Used to determine the direction the hand.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The calculated palm up vector</returns>
        public static Vector3 GetPalmUp(this QCHTHand hand)
        {
            var palmUp = hand.points[(int) QCHTPointId.MIDDLE_MCP] - hand.GetWristPosition();
            return palmUp.normalized;
        }

        /// <summary>
        /// Returns the normalized up vector from the wrist position.
        /// Used to compute the looking direction the hand.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The calculated palm up vector</returns>
        public static Vector3 GetPalmForward(this QCHTHand hand)
        {
            var palmRight = hand.points[(int) QCHTPointId.RING_MCP] - hand.points[(int) QCHTPointId.MIDDLE_MCP];
            palmRight = hand.IsLeft ? Vector3.Normalize(palmRight) : Vector3.Normalize(-palmRight);
            return Vector3.Cross(hand.GetPalmUp(), palmRight);
        }

        /// <summary>
        /// Computes the palm scale relative to the hand bounding joints. 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>The calculated palm scale</returns>
        public static Vector3 GetPalmScale(this QCHTHand hand)
        {
            var palmScale = Vector3.zero;
            if (hand == null)
                return palmScale;

            QCHTPointId[] palmPointIds =
            {
                QCHTPointId.INDEX_TIP, QCHTPointId.MIDDLE_TIP, QCHTPointId.PINKY_TIP, QCHTPointId.RING_TIP,
                QCHTPointId.WRIST_END, QCHTPointId.WRIST_START
            };

            float
                minX = float.MaxValue,
                minY = float.MaxValue,
                minZ = float.MaxValue,
                maxX = float.MinValue,
                maxY = float.MinValue,
                maxZ = float.MinValue;

            for (var i = 0; i < palmPointIds.Length; i++)
            {
                var p = hand.points[i];

                if (p.x > maxX) maxX = p.x;
                if (p.x < minX) minX = p.x;

                if (p.y > maxY) maxY = p.y;
                if (p.y < minY) minY = p.y;

                if (p.z > maxZ) maxZ = p.z;
                if (p.z < minZ) minZ = p.z;

                palmScale = new Vector3(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY), Mathf.Abs(maxZ - minZ));
            }

            return palmScale;
        }

        /// <summary>
        /// Overrides qcht SDK gestures id by computing it from qcht hand data instead.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        public static void ComputeGesture(this QCHTHand hand)
        {
            const float pinchThreshold = .5f;
            var gestureId = GestureId.UNKNOWN;
            var thumbFlex = hand.GetFingerFlexionValue(FingerId.THUMB);
            var indexFlex = hand.GetFingerFlexionValue(FingerId.INDEX);
            var middleFlex = hand.GetFingerFlexionValue(FingerId.MIDDLE);
            var ringFlex = hand.GetFingerFlexionValue(FingerId.RING);
            var pinkyFlex = hand.GetFingerFlexionValue(FingerId.PINKY);

            if (thumbFlex > pinchThreshold &&
                indexFlex > pinchThreshold &&
                middleFlex > pinchThreshold &&
                ringFlex > pinchThreshold &&
                pinkyFlex > pinchThreshold)
            {
                gestureId = GestureId.GRAB;
            }
            else if (indexFlex > pinchThreshold &&
                     middleFlex > pinchThreshold &&
                     ringFlex > pinchThreshold &&
                     pinkyFlex > pinchThreshold)
            {
                gestureId = GestureId.UP;
            }
            else if (thumbFlex < pinchThreshold &&
                     indexFlex < pinchThreshold &&
                     middleFlex < pinchThreshold &&
                     ringFlex < pinchThreshold &&
                     pinkyFlex < pinchThreshold)
            {
                gestureId = GestureId.OPEN_HAND;
            }

            hand.gesture = gestureId;
        }

        /// <summary>
        /// Returns the flexion degree of a finger.
        /// It corresponds to the signed angle value between the palm and the finger first bone. 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <returns>0f flexion opened - 1f flexion closed</returns>
        public static float GetFingerFlexionValue(this QCHTHand hand, FingerId fingerId)
        {
            var root = hand.GetWristPosition();
            var joints = QCHTFinger.GetFingerJointsId(fingerId);
            var bone1 = root - hand.points[(int) joints[0]];
            var bone2 = hand.points[(int) joints[1]] - hand.points[(int) joints[0]];
            var angle = Vector3.SignedAngle(bone1, bone2, hand.rotations[(int) joints[0]] * Vector3.right);
            if (angle < 0f) angle += 360f;
            return Mathf.Clamp01(Mathf.Abs((angle - 180f) / 90f));
        }

        /// <summary>
        /// Returns the curl value of a finger.
        /// It corresponds to the distal to tip composed flexion value.
        /// For example a fist gestures occurs when all the finger flexion values and the curl values return 1f. 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="fingerId"></param>
        /// <returns>0f when distal/tip rotation is open and 1f when distal/tip rotation is closed</returns>
        public static float GetFingerCurlValue(this QCHTHand hand, FingerId fingerId)
        {
            var joints = QCHTFinger.GetFingerJointsId(fingerId);

            // First bone joint angle
            var pos1 = hand.points[(int) joints[0]];
            var pos2 = hand.points[(int) joints[1]];
            var pos3 = hand.points[(int) joints[2]];
            var rot1 = hand.rotations[(int) joints[1]];
            var bone1 = pos1 - pos2;
            var bone2 = pos3 - pos2;
            var a1 = Vector3.SignedAngle(bone1, bone2, rot1 * Vector3.right);
            if (a1 < 0f) a1 += 360f;
            var sum = a1;

            if (fingerId > FingerId.THUMB)
            {
                // Second bone joint angle
                var pos4 = hand.points[(int) joints[3]];
                var rot2 = hand.rotations[(int) joints[2]];
                var bone3 = pos4 - pos3;
                bone2 = -bone2; // reflected bone2
                var a2 = Vector3.SignedAngle(-bone2, bone3, rot2 * Vector3.right);
                if (a2 < 0f) a2 += 360f;
                sum += a2;
                sum *= .5f;
            }

            return 1f - Mathf.Clamp01(Mathf.Abs((sum - 180f) / 90f));
        }

        /// <summary>
        /// Returns the abduction value of a finger.
        /// It corresponds to the angle value between the finger and the next finger.
        /// For example the abduction value of the index will return the angle between the base bone of the index and the base bone of the middle finger.
        /// The abduction value of the pinky finger will always return 0. 
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <returns>Signed angle of abduction, 0f for adduction >0f for abduction</returns>
        public static float GetFingerAbductionValue(this QCHTHand hand, FingerId fingerId)
        {
            if (fingerId >= FingerId.PINKY)
                return 0f;

            var nextFingerId = fingerId + 1;
            var fJoints = QCHTFinger.GetFingerJointsId(fingerId);
            var nfJoints = QCHTFinger.GetFingerJointsId(nextFingerId);
            var fBase = hand.points[(int) fJoints[0]];
            var nfBase = hand.points[(int) nfJoints[0]];
            var fTip = hand.points[(int) fJoints[fJoints.Length - 1]];
            var nfTip = hand.points[(int) nfJoints[nfJoints.Length - 1]];
            var baseMidPoint = Vector3.Lerp(fBase, nfBase, 0.5f);

            Vector3 n1;
            if (fingerId == FingerId.THUMB)
            {
                n1 = fTip - fBase;
            }
            else
            {
                n1 = fTip - baseMidPoint;
            }

            var n2 = nfTip - baseMidPoint;
            var axis = Vector3.Cross(n1, n2);
            return Vector3.SignedAngle(n1, n2, axis);
        }

        /// <summary>
        /// Returns the opposition value of a finger from the thumb.
        /// It corresponds to the distance between the thumb tip and the performed finger tip.
        /// The opposition value of the thumb will therefore always return 0.
        /// </summary>
        /// <returns>The distance between thumb tip and finger tip</returns>
        public static float GetFingerOppositionValue(this QCHTHand hand, FingerId fingerId)
        {
            if (fingerId <= FingerId.THUMB)
                return 0f;

            var thumbTip = hand.GetPoint(QCHTPointId.THUMB_TIP);
            var fingerJoints = QCHTFinger.GetFingerJointsId(fingerId);
            var fingerTip = hand.GetPoint(fingerJoints[fingerJoints.Length - 1]);
            return Vector3.Magnitude(fingerTip - thumbTip);
        }
        
        /// <summary>
        /// Returns the minimum distance from any finger tip to the thumb tip.
        /// </summary>
        /// <returns>The closest distance between thumb tip and finger tip</returns>
        public static float GetMinDistTipToThumbTip(this QCHTHand hand)
        {
            var minDist = float.MaxValue;
            for (int i = 1; i < 5; i++)
            {
                var thumbTip = hand.GetPoint(QCHTPointId.THUMB_TIP);
                var tipIdx = (int)QCHTPointId.INDEX_TIP + 4 * (i - 1);
                var dist = Vector3.Distance(thumbTip, hand.points[tipIdx]);
                if (dist < minDist) minDist = dist;
            }

            return minDist;
        }
        
        public static float GetMinDistToThumbTip(this QCHTHand hand)
        {
            var minDist = float.MaxValue;
            for (int i = 1; i < 5; i++)
            {
                var thumbTip = hand.GetPoint(QCHTPointId.THUMB_TIP);
                var tipIdx = (int)QCHTPointId.INDEX_TIP + 4 * (i - 1);
                var dist = Vector3.Distance(thumbTip, hand.points[tipIdx]);
                if (dist < minDist) minDist = dist;
            }
            
            for (int i = 1; i < 5; i++)
            {
                var thumbDip = hand.GetPoint(QCHTPointId.THUMB_DIP);
                var tipIdx = (int)QCHTPointId.INDEX_TIP + 4 * (i - 1);
                var dist = Vector3.Distance(thumbDip, hand.points[tipIdx]);
                if (dist < minDist) minDist = dist;
            }

            return minDist;
        }

        #region Obsolete

        [Obsolete]
        public static bool IsForwardFacing(this QCHTHand hand)
        {
            return true;
        }
        
        /// <summary>
        /// Returns the normalized distance between thumb and index tips.
        /// </summary>
        /// <param name="hand">Self. qcht hand data</param>
        /// <returns>Distance between thumb tip and index tip</returns>
        [Obsolete("Use GetFingerOppositionValue instead and divide it by 0.07f to get the same result.")]
        public static float GetNormalizedDistanceBetweenIndexTipAndThumbTip(this QCHTHand hand)
        {
            const float indexTipThumbTipDistance = 0.07f;
            var indexTipPosition = hand.GetPoint(QCHTPointId.INDEX_TIP);
            var thumbTipPosition = hand.GetPoint(QCHTPointId.THUMB_TIP);
            var normalizedDistance = Vector3.Distance(indexTipPosition, thumbTipPosition) / indexTipThumbTipDistance;
            return Mathf.Clamp01(normalizedDistance);
        }

        #endregion
    }
}