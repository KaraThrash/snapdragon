// /******************************************************************************
//  * File: HandJointUpdater.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public class HandJointUpdater : MonoBehaviour, IHandJointUpdater
    { 
        public virtual BoneData BoneRef { get; set; }

        public virtual void UpdateJoint()
        {
            if (BoneRef == null)
                return;

            var t = transform;

            if (BoneRef.UpdatePosition)
            {
                t.localPosition = BoneRef.Position;
            }

            if (BoneRef.UpdateRotation)
            {
                t.localRotation = BoneRef.Rotation;
            }
        }
    }
}