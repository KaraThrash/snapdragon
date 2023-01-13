/******************************************************************************
 * File: HandTrackingFeature.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new []{ BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Enables Hand Tracking and gestures feature on Snapdragon Spaces enabled devices.",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.9.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class HandTrackingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Hand Tracking";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.handtracking";
        public const string FeatureExtensions = "XR_EXT_hand_tracking XR_QCOM_hand_tracking_gesture";

        private ulong _leftHandTrackerHandle;
        internal ulong LeftHandTrackerHandle => _leftHandTrackerHandle;

        private ulong _rightHandTrackerHandle;
        internal ulong RightHandTrackerHandle => _rightHandTrackerHandle;

        private XrHandGestureQCOM _leftGesture = new XrHandGestureQCOM(-1, 0, 0f);
        private XrHandGestureQCOM _rightGesture = new XrHandGestureQCOM(-1, 0, 0f);
        protected override bool IsRequiringBaseRuntimeFeature => true;

        private BaseRuntimeFeature _baseRuntimeFeature;
        private static List<XRHandTrackingSubsystemDescriptor> _handTrackingSubsystemDescriptors = new List<XRHandTrackingSubsystemDescriptor>();

        protected override bool OnInstanceCreate(ulong instanceHandle) {
            base.OnInstanceCreate(instanceHandle);
            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any()) {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }
            // TODO GÖ: Add check for Spaces Services camera permissions.
            return true;
        }

        protected override void OnSubsystemCreate() => CreateSubsystem<XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem>(_handTrackingSubsystemDescriptors, HandTrackingSubsystem.ID);

        protected override void OnSubsystemStop() => StopSubsystem<XRHandTrackingSubsystem>();

        protected override void OnSubsystemDestroy() => DestroySubsystem<XRHandTrackingSubsystem>();

        protected override void OnHookMethods() {
            HookMethod("xrCreateHandTrackerEXT",out _xrCreateHandTrackerEXT);
            HookMethod("xrDestroyHandTrackerEXT", out _xrDestroyHandTrackerEXT);
            HookMethod("xrLocateHandJointsEXT", out _xrLocateHandJointsEXT);
            HookMethod("xrGetHandGestureQCOM",out _xrGetHandGestureQCOM);
        }

        public bool TryCreateHandTracking() {
            if (_xrCreateHandTrackerEXT == null) {
                Debug.LogError("xrCreateHandTrackerEXT method not found!");
                return false;
            }

            var leftCreateInfo = new XrHandTrackerCreateInfoEXT(XrHandEXT.XR_HAND_LEFT_EXT);
            var rightCreateInfo = new XrHandTrackerCreateInfoEXT(XrHandEXT.XR_HAND_RIGHT_EXT);

            var result = _xrCreateHandTrackerEXT(SessionHandle, ref leftCreateInfo, ref _leftHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to create left hand tracker: " + Enum.GetName(typeof(XrResult), result));
            }

            result = _xrCreateHandTrackerEXT(SessionHandle, ref rightCreateInfo, ref _rightHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to create right hand tracker: " + Enum.GetName(typeof(XrResult), result));
            }

            return result == XrResult.XR_SUCCESS;
        }

        public bool TryDestroyHandTracking() {
            if (_xrDestroyHandTrackerEXT == null) {
                Debug.LogError("xrDestroyHandTrackerEXT method not found!");
                return false;
            }

            var result = _xrDestroyHandTrackerEXT(_leftHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to destroy left hand tracker: " + Enum.GetName(typeof(XrResult), result));
            }
            result = _xrDestroyHandTrackerEXT(_rightHandTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to destroy right hand tracker: " + Enum.GetName(typeof(XrResult), result));
            }

            return result == XrResult.XR_SUCCESS;
        }

        public Tuple<List<Pose>, TrackingState> TryGetHandTrackingJointsAndTrackingState(bool forLeftHand) {
            var defaultReturn = new Tuple<List<Pose>, TrackingState>(new List<Pose>(), TrackingState.None);

            if (_xrLocateHandJointsEXT == null) {
                Debug.LogError("xrLocateHandJointsEXT method not found!");
                return defaultReturn;
            }

            var handTrackerHandle = forLeftHand ? _leftHandTrackerHandle : _rightHandTrackerHandle;

            if (handTrackerHandle == 0) {
                return defaultReturn;
            }

            var handJointsLocateInfoExt = new XrHandJointsLocateInfoEXT(0, _baseRuntimeFeature.PredictedDisplayTime);

            int size = Marshal.SizeOf(typeof(XrHandJointLocationEXT)) * (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT;
            IntPtr jointsPointer = Marshal.AllocHGlobal(size);

            var xrHandJointLocations = new XrHandJointLocationsEXT((int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT, jointsPointer);
            var result = _xrLocateHandJointsEXT(handTrackerHandle, ref handJointsLocateInfoExt, ref xrHandJointLocations);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Locate hand joints for " + (forLeftHand ? "left" : "right") + " hand failed: " + Enum.GetName(typeof(XrResult), result));
                return defaultReturn;
            }

            var joints = new List<Pose>();
            if (xrHandJointLocations.IsActive) {
                if (xrHandJointLocations.JointCount != (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT) {
                    Debug.LogError("Located fewer joints than requested. Returning empty joint list.");
                    return defaultReturn;
                }
                for (int i = 0; i < xrHandJointLocations.JointCount; i++) {
                    var joint = Marshal.PtrToStructure<XrHandJointLocationEXT>(xrHandJointLocations.JointLocations + Marshal.SizeOf(typeof(XrHandJointLocationEXT)) * i);
                    joints.Add(joint.pose.ToPose());
                }
            }
            Marshal.FreeHGlobal(jointsPointer);
            return new Tuple<List<Pose>, TrackingState>(joints, xrHandJointLocations.IsActive ? TrackingState.Tracking : TrackingState.None);
        }

        public Tuple<int, float, float>  TryGetHandGestureData(bool forLeftHand) {
            var defaultReturn = new Tuple<int, float, float>(-1, 0f, 0f);
            if (_xrGetHandGestureQCOM == null) {
                Debug.LogError("xrGetHandGestureQCOM method not found!");
                return defaultReturn;
            }

            var handTracker = forLeftHand ? LeftHandTrackerHandle : RightHandTrackerHandle;
            var receivedGesture = forLeftHand ? _leftGesture : _rightGesture;

            var result = _xrGetHandGestureQCOM(handTracker, _baseRuntimeFeature.PredictedDisplayTime, ref receivedGesture);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Get hand gesture for " + (forLeftHand ? "left" : "right") + " hand failed: " + Enum.GetName(typeof(XrResult), result));
                return defaultReturn;
            }

            return new Tuple<int, float, float>(receivedGesture.Gesture, receivedGesture.GestureRatio, receivedGesture.FlipRatio);
        }
    }
}