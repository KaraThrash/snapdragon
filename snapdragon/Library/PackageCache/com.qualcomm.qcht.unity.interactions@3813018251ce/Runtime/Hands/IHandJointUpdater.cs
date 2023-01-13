// /******************************************************************************
//  * File: IHandJointUpdater.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

namespace QCHT.Interactions.Hands
{
    public interface IHandJointUpdater
    {
        public BoneData BoneRef { get; set; }
        
        public void UpdateJoint();
    }
}