/******************************************************************************
 * File: SpacesHandJointVisualizer.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    [RequireComponent(typeof(SpacesHand))]
    public class SpacesHandJointVisualizer : MonoBehaviour
    {
        public Mesh JointMesh;
        public Material JointMaterial;
        [Range(0.005f, 0.05f)]
        public float JointMeshScale = 0.01f;
        public bool UseNormalizedColors = true;

        private SpacesHand _spacesHand;
        private readonly int _color = Shader.PropertyToID("_Color");
        private readonly Matrix4x4[] _pointsMatrix = new Matrix4x4[(int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
        private readonly MaterialPropertyBlock[] _propertyBlock = new MaterialPropertyBlock[(int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];

        private void Start() {
            _spacesHand = GetComponent<SpacesHand>();
        }

        private void Update() {
            UpdatePointMatrices(_spacesHand);

            if (UseNormalizedColors) {
                for (var i = 0; i < (int) XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT; i++) {
                    _propertyBlock[i] ??= new MaterialPropertyBlock();
                    var color = NormalizedColorForJoint((XrHandJointEXT) i);
                    _propertyBlock[i].SetColor(_color,  color);

                    /* Draws instance by instance but will be merge if the GPU instancing is enabled. */
                    Graphics.DrawMesh(JointMesh, _pointsMatrix[i], JointMaterial, 0, null, 0, _propertyBlock[i]);
                }
            } else {
                /* Directly renders instances in batch. */
                Graphics.DrawMeshInstanced(JointMesh, 0, JointMaterial, _pointsMatrix, _pointsMatrix.Length);
            }
        }

        private void UpdatePointMatrices(SpacesHand hand) {
            for (var i = 0; i < _pointsMatrix.Length; i++) {
                var pose = hand.Joints[i].Pose;
                _pointsMatrix[i].SetTRS(pose.position, pose.rotation, Vector3.one * JointMeshScale);
            }
        }

        private static Color NormalizedColorForJoint(XrHandJointEXT jointId) {
            return Enum.GetName(typeof(XrHandJointEXT), jointId).Split('_')[3] switch {
                "PALM" => Color.white,
                "WRIST" => new Color(200f / 255f, 200f / 255f, 200f / 255f),
                "THUMB" => new Color(255f / 255f, 196f / 255f, 0f / 255f),
                "INDEX" => new Color(26f / 255f, 201f / 255f, 56f / 255f),
                "MIDDLE" => new Color(0f / 255f, 215f / 255f, 255f / 255f),
                "RING" => new Color(139f / 255f, 0f / 255f, 226f / 255f),
                "LITTLE" => new Color(200f / 255f, 0f / 255f, 200f / 255f),
                _ => Color.black
            };
        }
    }
}