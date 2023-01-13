// /******************************************************************************
//  * File: QCHTDataExtensions.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Linq;
using UnityEngine;

namespace QCHT.Core.Extensions
{
    public static class QCHTDataExtension
    {
        public static void LoadBytesToData(this QCHTData data, byte[] buffer, int handSize, int state, int handCount)
        {
            var leftHand = data.GetHand(true);
            var rightHand = data.GetHand(false);

            leftHand.Clear();
            rightHand.Clear();

            for (var i = 0; i < handCount; i++)
            {
                const int handIdPadding = sizeof(Int32);
                var offset = i * handSize;
                var isLeft = BitConverter.ToBoolean(buffer, offset + handIdPadding);
                var hand = isLeft ? leftHand : rightHand;
                PopulateHandDataFromBuffer(hand, buffer, offset);
            }
        }

        public static void LoadFloatsToData(this QCHTData data, float[] rawData, int size)
        {
            var leftHand = data.GetHand(true);
            var rightHand = data.GetHand(false);

            leftHand.Clear();
            rightHand.Clear();

            var handCount = (int) rawData[1];
            for (var i = 0; i < handCount; i++)
            {
                const int dataPadding = 2;
                const int handIdPadding = 1;
                var structLength = (size - dataPadding) / 2;
                var offset = i * structLength;
                var isLeft = rawData[offset + dataPadding + handIdPadding] > 0;
                var hand = isLeft ? leftHand : rightHand;
                PopulateHandDataFromFloats(hand, rawData, offset + dataPadding);
            }
        }

        private static void PopulateHandDataFromBuffer(QCHTHand hand, byte[] buffer, int startIndex)
        {
            var i = startIndex;

            hand.IsDetected = true;
            hand.handId = BitConverter.ToInt32(buffer, i); i += sizeof(Int32);
            hand.IsLeft = BitConverter.ToBoolean(buffer, i); i += sizeof(Int32);

            for (var j = 0; j < (int) QCHTPointId.POINT_COUNT; j++)
            {
                hand.points[j].x = BitConverter.ToSingle(buffer, i); i += sizeof(float);
                hand.points[j].y = BitConverter.ToSingle(buffer, i); i += sizeof(float);
                hand.points[j].z = BitConverter.ToSingle(buffer, i); i += sizeof(float);
            }

            for (var j = 0; j < (int) QCHTPointId.POINT_COUNT; j++)
            {
                hand.rotations[j].x = BitConverter.ToSingle(buffer, i); i += sizeof(float);
                hand.rotations[j].y = BitConverter.ToSingle(buffer, i); i += sizeof(float);
                hand.rotations[j].z = BitConverter.ToSingle(buffer, i); i += sizeof(float);
                hand.rotations[j].w = BitConverter.ToSingle(buffer, i); i += sizeof(float);
            }

            hand.gesture = (GestureId) BitConverter.ToInt32(buffer, i); i += sizeof(Int32);
            hand.gestureRatio = BitConverter.ToSingle(buffer, i); i += sizeof(float);
            hand.scale = BitConverter.ToSingle(buffer, i); i += sizeof(float);
            hand.flipRatio = BitConverter.ToSingle(buffer, i);
        }

        private static void PopulateHandDataFromFloats(QCHTHand hand, float[] rawData, int startIndex)
        {
            var i = startIndex;

            hand.IsDetected = true;
            hand.handId = rawData[i++];
            hand.IsLeft = rawData[i++] > 0;

            for (var j = 0; j < (int) QCHTPointId.POINT_COUNT; j++)
            {
                hand.points[j].x = rawData[i++];
                hand.points[j].y = rawData[i++];
                hand.points[j].z = rawData[i++];
            }

            for (var j = 0; j < (int) QCHTPointId.POINT_COUNT; j++)
            {
                hand.rotations[j].x = rawData[i++];
                hand.rotations[j].y = rawData[i++];
                hand.rotations[j].z = rawData[i++];
                hand.rotations[j].w = rawData[i++];
            }

            hand.gesture = (GestureId) rawData[i++];
            hand.gestureRatio = rawData[i++];
            hand.scale = rawData[i++];
            hand.flipRatio = rawData[i];
        }

        public static void Log(this QCHTData data)
        {
            var hand = data.GetHand(true);

            if (hand == null)
                return;

            var pc = hand.GetPoint(QCHTPointId.PALM_CENTER);

            Debug.Log("-----DATA START-----");

            var debug = "{";

            foreach (var handPoint in hand.points)
            {
                var localPoint = handPoint - pc;
                debug += $"{localPoint.x}f,{localPoint.y}f,{localPoint.z}f,";
            }

            Debug.Log(debug);

            debug = hand.rotations.Aggregate("",
                (current, handRot) => current + $"{handRot.x}f,{handRot.y}f,{handRot.z}f,{handRot.w}f,");

            debug += "};";
            Debug.Log(debug);
            Debug.Log("-----DATA END-----");
        }
    }
}