// /******************************************************************************
//  * File: PhysicalHandConfiguration.cs
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
    public class PhysicalBoneConfiguration
    {
        [Header("Drives")]
        public JointSettings JointSettings;
    
        [Header("Rigidbody")]
        public bool UseGravity;
        public float RigidbodyMass = 0.1f;
        public float RigidbodyDrag;
        public float RigidbodyAngularDrag;

        [Header("Joint")]
        public float JointMassScale = 1.0f;
        public float JointConnectedMassScale = 1.0f;
    }

    [CreateAssetMenu(menuName = "QCHT/Interactions/VFF/PhysicalHandConfiguration")]
    public sealed class PhysicalHandConfiguration : ScriptableObject
    {
        public PhysicalBoneConfiguration Root;
        public PhysicalBoneConfiguration Thumb;
        public PhysicalBoneConfiguration Standard;
    }
}