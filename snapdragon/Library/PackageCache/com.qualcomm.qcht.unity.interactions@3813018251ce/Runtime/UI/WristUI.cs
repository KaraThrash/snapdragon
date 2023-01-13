// /******************************************************************************
//  * File: WristUI.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class WristUI : MonoBehaviour
    {
        [SerializeField] protected bool isLeftHand;

        [Header("Lerp")]
        [SerializeField] private bool lerpEnabled;
        [SerializeField] private float speed = 1f;

        [Header("Transformations")]
        [SerializeField] private Transform handTransform;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffset;

        private Canvas _canvas;

        #region MonoBehaviour Functions

        protected void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected void OnEnable()
        {
            Hide();
        }

        protected void Update()
        {
            Vector3 position = (handTransform.localToWorldMatrix * positionOffset);
            position += handTransform.position;

            transform.position =
                lerpEnabled ? Vector3.Lerp(transform.position, position, Time.deltaTime * speed) : position;
            transform.rotation = handTransform.rotation * Quaternion.Euler(rotationOffset);
        }

        protected void Show()
        {
            _canvas.enabled = true;
        }

        protected void Hide()
        {
            _canvas.enabled = false;
        }

        #endregion
    }
}