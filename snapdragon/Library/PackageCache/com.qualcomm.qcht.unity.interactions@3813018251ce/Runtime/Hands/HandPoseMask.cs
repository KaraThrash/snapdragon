// /******************************************************************************
//  * File: HandPoseMask.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Hands
{
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPoseMask")]
    public sealed class HandPoseMask : ScriptableObject
    {
        public enum MaskState
        {
            Required,
            Free
        }

        public MaskState Thumb;
        public MaskState Index;
        public MaskState Middle;
        public MaskState Ring;
        public MaskState Pinky;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HandPoseMask))]
    public class HandPoseMaskEditor : UnityEditor.Editor
    {
        public static HandPoseMask CreateNewHandPoseMaskAsset()
        {
            var handPoseMask = CreateInstance<HandPoseMask>();
            Editor.EditorUtility.CreateAssetInSettingsFromObj(handPoseMask, "HandPoseMasks", "NewHandPoseMask");
            return handPoseMask;
        }

        public static HandPoseMask DuplicatePoseMaskAsset(HandPoseMask handPoseMask)
        {
            var newMask = Instantiate(handPoseMask);
            Editor.EditorUtility.CreateAssetInSettingsFromObj(handPoseMask, "HandPoseMasks", handPoseMask.name);
            return newMask;
        }
    }

#endif
}