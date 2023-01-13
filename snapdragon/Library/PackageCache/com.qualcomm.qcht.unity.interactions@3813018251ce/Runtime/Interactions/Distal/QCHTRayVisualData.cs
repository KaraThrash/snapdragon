// /******************************************************************************
//  * File: QCHTRayVisualData.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    [Flags]
    public enum RAY_STATE
    {
        RAY_STATE_NONE = 0,
        RAY_STATE_REST = 1,
        RAY_STATE_HOVER = 2,
        RAY_STATE_SELECTED = 4,
        RAY_STATE_DRAG = 8
    }

    public struct QCHTRayVisualData
    {
        public RAY_STATE State;
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public Sprite ReticleImage;
        public float ReticleScaleFactor;
        public Color? ReticleColor;
        public Color? RayColor;

        public QCHTRayVisualData(RAY_STATE state, Sprite reticleImage, Color? reticleColor, Color? rayColor,
            float scaleFactor = 1f)
        {
            State = state;
            StartPoint = Vector3.zero;
            EndPoint = Vector3.zero;
            ReticleImage = reticleImage;
            ReticleColor = reticleColor;
            RayColor = rayColor;
            ReticleScaleFactor = scaleFactor;
        }

        public static QCHTRayVisualData Default = new QCHTRayVisualData(RAY_STATE.RAY_STATE_NONE, null, null, null);
    }
}