// /******************************************************************************
//  * File: QCHTRay.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    public class QCHTRay : MonoBehaviour
    {
        private const int TotalFadeFrameCount = 40;
        private const float FadeSpeed = 20f;
        private const float ReticleZOffset = 0.05f; // Slightly moves the reticle in head direction 

        [Tooltip("Determines the hand side")]
        [SerializeField] private bool isLeftHand;

        [Header("Laser settings")] [SerializeField]
        private LineRenderer lineRenderer;

        [Tooltip("Transform from which the ray start")] [SerializeField]
        private Transform startTransform;

        [Tooltip("Number of step quality of the laser")] [Range(2, 100)] [SerializeField]
        private int rayDefinition = 50;

        [Tooltip("Define the Ray lenght at rest state")] [Range(0.01f, 10f)] [SerializeField]
        private float restRayLength = 2.5f;

        [Tooltip("Raycast display threshold")] [SerializeField]
        private RAY_STATE laserActivationThreshold = RAY_STATE.RAY_STATE_REST;

        [Tooltip("Raycast bend activation threshold")] [SerializeField]
        private RAY_STATE laserBendActivationThreshold = RAY_STATE.RAY_STATE_SELECTED;

        [Tooltip("Regulate the intensity of raycast curve")] [Range(0.1f, 1f)] [SerializeField]
        private float laserBendRatio = 0.2f;

        [Tooltip("Regulate the velocity of raycast curve")] [Range(1f, 20f)] [SerializeField]
        private float laserBendSpeed = 5f;

        [Tooltip("Default ray color when no interaction is detected")] [SerializeField]
        private Color rayRestColor = Color.white;

        [Tooltip("Default ray color when hovering an object (if no override coming from the hovered object)")]
        [SerializeField]
        private Color rayHoverColor = Color.white;

        [Tooltip("Default ray color when selecting an object (if no override coming from the selected object)")]
        [SerializeField]
        private Color raySelectedColor = Color.white;

        [Header("Reticle settings")] [SerializeField]
        private SpriteRenderer reticleRenderer;

        [Tooltip("Reticle display threshold")] [SerializeField]
        private RAY_STATE reticleActivationThreshold = RAY_STATE.RAY_STATE_REST;

        [Tooltip("Default reticle sprite")]
        [SerializeField] private Sprite reticleSprite;

        [Tooltip("Default reticle color when no modifier is applied")] [SerializeField]
        private Color reticleRestColor = Color.white;

        [Tooltip("Default reticle color when no modifier is applied")] [SerializeField]
        private Color reticleHoverColor = Color.white;

        [Tooltip("Default reticle color when no modifier is applied")] [SerializeField]
        private Color reticleSelectedColor = Color.white;

        [Tooltip("Global scale of the reticle")] [Range(0f, 10f)] [SerializeField]
        private float reticleScaleMultiplier = 1f;

        [Header("Origin effect")]
        [SerializeField] private bool enableOriginEffect;
        [SerializeField] private Transform effectTransform;

        private Vector3 _previousStartPoint = Vector3.zero;
        private Vector3 _previousControlPoint = Vector3.zero;
        private Vector3 _endPoint;
        private Vector3 _startingReticleScale;
        private bool _isFadingIn;
        private int _currentFadeFrame;
        private bool _shouldFade;

        private Vector3[] _positions;

        #region MonoBehaviour Functions

        private void Start()
        {
            _startingReticleScale = reticleRenderer.transform.localScale;
        }

        private void OnDisable()
        {
            _previousControlPoint = Vector3.zero;
        }

        private void Update()
        {
            var rayData = QCHTRayData.GetRayData(isLeftHand);

            // Update Reticle
            reticleRenderer.enabled = rayData.State >= reticleActivationThreshold;
            reticleRenderer.sprite = rayData.ReticleImage ? rayData.ReticleImage : reticleSprite;

            if (rayData.ReticleColor != null)
                reticleRenderer.color = (Color) rayData.ReticleColor;
            else if (rayData.State == RAY_STATE.RAY_STATE_REST)
                reticleRenderer.color = reticleRestColor;
            else if (rayData.State < RAY_STATE.RAY_STATE_SELECTED)
                reticleRenderer.color = reticleHoverColor;
            else if (rayData.State >= RAY_STATE.RAY_STATE_SELECTED)
                reticleRenderer.color = reticleSelectedColor;


            if (rayData.State != RAY_STATE.RAY_STATE_NONE)
            {
                var t = reticleRenderer.transform;
                var zOffset = (rayData.EndPoint - QCHTManager.Instance.Head.position).normalized * ReticleZOffset;
                t.position = rayData.EndPoint - zOffset;
                t.LookAt(QCHTManager.Instance.Head);
                var distance = (t.position - rayData.StartPoint).magnitude;
                t.localScale = _startingReticleScale * (distance * rayData.ReticleScaleFactor * reticleScaleMultiplier);
            }

            // Update laser
            QCHTRayData.SetStartPoint(isLeftHand, startTransform.position);

            if (effectTransform)
            {
                effectTransform.gameObject.SetActive(enableOriginEffect);
                effectTransform.position = startTransform.position;
            }

            if (QCHTRayData.GetState(isLeftHand) <= RAY_STATE.RAY_STATE_REST)
                QCHTRayData.SetEndPoint(isLeftHand,
                    startTransform.position + startTransform.forward * restRayLength);

            UpdateEndPoint(rayData);
            reticleRenderer.transform.position = _endPoint;

            if (rayData.State < laserActivationThreshold)
            {
                lineRenderer.enabled = false;
                _previousControlPoint = Vector3.zero;
            }
            else
            {
                lineRenderer.enabled = true;

                if (rayData.RayColor != null)
                {
                    ApplyRayColor(rayData.RayColor.Value);
                }
                else
                {
                    if (rayData.State == RAY_STATE.RAY_STATE_REST)
                        ApplyRayColor(rayRestColor);
                    else if (rayData.State < RAY_STATE.RAY_STATE_SELECTED)
                        ApplyRayColor(rayHoverColor);
                    else if (rayData.State >= RAY_STATE.RAY_STATE_SELECTED)
                        ApplyRayColor(raySelectedColor);
                }

                UpdateCurve(rayData);
            }
        }

        #endregion

        #region Helpers

        private void ApplyRayColor(Color color)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        private void UpdateEndPoint(QCHTRayVisualData rayData)
        {
            if (_shouldFade && !_isFadingIn)
            {
                _shouldFade = false;
                _isFadingIn = true;
                _currentFadeFrame = 0;
            }

            if (_isFadingIn)
            {
                if (_currentFadeFrame >= TotalFadeFrameCount)
                {
                    _isFadingIn = false;
                    _endPoint = rayData.EndPoint;
                    return;
                }

                _endPoint = GetFadeEndPoint(rayData);
                _currentFadeFrame += 1;
            }
            else
            {
                _endPoint = rayData.EndPoint;
            }
        }

        private Vector3 GetFadeEndPoint(QCHTRayVisualData rayData)
        {
            var endPoint = rayData.StartPoint;
            if (_currentFadeFrame <= 5)
            {
                lineRenderer.widthMultiplier = 0;
            }
            else if (_currentFadeFrame > 5 && _currentFadeFrame <= 10)
            {
                endPoint = Vector3.Lerp(_endPoint, rayData.EndPoint, Time.deltaTime * FadeSpeed);
                lineRenderer.widthMultiplier += 0.2f;
            }
            else if (_currentFadeFrame > 10 && _currentFadeFrame <= 15)
            {
                endPoint = Vector3.Lerp(_endPoint, rayData.EndPoint, Time.deltaTime * FadeSpeed);
                lineRenderer.widthMultiplier -= 0.06f;
            }
            else if (_currentFadeFrame > 15)
            {
                endPoint = Vector3.Lerp(_endPoint, rayData.EndPoint, Time.deltaTime * FadeSpeed);

                if (!(lineRenderer.widthMultiplier > 0.1f))
                    return endPoint;

                var widthMultiplier = lineRenderer.widthMultiplier;
                widthMultiplier -= (widthMultiplier - 0.1f) / (TotalFadeFrameCount - _currentFadeFrame);
                lineRenderer.widthMultiplier = widthMultiplier;
            }

            return endPoint;
        }

        private void UpdateCurve(QCHTRayVisualData rayData)
        {
            var controlPoint = GetControlPoint(rayData.StartPoint, _endPoint,
                rayData.State >= laserBendActivationThreshold);
            DrawCurve(rayData.StartPoint, controlPoint, _endPoint);
        }

        #endregion

        #region CURVE DRAWING

        private void DrawCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
        {
            if (_positions == null || _positions.Length != rayDefinition)
                _positions = new Vector3[rayDefinition];

            lineRenderer.positionCount = rayDefinition;
            lineRenderer.GetPositions(_positions);
            for (var i = 1; i < rayDefinition + 1; i++)
            {
                var t = i / (float) rayDefinition;
                _positions[i - 1] = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
            }

            lineRenderer.SetPositions(_positions);
        }

        private Vector3 GetControlPoint(Vector3 startPoint, Vector3 endPoint, bool bend)
        {
            var handDirection = QCHTInput.GetHandDirection(isLeftHand);
            var controlPoint = startPoint.MidPoint(endPoint);
            var finalControlPoint = controlPoint;

            if (bend)
            {
                controlPoint += Vector3.up * laserBendRatio; /*+ Vector3.up * laserBendUp;*/
                finalControlPoint = _previousControlPoint != Vector3.zero
                    ? Vector3.Lerp(_previousControlPoint, controlPoint, Time.deltaTime * laserBendSpeed)
                    : controlPoint;
            }

            _previousControlPoint = finalControlPoint;

            return finalControlPoint;
        }

        private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var u = 1 - t;
            var tt = t * t;
            var uu = u * u;
            var p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;
            return p;
        }

        #endregion
    }
}