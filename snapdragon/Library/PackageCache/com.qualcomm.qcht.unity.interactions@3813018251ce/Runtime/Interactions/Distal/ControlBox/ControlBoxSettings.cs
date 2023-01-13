// /******************************************************************************
//  * File: ControlBoxSettings.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Distal.ControlBox
{
    [CreateAssetMenu(menuName = "QCHT/ControlBoxSettings")]
    public class ControlBoxSettings : ScriptableObject
    {
        public enum ControlBoxDisplayType
        {
            Never,
            When_hovered,
            Always
        }

        public enum CameraConstraint
        {
            None,
            Vertical,
            Horizontal
        }

        [Serializable]
        public struct HandleState
        {
            public Color color;
            public float size;
        }
        
        [Header("Logic settings")]
        [Range(0f, 10f)] public float SmoothSpeed = DefaultSmoothSpeed;
        public CameraConstraint Constraint = CameraConstraint.None;

        [Header("Show Control Box")]
        public ControlBoxDisplayType DisplayType;
        [Range(0f, 0.5f)] public float ScaleOffset = 0.05f;

        [Header("Reticle icons")]
        public Sprite Move;
        public Sprite Rotate;
        public Sprite Resize;

        [Header("Reticle colors")]
        public Color HoverReticleColor = Color.white;
        public Color SelectedReticleColor = Color.white;

        [Header("Reticle size")]
        [Range(0f, 2f)] public float HoverReticleSize;
        [Range(0f, 2f)] public float SelectedReticleSize;

        [Header("Light settings")]
        [ColorUsage(true, true)] public Color HoverColor = new Color(3.56416106f, 0.857415974f, 1.83230662f, 1);
        [ColorUsage(true, true)] public Color SelectedColor = new Color(3.56416106f, 0.857415974f, 1.83230662f, 1);

        [Header("Handles settings")]
        [Tooltip("Idle state")] public HandleState NormalState;
        [Tooltip("Highlighted state")] public HandleState  HoverState;
        [Tooltip("Selected state")] public HandleState SelectedState;

        [Header("Grid settings")]
        public Color HoverRayColor = Color.white;
        public Color SelectedRayColor = Color.white;

        public const float DefaultSmoothSpeed = 7.0f;
    }
}