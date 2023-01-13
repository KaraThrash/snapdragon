// /******************************************************************************
//  * File: QCHTProximalInteractor.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using QCHT.Core.Extensions;
using QCHT.Interactions.Distal;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Proximal
{
    [Serializable]
    public class SnapData
    {
        public HandPose HandPose;
        public HandPoseMask HandPoseMask;
        public Vector3 LocalOffsetPosition;
        public Vector3 LocalOffsetRotation;

#if UNITY_EDITOR
        public static SnapData DeserializeSnapData(SerializedProperty property)
        {
            var handPoseProperty = property.FindPropertyRelative("HandPose");
            var handMaskProperty = property.FindPropertyRelative("HandPoseMask");
            var offsetPositionProperty = property.FindPropertyRelative("LocalOffsetPosition");
            var offsetRotationProperty = property.FindPropertyRelative("LocalOffsetRotation");
            return new SnapData
            {
                HandPose = handPoseProperty.objectReferenceValue as HandPose,
                HandPoseMask = handMaskProperty.objectReferenceValue as HandPoseMask,
                LocalOffsetPosition = offsetPositionProperty.vector3Value,
                LocalOffsetRotation = offsetRotationProperty.vector3Value
            };
        }

        public static void SerializeSnapData(SerializedProperty property, SnapData snapData)
        {
            var handPoseProperty = property.FindPropertyRelative("HandPose");
            var handMaskProperty = property.FindPropertyRelative("HandPoseMask");
            var offsetPositionProperty = property.FindPropertyRelative("LocalOffsetPosition");
            var offsetRotationProperty = property.FindPropertyRelative("LocalOffsetRotation");
            handPoseProperty.objectReferenceValue = snapData.HandPose;
            handMaskProperty.objectReferenceValue = snapData.HandPoseMask;
            offsetPositionProperty.vector3Value = snapData.LocalOffsetPosition;
            offsetRotationProperty.vector3Value = snapData.LocalOffsetRotation;
            property.serializedObject.ApplyModifiedProperties();
        }
#endif
    }


    public class InteractionData : BaseEventData
    {
        public QCHTHandInteractionController Controller;
        public GameObject ProximalEnter;
        public GameObject ProximalGrab;
        public GameObject ProximalDoubleGrab;
        public IInteractionDataProvider Data;

        public InteractionData(EventSystem eventSystem) : base(eventSystem)
        {
        }
    }

    public class DoubleInteractionData : BaseEventData
    {
        public InteractionData LeftData;
        public InteractionData RightData;

        public DoubleInteractionData(EventSystem eventSystem) : base(eventSystem)
        {
        }
    }

    public struct InteractionResult
    {
        public GameObject currentGameObject;
    }

    public sealed class QCHTProximalInteractor : MonoBehaviour
    {
        public enum ProximalGestureId
        {
            Pinch = GestureId.PINCH,
            Grab = GestureId.GRAB,
            Point = GestureId.POINT,
            OpenHand = GestureId.OPEN_HAND,
        }

        [Header("Gestures for proximal interaction")]
        [SerializeField]
        [Tooltip("Default gesture to start proximal interaction, IBeginGrabHandler")]
        private ProximalGestureId mainGesture = ProximalGestureId.Grab;

        [SerializeField]
        [Tooltip("A second gesture can be used to start proximal interaction.")]
        private ProximalGestureId secondaryGesture = ProximalGestureId.Pinch;

        [SerializeField]
        [Tooltip("Default gesture to release the proximal interaction")]
        private ProximalGestureId defaultReleaseGesture = ProximalGestureId.OpenHand;

        private static InteractionData s_leftInteractionData;
        private static InteractionData s_rightInteractionData;
        private static DoubleInteractionData s_doubleInteractionData;

        private void Update()
        {
            ProcessControllers();
        }

        private void ProcessControllers()
        {
            if (s_leftController)
                ProcessController(s_leftController, s_leftInteractionData);

            if (s_rightController)
                ProcessController(s_rightController, s_rightInteractionData);

            if (s_doubleInteractionData != null)
                ProcessDoubleGrab(s_doubleInteractionData);
        }

        private void ProcessController(IInteractionController controller, InteractionData interactionData)
        {
            GetDataFromController(controller, interactionData);
            ProcessHandInput(controller, interactionData);
            ProcessGrab(interactionData);
        }

        private static void GetDataFromController(IInteractionController controller, InteractionData eventData)
        {
            if (controller == null)
                return;

            var results = controller.Process();

            if (!results.currentGameObject)
            {
                if (eventData.ProximalEnter)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalEnter, eventData,
                        ExecuteProximalEventsExtensions.exitHandler);
                    eventData.ProximalEnter = null;
                }
            }

            if (!eventData.ProximalEnter || eventData.ProximalEnter != results.currentGameObject)
            {
                eventData.ProximalEnter = results.currentGameObject;

                if (eventData.ProximalEnter && !eventData.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalEnter, eventData,
                        ExecuteProximalEventsExtensions.enterHandler);
                    var dataProvider = eventData.ProximalEnter.GetComponentInChildren<IInteractionDataProvider>();
                    eventData.Data = dataProvider;
                }
            }
        }

        private void ProcessHandInput(IInteractionController controller, InteractionData eventData)
        {
            var gesture = (GestureId) mainGesture;
            var secondary = (GestureId) secondaryGesture;
            var releaseGesture = (GestureId) defaultReleaseGesture;

            if (eventData.Data != null)
            {
                gesture = eventData.Data.GrabGesture;
                secondary = eventData.Data.SecondGrabGesture;
            }

            if (GetGestureDownThisFrame(eventData, gesture, secondary))
            {
                if (!eventData.ProximalGrab)
                {
                    var grabbable = ExecuteEvents.GetEventHandler<IProximalGrabHandler>(eventData.ProximalEnter);

                    if (grabbable)
                    {
                        ExecuteEvents.ExecuteHierarchy(grabbable, eventData,
                            ExecuteProximalEventsExtensions.beginGrabHandler);
                        eventData.ProximalGrab = grabbable;
                        controller.OnBeginInteraction();
                    }
                }
            }
            //if (GetGestureDownThisFrame(eventData, releaseGesture))
            else if (GetReleaseGestureDownThisFrame(eventData, releaseGesture))
            {
                if (eventData.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalGrab, eventData,
                        ExecuteProximalEventsExtensions.endGrabHandler);
                    eventData.ProximalGrab = null;
                    eventData.ProximalDoubleGrab = null;
                    controller.OnEndInteraction();
                }
            }
        }

        private static void ProcessDoubleGrab(DoubleInteractionData eventData)
        {
            var leftData = eventData.LeftData;
            var rightData = eventData.RightData;

            // Totally stops double grab when one hand stopped double grabbing
            if (leftData.ProximalDoubleGrab != rightData.ProximalDoubleGrab)
            {
                leftData.ProximalDoubleGrab = null;
                rightData.ProximalDoubleGrab = null;

                // Get back to proximal for the hand which still grabbing
                var data = leftData.ProximalGrab ? leftData : rightData;
                if (data.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(data.ProximalGrab, data,
                        ExecuteProximalEventsExtensions.beginGrabHandler);
                }
            }

            // Begin double grabbing
            if (!leftData.ProximalDoubleGrab && leftData.ProximalGrab &&
                leftData.ProximalGrab == rightData.ProximalGrab)
            {
                var doubleGrab = ExecuteEvents.GetEventHandler<IDoubleGrabHandler>(leftData.ProximalGrab);

                if (doubleGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(doubleGrab, s_doubleInteractionData,
                        ExecuteProximalEventsExtensions.beginDoubleGrabHandler);
                    leftData.ProximalDoubleGrab = doubleGrab;
                    rightData.ProximalDoubleGrab = doubleGrab;
                }
            }

            // Double grabbing
            if (leftData.ProximalDoubleGrab)
            {
                var doubleGrab = ExecuteEvents.GetEventHandler<IDoubleGrabHandler>(leftData.ProximalDoubleGrab);

                if (doubleGrab)
                {
                    ExecuteEvents.Execute(doubleGrab, s_doubleInteractionData,
                        ExecuteProximalEventsExtensions.doubleGrabHandler);
                    leftData.ProximalDoubleGrab = doubleGrab;
                    rightData.ProximalDoubleGrab = doubleGrab;
                }
            }
        }

        private static void ProcessGrab(InteractionData eventData)
        {
            if (eventData.ProximalGrab && !eventData.ProximalDoubleGrab)
            {
                ExecuteEvents.Execute(eventData.ProximalGrab, eventData, ExecuteProximalEventsExtensions.grabHandler);
            }
        }

        #region static

        private static bool GetGestureDownThisFrame(InteractionData eventData, GestureId gesture,
            GestureId secondaryGesture = GestureId.UNKNOWN)
        {
            var isLeft = eventData.Controller.Type == HandType.Left;
            var mainGesture = QCHTInput.GestureDown(isLeft, gesture);
            var otherGesture = secondaryGesture != GestureId.UNKNOWN && QCHTInput.GestureDown(isLeft, secondaryGesture);
            return mainGesture || otherGesture;
        }
        
        private static bool GetReleaseGestureDownThisFrame(InteractionData eventData, GestureId gesture,
            GestureId secondaryGesture = GestureId.UNKNOWN)
        {
            var isLeft = eventData.Controller.Type == HandType.Left;
            var hand = isLeft ? QCHTInput.LeftHand : QCHTInput.RightHand;
            var releaseThreshold = hand.GetMinDistToThumbTip();
            return releaseThreshold >= 0.048f;
        }

        private static QCHTHandInteractionController s_leftController;
        private static QCHTHandInteractionController s_rightController;

        public static void RegisterController(QCHTHandInteractionController controller, HandType handType)
        {
            switch (handType)
            {
                case HandType.Left:
                    s_leftController = controller;
                    s_leftInteractionData = new InteractionData(EventSystem.current) {Controller = s_leftController};
                    break;

                case HandType.Right:
                    s_rightController = controller;
                    s_rightInteractionData = new InteractionData(EventSystem.current)
                        {Controller = s_rightController};
                    break;
            }

            if (s_leftController && s_rightController)
            {
                s_doubleInteractionData ??= new DoubleInteractionData(EventSystem.current);
                s_doubleInteractionData.LeftData = s_leftInteractionData;
                s_doubleInteractionData.RightData = s_rightInteractionData;
            }
        }

        public static void UnRegisterController(HandType handType)
        {
            ref var controller = ref handType == HandType.Left ? ref s_leftController : ref s_rightController;
            ref var eventData = ref handType == HandType.Left ? ref s_leftInteractionData : ref s_rightInteractionData;

            // Stop grabbing
            if (eventData.ProximalGrab)
            {
                ExecuteEvents.ExecuteHierarchy(eventData.ProximalGrab, eventData,
                    ExecuteProximalEventsExtensions.endGrabHandler);
            }

            controller = null;
            eventData = null;

            if (s_leftController == null || s_rightController == null)
                s_doubleInteractionData = null;
        }

        #endregion
    }
}