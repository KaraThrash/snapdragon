// /******************************************************************************
//  * File: QCHTSDKAndroid.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if AR_SDK_NREAL
using NRKernal;
#endif

namespace QCHT.Core
{
    public sealed class QCHTSDKAndroid : QCHTSDK
    {
        public QCHTSDKAndroid()
        {
            NeedDelay = true;
        }

        public override void StartQcht()
        {
            StartQCHTSDK();
        }

        public override void StopQcht()
        {
            StopQCHTSDK();
#if DEBUG
            QCHTSDKAndroidDebug.Instance.Destroy();
#endif
        }

#if AR_SDK_NREAL
        [DllImport(NativeConstants.NRNativeLibrary)]
        public static extern NativeResult NRHandTrackingCreate(ref UInt64 out_handtracking_handle);
        [DllImport(NativeConstants.NRNativeLibrary)]
        public static extern NativeResult NRHandTrackingStart(UInt64 handtracking_handle);
        [DllImport(NativeConstants.NRNativeLibrary)]
        public static extern NativeResult NRHandTrackingStop(UInt64 handtracking_handle);
        [DllImport(NativeConstants.NRNativeLibrary)]
        public static extern NativeResult NRHandTrackingDestroy(UInt64 handtracking_handle);

        private UInt64 _handtrackingHandle = 0;

        private void StartQCHTSDK()
        {
            NativeResult result = NRHandTrackingCreate(ref _handtrackingHandle);
            if (result == NativeResult.Success)
            {
                NRHandTrackingStart(_handtrackingHandle);
            }
            else
            {
                Debug.LogError("Unable to start Hand Tracking");
            }
        }

        private void StopQCHTSDK()
        {
            if (_handtrackingHandle == 0)
            {
                return;
            }

            NRHandTrackingStop(_handtrackingHandle);
            NRHandTrackingDestroy(_handtrackingHandle);
        }

#else
        [DllImport("claysdk")]
        private static extern bool ClaySDKStart(IntPtr jniEnv);

        [DllImport("claysdk")]
        private static extern bool ClaySDKStop();

        private void StartQCHTSDK()
        {
            ClaySDKStart(GetJNIEnv());
        }

        private void StopQCHTSDK()
        {
            ClaySDKStop();
        }
#endif

        private static IntPtr GetJNIEnv()
        {
            var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            return unityActivity.GetRawObject();
        }
    }
}