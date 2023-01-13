/****************************************************************
 * Copyright (c) 2022 Qualcomm Technologies, Inc.
 * All Rights Reserved.
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrRayCollisionQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private ulong _targetId;
        private XrRayCastTargetTypeQCOM _targetType;
        private XrVector3f _position;
        private XrVector3f _surfaceNormal;
        private long _time;

        public ulong TargetId => _targetId;
        public Vector3 Position => _position.ToVector3();
        public Vector3 SurfaceNormal => _surfaceNormal.ToVector3();
    }
}