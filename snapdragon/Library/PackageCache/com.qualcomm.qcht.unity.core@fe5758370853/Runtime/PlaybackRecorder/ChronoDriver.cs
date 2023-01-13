// /******************************************************************************
//  * File: ChronoDriver.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core.PlaybackRecorder;
using UnityEngine.UI;
using UnityEngine;

namespace QCHT.Core
{
    public sealed class ChronoDriver : MonoBehaviour
    {
        [SerializeField] private Text chronoLabel;
        [SerializeField] private RecorderManager recorderManager;
        
        private void Update()
        {
            chronoLabel.text = FormatTime(recorderManager.RecordTime);
        }

        private static string FormatTime(float timeToDisplay)
        {
            var minutes = Mathf.FloorToInt(timeToDisplay / 60);
            var seconds = Mathf.FloorToInt(timeToDisplay % 60);
            var milliSeconds = timeToDisplay % 1 * 1000;
            return $"{minutes:00} : {seconds:00} : {milliSeconds:000}";
        }
    }
}