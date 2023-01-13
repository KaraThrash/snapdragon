// /******************************************************************************
//  * File: InteractionGridControlPoint.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal.ControlBox
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(ParticleSystem))]
    public class InteractionGridControlPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IRayDataProvider
    {
        public InteractionGrid.InteractionType Type;

        [SerializeField] private bool isCorner;

        [SerializeField] private Vector3 axis = Vector3.one;

        private Collider _pointCollider;
        private ParticleSystem _particleSystem;


        private ControlBoxSettings.HandleState NormalState;
        private ControlBoxSettings.HandleState HoverState;
        private ControlBoxSettings.HandleState SelectedState;
        private ParticleSystem.MainModule _psMainModule;

        private Vector3 _initialScale = Vector3.zero;
        private Vector3 _opposite;
        private Vector3 _originalPos;
        private Vector3 _originalGizmoPos;
        private Vector3 _diagDir;
        private Quaternion _originalGizmoRot;

        private bool _leftSelected;
        private bool _rightSelected;
        private bool IsSelected => _leftSelected || _rightSelected;

        private bool _leftHover;
        private bool _rightHover;
        private bool IsHovered => _leftHover || _rightHover;

        public delegate void HandleEventHandler(InteractionGridControlPoint handle);

        public event HandleEventHandler OnStartInteractingHandle;
        public event HandleEventHandler OnStopInteractingHandle;

        private Transform GridTransform => transform.parent;
        private Transform ControlBoxTransform => transform.parent.parent;

        private RayData _hoverRayData;
        public RayData HoverRayData => _hoverRayData;

        private RayData _selectedRayData;
        public RayData SelectedRayData => _selectedRayData;

        #region MonoBehaviour Functions

        private void Awake()
        {
            _pointCollider = GetComponent<Collider>();
            _particleSystem = GetComponent<ParticleSystem>();
            _psMainModule = _particleSystem.main;
        }

        #endregion

        #region IPointerHandlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = true;

            UpdateHandleAppearance(qchtEvent.isLeftHand);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = false;

            UpdateHandleAppearance(qchtEvent.isLeftHand);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = true;

            UpdateHandleAppearance(qchtEvent.isLeftHand);

            var t = transform;
            var localHandleBoxPosition = t.localPosition;
            _initialScale = ControlBoxTransform.localScale;
            _originalGizmoPos = ControlBoxTransform.position;
            _originalGizmoRot = ControlBoxTransform.rotation;
            _originalPos = GridTransform.TransformPoint(localHandleBoxPosition);
            _opposite = GridTransform.TransformPoint(-localHandleBoxPosition);
            _diagDir = (_opposite - _originalPos).normalized;

            OnStartInteractingHandle?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = false;

            UpdateHandleAppearance(qchtEvent.isLeftHand);

            OnStopInteractingHandle?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            switch (Type)
            {
                case InteractionGrid.InteractionType.Scale:
                    var initialDist = Vector3.Dot(qchtEvent.pointerPressRaycast.worldPosition - _opposite, _diagDir);
                    var currentDist = Vector3.Dot(qchtEvent.dragPosition - _opposite, _diagDir);
                    var scaleFactor = 1 + (currentDist - initialDist) / initialDist;
                    var targetScale = Vector3.one * Mathf.Clamp(0f, scaleFactor, scaleFactor);
                    var finalScale = _initialScale.Multiply(targetScale);
                    var dir = ControlBoxTransform.InverseTransformDirection(_originalGizmoPos - _opposite);
                    var newPosition = _opposite + ControlBoxTransform.TransformDirection(dir.Multiply(targetScale));
                    ControlBoxTransform.localScale = finalScale;
                    ControlBoxTransform.position = newPosition;
                    break;

                case InteractionGrid.InteractionType.Rotate:
                    var controlBoxPosition = ControlBoxTransform.position;
                    var initDir = Vector3
                        .ProjectOnPlane(qchtEvent.pointerPressRaycast.worldPosition - controlBoxPosition, axis)
                        .normalized;
                    var currentDir = Vector3.ProjectOnPlane(qchtEvent.dragPosition - controlBoxPosition, axis)
                        .normalized;
                    var goal = Quaternion.FromToRotation(initDir, currentDir) * _originalGizmoRot;
                    ControlBoxTransform.rotation = goal;
                    break;
            }

            // Forces end point to handler position
            QCHTRayData.SetEndPoint(qchtEvent.isLeftHand, transform.position);
        }

        #endregion

        public void SetStates(ControlBoxSettings.HandleState normal, ControlBoxSettings.HandleState hover,
            ControlBoxSettings.HandleState selected)
        {
            NormalState = normal;
            HoverState = hover;
            SelectedState = selected;
        }

        public void SetHoverRayData(RayData rayData)
        {
            _hoverRayData = rayData;
        }

        public void SetSelectedRayData(RayData rayData)
        {
            _selectedRayData = rayData;
        }

        public void ToggleVisibility(bool visible)
        {
            if (visible)
            {
                float particleSize;
                
                if (IsSelected)
                    particleSize = SelectedState.size;
                else if (IsHovered)
                    particleSize = HoverState.size;
                else
                    particleSize = NormalState.size;
                
                _psMainModule.startSize = particleSize;
                _particleSystem.Play();
            }
            else
            {
                _particleSystem.Stop();
            }
        }

        public void ToggleActivation(bool on)
        {
            _pointCollider.enabled = on;
        }

        public bool IsHandleFacingUser()
        {
            var t = transform.position;
            var h = QCHTManager.Instance.Head.position;
            var ht = (t - h).normalized;
            if (Vector3.Dot(ht, transform.forward) < 0) return true;
            if (Vector3.Dot(ht, transform.right) < 0) return true;
            if (isCorner && Vector3.Dot(ht, transform.up) < 0) return true;
            return false;
        }

        private void UpdateHandleAppearance(bool isLeft)
        {
            var isSelected = isLeft ? _leftSelected : _rightSelected;
            if (isSelected)
            {
                _psMainModule.startColor = SelectedState.color;
                _psMainModule.startSize = SelectedState.size;
                return;
            }

            var isHover = isLeft ? _leftHover : _rightHover;
            if (isHover)
            {
                _psMainModule.startColor = HoverState.color;
                _psMainModule.startSize = HoverState.size;
                return;
            }

            _psMainModule.startColor = NormalState.color;
            _psMainModule.startSize = NormalState.size;
        }
    }
}