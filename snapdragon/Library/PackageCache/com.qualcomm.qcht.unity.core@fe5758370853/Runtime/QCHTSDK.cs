// /******************************************************************************
//  * File: QCHTSDK.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace QCHT.Core
{
    public class QCHTSDK
    {
        #region Instance

        private static QCHTSDK s_instance;

        public static QCHTSDK Instance
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                s_instance = Application.isEditor ? (QCHTSDK) new QCHTSDKEditor() : new QCHTSDKAndroid();

                return s_instance;
            }

            set => s_instance = value;
        }

        #endregion

        private const int StructSize = sizeof(int) + // Hand ID
                                       sizeof(int) + // Is Left
                                       sizeof(float) * 7 * 23 + // Points and Rotations
                                       sizeof(int) + // Gesture
                                       sizeof(float) + // Gesture Ratio
                                       sizeof(float) + // Scale
                                       sizeof(float); // Flip Ratio

        private static readonly byte[] s_buffer = new byte[StructSize * 2];

        public static QCHTSimulationMode simulationMode;

        protected readonly QCHTData _data = new QCHTData();

        public bool NeedDelay;

        public virtual void StartQcht()
        {
        }

        public virtual void StopQcht()
        {
        }

        public virtual void UpdateData()
        {
            var sdkStatus = ClaySDKGetStatus();
            if (sdkStatus < 0) return;

            var handCount = 0;
            var dataPtr = IntPtr.Zero;
            var success = ClaySDKGetData(ref dataPtr, ref handCount);
            if (!success) return;

            Marshal.Copy(dataPtr, s_buffer, 0, StructSize * handCount);
            _data.Update(s_buffer, StructSize, sdkStatus, handCount);
        }

        public QCHTData GetSDKData()
        {
            return _data;
        }

        public static QCHTData GetData()
        {
            return s_instance.GetSDKData();
        }

        [DllImport("claysdk")]
        private static extern bool ClaySDKGetData(ref IntPtr outData, ref int outSize);

        [DllImport("claysdk")]
        private static extern int ClaySDKGetStatus();
    }
}