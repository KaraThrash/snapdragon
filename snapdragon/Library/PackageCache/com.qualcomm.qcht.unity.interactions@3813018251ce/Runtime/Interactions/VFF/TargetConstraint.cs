// /******************************************************************************
//  * File: TargetConstraint.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.VFF
{
    /// <summary>
    /// Defines a constraint to a target using a unity configurable joint.
    /// </summary>
    [Serializable]
    public class TargetConstraint
    {
        public Transform Anchor;
        public Transform ConnectedAnchor;
        public JointSettings Settings;

        [HideInInspector] public Transform Axis;
        [HideInInspector] public ConfigurableJoint Joint;
        [HideInInspector] public Rigidbody ConnectedBody;
        [HideInInspector] public PhysicalObject PhysicalObject;
        [HideInInspector] public Vector3 TmpConnAnchor;
        [HideInInspector] public Transform TargetPosition;
        [HideInInspector] public Vector3 TmpTargetPos;

        public Quaternion GetJointAxisWorldRotation()
        {
            var xAxis = Joint.axis;
            var zAxis = Vector3.Cross(xAxis, Joint.secondaryAxis);
            var yAxis = Vector3.Cross(zAxis, xAxis);

            var axisRot = Quaternion.LookRotation(zAxis, yAxis);

            if (Joint.configuredInWorldSpace)
                return axisRot;

            return Joint.transform.rotation * axisRot;
        }
    }
}