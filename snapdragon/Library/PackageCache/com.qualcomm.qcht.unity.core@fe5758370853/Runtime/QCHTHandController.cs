#if ENABLE_INPUT_SYSTEM
// /******************************************************************************
//  * File: QCHTHandController.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace QCHT.Core
{
    public struct QCHTDeviceState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('Q', 'C', 'H', 'T');

        [Preserve, InputControl(name = "isTracked")]
        public bool isTracked;

        [Preserve, InputControl(name = "trackingState")]
        public int trackingState;

        [Preserve, InputControl(name = "devicePosition")]
        public Vector3 devicePosition;

        [Preserve, InputControl(name = "deviceRotation")]
        public Quaternion deviceRotation;

        [InputControl(name = "selectPressed", layout = "Button", aliases = new[] {"PrimaryButton", "triggerPressed"})]
        public bool selectPressed;

        [InputControl(name = "select", layout = "Axis", aliases = new[] {"Primary", "trigger"})]
        public float select;

        [InputControl(name = "gesture", layout = "Integer")]
        public int gesture;

        [InputControl(name = "pointerPosition")]
        public Vector3 pointerPosition;

        [InputControl(name = "pointerRotation")]
        public Quaternion pointerRotation;
    }

#if ENABLE_INPUT_SYSTEM
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = "QCHT hand", commonUsages = new[] {"LeftHand", "RightHand"},
        stateType = typeof(QCHTDeviceState))]
    public sealed class QCHTHandController : XRController
    {
        public AxisControl select { get; private set; }
        public ButtonControl selectPressed { get; private set; }
        public IntegerControl gestureId { get; private set; }
        public Vector3Control pointerPosition { get; private set; }
        public QuaternionControl pointerRotation { get; private set; }

        static QCHTHandController()
        {
            InputSystem.RegisterLayout<QCHTHandController>(
                matches:
                new InputDeviceMatcher()
                    .WithInterface(QCHTInput.kInterfaceName)
                    .WithProduct(QCHTInput.kDeviceName));
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            select = GetChildControl<AxisControl>("select");
            selectPressed = GetChildControl<ButtonControl>("selectPressed");
            gestureId = GetChildControl<IntegerControl>("gesture");
            pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
            pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer()
        {
        }
    }
}
#endif
#endif