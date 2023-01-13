/******************************************************************************
 * File: PlaneDetectionFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class PlaneDetectionFeature
    {
        #region XR_QCOM_plane_detection bindings
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreatePlaneDetectionQCOMDelegate(ulong session, ref XrPlaneDetectionCreateInfoQCOM createInfo, ref ulong planeDetection);
        private static CreatePlaneDetectionQCOMDelegate _xrCreatePlaneDetectionQCOM;

        private delegate XrResult DestroyPlaneDetectionQCOMDelegate(ulong planeDetection);
        private static DestroyPlaneDetectionQCOMDelegate _xrDestroyPlaneDetectionQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult LocatePlanesQCOMDelegate(ulong planeDetection, ref XrPlanesLocateInfoQCOM locateInfo, ref XrPlaneLocationsQCOM locations);
        private static LocatePlanesQCOMDelegate _xrLocatePlanesQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult GetPlaneDetectioNStateQCOMDelegate(ulong planeDetection, ref XrPlaneDetectionState state);
        private static GetPlaneDetectioNStateQCOMDelegate _xrGetPlaneDetectionStateQCOM;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult xrGetPlaneConvexHullVertexBufferQCOMDelegate(ulong planeDetection, ref XrPlaneConvexHullBufferInfoQCOM convexHullInfo, ref XrPlaneConvexHullVertexBufferQCOM buffers);
        private static xrGetPlaneConvexHullVertexBufferQCOMDelegate _xrGetPlaneConvexHullVertexBufferQCOM;
        #endregion
    }
}