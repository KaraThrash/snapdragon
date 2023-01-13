﻿/******************************************************************************
 * File: XrImageTrackerCreateInfoQCOM.cs
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
    internal struct XrImageTrackerCreateInfoQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _dataSetCount;
        private IntPtr /* XrImageTrackerDataSetImageQCOM */ _dataSets;
        private uint _imageCount;

        public XrImageTrackerCreateInfoQCOM(IntPtr dataSets, uint dataSetCount, uint imageCount) {
            _type = XrStructureType.XR_TYPE_IMAGE_TRACKER_CREATE_INFO_QCOM;
            _next = IntPtr.Zero;
            _dataSets = dataSets;
            _dataSetCount = dataSetCount;
            _imageCount = imageCount;
        }
    }
}