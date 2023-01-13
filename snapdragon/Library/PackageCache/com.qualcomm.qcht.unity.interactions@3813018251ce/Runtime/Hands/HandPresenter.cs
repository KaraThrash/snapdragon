// /******************************************************************************
//  * File: HandPresenter.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections;
using QCHT.Core;
using QCHT.Interactions.Distal;
using QCHT.Interactions.VFF;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [Serializable]
    public enum HandInteractionType
    {
        Triggering,
        Colliding,
        None,
    }

    public sealed class HandPresenter : MonoBehaviour
    {
        private static readonly int s_globalAlpha = Shader.PropertyToID("_Alpha");
        private static readonly int s_userAlpha = Shader.PropertyToID("_OverrideAlpha");

        [Header("General")]
        [SerializeField] private HandPoseDriver mainHand;
        [SerializeField] private HandPoseDriver ghostHand;

        [SerializeField] private SkinnedMeshRenderer mainSkinnedMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer ghostSkinnedMeshRenderer;

        [SerializeField, Range(0.01f, 5f)] private float handScaleMultiplier = 1f;
        
        [Header("Proximal interaction")]
        [Tooltip(
            "Triggering: this one is used for distal and proximal interaction, but also for the control box and snapping features. \n \nColliding (for VFF): choose this interaction type if you want natural interaction using physics. \n \nNone: this one only displays a ghost hand avatar.")]
        [SerializeField] private HandInteractionType interactionType;

        [Header("Distal interaction with QCHT")]
        [SerializeField, Tooltip("Should enable physics raycast?\nIt needs a QCHTInputModule to process raycasting.")]
        private bool enablePhysicsRaycast = true;
        [SerializeField] private QCHTPhysicsRaycaster raycaster;

        [SerializeField, Tooltip("Should enable the qcht ray visual?")]
        private bool enableRaycastDisplay = true;
        [SerializeField] private QCHTRay ray;

        [Header("Hide Hand Avatar")]
        [Tooltip("Setup the fade-out duration when the hand is not detected anymore.")]
        [SerializeField] private float fadeDuration = 0.33f;

        [Header("VFF settings")]
        [SerializeField] private bool displayGhostHand = true;
        [Tooltip(
            "Determines the distance at which the ghost hand will be displayed once the default hand avatar is being physically constrained by the object.")]
        [SerializeField, MinMax(0.01f, 1f)]
        private Vector2 distanceBlendAlpha = new Vector2(0.01f, 1f);
        [SerializeField] private PhysicalHandConfiguration physicsHandConfiguration;
        [SerializeField] private HandPartPresenter rootPart;

        [Header("Skin")]
        [CanBeEmpty, SerializeField] private HandSkin defaultSkin;
        
        private MaterialPropertyBlock _mainMaterialPropertyBlock;
        private MaterialPropertyBlock _ghostMaterialPropertyBlock;
        private Coroutine _fadeRoutine;

        private bool _handDisplayAllowed = true;

        public void AllowHandDisplay(bool on)
        {
            if (_handDisplayAllowed == on) return;
            _handDisplayAllowed = on;
            if (_detected)
            {
                ActivateHand(on);
            }
        }

        private bool _detected = true;

        private bool IsDetected
        {
            set
            {
                if (_detected == value)
                    return;

                _detected = value;
                if (!_handDisplayAllowed) return;
                ActivateHand(_detected);
            }
        }

        public bool EnablePhysicsRaycast
        {
            set => enablePhysicsRaycast = value;
            get => enablePhysicsRaycast;
        }

        public bool EnableRayDisplay
        {
            set => enableRaycastDisplay = value;
            get => enableRaycastDisplay;
        }

        private HandSkin _handSkin;
        public HandSkin HandSkin
        {
            set
            {
                if (_handSkin == value)
                    return;
                
                ApplySkin(value);
            }

            get => _handSkin;
        }

        private void ActivateHand(bool on)
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(Fade(on ? 1f : 0f));
        }

        public HandPartPresenter Root => rootPart;

        #region MonoBehaviour Functions

        private void Awake()
        {
            _mainMaterialPropertyBlock = new MaterialPropertyBlock();
            _ghostMaterialPropertyBlock = new MaterialPropertyBlock();
            mainSkinnedMeshRenderer.GetPropertyBlock(_mainMaterialPropertyBlock);
            ghostSkinnedMeshRenderer.GetPropertyBlock(_ghostMaterialPropertyBlock);
            ApplySkin(defaultSkin);
        }

        private void Start()
        {
            SetupHandType(interactionType);
        }

        private void Update()
        {
            var isLeft = mainHand.HandModel.Type == HandType.Left;
            IsDetected = QCHTInput.IsHandDetected(isLeft);
            var canRaycast = _detected && QCHTInput.CanPhysicsRaycast(isLeft);

            if (raycaster)
            {
                var eRayR = enablePhysicsRaycast && canRaycast;
                raycaster.enabled = eRayR;
                raycaster.gameObject.SetActive(eRayR);
            }

            if (ray)
            {
                var eRayD = enableRaycastDisplay;
                ray.gameObject.SetActive(eRayD);
            }

            if (interactionType == HandInteractionType.Colliding)
            {
                var alpha = GetNormalizedDistanceInMinMaxRange();
                ghostHand.gameObject.SetActive(Root.IsColliding && _detected && alpha > 0);
                SetGhostHandAlpha(alpha);
            }

            mainHand.ScaleMultiplier = handScaleMultiplier;
            ghostHand.ScaleMultiplier = handScaleMultiplier;
        }

        #endregion

        #region Type

        public void SetupHandTypeFromId(int typeId)
        {
            try
            {
                SetupHandType((HandInteractionType) typeId);
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogError($"[HandPresenter:SetupHandType] Can't set type id {typeId}");
            }
        }

        public void SetupHandType(HandInteractionType handInteractionType)
        {
            switch (handInteractionType)
            {
                case HandInteractionType.Triggering:
                    SetupTriggering();
                    break;

                case HandInteractionType.Colliding:
                    SetupColliding();
                    break;

                case HandInteractionType.None:
                    SetupNone();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            interactionType = handInteractionType;
        }

        private void SetupNone()
        {
            mainHand.gameObject.SetActive(_detected);
            ghostHand.gameObject.SetActive(false);

            var handPartPresenters = rootPart.GetComponentsInChildren<HandPartPresenter>();

            foreach (var part in handPartPresenters)
            {
                part.Type = HandPartColliderType.None;
                rootPart.TriggerCollisionsDetection(false);
            }
        }

        private void SetupTriggering()
        {
            mainHand.gameObject.SetActive(_detected);
            ghostHand.gameObject.SetActive(false);

            var handPartPresenters = rootPart.GetComponentsInChildren<HandPartPresenter>();

            foreach (var part in handPartPresenters)
            {
                part.Type = HandPartColliderType.Triggering;
                rootPart.TriggerCollisionsDetection(true);
            }
        }

        private void SetupColliding()
        {
            mainHand.gameObject.SetActive(_detected);
            ghostHand.gameObject.SetActive(_detected && displayGhostHand);

            var handPartPresenters = rootPart.GetComponentsInChildren<HandPartPresenter>();

            foreach (var part in handPartPresenters)
            {
                part.Configuration = physicsHandConfiguration;
                part.Type = HandPartColliderType.Colliding;
                part.TriggerCollisionsDetection(true);
            }
        }

        public void SetMainHandAlpha(float alpha)
        {
            _mainMaterialPropertyBlock.SetFloat(s_globalAlpha, alpha);
            mainSkinnedMeshRenderer.SetPropertyBlock(_mainMaterialPropertyBlock);
        }

        public void SetGhostHandAlpha(float alpha)
        {
            _ghostMaterialPropertyBlock.SetFloat(s_globalAlpha, alpha);
            ghostSkinnedMeshRenderer.SetPropertyBlock(_ghostMaterialPropertyBlock);
        }
        
        public void SetMainHandOptionalAlpha(float alpha)
        {
            _mainMaterialPropertyBlock.SetFloat(s_userAlpha, alpha);
            mainSkinnedMeshRenderer.SetPropertyBlock(_mainMaterialPropertyBlock);
        }

        public void SetGhostHandOptionalAlpha(float alpha)
        {
            _ghostMaterialPropertyBlock.SetFloat(s_userAlpha, alpha);
            ghostSkinnedMeshRenderer.SetPropertyBlock(_ghostMaterialPropertyBlock);
        }

        #endregion

        #region Skin

        public void ApplySkin(HandSkin skin)
        {
            _handSkin = skin;
            
            if (_handSkin == null)
            {
                mainSkinnedMeshRenderer.material = null;
                mainSkinnedMeshRenderer.sharedMesh = null;
                
                ghostSkinnedMeshRenderer.material = null;
                ghostSkinnedMeshRenderer.sharedMesh = null;
                return;
            }
            
            mainSkinnedMeshRenderer.sharedMesh = _handSkin.MainMesh;
            mainSkinnedMeshRenderer.material = _handSkin.MainMaterial;

            ghostSkinnedMeshRenderer.sharedMesh = !_handSkin.GhostMesh ? _handSkin.GhostMesh : _handSkin.MainMesh;
            ghostSkinnedMeshRenderer.material = _handSkin.GhostMaterial;
        }

        #endregion

        private float GetNormalizedDistanceInMinMaxRange()
        {
            var distance = Vector3.Distance(mainHand.RootTransform.position, ghostHand.RootTransform.position);
            distance = (distance - distanceBlendAlpha.x) / distanceBlendAlpha.y - distanceBlendAlpha.x;
            return Mathf.Clamp01(distance);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (targetAlpha > 0)
            {
                switch (interactionType)
                {
                    case HandInteractionType.Triggering:
                        mainHand.gameObject.SetActive(_detected);
                        ghostHand.gameObject.SetActive(false);
                        break;

                    case HandInteractionType.Colliding:
                        mainHand.gameObject.SetActive(_detected);
                        ghostHand.gameObject.SetActive(_detected && displayGhostHand);
                        break;

                    case HandInteractionType.None:
                        mainHand.gameObject.SetActive(_detected);
                        ghostHand.gameObject.SetActive(false);
                        break;
                }

                mainHand.enabled = true;
                ghostHand.enabled = true;
            }
            else
            {
                if (_handDisplayAllowed)
                {
                    mainHand.enabled = false;
                    ghostHand.enabled = false;
                }
            }

            mainSkinnedMeshRenderer.GetPropertyBlock(_mainMaterialPropertyBlock);
            ghostSkinnedMeshRenderer.GetPropertyBlock(_ghostMaterialPropertyBlock);
            var startMainAlpha = _mainMaterialPropertyBlock.GetFloat(s_globalAlpha);
            var startGhostAlpha = _ghostMaterialPropertyBlock.GetFloat(s_globalAlpha);
            float time = 0;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                var dt = time / fadeDuration;
                var mainAlpha = Mathf.Lerp(startMainAlpha, targetAlpha, dt);
                var ghostAlpha = Mathf.Lerp(startGhostAlpha, targetAlpha, dt);
                SetMainHandAlpha(mainAlpha);
                SetGhostHandAlpha(ghostAlpha);
                yield return null;
            }

            if (targetAlpha > 0)
                yield break;

            if (!_handDisplayAllowed)
                yield break;

            mainHand.gameObject.SetActive(false);
            ghostHand.gameObject.SetActive(false);
        }
    }
}