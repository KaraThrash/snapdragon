// /******************************************************************************
//  * File: QCHTSDKDataCsvReader.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QCHT.Core
{
    public sealed class QCHTSDKDataCsvReader
    {
        private const string SPLIT_REG_EX = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string LINE_SPLIT_REG_EX = @"\r\n|\n\r|\n|\r";
        private const int HAND_COLUMN_COUNT = 23 * 7 + 6;

        private bool _isRunning;
        private string _csvFilePath;
        private string[] _csvFrames;
        private int _frameIndex;
        private float[] _frameData = new float[2 + HAND_COLUMN_COUNT * 2];
        private float[] _headData = new float[7];
        private float[] _anchorData = new float[7];

        private float _previousTimeStamp;
        private float _previousUpdateTime;

        public Pose CurrentHeadPose
        {
            get
            {
                if (_headData.Length == 7)
                {
                    return new Pose(new Vector3(_headData[0], _headData[1], _headData[2]),
                        new Quaternion(_headData[3], _headData[4], _headData[5], _headData[6]));
                }

                return Pose.identity;
            }
        }
        
        public Pose CurrentAnchorPose
        {
            get
            {
                if (_anchorData.Length == 7)
                {
                    return new Pose(new Vector3(_anchorData[0], _anchorData[1], _anchorData[2]),
                        new Quaternion(_anchorData[3], _anchorData[4], _anchorData[5], _anchorData[6]));
                }

                return Pose.identity;
            }
        }

        #region Singleton

        private static QCHTSDKDataCsvReader _instance;
        public static QCHTSDKDataCsvReader Instance => _instance ??= new QCHTSDKDataCsvReader();

        #endregion

        #region Static APIs

        public static void StartReplay(string filePath, float replaySpeed)
        {
            Instance.Start(filePath, replaySpeed);
        }

        public static void StopReplay()
        {
            Instance.Stop();
        }

        public static void UpdateReplay()
        {
            Instance.UpdateData();
        }

        #endregion

        public void Start(string filePath, float replaySpeed)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(
                    "CSV File path not provided. Please check the path under QchtManager object in Unity Editor");
            }

            _csvFilePath = filePath;
            var csvFile = System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(_csvFilePath));
            _csvFrames = Regex.Split(csvFile, LINE_SPLIT_REG_EX);
            // Debug.Log($"{_csvFrames.Length - 1} frames loaded from csv file at {_csvFilePath}");
            _isRunning = true;
            _frameIndex = 1;
        }

        public void Stop()
        {
            _isRunning = false;
            _frameIndex = 1;
        }

        public void UpdateData()
        {
            if (!_isRunning)
                return;

            if (_csvFrames == null || _csvFrames.Length == 0 || _frameIndex >= _csvFrames.Length)
            {
                Stop();
                return;
            }

            ReadCSVFrame(_csvFrames[_frameIndex], ref _frameData, ref _headData, ref _anchorData);
            var data = QCHTSDK.GetData();
            data.Update(_frameData, _frameData.Length);
            ++_frameIndex;
        }

        private float ReadCSVFrame(string frame, ref float[] qchtData, ref float[] headPoseData, ref float[] anchorPoseData)
        {
            var dataIndex = 0;
            float frameTimeStamp = 0;
            var values = frame.Split(',');
            if (values.Length == 0 || values[0] == string.Empty) return frameTimeStamp;
            float.TryParse(values[dataIndex++], NumberStyles.Float, CultureInfo.InvariantCulture, out frameTimeStamp);


            for (var i = 0; i < 7; i++)
            {
                float param;
                float.TryParse(values[dataIndex++], NumberStyles.Float, CultureInfo.InvariantCulture, out param);
                headPoseData[i] = param;
            }
            
            for (var i = 0; i < 7; i++)
            {
                float param;
                float.TryParse(values[dataIndex++], NumberStyles.Float, CultureInfo.InvariantCulture, out param);
                anchorPoseData[i] = param;
            }
            

            int handCount;
            int qchtIndex = 0;
            int.TryParse(values[dataIndex++], out handCount);
            qchtData[qchtIndex++] = handCount > 0 ? 1 : 0; //Detection state
            qchtData[qchtIndex++] = handCount; // hand count

            for (var i = dataIndex; dataIndex < values.Length; dataIndex++)
            {
                float param;
                float.TryParse(values[dataIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out param);
                qchtData[qchtIndex++] = param;
            }

            return frameTimeStamp;
        }
    }
}