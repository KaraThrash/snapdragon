// /******************************************************************************
//  * File: QCHTData.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Core.Extensions;

namespace QCHT.Core
{
    public sealed class QCHTData
    {
        private readonly QCHTHand _leftHand = new QCHTHand {IsLeft = true};
        private readonly QCHTHand _rightHand = new QCHTHand {IsLeft = false};
        
        public void Update(byte[] buffer, int size, int state, int handCount)
        {
            this.LoadBytesToData(buffer, size, state, handCount);
        }

        public void Update(float[] data, int size)
        {
            this.LoadFloatsToData(data, size);
        }

        public QCHTHand GetHand(bool isLeft)
        {
            return isLeft ? _leftHand : _rightHand;
        }
    }

    public sealed class QCHTHand
    {
        // Unity Side Data
        public bool IsDetected;

        // QCHT SDK Data
        public float handId;
        public bool IsLeft;
        public readonly Vector3[] points = new Vector3[(int) QCHTPointId.POINT_COUNT]; // In camera space
        public readonly Quaternion[] rotations = new Quaternion[(int) QCHTPointId.POINT_COUNT];
        public GestureId gesture = GestureId.UNKNOWN;
        public float gestureRatio = 0f;
        public float scale = 1f;
        public float flipRatio = 0f;

        public QCHTHand()
        {
            for(var i = 0; i < points.Length; i++)
                points[i] = Vector3.zero;
            
            for(var i = 0; i < rotations.Length; i++)
                rotations[i] = Quaternion.identity;
        }
        
        public void Clear()
        {
            IsDetected = false;
            gesture = GestureId.UNKNOWN;
        }
    }

    public enum HandType
    {
        Left,
        Right
    }

    public enum QCHTPointId
    {
        THUMB_PIP = 0,
        THUMB_DIP = 1,
        THUMB_TIP = 2,
        INDEX_MCP = 3,
        INDEX_PIP = 4,
        INDEX_DIP = 5,
        INDEX_TIP = 6,
        MIDDLE_MCP = 7,
        MIDDLE_PIP = 8,
        MIDDLE_DIP = 9,
        MIDDLE_TIP = 10,
        RING_MCP = 11,
        RING_PIP = 12,
        RING_DIP = 13,
        RING_TIP = 14,
        PINKY_MCP = 15,
        PINKY_PIP = 16,
        PINKY_DIP = 17,
        PINKY_TIP = 18,
        PALM_CENTER = 19,
        WRIST_START = 20,
        WRIST_END = 21,
        WRIST_CENTER = 22,
        POINT_COUNT = 23
    }

    public enum FingerId
    {
        UNKNOWN = -1,
        THUMB = 0,
        INDEX = 1,
        MIDDLE = 2,
        RING = 3,
        PINKY = 4,
        COUNT = 5
    }

    public enum KnuckleId
    {
        BASE = 0,
        PIP = 1,
        DIP = 2,
        TIP = 3
    }

    public enum GestureId
    {
        UNKNOWN = -1,
        // Gestures
        OPEN_HAND = 0,
        FLIP = 1,
        GRAB = 2,
        UP = 3,
        DOWN = 4,
        SWIPE = 5,
        SWIPE_OUT = 6,
        PINCH = 7,
        POINT = 8,
        VICTORY = 9,
        CALL = 10,
        METAL = 11,
    }
}