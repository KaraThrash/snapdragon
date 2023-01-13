// /******************************************************************************
//  * File: RecorderManager.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace QCHT.Core.PlaybackRecorder
{
    public sealed class RecorderManager : MonoBehaviour
    {
        [SerializeField] private string fileName = "QStudioData";
        [SerializeField] private bool logTime = true;
        [CanBeEmpty]
        [SerializeField] private Transform anchor;
        [Space]
        [SerializeField] private CustomEvent OnStartRecordingEvent;
        [SerializeField] private CustomEvent OnStopRecordingEvent;

        private QCHTSDKDataCsvRecorder _recorder;

        private string _outputPathFile;
        private float _timer = -1f;

        public bool IsRecording => _recorder != null && _recorder.IsRunning;
        public float RecordTime => _timer < 0f ? 0f : _timer;

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            OnStartRecordingEvent.Event.AddListener(StartRecording);
            OnStopRecordingEvent.Event.AddListener(StopRecording);
        }

        private void OnDisable()
        {
            OnStartRecordingEvent.Event.RemoveListener(StartRecording);
            OnStopRecordingEvent.Event.RemoveListener(StopRecording);
        }

        private void FixedUpdate()
        {
            if (_recorder == null)
                return;
            
            if (!_recorder.IsRunning)
                return;

            _recorder.RecordData(QCHTSDK.Instance.GetSDKData(), QCHTManager.Instance.Head, anchor);
        }

        private void Update()
        {
            if(IsRecording)
                _timer += Time.deltaTime;
        }

        #endregion

        private void InitRecorder()
        {
            var directory = Application.persistentDataPath;

#if UNITY_ANDROID && !UNITY_EDITOR
            directory = QCHTTools.GetAndroidExternalFilesDir();
#endif
            var now = DateTime.Now;
            var outputPath = $"{directory}/{now:yyyyMMdd}/{now:HHmmss}";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            _outputPathFile = $"{outputPath}/{fileName}.csv";
            _recorder = new QCHTSDKDataCsvRecorder(_outputPathFile);
        }

        public void StartRecording()
        {
            if (_recorder == null)
            {
                InitRecorder();
            }

            _recorder.Start();
            _timer = -1f;
        }

        public void StopRecording()
        {
            if (_recorder == null)
                return;

            _recorder.Stop();

            if (logTime)
            {
                LogTime();
            }

            _timer = -1f;
        }

        private void LogTime()
        {
            using var logFile = File.CreateText(Path.ChangeExtension(_outputPathFile, ".log"));
            var sb = new StringBuilder();
            var timeSpan = TimeSpan.FromSeconds(_timer);
            sb.Append($"Record Time : {timeSpan:ss\\.ff}");
            logFile.Write(sb);
            logFile.Close();
        }
    }
}