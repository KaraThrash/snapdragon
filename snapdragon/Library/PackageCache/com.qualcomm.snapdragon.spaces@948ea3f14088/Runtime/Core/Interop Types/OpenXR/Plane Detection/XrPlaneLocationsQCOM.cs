/******************************************************************************
 * File: XrPlaneLocationsQCOM.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrPlaneLocationsQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _planeCapacityInput;
        private IntPtr /*uint*/ _planeCountOutput;
        private IntPtr /*XrPlaneLocationQCOM*/ _planeLocations;

        public XrPlaneLocationsQCOM(IntPtr planeCountOutput) {
            _type = XrStructureType.XR_TYPE_PLANE_LOCATIONS_QCOM;
            _next = IntPtr.Zero;
            _planeCapacityInput = 0;
            _planeCountOutput = planeCountOutput;
            _planeLocations = IntPtr.Zero;
        }

        public XrPlaneLocationsQCOM(uint planeCapacityInput, IntPtr planeCountOutput, IntPtr planeLocations) {
            _type = XrStructureType.XR_TYPE_PLANE_LOCATIONS_QCOM;
            _next = IntPtr.Zero;
            _planeCapacityInput = planeCapacityInput;
            _planeCountOutput = planeCountOutput;
            _planeLocations = planeLocations;
        }

        public IntPtr PlaneLocations => _planeLocations;
        public IntPtr PlaneCountOutput => _planeCountOutput;
    }
}