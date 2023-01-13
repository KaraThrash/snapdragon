// /******************************************************************************
//  * File: TextureAnimator.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions
{
    public sealed class TextureAnimator : MonoBehaviour
    {
        private static readonly int s_mainTex = Shader.PropertyToID("_MainTex");

        public float xSpeed;
        public float ySpeed;
        public float defaultOffsetX;
        public float defaultOffsetY;

        private Renderer _rend;
        private Image _image;

        private Vector2 _uvOffset = Vector2.zero;

        #region MonoBehaviour Functions

        private void Start()
        {
            _rend = GetComponent<Renderer>();

            if (_rend == null)
                _image = GetComponent<Image>();
        }

        private void Update()
        {
            _uvOffset.x = defaultOffsetX + Time.time * xSpeed;
            _uvOffset.y = defaultOffsetY + Time.time * ySpeed;

            if (_rend)
                _rend.material.SetTextureOffset(s_mainTex, _uvOffset);
            else
                _image.materialForRendering.SetTextureOffset(s_mainTex, _uvOffset);
        }

        #endregion
    }
}