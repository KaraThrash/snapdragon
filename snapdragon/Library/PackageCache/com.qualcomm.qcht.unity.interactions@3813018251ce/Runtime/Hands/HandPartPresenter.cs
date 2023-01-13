// /******************************************************************************
//  * File: HandPartPresenter.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.VFF;
using UnityEngine;
using UnityEngine.Events;

namespace QCHT.Interactions.Hands
{
    public enum HandPartColliderType
    {
        None,
        Triggering,
        Colliding
    }

    public class HandPartPresenter : HandJointUpdater
    {
        public HandPartPresenter ConnectedPart;
        public Collider Collider;

        [HideInInspector] public UnityEvent<GameObject> OnTriggerEntered = new UnityEvent<GameObject>();
        [HideInInspector] public UnityEvent OnTriggerExited = new UnityEvent();

        [SerializeField] private bool relativeToParentBone;

        [Header("Locking")]
        public float MinLocalRotZ = -1;

        public float MaxLocalRotZ = 361;

        public Quaternion FixedLocalRot = Quaternion.identity;

        private Rigidbody _rigidbody;
        private PhysicalObject _physicalObject;
        private TargetConstraint _constraint;
        private GameObject _triggeredObject;

        private HandPartPresenter[] _childParts;

        private bool _forceMatching;
        private int _triggerCount;

        private bool _isColliding;
        public bool IsColliding => _isColliding;

        public Vector3 LocalPosition
        {
            get => GetLocalPosition(this);
            set => transform.position = GetWorldFromLocalPosition(value, this);
        }

        public Quaternion LocalRotation
        {
            get => GetLocalRotation(this);
            set => transform.rotation = GetWorldFromLocalRotation(value, this);
        }

        public float LocalRotZ => GetProcessedAngleZ(LocalRotation);

        private HandPartColliderType _type = HandPartColliderType.None;

        public HandPartColliderType Type
        {
            get => _type;

            set
            {
                _type = value;
                switch (_type)
                {
                    case HandPartColliderType.None:
                        if (_physicalObject)
                            _physicalObject.enabled = false;

                        if (Collider)
                        {
                            Collider.enabled = false;
                            Collider.isTrigger = true;
                        }

                        break;

                    case HandPartColliderType.Triggering:
                        if (_physicalObject)
                            _physicalObject.enabled = false;

                        if (Collider)
                        {
                            Collider.enabled = true;
                            Collider.isTrigger = true;
                        }

                        break;

                    case HandPartColliderType.Colliding:
                        if (_physicalObject)
                            _physicalObject.enabled = true;

                        if (Collider)
                        {
                            Collider.enabled = true;
                            Collider.isTrigger = false;
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public PhysicalHandConfiguration Configuration { get; set; }

        #region MonoBehaviour Functions

        private void Awake()
        {
            _constraint = new TargetConstraint();
            _childParts = GetComponentsInChildren<HandPartPresenter>(true);
        }

        private void OnCollisionStay()
        {
            // inform upper parts concerning the collision
            if (ConnectedPart != null)
            {
                var connectedPart = ConnectedPart;
                if (connectedPart)
                    connectedPart.OnCollisionStay();
            }

            _isColliding = true;
        }

        private void FixedUpdate()
        {
            _isColliding = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (BoneRef == null)
                return;

            // Not root
            if (!BoneRef.IsRoot)
            {
                if (ConnectedPart != null)
                    ConnectedPart.OnTriggerEnter(other);

                return;
            }

            var newInteractable = other.gameObject;

            // Release the current interactable if it's different from the new interactable
            if (_triggeredObject != null && _triggeredObject != newInteractable)
            {
                OnTriggerExited.Invoke();
                _triggeredObject = null;
                _triggerCount = 0;
            }

            _triggerCount++;
            _triggeredObject = newInteractable;


            if (_triggerCount == 1) // first enter
                OnTriggerEntered.Invoke(_triggeredObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (BoneRef == null)
                return;

            // Not root
            if (!BoneRef.IsRoot)
            {
                if (ConnectedPart != null)
                    ConnectedPart.OnTriggerExit(other);

                return;
            }

            // Currently not triggering an interactable
            if (_triggeredObject == null)
                return;

            _triggerCount--;

            if (_triggerCount < 0)
                _triggerCount = 0;

            // Another child part is triggering too
            if (_triggerCount > 0)
                return;

            OnTriggerExited.Invoke();
            _triggeredObject = null;
        }

        private void OnDisable()
        {
            _triggerCount = 0;
        }

        #endregion

        public override void UpdateJoint()
        {
            if (_type != HandPartColliderType.Colliding)
            {
                base.UpdateJoint();
                return;
            }

            UpdateJointColliding();
        }

        public void ForceMatching(bool forceMatching, bool updateChildren = true)
        {
            if (_forceMatching == forceMatching)
                return;

            _forceMatching = forceMatching;

            if (updateChildren)
            {
                foreach (var child in _childParts)
                    child.ForceMatching(forceMatching, false);
            }
        }

        public void TriggerCollisionsDetection(bool enable)
        {
            if (!_rigidbody)
                return;

            _rigidbody.detectCollisions = enable;
            _rigidbody.isKinematic = !enable;
        }

        private void UpdateJointColliding()
        {
            if (BoneRef == null)
                return;

            if (!_physicalObject)
                InitJointPhysicsConstraint();

            _physicalObject.Rigidbody.detectCollisions = !_forceMatching;

            if (BoneRef.IsRoot)
            {
                if (!IsColliding || _forceMatching)
                {
                    var rigidbodyTransform = _physicalObject.Rigidbody.transform;
                    rigidbodyTransform.localRotation = BoneRef.Rotation;
                    rigidbodyTransform.localPosition = BoneRef.Position;
                }

                if (BoneRef.UpdatePosition)
                    _constraint.ConnectedAnchor.localPosition = BoneRef.Position;

                if (BoneRef.UpdateRotation)
                    _constraint.ConnectedAnchor.localRotation = BoneRef.Rotation;
            }
            else
            {
                if (BoneRef.UpdatePosition)
                    _constraint.ConnectedAnchor.localPosition = BoneRef.Position;

                if (!BoneRef.UpdateRotation)
                    return;

                var connectedPart = ConnectedPart;
                if (connectedPart && connectedPart.LocalRotZ < MaxLocalRotZ &&
                    connectedPart.LocalRotZ > MinLocalRotZ)
                {
                    _constraint.ConnectedAnchor.rotation = _constraint.ConnectedAnchor.parent.rotation
                                                           * BoneRef.Rotation;
                }
                else
                {
                    _constraint.ConnectedAnchor.localRotation = FixedLocalRot;
                }
            }
        }

        private void InitJointPhysicsConstraint()
        {
            if (!gameObject.TryGetComponent<PhysicalObject>(out var physicalObject))
                physicalObject = gameObject.AddComponent<PhysicalObject>();

            _physicalObject = physicalObject;

            PhysicalBoneConfiguration configuration;

            if (BoneRef.IsRoot)
                configuration = Configuration.Root;
            else if (BoneRef.IsThumb)
                configuration = Configuration.Thumb;
            else
                configuration = Configuration.Standard;

            _constraint.Settings = configuration.JointSettings;
            _physicalObject.TargetConstraints.Clear(); // Override predefined constraints
            _physicalObject.AddConstraint(_constraint);

            if (!TryGetComponent<ConfigurableJoint>(out var joint))
                return;

            joint.massScale = configuration.JointMassScale;
            joint.connectedMassScale = configuration.JointConnectedMassScale;
            joint.rotationDriveMode = RotationDriveMode.Slerp;

            if (TryGetComponent(out _rigidbody))
            {
                _rigidbody.mass = configuration.RigidbodyMass;
                _rigidbody.drag = configuration.RigidbodyDrag;
                _rigidbody.angularDrag = configuration.RigidbodyAngularDrag;
                _rigidbody.useGravity = configuration.UseGravity;
            }

            if (ConnectedPart && ConnectedPart.TryGetComponent<Rigidbody>(out var connectedBody))
            {
                _constraint.ConnectedBody = connectedBody;
                _constraint.Joint.connectedBody = connectedBody;
                _constraint.Joint.enableCollision = false;
                _constraint.Joint.enablePreprocessing = false;
            }
        }

        #region Static

        private static float GetProcessedAngleZ(Quaternion rotation)
        {
            var angle = rotation.eulerAngles.z;

            if (angle >= 180.0f)
                angle -= 180.0f;
            else
                angle += 180.0f;

            return angle;
        }

        private static Quaternion GetLocalRotation(HandPartPresenter part)
        {
            if (!part.ConnectedPart || !part.relativeToParentBone)
                return part.transform.localRotation;

            return Quaternion.Inverse(part.ConnectedPart.transform.rotation) * part.transform.rotation;
        }

        private static Quaternion GetWorldFromLocalRotation(Quaternion newLocalRot, HandPartPresenter part)
        {
            if (!part.ConnectedPart || !part.relativeToParentBone)
                return part.transform.parent.rotation * newLocalRot;

            return part.ConnectedPart.transform.rotation * newLocalRot;
        }

        private static Vector3 GetLocalPosition(HandPartPresenter part)
        {
            if (!part.ConnectedPart || !part.relativeToParentBone)
                return part.transform.localPosition;

            return part.ConnectedPart.transform.InverseTransformPoint(part.transform.position);
        }

        private static Vector3 GetWorldFromLocalPosition(Vector3 newLocalPos, HandPartPresenter part)
        {
            if (!part.ConnectedPart || !part.relativeToParentBone)
                return part.transform.parent.TransformPoint(newLocalPos);

            return part.ConnectedPart.transform.TransformPoint(newLocalPos);
        }

        #endregion
    }
}