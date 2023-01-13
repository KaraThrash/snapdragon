// /******************************************************************************
//  * File: HandSkin.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [CreateAssetMenu(menuName = "QCHT/HandSkin")]
    public class HandSkin : ScriptableObject
    {
        [Tooltip("Main hand mesh")]
        public Mesh MainMesh;

        [Tooltip("Main hand material")]
        public Material MainMaterial;
        
        [CanBeEmpty, Tooltip("Ghost mesh when vff is active. If null the mesh will be the same as MainMesh")]
        public Mesh GhostMesh;

        [CanBeEmpty, Tooltip("Ghost material when vff is active. If null no material will be applied")]
        public Material GhostMaterial;
    }
}