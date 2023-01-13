// /******************************************************************************
//  * File: IRayDataProvider.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    [Serializable]
    public struct RayData
    {
        public Sprite ReticleSprite;
        public Color ReticleColor;
        public Color RayColor;
        public float ReticleScaleFactor;
    }

    public interface IRayDataProvider
    {
        public RayData HoverRayData { get; }
        public RayData SelectedRayData { get; }
    }
}