/******************************************************************************
 * File: BaseRuntimeFeature.InterceptWaitFrame.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using AOT;
using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        private static long _predictedDisplayTime = 0;
        public long PredictedDisplayTime => _predictedDisplayTime;

        [DllImport("libInterceptWaitFrame", EntryPoint = "GetInterceptedInstanceProcAddr")]
        private static extern IntPtr GetInterceptedInstanceProcAddr(IntPtr func);

        internal delegate void ReceiveFrameStateCallback(XrFrameState frameState);

        [DllImport("libInterceptWaitFrame", EntryPoint = "SetFrameStateCallback")]
        private static extern void SetFrameStateCallback(ReceiveFrameStateCallback callback);

        [MonoPInvokeCallback(typeof(ReceiveFrameStateCallback))]
        private static void OnFrameStateUpdate(XrFrameState frameState) {
            _predictedDisplayTime = frameState.PredictedDisplayTime;
        }

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func) {
            return GetInterceptedInstanceProcAddr(func);
        }
    }
}