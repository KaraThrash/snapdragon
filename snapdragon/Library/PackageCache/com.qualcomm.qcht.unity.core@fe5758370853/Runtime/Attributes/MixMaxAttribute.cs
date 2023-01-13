// /******************************************************************************
//  * File: MinMaxAttribute.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Core
{
    public sealed class MinMaxAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;

        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}