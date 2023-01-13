// /******************************************************************************
//  * File: Keypoint.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections;
using UnityEngine;

namespace QCHT.Core
{
    public class Keypoint : MonoBehaviour
    {
        Material _material;
        Renderer _rend;
        Coroutine _currentFade;

        private float MAX_Z_DISPLAY_END = 0.65f + 0.5f; // 65 cm
        private float MAX_Z_DISPLAY_START = 0.62f + 0.5f; // 60 cm
        private bool _IsShown;

        void Start()
        {
            _rend = GetComponent<Renderer>();
            _material = _rend.material;
        }

        public void SetColor(Color c)
        {
            _material.color = c;
        }

        public void Update()
        {
            if (_IsShown)
            {
                Color c = _material.color;

                if (transform.localPosition.z > MAX_Z_DISPLAY_START)
                {
                    float fadeDelta = (transform.localPosition.z - MAX_Z_DISPLAY_START) /
                                      (MAX_Z_DISPLAY_END - MAX_Z_DISPLAY_START);
                    c.a = Mathf.Lerp(c.a, 1 - fadeDelta, Time.deltaTime * 5f);
                }
                else
                {
                    c.a = 1;
                }

                _material.color = c;
            }
        }
        
        public void Fade(bool show)
        {
            if (_IsShown != show)
            {
                if (_currentFade != null)
                {
                    StopCoroutine(_currentFade);
                }

                _currentFade = StartCoroutine(FadeTo(show ? 1 : 0, 0.1f));
                _IsShown = show;
            }
        }

        IEnumerator FadeTo(float targetOpacity, float duration)
        {
            Color color = _material.color;
            float startOpacity = color.a;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float blend = Mathf.Clamp01(t / duration);
                color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);
                _material.color = color;
                yield return null;
            }
        }

        public void ShowPoint(bool show)
        {
            _rend.enabled = show;
        }
    }
}