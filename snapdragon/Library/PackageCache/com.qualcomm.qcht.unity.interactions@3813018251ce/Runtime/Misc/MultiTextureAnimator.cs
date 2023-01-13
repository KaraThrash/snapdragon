// /******************************************************************************
//  * File: MultiTextureAnimator.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions
{
    public class MultiTextureAnimator : MonoBehaviour
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        [Serializable]
        private struct TextureAnimatorData
        {
            public int materialId;
            public Vector2 defaultOffset;
            public Vector2 speed;
        }

        [SerializeField] private TextureAnimatorData[] data;

        public bool allMaterials = true;

        private Renderer _rend;
        private Image _image;

        private void Awake()
        {
            _rend = GetComponent<Renderer>();

            if (!_rend)
            {
                _image = GetComponent<Image>();
            }

            if (data.Length == 0)
            {
                data = new[]
                {
                    new TextureAnimatorData()
                };
            }
        }

        private void Update()
        {
            if (_rend)
            {
                var materials = _rend.materials;

                if (allMaterials)
                {
                    var offset = data[0].defaultOffset + Time.time * data[0].speed;

                    foreach (var m in materials)
                    {
                        m.SetTextureOffset(MainTex, offset);
                    }
                }
                else
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        var id = data[i].materialId;
                        var material = materials[id];
                        var offset = data[i].defaultOffset + Time.time * data[i].speed;
                        material.SetTextureOffset(MainTex, offset);
                    }
                }
            }
            else if (_image)
            {
                var offset = data[0].defaultOffset + Time.time * data[0].speed;
                _image.materialForRendering.SetTextureOffset(MainTex, offset);
            }
        }

        private void OnValidate()
        {
            var rend = GetComponent<Renderer>();

            if (rend)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    var id = data[i].materialId;

                    if (id < 0)
                        id = 0;
                    else if (id >= rend.sharedMaterials.Length)
                        id = rend.sharedMaterials.Length - 1;

                    data[i].materialId = id;
                }
            }
            else
            {
                var image = GetComponent<Image>();

                if (!image)
                    return;

                for (var i = 0; i < data.Length; i++)
                {
                    data[i].materialId = 0;
                }
            }
        }
    }
}