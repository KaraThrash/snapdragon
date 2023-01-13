/******************************************************************************
 * File: ImageTrackingFeature.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
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
        Desc = "Enables Image Tracking feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.9.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    public sealed partial class ImageTrackingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Image Tracking";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.imagetracking";
        public const string FeatureExtensions = "XR_QCOM_image_tracking";

        public bool ExtendedRangeMode = false;
        public bool LowPowerMode = false;

        protected override bool IsRequiringBaseRuntimeFeature => true;
        private BaseRuntimeFeature _baseRuntimeFeature;

        private static List<XRImageTrackingSubsystemDescriptor> _imageTrackingSubsystemDescriptors = new List<XRImageTrackingSubsystemDescriptor>();

        private Dictionary<string, XRReferenceImage> _trackablesNameToSourceXRReferenceImages = new Dictionary<string, XRReferenceImage>();

        protected override bool OnInstanceCreate(ulong instanceHandle) {
            base.OnInstanceCreate(instanceHandle);

            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any()) {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
                Debug.LogError("The Image Tracking Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif
            return true;
        }

        protected override void OnSubsystemCreate() => CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(_imageTrackingSubsystemDescriptors, ImageTrackingSubsystem.ID);

        protected override void OnSubsystemStop() => StopSubsystem<XRImageTrackingSubsystem>();

        protected override void OnSubsystemDestroy() => DestroySubsystem<XRImageTrackingSubsystem>();

        protected override void OnHookMethods() {
            HookMethod("xrCreateImageTrackerQCOM"    , out _xrCreateImageTrackerQCOM);
            HookMethod("xrDestroyImageTrackerQCOM"   , out _xrDestroyImageTrackerQCOM);
            HookMethod("xrLocateImageTargetsQCOM"    , out _xrLocateImageTargetsQCOM);
            HookMethod("xrImageTargetToNameAndIdQCOM", out _xrImageTargetToNameAndIdQCOM);
        }

        public bool TryCreateImageTracker(out ulong imageTrackerHandle, RuntimeReferenceImageLibrary imageLibrary, int maxNumberOfMovingImages) {
            imageTrackerHandle = 0;

            if (_xrCreateImageTrackerQCOM == null) {
                Debug.LogError("XrCreateImageTrackerQCOM method not found!");
                return false;
            }

            /* Set optimization hints */
            if (_xrSetTrackingOptimizationSettingsHintQCOM != null) {
                if (LowPowerMode) {
                    _xrSetTrackingOptimizationSettingsHintQCOM(SessionHandle,
                        XrTrackingOptimizationSettingsDomainQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_DOMAIN_IMAGE_TRACKING_QCOM,
                        XrTrackingOptimizationSettingsHintQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_HINT_LOW_POWER_PRIORIZATION_QCOM);
                }
                if (ExtendedRangeMode) {
                    _xrSetTrackingOptimizationSettingsHintQCOM(SessionHandle,
                        XrTrackingOptimizationSettingsDomainQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_DOMAIN_IMAGE_TRACKING_QCOM,
                        XrTrackingOptimizationSettingsHintQCOM.XR_TRACKING_OPTIMIZATION_SETTINGS_HINT_LONG_RANGE_PRIORIZATION_QCOM);
                }
            }

            /* Create Array of XrImageTrackerDataSetImageQCOM */
            IntPtr[] dataSetArray = new IntPtr[imageLibrary.count];
            IntPtr[] tempTextureBufferPtrs = new IntPtr[imageLibrary.count];

            for (int i = 0; i < imageLibrary.count; i++) {
                Debug.Log("Creating Image target: " + imageLibrary[i].name);
                if (!_trackablesNameToSourceXRReferenceImages.ContainsKey(imageLibrary[i].name)) {
                    _trackablesNameToSourceXRReferenceImages.Add(imageLibrary[i].name, imageLibrary[i]);
                }

                NativeArray<byte> nativeTextureBuffer = imageLibrary[i].texture.GetRawTextureData<byte>();
                tempTextureBufferPtrs[i] = Marshal.AllocHGlobal(nativeTextureBuffer.Length);
                Marshal.Copy(nativeTextureBuffer.ToArray(), 0, tempTextureBufferPtrs[i], nativeTextureBuffer.Length);

                var dataSetImage = new XrImageTrackerDataSetImageQCOM(
                    imageLibrary[i].name,
                    imageLibrary[i].height,
                    new XrVector2f(imageLibrary[i].texture.width, imageLibrary[i].texture.height),
                    tempTextureBufferPtrs[i],
                    (uint)nativeTextureBuffer.Length);

                dataSetArray[i] = Marshal.AllocHGlobal(Marshal.SizeOf<XrImageTrackerDataSetImageQCOM>());
                Marshal.StructureToPtr(dataSetImage, dataSetArray[i], false);
            }

            GCHandle pinnedDataSetArray = GCHandle.Alloc(dataSetArray, GCHandleType.Pinned);
            IntPtr dataSetPtr = pinnedDataSetArray.AddrOfPinnedObject();

            /* Create XrImageTrackerCreateInfoQCOM */
            XrImageTrackerCreateInfoQCOM createInfo = new XrImageTrackerCreateInfoQCOM(dataSetPtr, (uint)imageLibrary.count, (uint)maxNumberOfMovingImages);

            /* Create Image Tracker */
            XrResult result = _xrCreateImageTrackerQCOM(SessionHandle, ref createInfo, ref imageTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to create Image Tracker: " + result);
                pinnedDataSetArray.Free();
                for (int i = 0; i < imageLibrary.count; i++) {
                    Marshal.FreeHGlobal(tempTextureBufferPtrs[i]);
                    Marshal.FreeHGlobal(dataSetArray[i]);
                }
                return false;
            }

            pinnedDataSetArray.Free();
            for (int i = 0; i < imageLibrary.count; i++) {
                Marshal.FreeHGlobal(tempTextureBufferPtrs[i]);
                Marshal.FreeHGlobal(dataSetArray[i]);
            }
            return true;
        }

        public bool TryDestroyImageTracker(ulong imageTrackerHandle) {
            if (_xrDestroyImageTrackerQCOM == null) {
                Debug.LogError("XrDestroyImageTrackerQCOM method not found!");
                return false;
            }

            XrResult result = _xrDestroyImageTrackerQCOM(imageTrackerHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to destroy ImageTracker");
                return false;
            }

            _trackablesNameToSourceXRReferenceImages.Clear();
            return true;
        }

        public bool TryLocateImageTargets(ulong imageTracker, int imageCount, out List<XRTrackedImage> updatedTrackedImages) {
            updatedTrackedImages = new List<XRTrackedImage>();
            if (_xrLocateImageTargetsQCOM == null) {
                Debug.LogError("XrLocateImageTargetsQCOM method not found!");
                return false;
            }

            int imageTargetlocationSize = Marshal.SizeOf<XrImageTargetLocationQCOM>();
            IntPtr imageTargetLocationsPtr = Marshal.AllocHGlobal(imageTargetlocationSize * imageCount);

            XrImageTargetLocationsQCOM locations = new XrImageTargetLocationsQCOM((uint)imageCount, imageTargetLocationsPtr);
            XrImageTargetsLocateInfoQCOM locateInfo = new XrImageTargetsLocateInfoQCOM(SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime);
            XrResult result = _xrLocateImageTargetsQCOM(imageTracker, ref locateInfo, ref locations);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to locate image targets: " + result);
                Marshal.FreeHGlobal(imageTargetLocationsPtr);
                return false;
            }

            Debug.Log("Located " + locations.Count + " image targets");
            for (int i = 0; i < locations.Count; i++) {
                IntPtr imageLocationPtr = imageTargetLocationsPtr + Marshal.SizeOf<XrImageTargetLocationQCOM>() * i;
                var imageLocation = Marshal.PtrToStructure<XrImageTargetLocationQCOM>(imageLocationPtr);


                if (TryGetImageTargetNameAndId(imageLocation.ImageTargetHandle, out string identifier, out uint id)) {
                    updatedTrackedImages.Add(new XRTrackedImage(
                        new TrackableId(imageTracker, id),
                        _trackablesNameToSourceXRReferenceImages[identifier].guid,
                        imageLocation.XrPose.ToPose(),
                        _trackablesNameToSourceXRReferenceImages[identifier].size,
                        TrackingState.Tracking,
                        IntPtr.Zero));

                    Debug.Log("Located name: " + identifier + " id: " + id);
                }
            }

            Marshal.FreeHGlobal(imageTargetLocationsPtr);

            return true;
        }

        private bool TryGetImageTargetNameAndId(ulong imageTarget, out string identifier, out uint id) {
            identifier = null;
            id = 0;
            if (_xrImageTargetToNameAndIdQCOM == null) {
                Debug.LogError("XrImageTargetToNameAndIdQCOM method not found!");
                return false;
            }

            uint bufferCountOutput = 0;

            XrResult result = _xrImageTargetToNameAndIdQCOM(
                imageTarget,
                0,
                ref bufferCountOutput,
                IntPtr.Zero,
                ref id);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get name buffer size");
                return false;
            }

            IntPtr bufferPtr = Marshal.AllocHGlobal((int)bufferCountOutput);

            result = _xrImageTargetToNameAndIdQCOM(
                imageTarget,
                bufferCountOutput,
                ref bufferCountOutput,
                bufferPtr,
                ref id);
            if (result != XrResult.XR_SUCCESS) {
                Marshal.FreeHGlobal(bufferPtr);
                Debug.LogError("Failed to get name buffer size");
                return false;
            }

            identifier = Marshal.PtrToStringAnsi(bufferPtr);

            Marshal.FreeHGlobal(bufferPtr);
            return true;
        }
    }
}