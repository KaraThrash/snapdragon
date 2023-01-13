// /******************************************************************************
//  * File: HandGhostSO.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public sealed class HandGhostSO : ScriptableObject
    {
        public HandGhost LeftGhost;
        public HandGhost RightGhost;
    }
}