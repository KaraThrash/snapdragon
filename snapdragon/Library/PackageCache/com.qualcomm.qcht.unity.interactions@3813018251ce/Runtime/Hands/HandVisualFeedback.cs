// /******************************************************************************
//  * File: HandVisualFeedback.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class HandVisualFeedback : MonoBehaviour
    {
        private static readonly int PinchPowerShaderId = Shader.PropertyToID("_PinchPower");
        private static readonly int PinchColorShaderId = Shader.PropertyToID("_PinchIndexColor");
        private static readonly int RimColorShaderId = Shader.PropertyToID("_RimColor");

        [SerializeField] private bool isLeftHand;

        [Header("Display")]
        [SerializeField] private Color onEnterRimColor = Color.cyan;
        [SerializeField] private Color onErrorRimColor = Color.red;

        private SkinnedMeshRenderer _skinnedMeshRenderer;

        /// <summary>
        /// Normal rim color when nothing happened.
        /// It is directly set from the material at start.
        /// </summary>
        private Color _normalRimColor;

        /// <summary>
        /// Normal pinch color when nothing happened.
        /// It is directly set from the material at start.
        /// </summary>
        private Color _normalPinchColor;

        #region MonoBehaviour Functions

        private void Awake()
        {
            _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            GetMaterialColors();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void Update()
        {
            UpdatePinchPower();
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Change the hand material by antoher.
        /// Resets the material colors set from the material.
        /// </summary>
        /// <param name="newMaterial"> the new material to set. </param>
        public void SetMaterial(Material newMaterial)
        {
            _skinnedMeshRenderer.material = newMaterial;
            GetMaterialColors();
        }

        /// <summary>
        /// Changes the rim color.
        /// </summary>
        /// <param name="color"> the new rim color. </param>
        public void SetRimColor(Color color)
        {
            _skinnedMeshRenderer.material.SetColor(RimColorShaderId, color);
        }

        /// <summary>
        /// Changes the pinch color.
        /// </summary>
        /// <param name="color"> the new pinch color. </param>
        public void SetPinchColor(Color color)
        {
            _skinnedMeshRenderer.material.SetColor(PinchColorShaderId, color);
        }

        /// <summary>
        /// Resets the rim color to normal.
        /// </summary>
        public void ResetRimColor()
        {
            SetRimColor(_normalRimColor);
        }

        /// <summary>
        /// Resets the pinch color to normal.
        /// </summary>
        public void ResetPinchColor()
        {
            SetPinchColor(_normalPinchColor);
        }

        /// <summary>
        /// Sets the OnEnter color when the hand entering a proximal object.
        /// </summary>
        public void SetEnterColor()
        {
            SetRimColor(onEnterRimColor);
        }

        /// <summary>
        /// Sets the onError color when the hand is in incorrect state.
        /// </summary>
        public void SetErrorColor()
        {
            SetRimColor(onErrorRimColor);
        }

        #endregion
        
        private void Clear()
        {
            ResetRimColor();
            ResetPinchColor();
        }
        
        #region Error

        // TODO: Implement error when hand tracking lost 

        #endregion

        /// <summary>
        /// Calculates and update the pinch power value in the material.
        /// Pinch power is relative to the distance between index tip and thumb tip.
        /// </summary>
        private void UpdatePinchPower()
        {
            var hand = isLeftHand ? QCHTInput.WorldLeftHand : QCHTInput.WorldRightHand;

            if (hand == null)
            {
                _skinnedMeshRenderer.material.SetFloat(PinchPowerShaderId, 0f);
                return;
            }

            var distance = hand.GetFingerOppositionValue(FingerId.INDEX);
            _skinnedMeshRenderer.material.SetFloat(PinchPowerShaderId, 1f - distance);
        }

        /// <summary>
        /// Gets the normal colors from the current material.
        /// </summary>
        private void GetMaterialColors()
        {
            _normalRimColor = _skinnedMeshRenderer.material.GetColor(RimColorShaderId);
            _normalPinchColor = _skinnedMeshRenderer.material.GetColor(PinchColorShaderId);
        }
    }
}