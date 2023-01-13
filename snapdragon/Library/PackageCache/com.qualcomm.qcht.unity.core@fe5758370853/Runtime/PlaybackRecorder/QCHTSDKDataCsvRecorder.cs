// /******************************************************************************
//  * File: QCHTSDKDataCsvRecorder.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace QCHT.Core.PlaybackRecorder
{
    public sealed class QCHTSDKDataCsvRecorder
    {
        private string _csvFilePath;
        private StreamWriter _writter;
        private StringBuilder _builder;
        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
        }

        public QCHTSDKDataCsvRecorder(string filePath)
        {
            _csvFilePath = filePath;
        }

        public void Start()
        {
            if (_isRunning) return;
            _writter = File.CreateText(_csvFilePath);
            _builder = new StringBuilder();
            _builder.AppendLine("TimeStamp,HeadPose_TX,HeadPose_TY,HeadPose_TZ,HeadPose_RX,HeadPose_RY,HeadPose_RZ,HeadPose_RW,Anchor_TX,Anchor_TY,Anchor_TZ,Anchor_RX,Anchor_RY,Anchor_RZ,Anchor_RW,handcount,id,isLeft,tp.x,tp.y,tp.z,td.x,td.y,td.z,tt.x,tt.y,tt.z,im.x,im.y,im.z,ip.x,ip.y,ip.z,id.x,id.y,id.z,it.x,it.y,it.z,mm.x,mm.y,mm.z,mp.x,mp.y,mp.z,md.x,md.y,md.z,mt.x,mt.y,mt.z,rm.x,rm.y,rm.z,rp.x,rp.y,rp.z,rd.x,rd.y,rd.z,rt.x,rt.y,rt.z,pm.x,pm.y,pm.z,pp.x,pp.y,pp.z,pd.x,pd.y,pd.z,pt.x,pt.y,pt.z,pc.x,pc.y,pc.z,ws.x,ws.y,ws.z,we.x,we.y,we.z,wc.x,wc.y,wc.z,r_tp.x,r_tp.y,r_tp.z,r_tp.w,r_td.x,r_td.y,r_td.z,r_td.w,r_tt.x,r_tt.y,r_tt.z,r_tt.w,r_im.x,r_im.y,r_im.z,r_im.w,r_ip.x,r_ip.y,r_ip.z,r_ip.w,r_id.x,r_id.y,r_id.z,r_id.w,r_it.x,r_it.y,r_it.z,r_it.w,r_mm.x,r_mm.y,r_mm.z,r_mm.w,r_mp.x,r_mp.y,r_mp.z,r_mp.w,r_md.x,r_md.y,r_md.z,r_md.w,r_mt.x,r_mt.y,r_mt.z,r_mt.w,r_rm.x,r_rm.y,r_rm.z,r_rm.w,r_rp.x,r_rp.y,r_rp.z,r_rp.w,r_rd.x,r_rd.y,r_rd.z,r_rd.w,r_rt.x,r_rt.y,r_rt.z,r_rt.w,r_pm.x,r_pm.y,r_pm.z,r_pm.w,r_pp.x,r_pp.y,r_pp.z,r_pp.w,r_pd.x,r_pd.y,r_pd.z,r_pd.w,r_pt.x,r_pt.y,r_pt.z,r_pt.w,r_pc.x,r_pc.y,r_pc.z,r_pc.w,r_ws.x,r_ws.y,r_ws.z,r_ws.w,r_we.x,r_we.y,r_we.z,r_we.w,r_wc.x,r_wc.y,r_wc.z,r_wc.w,gesture, gestureRatio, scale, flipRatio");
            _isRunning = true;
            Debug.Log("CSV Recorder started");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _writter.WriteLine(_builder);
            _writter.Close();
            _builder.Clear();
            _isRunning = false;
            Debug.Log("CSV Recorder stopped");
        }

        public void RecordData(QCHTData qchtData, Transform headPose = null, Transform anchorPose = null)
        {
            if (!_isRunning)
            {
                throw new AssertionException("QCHTSDKDataCSVRecorder",
                    "Trying to write data to a not started Recorder.Start the recorder first");
            }

            var frameData = $"{Time.time * 1000},";
            frameData += TransformToCSV(headPose);
            frameData += $",{TransformToCSV(anchorPose)}";
            frameData += $",{QCHTDataToCSV(qchtData)}";
            _builder.AppendLine(frameData);
        }

        private string QCHTDataToCSV(QCHTData qchtData)
        {
            var left = qchtData.GetHand(true);
            var right = qchtData.GetHand(false);

            var handCount = left.IsDetected ? 1 : 0;
            handCount += right.IsDetected ? 1 : 0;
            var qchtCSV = $"{handCount}";

            if (left.IsDetected)
            {
                qchtCSV += $",{HandDataToCSV(left)}";
            }

            if (right.IsDetected)
            {
                qchtCSV += $",{HandDataToCSV(right)}";
            }

            return qchtCSV;
        }

        private string HandDataToCSV(QCHTHand hand)
        {
            if (hand == null || !hand.IsDetected) return string.Empty;
            int isLeft = hand.IsLeft ? 1 : 0;
            var handData = $"{hand.handId}, {isLeft},";
            for (int i = 0; i < (int) QCHTPointId.POINT_COUNT; i++)
            {
                var pos = hand.points[i];
                handData += $"{pos.x},{pos.y},{pos.z},";
            }

            for (int i = 0; i < (int) QCHTPointId.POINT_COUNT; i++)
            {
                var rot = hand.rotations[i];
                handData += $"{rot.x},{rot.y},{rot.z},{rot.w},";
            }

            handData += $"{(int) hand.gesture},{hand.gestureRatio},{hand.scale},{hand.flipRatio}";

            return handData;
        }

        private string TransformToCSV(Transform t)
        {
            if (t == null)
            {
                return "0,0,0,0,0,0,0";
            }
            
            return $"{t.position.x},{t.position.y},{t.position.z}," +
                   $"{t.rotation.x},{t.rotation.y},{t.rotation.z},{t.rotation.w}";
        }
    }
}