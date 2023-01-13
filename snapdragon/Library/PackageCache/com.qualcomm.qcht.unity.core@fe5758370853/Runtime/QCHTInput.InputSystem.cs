#if ENABLE_INPUT_SYSTEM
// /******************************************************************************
//  * File: QCHTInput.InputSystem.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Linq;
using QCHT.Core.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;

namespace QCHT.Core
{
    public partial class QCHTInput
    {
        public const string kInterfaceName = "QCHTInput";
        public const string kDeviceName = "QCHTHand";

        private static bool s_leftHandWasDetected;
        private static bool s_rightHandWasDetected;

        private static void UpdateDevices()
        {
            var handDevices = InputSystem.devices.Where(x => x is QCHTHandController);

            foreach (var device in handDevices)
            {
                var isLeft = device.usages.Contains(CommonUsages.LeftHand);
                var hand = isLeft ? WorldLeftHand : WorldRightHand;

                // Calculates the pointer position and rotation
                const float offset = 0.15f;
                var positionOffset = new Vector3(0, 0, 0.0724000037f);
                var posOffset = QCHTManager.Instance.Head.rotation * hand.GetRotation(QCHTPointId.PALM_CENTER) *
                                positionOffset;
                var bottomHead = QCHTManager.Instance.Head.position - QCHTManager.Instance.Head.up * offset;
                var shoulderOffset = isLeft ? -QCHTManager.Instance.Head.right : QCHTManager.Instance.Head.right;
                var shoulderPosition = bottomHead + shoulderOffset * offset;
                var position = hand.GetPalmPosition() + posOffset;
                var direction = Vector3.Normalize(position - shoulderPosition);
                var pointerPosition = position;
                var pointerRotation = Quaternion.LookRotation(direction);

                var state = new QCHTDeviceState
                {
                    isTracked = hand.IsDetected,
                    trackingState = (int) (InputTrackingState.Position | InputTrackingState.Rotation),
                    selectPressed = Gesture(isLeft, GestureId.PINCH),
                    select = Gesture(isLeft, GestureId.PINCH) ? 1f : 0f,
                    gesture = (int) hand.gesture,
                    devicePosition = hand.GetWristPosition(),
                    deviceRotation = hand.GetWristRotation(),
                    pointerPosition = pointerPosition,
                    pointerRotation = pointerRotation
                };

                InputSystem.QueueStateEvent(device, state);
            }
        }

        private static void AddHandDevice(bool isLeft)
        {
            var characteristics = InputDeviceCharacteristics.HandTracking;
            characteristics |= isLeft ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
            InputSystem.AddDevice(new InputDeviceDescription
            {
                interfaceName = kInterfaceName,
                product = kDeviceName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = characteristics
                }.ToJson()
            });
        }

        private static void RemoveHandDevice(bool isLeft)
        {
            var characteristics = InputDeviceCharacteristics.HandTracking;
            characteristics |= isLeft ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
            var device = InputSystem.devices.FirstOrDefault(
                x => x.description == new InputDeviceDescription
                {
                    interfaceName = kInterfaceName,
                    product = kDeviceName,
                    capabilities = new XRDeviceDescriptor
                    {
                        characteristics = characteristics
                    }.ToJson()
                });

            if (device != null)
                InputSystem.RemoveDevice(device);
        }
    }
}
#endif