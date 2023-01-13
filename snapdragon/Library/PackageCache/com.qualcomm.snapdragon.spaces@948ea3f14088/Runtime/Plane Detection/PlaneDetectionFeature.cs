/******************************************************************************
 * File: PlaneDetectionFeature.cs
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
        Desc = "Enables Plane Detection feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.9.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    internal sealed partial class PlaneDetectionFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Plane Detection";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.planedetection";
        public const string FeatureExtensions = "XR_QCOM_plane_detection";

        public bool IsRunning => _activePlaneDetectionHandle != 0;

        protected override bool IsRequiringBaseRuntimeFeature => true;

        private BaseRuntimeFeature _baseRuntimeFeature;
        private static List<XRPlaneSubsystemDescriptor> _planeSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();

        private ulong _activePlaneDetectionHandle;
        private List<string> _subscribedSubsystems;

        protected override bool OnInstanceCreate(ulong instanceHandle) {
            base.OnInstanceCreate(instanceHandle);

            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            _subscribedSubsystems = new List<string>();

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any()) {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
                Debug.LogError("The Plane Detection Feature is missing the camera permissions and can't be created therefore!");
                return false;
            }
#endif
            return true;
        }

        protected override void OnSubsystemCreate() => CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(_planeSubsystemDescriptors, PlaneDetectionSubsystem.ID);

        protected override void OnSubsystemStop() => StopSubsystem<XRPlaneSubsystem>();

        protected override void OnSubsystemDestroy() => DestroySubsystem<XRPlaneSubsystem>();

        protected override void OnHookMethods() {
            HookMethod("xrCreatePlaneDetectionQCOM", out _xrCreatePlaneDetectionQCOM);
            HookMethod("xrDestroyPlaneDetectionQCOM", out _xrDestroyPlaneDetectionQCOM);
            HookMethod("xrLocatePlanesQCOM", out _xrLocatePlanesQCOM);
            HookMethod("xrGetPlaneDetectionStateQCOM", out _xrGetPlaneDetectionStateQCOM);
            HookMethod("xrGetPlaneConvexHullVertexBufferQCOM", out _xrGetPlaneConvexHullVertexBufferQCOM);
        }

        private XrPlaneFilterQCOM PlaneDetectionModeToXrPlaneFilter(PlaneDetectionMode planeDetectionMode) {
            switch (planeDetectionMode) {
                case PlaneDetectionMode.Horizontal:
                    return XrPlaneFilterQCOM.XR_PLANE_FILTER_HORIZONTAL_QCOM;
                case PlaneDetectionMode.Vertical:
                    return XrPlaneFilterQCOM.XR_PLANE_FILTER_VERTICAL_QCOM;
                default:
                    return XrPlaneFilterQCOM.XR_PLANE_FILTER_ANY_QCOM;
            }
        }

        public ulong TryCreatePlaneDetection(string subsystemID, PlaneDetectionMode planeDetectionMode, bool enableConvexHull) {
            if (_xrCreatePlaneDetectionQCOM == null) {
                Debug.LogError("XrCreatePlaneDetectionQCOM method not found!");
                return 0;
            }

            if (planeDetectionMode == PlaneDetectionMode.None) {
                Debug.LogWarning("Won't create Plane Detection feature if PlaneDetectionMode is 'None'!");
                return 0;
            }

            if (_subscribedSubsystems.Contains(subsystemID)) {
                Debug.LogWarning("Won't create Plane Detection for subsystem " + subsystemID + " , because it already created it before.");
                return 0;
            }
            _subscribedSubsystems.Add(subsystemID);

            if (subsystemID != PlaneDetectionSubsystem.ID && subsystemID != RaycastSubsystem.ID) {
                Debug.LogWarning("Won't create Plane Detection for unsupported subsystem " + subsystemID);
                return 0;
            }

            if (_activePlaneDetectionHandle != 0 && _subscribedSubsystems.Count != 0) {
                if (_subscribedSubsystems[0] == PlaneDetectionSubsystem.ID && subsystemID == RaycastSubsystem.ID) {
                    Debug.LogWarning("The Plane Detection subsystem was started before the Ray Casting subsystem. The hit results depend on what was selected for the previous one. To get the best hit results, please ensure that the Plane Detection subsystem was started with convex hull for planes enabled.");
                    return _activePlaneDetectionHandle;
                }

                if (_subscribedSubsystems[0] == RaycastSubsystem.ID && subsystemID == PlaneDetectionSubsystem.ID) {
                    Debug.LogWarning("The Ray Casting subsystem was started before the Plane Detection subsystem. The underlying Plane Detection dependency will be restarted for the later one and the hit results depend on what was selected for that subsystem. To get the best hit results, please ensure that the Plane Detection subsystem was started with convex hull for planes enabled.");
                    TryDestroyPlaneDetection();
                }
            }

            if (_xrCreatePlaneDetectionQCOM != null) {
                var planeFilter = PlaneDetectionModeToXrPlaneFilter(planeDetectionMode);
                var planeDetecionCreateInfo = new XrPlaneDetectionCreateInfoQCOM(planeFilter, enableConvexHull);

                var callResult = _xrCreatePlaneDetectionQCOM(SessionHandle, ref planeDetecionCreateInfo, ref _activePlaneDetectionHandle);
                if (callResult != XrResult.XR_SUCCESS) {
                    Debug.LogError("Creating Plane Detection failed with error: " + Enum.GetName(typeof(XrResult), callResult));
                }
            }
            return _activePlaneDetectionHandle;
        }

        public bool TryDestroyPlaneDetection(string subsystemID) {
            _subscribedSubsystems.Remove(subsystemID);
            if (_subscribedSubsystems.Count > 1) {
                Debug.LogWarning("Plane Detection is still needed and won't be destroyed!");
                return false;
            }
            return TryDestroyPlaneDetection();
        }

        private bool TryDestroyPlaneDetection() {
            if (_xrDestroyPlaneDetectionQCOM == null) {
                Debug.LogError("XrDestroyPlaneDetectionQCOM method not found!");
                return false;
            }

            XrResult result = _xrDestroyPlaneDetectionQCOM(_activePlaneDetectionHandle);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to destroy Plane Detection");
                return false;
            }
            _activePlaneDetectionHandle = 0;
            return true;
        }

        public bool TryLocatePlanes(out List<Plane> updatedPlanes) {
            updatedPlanes = new List<Plane>();

            if (_xrLocatePlanesQCOM == null) {
                Debug.LogError("XrLocatePlanesQCOM method not found!");
                return false;
            }

            XrPlanesLocateInfoQCOM locateInfo = new XrPlanesLocateInfoQCOM(SpaceHandle, _baseRuntimeFeature.PredictedDisplayTime);

            int planeCountHandleSize = Marshal.SizeOf(typeof(uint));
            IntPtr planeCountOutputPtr = Marshal.AllocHGlobal(planeCountHandleSize);

            XrPlaneLocationsQCOM locations = new XrPlaneLocationsQCOM(planeCountOutputPtr);

            XrResult result = _xrLocatePlanesQCOM(_activePlaneDetectionHandle, ref locateInfo, ref locations);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get plane count output: " + result.ToString());
                Marshal.FreeHGlobal(planeCountOutputPtr);
                return false;
            }
            uint planeLocationsCount = Marshal.PtrToStructure<uint>(locations.PlaneCountOutput);

            int planeLocationHandleSize = Marshal.SizeOf(typeof(XrPlaneLocationQCOM));
            IntPtr planeLocationsPtr = Marshal.AllocHGlobal(planeLocationHandleSize * (int)planeLocationsCount);

            locations = new XrPlaneLocationsQCOM(planeLocationsCount, planeCountOutputPtr, planeLocationsPtr);
            result = _xrLocatePlanesQCOM(_activePlaneDetectionHandle, ref locateInfo, ref locations);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get plane locations: " + result);
                Marshal.FreeHGlobal(planeLocationsPtr);
                Marshal.FreeHGlobal(planeCountOutputPtr);
                return false;
            }

            uint planeCountOutput = Marshal.PtrToStructure<uint>(planeCountOutputPtr);
            Marshal.FreeHGlobal(planeCountOutputPtr);

            for (int i = 0; i < planeCountOutput; i++) {
                IntPtr planeLocationPtr = planeLocationsPtr + Marshal.SizeOf(typeof(XrPlaneLocationQCOM)) * i;
                var planeLocation = Marshal.PtrToStructure<XrPlaneLocationQCOM>(planeLocationPtr);

                updatedPlanes.Add(new Plane(planeLocation.GetBoundedPlane(_activePlaneDetectionHandle), planeLocation.ConvexHullId));
            }

            Marshal.FreeHGlobal(planeLocationsPtr);

            return true;
        }

        public bool TryGetPlaneDetectionState() {
            if (_xrGetPlaneDetectionStateQCOM == null) {
                Debug.LogError("XrGetPlaneDetectionStateQCOM method not found!");
                return false;
            }

            XrPlaneDetectionState state = XrPlaneDetectionState.XR_PLANE_DETECTION_STATE_NONE_QCOM;
            XrResult result = _xrGetPlaneDetectionStateQCOM(_activePlaneDetectionHandle, ref state);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get plane detection state : " + result);
                return false;
            }

            if (state != XrPlaneDetectionState.XR_PLANE_DETECTION_STATE_TRACKING_QCOM) {
                return false;
            }

            return true;
        }

        public bool TryGetPlaneConvexHullVertexBuffer(ulong convexHullBufferId, ref List<Vector2> vertexPositions) {
            if (_xrGetPlaneConvexHullVertexBufferQCOM == null) {
                Debug.LogError("XrGetPlaneConvexHullVertexBufferQCOM method not found!");
                return false;
            }

            XrPlaneConvexHullBufferInfoQCOM convexHullInfo = new XrPlaneConvexHullBufferInfoQCOM(convexHullBufferId);

            IntPtr vertexCapacityOutputPtr = Marshal.AllocHGlobal(Marshal.SizeOf<uint>());
            XrPlaneConvexHullVertexBufferQCOM vertexBuffer = new XrPlaneConvexHullVertexBufferQCOM(vertexCapacityOutputPtr);

            XrResult result = _xrGetPlaneConvexHullVertexBufferQCOM(_activePlaneDetectionHandle, ref convexHullInfo, ref vertexBuffer);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get convex hull buffer count output: " + result);
                Marshal.FreeHGlobal(vertexCapacityOutputPtr);
                return false;
            }

            uint vertexCapacityInput = Marshal.PtrToStructure<uint>(vertexCapacityOutputPtr);

            IntPtr verticesPtr = Marshal.AllocHGlobal(Marshal.SizeOf<XrVector3f>() * (int)vertexCapacityInput);
            vertexBuffer = new XrPlaneConvexHullVertexBufferQCOM(vertexCapacityInput, vertexCapacityOutputPtr, verticesPtr);

            result = _xrGetPlaneConvexHullVertexBufferQCOM(_activePlaneDetectionHandle, ref convexHullInfo, ref vertexBuffer);
            if (result != XrResult.XR_SUCCESS) {
                Debug.LogError("Failed to get convex hull vertex buffer: " + result);
                Marshal.FreeHGlobal(vertexCapacityOutputPtr);
                Marshal.FreeHGlobal(verticesPtr);
                return false;
            }

            uint vertexCount = Marshal.PtrToStructure<uint>(vertexCapacityOutputPtr);
            Marshal.FreeHGlobal(vertexCapacityOutputPtr);

            /* NOTE(AF): Traverse the OpenXR in inverse order because changing the coordinate system handedness
             * also changes the winding order. Without this, the plane meshes would face the wrong way.
             */
            for (int i = (int)vertexCount - 1; i >= 0; i--) {
                IntPtr vertexPtr = verticesPtr + Marshal.SizeOf<XrVector3f>() * i;
                Vector3 vertexPosition = Marshal.PtrToStructure<XrVector3f>(vertexPtr).ToVector3();

                /* NOTE(AF): Flip the Y position to account for the conversion from OpenXR to Unity coordinate system. */
                vertexPositions.Add(new Vector2(vertexPosition.x, -vertexPosition.y));
            }
            Marshal.FreeHGlobal(verticesPtr);

            return true;
        }
    }
}