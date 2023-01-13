// /******************************************************************************
//  * File: Vector3Extensions.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Core.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 Multiply(this Vector3 v, Vector3 vector)
        {
            return new Vector3(v.x * vector.x, v.y * vector.y, v.z * vector.z);
        }

        public static Vector3 Divide(this Vector3 v, Vector3 vector)
        {
            return new Vector3(v.x / vector.x, v.y / vector.y, v.z / vector.z);
        }

        public static Vector3 MidPoint(this Vector3 pointA, Vector3 pointB)
        {
            return (pointA + pointB) * 0.5f;
        }

        public static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Vector3 FlippedXAxis(this Vector3 v)
        {
            return new Vector3(-v.x, v.y, v.z);
        }
    }
}