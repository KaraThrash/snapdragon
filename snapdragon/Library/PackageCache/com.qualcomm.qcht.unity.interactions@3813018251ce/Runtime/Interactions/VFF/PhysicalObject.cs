// /******************************************************************************
//  * File: PhysicalObject.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace QCHT.Interactions.VFF
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalObject : MonoBehaviour
    {
        public Rigidbody Rigidbody;

        [Header("Rigidbody params ext")]
        [SerializeField] public Transform centerOfMass;
        [SerializeField] private float maxDepenetrationVelocity = 1.0f;

        [Space]
        public List<TargetConstraint> TargetConstraints = new List<TargetConstraint>();

        [Header("Limits")]
        [SerializeField] private bool useLimits;
        [SerializeField] private float maxVelocity = 10f;
        [SerializeField] private float maxAngularVelocity = 10f;

        private readonly List<Collider> _colliders = new List<Collider>();

        #region MonoBehaviour Functions

        public void Start()
        {
            foreach (var tC in TargetConstraints)
                SetupTargetConstraint(tC);

            _colliders.AddRange(GetComponentsInChildren<Collider>());
        }

        public void FixedUpdate()
        {
            if (Rigidbody.isKinematic)
                return;

            if (centerOfMass)
            {
                var localCenterOfMass = Rigidbody.transform.InverseTransformPoint(centerOfMass.position);
                if (Rigidbody.centerOfMass != localCenterOfMass) Rigidbody.centerOfMass = localCenterOfMass;
            }

            foreach (var tC in TargetConstraints)
                UpdateJointWithTargetConstraint(tC);

            if (useLimits)
            {
                Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, maxVelocity);
                Rigidbody.angularVelocity = Vector3.ClampMagnitude(Rigidbody.angularVelocity, maxAngularVelocity);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Rigidbody)
                Rigidbody = GetComponent<Rigidbody>();

            if (!Rigidbody)
                Rigidbody = gameObject.AddComponent<Rigidbody>();
        }
#endif

        private void OnEnable()
        {
            if (!Rigidbody)
                Rigidbody = gameObject.GetComponent<Rigidbody>();

            if (!Rigidbody)
                Rigidbody = gameObject.AddComponent<Rigidbody>();

            Rigidbody.maxDepenetrationVelocity = maxDepenetrationVelocity;
            Rigidbody.isKinematic = false;

            if (!centerOfMass)
                InstantiateCenterOfMass();
        }

        private void OnDisable()
        {
            Rigidbody.isKinematic = true;
        }

        #endregion

        public void TogglePhysics(bool enable)
        {
            Rigidbody.isKinematic = !enable;
            Rigidbody.detectCollisions = enable;

            foreach (var col in _colliders)
            {
                if (!col.isTrigger)
                    col.enabled = enable;
            }
        }

        public void AddConstraint(TargetConstraint constraint)
        {
            TargetConstraints.Add(constraint);
            SetupTargetConstraint(constraint);
        }

        private void SetupTargetConstraint(TargetConstraint constraint)
        {
            var joint = GetComponent<ConfigurableJoint>();

            if (!joint)
                joint = gameObject.AddComponent<ConfigurableJoint>();

            joint.connectedBody = constraint.ConnectedBody;

            // Angular
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = constraint.Settings.AngularMotion;
            joint.angularXDrive = joint.angularYZDrive = constraint.Settings.AngularDrive.ToJointDrive();
            joint.slerpDrive = constraint.Settings.AngularDrive.ToJointDrive();

            //Motion
            joint.xMotion = joint.yMotion = joint.zMotion = constraint.Settings.LinearMotion;
            joint.xDrive = joint.yDrive = joint.zDrive = constraint.Settings.MotionDrive.ToJointDrive();

            // Control
            joint.enableCollision = false;

            constraint.Joint = joint;
            constraint.PhysicalObject = this;

            InstantiateAnchor(constraint);
            InstantiateConnectedAnchor(constraint);
            InstantiateAxis(constraint);
        }

        private void InstantiateCenterOfMass()
        {
            centerOfMass = new GameObject(name + ".CenterOfMass").transform;
            centerOfMass.SetParent(Rigidbody.transform);
            centerOfMass.localPosition = Rigidbody.centerOfMass;
        }

        private void InstantiateAxis(TargetConstraint constraint)
        {
            if (constraint.Axis)
                return;

            var axis = new GameObject(name + ".Axis").transform;
            axis.position = constraint.ConnectedAnchor.position;
            axis.rotation = constraint.GetJointAxisWorldRotation();
            axis.parent = constraint.ConnectedAnchor.parent;
            constraint.Axis = axis;
        }

        private void InstantiateAnchor(TargetConstraint constraint)
        {
            if (constraint.Anchor)
                return;

            var anchor = new GameObject(name + ".Anchor").transform;
            var jointTransform = constraint.Joint.transform;
            anchor.position = jointTransform.position;
            anchor.rotation = jointTransform.rotation;
            anchor.SetParent(jointTransform);
            constraint.Anchor = anchor;
        }

        private void InstantiateConnectedAnchor(TargetConstraint constraint)
        {
            if (constraint.ConnectedAnchor)
                return;

            var connectedAnchor = new GameObject(name + ".ConnectedAnchor").transform;
            var jointTransform = constraint.Joint.transform;
            connectedAnchor.SetParent(constraint.ConnectedBody
                ? constraint.ConnectedBody.transform
                : jointTransform.parent);
            connectedAnchor.position = jointTransform.position;
            connectedAnchor.rotation = jointTransform.rotation;
            constraint.ConnectedAnchor = connectedAnchor;
        }

        private static void UpdateJointWithTargetConstraint(TargetConstraint constraint)
        {
            constraint.Joint.autoConfigureConnectedAnchor = constraint.Settings.AutoConfigureConnectedAnchor;
            constraint.TmpConnAnchor = constraint.ConnectedBody
                ? constraint.Joint.connectedBody.transform.InverseTransformPoint(constraint.ConnectedAnchor.position)
                : constraint.ConnectedAnchor.position;

            if (constraint.Joint.connectedAnchor != constraint.TmpConnAnchor)
                constraint.Joint.connectedAnchor = constraint.TmpConnAnchor;

            // Target Rotation
            constraint.Joint.SetTargetRotationLocal(constraint.ConnectedAnchor.rotation, constraint.Anchor.rotation);

            // Target position
            if (constraint.TargetPosition)
            {
                constraint.TmpTargetPos =
                    -1.0f * constraint.Axis.InverseTransformPoint(constraint.TargetPosition.position);
                if (constraint.ConnectedBody)
                    constraint.Joint.targetPosition = Vector3.Scale(constraint.TmpTargetPos,
                        constraint.ConnectedBody.transform.localScale);
                else constraint.Joint.targetPosition = constraint.TmpTargetPos;
            }
            else if (constraint.Joint.targetPosition != Vector3.zero)
            {
                constraint.Joint.targetPosition = Vector3.zero;
            }
        }
    }
}

public static class ConfigurableJointExtensions
{
    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation,
        Quaternion startLocalRotation)
    {
        if (joint.configuredInWorldSpace)
        {
            return;
        }

        SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
    }

    public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation,
        Quaternion startWorldRotation)
    {
        if (!joint.configuredInWorldSpace)
        {
            return;
        }

        SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
    }

    private static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation,
        Quaternion startRotation, Space space)
    {
        var axis = joint.axis;
        var forward = Vector3.Cross(axis, joint.secondaryAxis).normalized;
        var up = Vector3.Cross(forward, axis).normalized;
        var worldToJointSpace = Quaternion.LookRotation(forward, up);

        var resultRotation = Quaternion.Inverse(worldToJointSpace);

        if (space == Space.World)
        {
            resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
        }
        else
        {
            resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
        }

        resultRotation *= worldToJointSpace;

        joint.targetRotation = resultRotation;
    }
}