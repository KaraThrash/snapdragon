// /******************************************************************************
//  * File: QCHTHeadProvider.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Core
{
    public sealed class QCHTHeadProvider : MonoBehaviour
    {
        [SerializeField] private Transform head;

        public Transform Head => head == null ? transform : head;
    }
}