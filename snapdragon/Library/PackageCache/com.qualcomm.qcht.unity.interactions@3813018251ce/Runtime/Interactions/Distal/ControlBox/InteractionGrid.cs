// /******************************************************************************
//  * File: InteractionGrid.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class InteractionGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDoubleDragBeginHandler, IDoubleDragHandler,
        IRayDataProvider
    {
        public enum InteractionType
        {
            None,
            Move,
            Rotate,
            Scale
        }

        private const string LightLeftKeyword = "LIGHT_L";
        private const string LightRightKeyword = "LIGHT_R";
        private const string Selected = "SELECTED";

        private static readonly int s_lightPosLeft = Shader.PropertyToID("_LightPosL");
        private static readonly int s_lightPosRight = Shader.PropertyToID("_LightPosR");
        private static readonly int s_lightColor = Shader.PropertyToID("_LightColor");

        private ControlBoxSettings _settings;

        private Material _material;

        private InteractionGridControlPoint[] _handlers;

        private Vector3 _localLightPosLeft;
        private Vector3 _localLightPosRight;

        private InteractionGridControlPoint _currentHandle;
        private bool IsInteractingWithAnHandle => _currentHandle != null;

        private Transform ControlBoxTransform => transform.parent;

        private bool _leftSelected;
        private bool _rightSelected;
        private bool IsSelected => _leftSelected | _rightSelected;

        private bool _leftHover;
        private bool _rightHover;
        private bool IsHover => _leftHover || _rightHover;

        private Vector3 _initialControlBoxPosition;
        private Vector3 _initialLocalPositionDeltaLeft;
        private Vector3 _initialLocalPositionDeltaRight;
        private Vector3 _initialControlBoxScale;
        private Vector3 _offsetToControlBoxCenter;

        private float _smoothSpeed = ControlBoxSettings.DefaultSmoothSpeed;

        private RayData _hoverRayData;
        public RayData HoverRayData => _hoverRayData;

        private RayData _selectedRayData;
        public RayData SelectedRayData => _selectedRayData;

        #region MonoBehaviour Functions

        private void Awake()
        {
            _material = GetComponent<Renderer>().material;
            _handlers = GetComponentsInChildren<InteractionGridControlPoint>();
        }

        private void Start()
        {
            foreach (var handler in _handlers)
            {
                handler.ToggleVisibility(false);
                handler.ToggleActivation(false);
            }

            Deselect();
        }

        private void OnEnable()
        {
            foreach (var handle in _handlers)
            {
                handle.OnStartInteractingHandle += OnStartInteractingWithHandle;
                handle.OnStopInteractingHandle += OnStopInteractingWithHandle;
            }
        }

        private void OnDisable()
        {
            foreach (var handle in _handlers)
            {
                handle.OnStartInteractingHandle -= OnStartInteractingWithHandle;
                handle.OnStopInteractingHandle -= OnStopInteractingWithHandle;
            }
        }

        private void Update()
        {
            UpdateHandles();
            UpdateLightPositionInMaterial();
        }

        #endregion

        #region IPointerHandlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = true;

            if (qchtEvent.dragging)
                return;

            AddLight(qchtEvent.isLeftHand);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = false;

            if (qchtEvent.dragging)
                return;

            RemoveLight(qchtEvent.isLeftHand);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent) || qchtEvent.dragging)
                return;

            SetLightPosition(qchtEvent.isLeftHand, eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = true;

            ref var initialLocalPosition = ref qchtEvent.isLeftHand
                ? ref _initialLocalPositionDeltaLeft
                : ref _initialLocalPositionDeltaRight;

            var cbPosition = ControlBoxTransform.position;
            initialLocalPosition = cbPosition - qchtEvent.pointerPressRaycast.worldPosition;
            _initialControlBoxPosition = cbPosition;

            Select();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            ref var selected = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            selected = false;

            Deselect();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!(eventData is QCHTPointerEventData qchtEvent))
                return;

            var initialLocalPosition =
                qchtEvent.isLeftHand ? _initialLocalPositionDeltaLeft : _initialLocalPositionDeltaRight;

            var dragPosition = qchtEvent.dragPosition;
            var controlBoxPosition = ControlBoxTransform.position;
            var draggingOffset = initialLocalPosition;
            
            if (_settings.Constraint == ControlBoxSettings.CameraConstraint.Horizontal)
            {
                dragPosition.y = _initialControlBoxPosition.y;
                draggingOffset.y = 0;
            }
            else if (_settings.Constraint == ControlBoxSettings.CameraConstraint.Vertical)
            {
                dragPosition.x = _initialControlBoxPosition.x;
                draggingOffset.x = 0;
            }
            
            var targetPosition = dragPosition + draggingOffset;

            var dt = Time.deltaTime * _smoothSpeed;
            controlBoxPosition = Vector3.Lerp(controlBoxPosition, targetPosition, dt);
            ControlBoxTransform.position = controlBoxPosition;
            
            
            /*var targetRotation = ControlBoxTransform.rotation;
            if (Camera.main)
            {
                var forward = Camera.main.transform.forward; // Align towards
                forward = new Vector3(forward.x, 0.0f, forward.z).normalized;
                targetRotation = Quaternion.LookRotation(forward, ControlBoxTransform.up);
            }
            ControlBoxTransform.rotation = Quaternion.Lerp(ControlBoxTransform.rotation,
                targetRotation, dt);
           */
           
            QCHTRayData.SetEndPoint(qchtEvent.isLeftHand, controlBoxPosition - initialLocalPosition);
        }

        public void OnBeginDoubleDrag(QCHTDoublePointerEventData qchtEvent)
        {
            _initialControlBoxScale = ControlBoxTransform.localScale;
            _offsetToControlBoxCenter =
                ControlBoxTransform.InverseTransformVector(qchtEvent.doubleDragPressPosition -
                                                           ControlBoxTransform.position);
        }

        public void OnDoubleDrag(QCHTDoublePointerEventData qchtDoubleEventData)
        {
            var pressPosLeft = qchtDoubleEventData.leftData.pointerPressRaycast.worldPosition;
            var pressPosRight = qchtDoubleEventData.rightData.pointerPressRaycast.worldPosition;
            var dragPosLeft = qchtDoubleEventData.leftData.dragPosition;
            var dragPosRight = qchtDoubleEventData.rightData.dragPosition;
            var initDistance = Vector3.Distance(pressPosLeft, pressPosRight);
            var currentDistance = Vector3.Distance(dragPosLeft, dragPosRight);

            var scaleFactor = 1 + (currentDistance - initDistance) / initDistance;
            var targetScale = _initialControlBoxScale.Multiply(scaleFactor * Vector3.one);
            ControlBoxTransform.localScale = targetScale;
            ControlBoxTransform.position = Vector3.Lerp(ControlBoxTransform.position,
                qchtDoubleEventData.doubleDragPosition - ControlBoxTransform.TransformVector(_offsetToControlBoxCenter),
                Time.deltaTime * _smoothSpeed);

            // Force scale reticle
            QCHTRayData.SetReticleImage(true, _settings.Resize);
            QCHTRayData.SetReticleImage(false, _settings.Resize);
        }

        #endregion

        #region InteractionGridControlPoint callbacks

        private void OnStartInteractingWithHandle(InteractionGridControlPoint handle)
        {
            _currentHandle = handle;
            Select();
        }

        private void OnStopInteractingWithHandle(InteractionGridControlPoint handle)
        {
            _currentHandle = null;
            Deselect();
        }

        #endregion

        public void Setup(ControlBoxSettings settings)
        {
            _settings = settings;

            if (!_settings)
                return;

            _smoothSpeed = _settings.SmoothSpeed;

            foreach (var handler in _handlers)
            {
                handler.SetStates(settings.NormalState, settings.NormalState, settings.NormalState);

                var type = handler.Type;

                var sprite = type switch
                {
                    InteractionType.Move => _settings.Move,
                    InteractionType.Scale => _settings.Resize,
                    InteractionType.Rotate => _settings.Rotate,
                    _ => null
                };

                var hoverRayData = new RayData
                {
                    ReticleSprite = sprite,
                    ReticleColor = _settings.HoverReticleColor,
                    ReticleScaleFactor = _settings.HoverReticleSize,
                    RayColor = _settings.HoverRayColor
                };

                var selectedRayData = new RayData
                {
                    ReticleSprite = sprite,
                    ReticleColor = _settings.SelectedReticleColor,
                    ReticleScaleFactor = _settings.SelectedReticleSize,
                    RayColor = _settings.SelectedRayColor
                };

                handler.SetHoverRayData(hoverRayData);
                handler.SetSelectedRayData(selectedRayData);
            }

            var gridHoverRayData = new RayData
            {
                ReticleSprite = _settings.Move,
                ReticleColor = _settings.HoverReticleColor,
                ReticleScaleFactor = _settings.HoverReticleSize,
                RayColor = _settings.HoverRayColor
            };

            var gridSelectedRayData = new RayData
            {
                ReticleSprite = _settings.Move,
                ReticleColor = _settings.SelectedReticleColor,
                ReticleScaleFactor = _settings.SelectedReticleSize,
                RayColor = _settings.SelectedRayColor
            };

            _hoverRayData = gridHoverRayData;
            _selectedRayData = gridSelectedRayData;
        }

        private void Select()
        {
            _material.EnableKeyword(Selected);

            if (_settings)
                SetLightColor(_settings.SelectedColor);
        }

        private void Deselect()
        {
            _material.DisableKeyword(Selected);

            if (_settings)
                SetLightColor(_settings.HoverColor);
        }

        private void AddLight(bool isLeft)
        {
            _material.EnableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void RemoveLight(bool isLeft)
        {
            _material.DisableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void SetLightColor(Color color)
        {
            _material.SetColor(s_lightColor, color);
        }

        private void SetLightPosition(bool isLeft, Vector3 position)
        {
            ref var localPos = ref isLeft ? ref _localLightPosLeft : ref _localLightPosRight;
            localPos = ControlBoxTransform.InverseTransformPoint(position);
        }

        private void UpdateHandles()
        {
            var type = _settings.DisplayType;
            foreach (var handle in _handlers)
            {
                if (type == ControlBoxSettings.ControlBoxDisplayType.Never)
                {
                    handle.ToggleActivation(false);
                    handle.ToggleVisibility(false);
                    continue;
                }

                if (IsInteractingWithAnHandle)
                {
                    // Interacting show only selected
                    var eq = _currentHandle.Equals(handle);
                    handle.ToggleActivation(eq);
                    handle.ToggleVisibility(eq);
                }
                else if (IsSelected)
                {
                    handle.ToggleActivation(false);
                    handle.ToggleVisibility(false);
                }
                else
                {
                    handle.ToggleActivation(type == ControlBoxSettings.ControlBoxDisplayType.Always || IsHover);
                    handle.ToggleVisibility(handle.IsHandleFacingUser() &&
                                            (type == ControlBoxSettings.ControlBoxDisplayType.Always || IsHover));
                }
            }
        }

        private void UpdateLightPositionInMaterial()
        {
            if (_material.IsKeywordEnabled(LightLeftKeyword))
            {
                var pos = ControlBoxTransform.TransformPoint(_localLightPosLeft);
                _material.SetVector(s_lightPosLeft, pos);
            }

            if (_material.IsKeywordEnabled(LightRightKeyword))
            {
                var pos = ControlBoxTransform.TransformPoint(_localLightPosRight);
                _material.SetVector(s_lightPosRight, pos);
            }
        }
    }
}