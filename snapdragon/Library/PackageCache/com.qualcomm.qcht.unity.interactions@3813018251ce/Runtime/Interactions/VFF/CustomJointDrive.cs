// /******************************************************************************
//  * File: CustomJointDrive.cs
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
    public class CustomJointDrive
    {
        public float Spring = 1f;
        public float Damper = 0.01f;
        public float MaxForce = float.MaxValue;
        
        public JointDrive ToJointDrive()
        {
            var jDrive = new JointDrive
            {
                positionSpring = Spring,
                positionDamper = Damper,
                maximumForce = MaxForce
            };

            return jDrive;
        }

        public SoftJointLimitSpring ToSoftJointLimitSpring()
        {
            var lSpring = new SoftJointLimitSpring
            {
                spring = Spring,
                damper = Damper
            };

            return lSpring;
        }
    }
}