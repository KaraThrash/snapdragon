// /******************************************************************************
//  * File: HandAudioClips.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [CreateAssetMenu(menuName = "QCHT/Audio/HandFeedbackClips", order = 1)]
    public sealed class HandAudioClips : ScriptableObject
    {
        public AudioClip Select;
        public AudioClip Unselect;
        public AudioClip Reset;
    }
}
