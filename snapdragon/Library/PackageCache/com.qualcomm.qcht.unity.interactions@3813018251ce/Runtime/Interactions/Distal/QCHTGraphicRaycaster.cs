// /******************************************************************************
//  * File: QCHTGraphicRaycaster.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using QCHT.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QCHT.Interactions.Distal
{
    /// <summary>
    /// Custom graphic raycaster for QCHT.
    /// Largely based on the official Unity's GraphicRaycaster. 
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class QCHTGraphicRaycaster : GraphicRaycaster, IPointerEnterHandler
    {
        [Header("Pointer")]
        [Tooltip("The canvas pointer prefab")]
        [SerializeField] private GameObject pointerPrefab;

        [Tooltip("Determines the pointer size")] [Range(0f, 100f)]
        [SerializeField] private float pointerSize = 28f;

        [Tooltip("Pointer sorting order from ui canvas order. UI canvas sorting order level + this sorting order")]
        [SerializeField, Min(0)] private int pointerSortingOrder = 1;

        private GameObject _pointerLeft;
        private GameObject _pointerRight;

        private SpriteRenderer _pointerLeftSpriteRenderer;
        private SpriteRenderer _pointerRightSpriteRenderer;

        /// <summary>
        /// Reference to the attached canvas.
        /// </summary>
        private Canvas _canvas;

        /// <summary>
        /// Gets the attached canvas.
        /// Find it if the reference doesn't exist.
        /// </summary>
        private Canvas canvas
        {
            get
            {
                if (_canvas != null)
                    return _canvas;

                _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }

        /// <summary>
        /// Returns the camera which screen positions will be calculated from.
        /// </summary>
        public override Camera eventCamera
        {
            get
            {
                var canvas = this.canvas;
                var renderMode = canvas.renderMode;
                if (renderMode == RenderMode.ScreenSpaceOverlay
                    || (renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
                    return null;

                return canvas.worldCamera ?? Camera.main;
            }
        }

        private struct RaycastHit
        {
            public Graphic Graphic;
            public Vector3 WorldPos;
        };

        [NonSerialized] private readonly List<RaycastHit> _raycastResults = new List<RaycastHit>();
        [NonSerialized] private static readonly List<RaycastHit> s_sortedGraphics = new List<RaycastHit>();

        protected override void OnEnable()
        {
            base.OnEnable();

            if (pointerPrefab)
            {
                _pointerLeft ??= Instantiate(pointerPrefab, transform);
                _pointerRight ??= Instantiate(pointerPrefab, transform);
                _pointerLeftSpriteRenderer = _pointerLeft.GetComponentInChildren<SpriteRenderer>();
                _pointerRightSpriteRenderer = _pointerRight.GetComponentInChildren<SpriteRenderer>();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!_pointerLeft)
                _pointerLeft.SetActive(false);

            if (!_pointerRight)
                _pointerRight.SetActive(false);
        }

        #region Graphic Raycaster

        /// <summary>
        /// Performs the raycast against the list of graphics associated with the Canvas.
        /// </summary>
        /// <param name="eventData">Current event data</param>
        /// <param name="resultAppendList">List of hit objects to append new results to.</param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            Raycast(resultAppendList, eventData.GetRay(), true, qchtEvent.isLeftHand);
        }

        #endregion

        /// <summary>
        /// Custom Raycast
        /// </summary>
        /// <param name="resultAppendList"></param>
        /// <param name="ray"></param>
        /// <param name="checkForBlocking"></param>
        /// <param name="isLeft"></param>
        private void Raycast(List<RaycastResult> resultAppendList, Ray ray, bool checkForBlocking, bool isLeft)
        {
            if (canvas == null)
                return;

            var hitDistance = float.MaxValue;

            if (checkForBlocking && blockingObjects != BlockingObjects.None)
            {
                var dist = eventCamera.farClipPlane;

                if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
                {
                    var hits = Physics.RaycastAll(ray, dist, m_BlockingMask);

                    if (hits.Length > 0 && hits[0].distance < hitDistance)
                    {
                        hitDistance = hits[0].distance;
                    }
                }

                if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
                {
                    var hits = Physics2D.GetRayIntersectionAll(ray, dist, m_BlockingMask);

                    if (hits.Length > 0 && hits[0].fraction * dist < hitDistance)
                    {
                        hitDistance = hits[0].fraction * dist;
                    }
                }
            }

            _raycastResults.Clear();

            var pointer = isLeft ? _pointerLeft : _pointerRight;
            if (pointer != null) pointer.SetActive(false);

            if (QCHTInput.IsHandDetected(isLeft))
                GraphicRaycast(ray, _raycastResults);

            for (var index = 0; index < _raycastResults.Count; index++)
            {
                var go = _raycastResults[index].Graphic.gameObject;
                var appendGraphic = true;

                if (ignoreReversedGraphics)
                {
                    // If we have a camera compare the direction against the cameras forward.
                    var cameraForward = ray.direction;
                    var dir = go.transform.rotation * Vector3.forward;
                    appendGraphic = Vector3.Dot(cameraForward, dir) > 0;
                }

                // Ignore points behind us (can happen with a canvas pointer)
                if (eventCamera.transform.InverseTransformPoint(_raycastResults[index].WorldPos).z <= 0)
                    appendGraphic = false;

                if (!appendGraphic)
                    continue;

                var distance = Vector3.Distance(ray.origin, _raycastResults[index].WorldPos);

                if (distance >= hitDistance)
                    continue;

                hitDistance = distance;

                var castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    index = resultAppendList.Count,
                    depth = _raycastResults[index].Graphic.depth,
                    worldPosition = _raycastResults[index].WorldPos,
                    worldNormal = transform.forward,
                    screenPosition = eventCamera.WorldToScreenPoint(_raycastResults[index].WorldPos,
                        Camera.MonoOrStereoscopicEye.Mono)
                };

                if (pointer != null)
                {
                    var sp = isLeft ? _pointerLeftSpriteRenderer : _pointerRightSpriteRenderer;
                    var t = pointer.transform;
                    t.localScale = Vector3.one * pointerSize;
                    SetPointerPosition(pointer, castResult);
                    pointer.SetActive(true);
                    sp.sortingOrder = canvas.sortingOrder + pointerSortingOrder;
                }

                resultAppendList.Add(castResult);
            }
        }

        private void GraphicRaycast(Ray ray, List<RaycastHit> results)
        {
            // Necessary for the event system
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            s_sortedGraphics.Clear();

            for (var i = 0; i < foundGraphics.Count; ++i)
            {
                var graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget)
                    continue;

                if (!RayIntersectsRectTransform(graphic.rectTransform, ray, out var worldPos))
                    continue;

                //Work out where this is on the screen for compatibility with existing Unity UI code
                Vector2 screenPos = eventCamera.WorldToScreenPoint(worldPos, Camera.MonoOrStereoscopicEye.Mono);

                // mask/image intersection - See Unity docs on eventAlphaThreshold for when this does anything
                if (!graphic.Raycast(screenPos, eventCamera))
                    continue;

                RaycastHit hit;
                hit.Graphic = graphic;
                hit.WorldPos = worldPos;
                s_sortedGraphics.Add(hit);
            }

            s_sortedGraphics.Sort((g1, g2) => g2.Graphic.depth.CompareTo(g1.Graphic.depth));
            results.AddRange(s_sortedGraphics);
        }

        private static void SetPointerPosition(GameObject pointer, RaycastResult raycastResult)
        {
            if (pointer == null)
                return;

            pointer.transform.position = raycastResult.worldPosition;
            pointer.transform.rotation = Quaternion.LookRotation(raycastResult.worldNormal);
        }

        /// <summary>
        /// Detects whether a ray intersects a RectTransform and if it does also 
        /// returns the world position of the intersection.
        /// </summary>
        /// <param name="rectTransform"> rect transform of the graphic.</param>
        /// <param name="ray"> the ray to test.</param>
        /// <param name="worldPos"> out world position if raycast is valid. </param> 
        /// <returns></returns>
        private static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos)
        {
            var corners = new Vector3[4]; // Not optimized, corners are re-calculated each raycast loop 
            rectTransform.GetWorldCorners(corners);
            var plane = new Plane(corners[0], corners[1], corners[2]); // Plane from 3 corners

            if (!plane.Raycast(ray, out var enter))
            {
                worldPos = Vector3.zero;
                return false;
            }

            var intersect = ray.GetPoint(enter);
            var left = corners[1] - corners[0];
            var bottom = corners[3] - corners[0];
            var lDot = Vector3.Dot(intersect - corners[0], left);
            var bDot = Vector3.Dot(intersect - corners[0], bottom);

            if (bDot < bottom.sqrMagnitude && lDot < left.sqrMagnitude && bDot >= 0 && lDot >= 0)
            {
                worldPos = corners[0] + lDot * left / left.sqrMagnitude + bDot * bottom / bottom.sqrMagnitude;
                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }

        #region IPointerEnterHandler

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.IsQCHTPointerEventData())
                return;

            var inputModule = EventSystem.current.currentInputModule as QCHTInputModule;

            if (inputModule == null)
                return;

            inputModule.activeGraphicRaycaster = this;
        }

        #endregion
    }
}