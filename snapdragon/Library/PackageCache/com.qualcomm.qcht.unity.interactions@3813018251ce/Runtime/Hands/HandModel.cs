// /******************************************************************************
//  * File: HandModel.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using QCHT.Core;
using QCHT.Core.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Hands
{
    /// <summary>
    /// Converts QCHT hand input data to Hand pose data and update bones transforms.
    /// </summary>
    public sealed class HandModel : MonoBehaviour, IHandPoseProvider
    {
        // From Thumb mcp used in QCHTSDK
        private static readonly Vector3 s_leftThumpMcp = new Vector3(0.0270322636f, 0.00485388096f, 0.00566395884f);
        private static readonly Vector3 s_rightThumpMcp = new Vector3(-0.0270322636f, 0.00485388096f, 0.00566395884f);

        public HandType Type;

        public HandPose Pose { get; private set; }

        #region MonoBehaviour Functions

        private void Awake()
        {
            Pose = InstantiateHandPose();
        }

        private void Update()
        {
            UpdateHandPose();
        }

        #endregion

        private HandPose InstantiateHandPose()
        {
            var p = ScriptableObject.CreateInstance<HandPose>();
            p.Type = Type == HandType.Left ? HandType.Left : HandType.Right;
            return p;
        }

        private void UpdateHandPose()
        {
            var hand = Type == HandType.Left ? QCHTInput.LeftHand : QCHTInput.RightHand;
            var rootRotation = hand.GetWristRotation();
            var rootPosition = hand.GetWristPosition();

            UpdateBoneData(Pose.Root, rootPosition, rootRotation);
            UpdateFingerData(hand, Pose.Thumb, FingerId.THUMB);
            UpdateFingerData(hand, Pose.Index, FingerId.INDEX);
            UpdateFingerData(hand, Pose.Middle, FingerId.MIDDLE);
            UpdateFingerData(hand, Pose.Ring, FingerId.RING);
            UpdateFingerData(hand, Pose.Pinky, FingerId.PINKY);

            Pose.Scale = Vector3.one * hand.scale;
        }

        private static void UpdateFingerData(QCHTHand hand, FingerData fingerData, FingerId fingerId)
        {
            var joints = QCHTFinger.GetFingerJointsId(fingerId);
            var rot = hand.rotations;
            var pos = hand.points;

            if (fingerId == FingerId.THUMB)
            {
                var thumbMcp = hand.IsLeft ? s_leftThumpMcp : s_rightThumpMcp;
                UpdateBoneData(fingerData.BaseData, thumbMcp, rot[(int) joints[0]]);
                UpdateBoneData(fingerData.MiddleData, pos[(int) joints[0]], rot[(int) joints[1]]);
                UpdateBoneData(fingerData.TopData, pos[(int) joints[1]], rot[(int) joints[2]]);
            }
            else
            {
                UpdateBoneData(fingerData.BaseData, pos[(int) joints[0]], rot[(int) joints[0]] * rot[(int) joints[1]]);
                UpdateBoneData(fingerData.MiddleData, pos[(int) joints[1]], rot[(int) joints[2]]);
                UpdateBoneData(fingerData.TopData, pos[(int) joints[2]], rot[(int) joints[3]]);
            }
        }

        private static void UpdateBoneData(BoneData boneData, Vector3 position, Quaternion rotation)
        {
            if (boneData.UpdatePosition)
                boneData.Position = position;

            if (boneData.UpdateRotation)
                boneData.Rotation = rotation;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HandModel))]
    public class HandModelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // if (!Application.isPlaying)
            //     return;
            //
            // if (GUILayout.Button("Save current model as pose asset"))
            // { 
            //     // TODO: Implement saving hand model as pose asset
            // }
        }
    }
#endif
}