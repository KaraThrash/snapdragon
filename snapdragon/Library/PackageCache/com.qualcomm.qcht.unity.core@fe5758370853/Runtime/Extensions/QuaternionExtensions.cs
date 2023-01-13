// /******************************************************************************
//  * File: QuaternionExtensions.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Core.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion FlippedXAxis(this Quaternion rotation)
        {
            return new Quaternion(-rotation.z, rotation.x, rotation.y,  -rotation.w);
        }

        public static Quaternion FlipXAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.w, rotation.x, -rotation.y, -rotation.z);
        }
        
        public static Quaternion FlipYAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.w, -rotation.x, rotation.y, -rotation.z);
        }
        
        public static Quaternion FlipZAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.w, -rotation.x, -rotation.y, rotation.z);
        }

        public static Quaternion FlipXYAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.z, rotation.y, rotation.x, rotation.w);
        }
        
        public static Quaternion FlipXZAxis(this Quaternion rotation)
        {
            return new Quaternion(-rotation.y, rotation.z, -rotation.w, rotation.x);
        }
        
        public static Quaternion FlipYXAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.z, -rotation.y, -rotation.x, rotation.w);
        }
        
        public static Quaternion FlipYZAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.x, rotation.w, rotation.z, rotation.y);
        }
        
        public static Quaternion FlipZXAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.y, rotation.z, rotation.w, rotation.x);
        }
        
        public static Quaternion FlipZYAxis(this Quaternion rotation)
        {
            return new Quaternion(rotation.x, rotation.w, -rotation.z, -rotation.y);
        }
    }
}