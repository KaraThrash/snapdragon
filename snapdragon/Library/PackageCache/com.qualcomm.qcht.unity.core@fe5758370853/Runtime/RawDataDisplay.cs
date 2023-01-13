// /******************************************************************************
//  * File: RawDataDisplay.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Core
{
    public sealed class RawDataDisplay : MonoBehaviour
    {
        private static readonly int s_color = Shader.PropertyToID("_Color");

        [SerializeField] private bool isLeft = true;

        [Header("Display")]
        [SerializeField] private Mesh mesh;
        [SerializeField, Tooltip("Should enable GPU instancing for performance")]
        private Material material;
        [SerializeField] private bool useNormalizedColors = true;
        [SerializeField, Range(0.005f, 0.05f)] private float scale = 0.01f;

        /// <summary>
        /// Force disabling points display
        /// </summary>
        private bool _neverShowPoints;

        /// <summary>
        /// Point instances matrices
        /// </summary>
        private readonly Matrix4x4[] _pointsMatrix = new Matrix4x4[(int) QCHTPointId.POINT_COUNT];

        /// <summary>
        /// Tweak color in property blocks for GPU instancing.
        /// Used only when UseNormalized colors is checked
        /// </summary>
        private readonly MaterialPropertyBlock[] _propertyBlock =
            new MaterialPropertyBlock[(int) QCHTPointId.POINT_COUNT];

        #region MonoBehaviour Functions

        private void Update()
        {
            var hand = isLeft ? QCHTInput.LeftHand : QCHTInput.RightHand;

            if (_neverShowPoints || !hand.IsDetected)
                return;

            UpdatePointMatrices(hand);

            if (useNormalizedColors)
            {
                for (var i = 0; i < (int) QCHTPointId.POINT_COUNT; i++)
                {
                    _propertyBlock[i] ??= new MaterialPropertyBlock();
                    _propertyBlock[i].SetColor(s_color, QCHTColor.DebugColorFromPointID(i));

                    // Draws instance by instance but will be merge if the GPU instancing is enabled
                    Graphics.DrawMesh(mesh, _pointsMatrix[i], material, 0, null, 0, _propertyBlock[i]);
                }
            }
            else
            {
                // Directly renders instances in batch
                Graphics.DrawMeshInstanced(mesh, 0, material, _pointsMatrix, _pointsMatrix.Length);
            }
        }

        #endregion

        /// <summary>
        /// Updates the points matrices table before rendering
        /// </summary>
        private void UpdatePointMatrices(QCHTHand hand)
        {
            for (var i = 0; i < _pointsMatrix.Length; i++)
            {
                var t = QCHTManager.Instance.Head.TransformPoint(hand.points[i]);
                var r = hand.rotations[i];
                var s = Vector3.one * scale;
                _pointsMatrix[i].SetTRS(t, r, s);
            }
        }

        /// <summary>
        /// Toggle for never show display 
        /// </summary>
        public void ToggleDisplay()
        {
            _neverShowPoints = !_neverShowPoints;
        }
    }
}