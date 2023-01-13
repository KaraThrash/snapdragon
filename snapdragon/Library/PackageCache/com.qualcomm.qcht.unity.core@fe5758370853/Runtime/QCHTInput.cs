// /******************************************************************************
//  * File: QCHTInput.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using QCHT.Core.Extensions;
using UnityEngine;

namespace QCHT.Core
{
    /// <summary>
    /// Handles qcht hand inputs.
    /// It detects down, "stay" and up states for each gestures.
    /// </summary>
    public partial class QCHTInput
    {
        private const float LongGrabDuration = 2f;
        private const float DoublePinchMaxDuration = 0.7f;
        private const int HistoryCount = 10;

        public static readonly QCHTHand RightHand = QCHTSDK.Instance.GetSDKData().GetHand(false);
        public static readonly QCHTHand LeftHand = QCHTSDK.Instance.GetSDKData().GetHand(true);

        public static QCHTHand WorldLeftHand { get; } = new QCHTHand();
        public static QCHTHand WorldRightHand { get; } = new QCHTHand();

        public static float LeftGestureRatio => LeftHand.gestureRatio;
        public static float RightGestureRatio => RightHand.gestureRatio;

        private static GestureId lastLeftGestureId = GestureId.UNKNOWN;
        private static GestureId lastRightGestureId = GestureId.UNKNOWN;

        private static float lastLeftPinchTime;
        private static int leftPinchCount;

        private static float lastRightPinchTime;
        private static int rightPinchCount;

        private static float leftGrabTime;
        private static float rightGrabTime;

        private static Vector3 leftGestureDownPosition;
        private static Vector3 rightGestureDownPosition;

        private static Vector3 previousLeftWc;
        private static Vector3 previousRightWc;

        private static readonly List<Vector3> leftDirectionHistory = new List<Vector3>();
        private static readonly List<Vector3> rightDirectionHistory = new List<Vector3>();

        private static bool s_useXRController = false;

        private static bool leftRaycastAllowed = true;
        private static bool rightRaycastAllowed = true;

        public static bool PhysicsRaycastAllowed { private get; set; } = true;

        public static bool IsRaycastAnimated { get; set; }

        #region Both Hand

        public static bool CanPhysicsRaycast(bool isLeftHand)
        {
            return PhysicsRaycastAllowed && (isLeftHand ? leftRaycastAllowed : rightRaycastAllowed);
        }

        public static bool IsHandDetected(bool isLeftHand)
        {
            return isLeftHand ? IsLeftHandDetected() : IsRightHandDetected();
        }

        public static bool GestureDown(bool isLeftHand, GestureId gesture)
        {
            return gesture switch
            {
                GestureId.PINCH => isLeftHand ? LeftPinchDown() : RightPinchDown(),
                GestureId.GRAB => isLeftHand ? LeftGrabDown() : RightGrabDown(),
                GestureId.OPEN_HAND => isLeftHand ? LeftOpenDown() : RightOpenDown(),
                _ => false
            };
        }

        public static bool Gesture(bool isLeftHand, GestureId gesture)
        {
            return gesture switch
            {
                GestureId.PINCH => isLeftHand ? LeftPinch() : RightPinch(),
                GestureId.GRAB => isLeftHand ? LeftGrab() : RightGrab(),
                GestureId.OPEN_HAND => isLeftHand ? LeftOpen() : RightOpen(),
                _ => false
            };
        }

        public static bool GestureUp(bool isLeftHand, GestureId gesture)
        {
            return gesture switch
            {
                GestureId.PINCH => isLeftHand ? LeftPinchUp() : RightPinchUp(),
                GestureId.GRAB => isLeftHand ? LeftGrabUp() : RightGrabUp(),
                GestureId.OPEN_HAND => isLeftHand ? LeftOpenUp() : RightOpenUp(),
                _ => false
            };
        }

        public static bool LongGesture(bool isLeftHand, GestureId gesture)
        {
            return gesture switch
            {
                GestureId.GRAB => isLeftHand ? LeftLongGrab() : RightLongGrab(),
                _ => false
            };
        }

        public static bool AnyHandLongGrab()
        {
            return RightLongGrab() || LeftLongGrab();
        }

        public static float GetGestureTimeNormalized(bool isLeftHand, GestureId gesture)
        {
            return gesture switch
            {
                GestureId.GRAB => isLeftHand ? GetLeftGrabTimeNormalized() : GetRightGrabTimeNormalized(),
                _ => 0f
            };
        }

        public static Vector3 GetGestureDownPosition(bool isLeftHand)
        {
            return isLeftHand ? leftGestureDownPosition : rightGestureDownPosition;
        }

        public static Vector3 GetGesturePosition(bool isLeftHand)
        {
            return isLeftHand ? WorldLeftHand.GetGesturePosition() : WorldRightHand.GetGesturePosition();
        }

        public static Vector3 GetHandDirection(bool isLeftHand)
        {
            var directionsHistory = isLeftHand ? leftDirectionHistory : rightDirectionHistory;
            var directionAverage = Vector3.zero;

            for (var i = 0; i < directionsHistory.Count; i++)
            {
                directionAverage += directionsHistory[i];
            }

            if (directionAverage == Vector3.zero) return Vector3.zero;

            directionAverage /= directionsHistory.Count;
            return directionAverage;
        }

        public static bool DoublePinch(ref bool isLeftHand)
        {
            if (LeftDoublePinch())
            {
                isLeftHand = true;
                return true;
            }

            if (RightDoublePinch())
            {
                isLeftHand = false;
                return true;
            }

            return false;
        }

        #endregion

        #region Right Hand

        private static bool IsRightHandDetected()
        {
            return RightHand.IsDetected;
        }

        private static bool RightPinchDown()
        {
            return lastRightGestureId != GestureId.PINCH && RightHand.gesture == GestureId.PINCH;
        }

        private static bool RightPinch()
        {
            return RightHand.gesture == GestureId.PINCH;
        }

        private static bool RightPinchUp()
        {
            return lastRightGestureId == GestureId.PINCH && RightHand.gesture != GestureId.PINCH;
        }

        private static bool RightDoublePinch()
        {
            return rightPinchCount == 2;
        }

        private static bool RightOpen()
        {
            return RightHand.gesture == GestureId.OPEN_HAND;
        }

        private static bool RightOpenUp()
        {
            return lastRightGestureId == GestureId.OPEN_HAND && RightHand.gesture != GestureId.OPEN_HAND;
        }

        private static bool RightOpenDown()
        {
            return lastRightGestureId != GestureId.OPEN_HAND && RightHand.gesture == GestureId.OPEN_HAND;
        }

        private static bool RightGrabDown()
        {
            return lastRightGestureId != GestureId.GRAB && RightHand.gesture == GestureId.GRAB;
        }

        private static bool RightGrab()
        {
            return RightHand.gesture == GestureId.GRAB;
        }

        public static bool RightLongGrab()
        {
            return rightGrabTime > LongGrabDuration;
        }

        private static bool RightGrabUp()
        {
            return lastRightGestureId == GestureId.GRAB && RightHand.gesture != GestureId.GRAB;
        }

        public static float GetRightGrabTimeNormalized()
        {
            return rightGrabTime / LongGrabDuration;
        }

        public static void ResetRightGrabTime()
        {
            rightGrabTime = 0f;
        }

        #endregion

        #region Left Hand

        private static bool IsLeftHandDetected()
        {
            return LeftHand.IsDetected;
        }

        private static bool LeftOpenDown()
        {
            return lastLeftGestureId != GestureId.OPEN_HAND && LeftHand.gesture == GestureId.OPEN_HAND;
        }

        private static bool LeftOpen()
        {
            return LeftHand.gesture == GestureId.OPEN_HAND;
        }

        private static bool LeftOpenUp()
        {
            return lastLeftGestureId == GestureId.OPEN_HAND && LeftHand.gesture != GestureId.OPEN_HAND;
        }

        private static bool LeftPinchDown()
        {
            return lastLeftGestureId != GestureId.PINCH && LeftHand.gesture == GestureId.PINCH;
        }

        private static bool LeftPinch()
        {
            return LeftHand.gesture == GestureId.PINCH;
        }

        private static bool LeftPinchUp()
        {
            return lastLeftGestureId == GestureId.PINCH && LeftHand.gesture != GestureId.PINCH;
        }

        private static bool LeftDoublePinch()
        {
            return leftPinchCount == 2;
        }

        private static bool LeftGrabDown()
        {
            return lastLeftGestureId != GestureId.GRAB && LeftHand.gesture == GestureId.GRAB;
        }

        private static bool LeftGrab()
        {
            return LeftHand.gesture == GestureId.GRAB;
        }

        public static bool LeftLongGrab()
        {
            return leftGrabTime > LongGrabDuration;
        }

        private static bool LeftGrabUp()
        {
            return lastLeftGestureId == GestureId.GRAB && LeftHand.gesture != GestureId.GRAB;
        }

        public static float GetLeftGrabTimeNormalized()
        {
            return leftGrabTime / LongGrabDuration;
        }

        public static void ResetLeftGrabTime()
        {
            leftGrabTime = 0f;
        }

        #endregion

        public static void Start(bool useXRController)
        {
#if ENABLE_INPUT_SYSTEM
            s_useXRController = useXRController;
            if (s_useXRController)
            {
                AddHandDevice(true);
                AddHandDevice(false);
            }
#endif
        }

        public static void Stop()
        {
#if ENABLE_INPUT_SYSTEM
            if (s_useXRController)
            {
                RemoveHandDevice(true);
                RemoveHandDevice(false);
            }
#endif
        }

        /// <summary>
        /// Retrieves the hands data from the QCHT SDK.
        /// Call it somewhere in an Update function and before any inputs method call. 
        /// </summary>
        public static void Update()
        {
            var head = QCHTManager.Instance.Head;
            LeftHand.ConvertToWorldSpaceNoAlloc(WorldLeftHand, head);
            RightHand.ConvertToWorldSpaceNoAlloc(WorldRightHand, head);

            if (LeftGrab())
                leftGrabTime += Time.deltaTime;
            else leftGrabTime = 0f;

            if (RightGrab())
                rightGrabTime += Time.deltaTime;
            else rightGrabTime = 0f;

            UpdateDoublePinch(false);
            UpdateDoublePinch(true);

            if (GestureUp(false, GestureId.PINCH))
                rightGestureDownPosition = Vector3.zero;

            if (GestureUp(true, GestureId.PINCH))
                leftGestureDownPosition = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
            if (s_useXRController)
            {
                UpdateDevices();
            }
#endif
            UpdateHandsDirection();
            UpdateRaycastState();
        }

        private static void UpdateRaycastState()
        {
            leftRaycastAllowed = IsHandDetected(true) && LeftHand.flipRatio < -0.1f && !Gesture(true, GestureId.GRAB);
            rightRaycastAllowed =
                IsHandDetected(false) && RightHand.flipRatio < -0.1f && !Gesture(false, GestureId.GRAB);
        }

        private static void UpdateDoublePinch(bool isLeftHand)
        {
            var hand = isLeftHand ? LeftHand : RightHand;

            if (hand == null)
                return;

            var pinchCount = isLeftHand ? leftPinchCount : rightPinchCount;
            var lastPinchTime = isLeftHand ? lastLeftPinchTime : lastRightPinchTime;

            if (GestureDown(isLeftHand, GestureId.PINCH))
            {
                if (pinchCount == 0)
                {
                    pinchCount = 1;
                    lastPinchTime = Time.time;
                }
                else if (pinchCount == 1)
                {
                    pinchCount = 2;
                }
            }

            if (isLeftHand)
            {
                leftPinchCount = pinchCount;
                lastLeftPinchTime = lastPinchTime;
            }
            else
            {
                rightPinchCount = pinchCount;
                lastRightPinchTime = lastPinchTime;
            }

            if (pinchCount == 0) return;

            if (Time.time - lastPinchTime > DoublePinchMaxDuration)
            {
                if (isLeftHand)
                {
                    leftPinchCount = 0;
                }
                else
                {
                    rightPinchCount = 0;
                }
            }
        }

        /// <summary>
        /// Stores the current hand gesture state for the next frame.
        /// Call it somewhere in a LateUpdate function.
        /// </summary>
        public static void LateUpdate()
        {
            lastLeftGestureId = LeftHand.gesture;
            lastRightGestureId = RightHand.gesture;
        }

        private static void UpdateHandsDirection()
        {
            if (WorldRightHand == null)
            {
                previousRightWc = Vector3.zero;
                rightDirectionHistory.Clear();
            }
            else
            {
                if (rightDirectionHistory.Count > HistoryCount)
                {
                    rightDirectionHistory.RemoveAt(rightDirectionHistory.Count - 1);
                }

                var rightDirection = WorldRightHand.GetWristPosition() - previousRightWc;
                rightDirectionHistory.Insert(0, rightDirection /*.normalized*/);
                previousRightWc = WorldRightHand.GetWristPosition();
            }

            if (WorldLeftHand == null)
            {
                previousLeftWc = Vector3.zero;
                leftDirectionHistory.Clear();
            }
            else
            {
                if (leftDirectionHistory.Count > HistoryCount)
                {
                    leftDirectionHistory.RemoveAt(leftDirectionHistory.Count - 1);
                }

                var leftDirection = WorldLeftHand.GetWristPosition() - previousLeftWc;
                leftDirectionHistory.Insert(0, leftDirection /*.normalized*/);
                previousLeftWc = WorldLeftHand.GetWristPosition();
            }
        }
    }
}