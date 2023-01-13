// /******************************************************************************
//  * File: JointSettings.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.VFF
{
    [Serializable]
    public class JointSettings
    {
        [Header("Connected Anchor")]
        public bool AutoConfigureConnectedAnchor;
        public CollisionDetectionMode CollisionDetectionMode;
        
        [Header("Freedom")]
        public ConfigurableJointMotion LinearMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion AngularMotion = ConfigurableJointMotion.Free;

        [Header("Drives")]
        public CustomJointDrive AngularDrive;
        public CustomJointDrive MotionDrive;
    }
}