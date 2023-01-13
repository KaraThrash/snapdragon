// /******************************************************************************
//  * File: HandLabel.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions.Hands
{
    public class HandLabel : MonoBehaviour
    {
        public Text label;

        public void UpdateLabel(string text)
        {
            label.text = text;
        }

        public void ToggleDisplay(bool on)
        {
            label.enabled = on;
        }
    } 
}