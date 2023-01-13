// /******************************************************************************
//  * File: QCHTInputModule.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal
{
    public sealed class QCHTInputModule : PointerInputModule
    {
        [SerializeField, Tooltip("Handle click when pinch down instead of pinch up")]
        private bool clickOnDown = true;

        [SerializeField]
        private bool selectionGrab;

        [NonSerialized]
        public QCHTGraphicRaycaster activeGraphicRaycaster;

        [Header("Dragging")]
        [Tooltip("Minimum pointer movement in degrees to start dragging")]
        public float angleDragThreshold = 1f;

        [Tooltip("Override ray transform for UI.")]
        [CanBeEmpty]
        public Transform LeftRayTransform;

        [CanBeEmpty]
        public Transform RightRayTransform;

        /// <summary>
        /// Pool for QCHTPointerEventData for ray based pointers
        /// Do the same thing as m_PointerData in PointerInputModule.
        /// </summary>
        private readonly Dictionary<int, QCHTPointerEventData> _qchtPointerData =
            new Dictionary<int, QCHTPointerEventData>();

        private QCHTDoublePointerEventData _doubleEventData;

        private readonly List<RaycastResult> _qchtRaycastResultCache = new List<RaycastResult>();

        /// <summary>
        /// Handles Mouse state. from PointerInputModule.
        /// </summary>
        private readonly MouseState _leftMouseState = new MouseState();

        private readonly MouseState _rightMouseState = new MouseState();

        #region InputModule Activation

        public override bool IsModuleSupported()
        {
            return true;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            return QCHTInput.IsHandDetected(true) || QCHTInput.IsHandDetected(false);
        }

        public override void ActivateModule()
        {
            base.ActivateModule();

            var toSelect = eventSystem.currentSelectedGameObject;

            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        #endregion

        #region Processing Events

        /// <summary>
        /// Process this InputModule.
        /// </summary>
        public override void Process()
        {
            var usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            UpdateQCHTPointerData();
            UpdateMouseEvent();
        }

        /// <summary>
        /// Process update event to the selected object.
        /// </summary>
        private bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        /// Process submit keys.
        /// </summary>
        private void SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return;

            var data = GetBaseEventData();

            if (QCHTInput.GestureDown(false, GestureId.PINCH))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (QCHTInput.Gesture(false, GestureId.GRAB))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
        }

        private void UpdateMouseEvent()
        {
            ProcessMouseEvent(_leftMouseState);
            ProcessMouseEvent(_rightMouseState);
            ProcessDoubleDraggingEvent(_leftMouseState, _rightMouseState);
        }

        /// <summary>
        /// Process all mouse events. This is the same as the StandaloneInputModule version except that
        /// it takes MouseState as a parameter, allowing it to be used for both Gaze and Mouse 
        /// pointers.
        /// </summary>
        private void ProcessMouseEvent(MouseState mouseData)
        {
            var pressed = mouseData.AnyPressesThisFrame();
            var released = mouseData.AnyReleasesThisFrame();
            var buttonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if (UseMouse(pressed, released, buttonData.buttonData))
            {
                // Process the right mouse button fully
                ProcessMousePress(buttonData);
                ProcessMove(buttonData.buttonData);
                ProcessDrag(buttonData.buttonData);

                if (!Mathf.Approximately(buttonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
                {
                    var scrollHandler =
                        ExecuteEvents.GetEventHandler<IScrollHandler>(
                            buttonData.buttonData.pointerCurrentRaycast.gameObject);
                    ExecuteEvents.ExecuteHierarchy(scrollHandler, buttonData.buttonData,
                        ExecuteEvents.scrollHandler);
                }
            }
        }

        private void ProcessDoubleDraggingEvent(MouseState leftMouseData, MouseState rightMouseData)
        {
            var leftEventData = leftMouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var rightEventData = rightMouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if (!(leftEventData.buttonData is QCHTPointerEventData qchtLeftData) ||
                !(rightEventData.buttonData is QCHTPointerEventData qchtRightData))
                return;

            // If dragging the same object in two hands then checks the double dragging handler
            if (!qchtLeftData.dragging || !qchtRightData.dragging ||
                qchtLeftData.pointerDrag != qchtRightData.pointerDrag)
            {
                if (_doubleEventData != null)
                    _doubleEventData.doubleDragging = false;
                return;
            }

            // Call begin if pressed this frame
            if (leftEventData.PressedThisFrame() || rightEventData.PressedThisFrame())
            {
                var pointerDragBegin = ExecuteEvents.GetEventHandler<IDoubleDragBeginHandler>(qchtLeftData.pointerDrag);

                if (pointerDragBegin)
                {
                    var pressPosition = Vector3.Lerp(qchtLeftData.pointerPressRaycast.worldPosition,
                        qchtRightData.pointerPressRaycast.worldPosition, 0.5f);

                    _doubleEventData ??= new QCHTDoublePointerEventData(eventSystem);
                    _doubleEventData.doubleDragPressPosition = pressPosition;
                    _doubleEventData.leftData = qchtLeftData;
                    _doubleEventData.rightData = qchtRightData;
                    _doubleEventData.doubleDragging = true;

                    ExecuteEvents.Execute(pointerDragBegin, _doubleEventData,
                        ExecuteEventsExtensions.pointerDoubleDragBeginHandler);
                }
            }

            if (!(_doubleEventData is {doubleDragging: true}))
                return;

            var doubleDragHandler = ExecuteEvents.GetEventHandler<IDoubleDragHandler>(qchtLeftData.pointerDrag);
            if (!doubleDragHandler)
                return;

            var doubleDragPosition = Vector3.Lerp(qchtLeftData.dragPosition, qchtRightData.dragPosition, 0.5f);
            _doubleEventData.doubleDragPosition = doubleDragPosition;

            ExecuteEvents.Execute(doubleDragHandler, _doubleEventData,
                ExecuteEventsExtensions.pointerDoubleDragHandler);
        }

        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        private void ProcessMousePress(MouseButtonEventData data)
        {
            var qchtEvent = data.buttonData as QCHTPointerEventData;

            if (qchtEvent == null)
                return;

            var currentOverGo = qchtEvent.pointerCurrentRaycast.gameObject;

            // Pointer Down
            if (data.PressedThisFrame())
            {
                qchtEvent.eligibleForClick = true;
                qchtEvent.delta = Vector2.zero;
                qchtEvent.dragging = false;
                qchtEvent.useDragThreshold = true;
                qchtEvent.pressPosition = qchtEvent.position;
                qchtEvent.pointerPressRaycast = qchtEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, qchtEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed =
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, qchtEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                var time = Time.unscaledTime;

                if (newPressed == qchtEvent.lastPress)
                {
                    var diffTime = time - qchtEvent.clickTime;

                    if (diffTime < 0.3f)
                        ++qchtEvent.clickCount;
                    else
                        qchtEvent.clickCount = 1;

                    qchtEvent.clickTime = time;
                }
                else
                {
                    qchtEvent.clickCount = 1;
                }

                qchtEvent.pointerPress = newPressed;
                qchtEvent.rawPointerPress = currentOverGo;
                qchtEvent.clickTime = time;
                qchtEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (clickOnDown && qchtEvent.pointerPress != null && qchtEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent, ExecuteEvents.pointerClickHandler);
                }

                if (qchtEvent.pointerDrag != null)
                    ExecuteEvents.Execute(qchtEvent.pointerDrag, qchtEvent,
                        ExecuteEvents.initializePotentialDrag);
            }

            // Pointer Up
            if (data.ReleasedThisFrame())
            {
                ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent, ExecuteEvents.pointerUpHandler);
                if (!clickOnDown)
                {
                    var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                    // Pointer Click and Drop events
                    if (qchtEvent.pointerPress == pointerUpHandler && qchtEvent.eligibleForClick)
                    {
                        ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent,
                            ExecuteEvents.pointerClickHandler);
                    }
                }

                if (qchtEvent.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, qchtEvent, ExecuteEvents.dropHandler);
                }

                qchtEvent.eligibleForClick = false;
                qchtEvent.pointerPress = null;
                qchtEvent.rawPointerPress = null;

                if (qchtEvent.pointerDrag != null && qchtEvent.dragging)
                    ExecuteEvents.Execute(qchtEvent.pointerDrag, qchtEvent, ExecuteEvents.endDragHandler);

                qchtEvent.dragging = false;
                qchtEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over something that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != qchtEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(qchtEvent, null);
                    HandlePointerExitAndEnter(qchtEvent, currentOverGo);
                }
            }

            if (currentOverGo != qchtEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(qchtEvent, currentOverGo);
            }
        }

        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            base.ProcessMove(pointerEvent);
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent,
                ExecuteEventsExtensions.pointerMoveHandler);
        }

        /// <summary>
        /// Exactly the same as the code from PointerInputModule, except that we call our own
        /// IsPointerMoving.
        /// 
        /// This would also not be necessary if PointerEventData.IsPointerMoving was virtual
        /// </summary>
        /// <param name="pointerEvent"></param>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!(pointerEvent is QCHTPointerEventData qchtEvent))
                return;

            var moving = IsPointerMoving(pointerEvent);

            if (moving && pointerEvent.pointerDrag != null
                       && !pointerEvent.dragging
                       && ShouldStartDrag(qchtEvent))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                qchtEvent.dragPosition = qchtEvent.worldSpaceRay.origin +
                                         qchtEvent.worldSpaceRay.direction *
                                         qchtEvent.pointerPressRaycast.distance;

                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                if (!(_doubleEventData is {doubleDragging: true}))
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        // walk up the tree till a common root between the last entered and the current entered is found
        // send exit events up to (but not including) the common root. Then send enter events up to
        // (but not including the common root).
        private new void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                var hoveredCount = currentPointerData.hovered.Count;

                for (var i = 0; i < hoveredCount; ++i)
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData,
                        ExecuteEvents.pointerExitHandler);

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            var commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                var t = currentPointerData.pointerEnter.transform;

                while (t)
                {
                    // if we reach the common root break out!
                    if (commonRoot && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;

            if (newEnterTarget)
            {
                var t = newEnterTarget.transform;
                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }

        /// <summary>
        /// Decide if mouse events need to be processed this frame. Same as StandloneInputModule except
        /// that the IsPointerMoving method from this class is used, instead of the method on PointerEventData
        /// </summary>
        private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
        {
            if (pressed || released || IsPointerMoving(pointerData) || pointerData.IsScrolling())
                return true;

            return false;
        }

        /// <summary>
        /// Clear pointer state for both types of pointer
        /// </summary>
        private new void ClearSelection()
        {
            var baseEventData = GetBaseEventData();

            foreach (var pointer in m_PointerData.Values)
            {
                // Clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            foreach (var pointer in _qchtPointerData.Values)
            {
                // Clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            m_PointerData.Clear();
            eventSystem.SetSelectedGameObject(null, baseEventData);
        }

        #endregion

        /// <summary>
        /// Convenience function for cloning QCHTPointerEventData
        /// </summary>
        /// <param name="from">Copy this value</param>
        /// <param name="to">to this object</param>
        private void CopyFromTo(QCHTPointerEventData from, QCHTPointerEventData to)
        {
            CopyFromTo(from, to as PointerEventData);
            to.isLeftHand = from.isLeftHand;
            to.worldSpaceRay = from.worldSpaceRay;
        }

        /// <summary>
        /// Convenience function for cloning PointerEventData
        /// </summary>
        /// <param name="from">Copy this value</param>
        /// <param name="to">to this object</param>
        private new void CopyFromTo(PointerEventData from, PointerEventData to)
        {
            to.position = from.position;
            to.delta = from.delta;
            to.scrollDelta = from.scrollDelta;
            to.pointerCurrentRaycast = from.pointerCurrentRaycast;
            to.pointerEnter = from.pointerEnter;
        }

        #region QCHTPointerEventData pool

        private void GetPointerData(int id, out QCHTPointerEventData data, bool create)
        {
            if (_qchtPointerData.TryGetValue(id, out data) || !create)
                return;

            data = new QCHTPointerEventData(eventSystem)
            {
                pointerId = id,
            };

            _qchtPointerData.Add(id, data);
        }

        #endregion

        /// <summary>
        /// State for a pointer controlled by a world space ray.
        /// </summary>
        /// <returns></returns>
        private void UpdateQCHTPointerData()
        {
            // Left
            UpdateMouseState(true);

            // Right
            UpdateMouseState(false);
        }

        private void UpdateMouseState(bool isLeftHand)
        {
            var mouseId = isLeftHand ? kMouseLeftId : kMouseRightId;
            GetPointerData(mouseId, out var data, true);
            data.Reset();
            
            data.button = PointerEventData.InputButton.Left;
            data.useDragThreshold = true;
            data.isLeftHand = isLeftHand;

            var mouseState = isLeftHand ? _leftMouseState : _rightMouseState;
            mouseState.SetButtonState(data.button, GetButtonState(isLeftHand), data);

            var physicsRaycaster = isLeftHand
                ? QCHTPhysicsRaycaster.LeftHandRaycaster
                : QCHTPhysicsRaycaster.RightHandRaycaster;

            var rayTransform = isLeftHand ? LeftRayTransform : RightRayTransform;

            if (rayTransform)
                data.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);
            else if (physicsRaycaster != null)
                data.worldSpaceRay = physicsRaycaster.Ray;

            // Performs raycast to find intersections with world
            eventSystem.RaycastAll(data, _qchtRaycastResultCache);
            var raycast = FindFirstRaycast(_qchtRaycastResultCache);
            data.pointerCurrentRaycast = raycast;
            _qchtRaycastResultCache.Clear();

            var qchtGraphicRaycaster = raycast.module as QCHTGraphicRaycaster;
            if (qchtGraphicRaycaster)
            {
                activeGraphicRaycaster = qchtGraphicRaycaster;
                var position = raycast.screenPosition;
                data.delta = position - data.position;
                data.position = position;
            }

            #region UpdateRay

            QCHTRayData.SetState(isLeftHand, RAY_STATE.RAY_STATE_NONE);
            
            var isOver = raycast.gameObject;
            if (isOver || _doubleEventData is {doubleDragging: true} || data.dragging)
            {
                QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_REST);
                QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_HOVER);

                if (isOver && data.pointerPress) 
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_SELECTED);
                else 
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_SELECTED);
                
                if (_doubleEventData is {doubleDragging: true} || data.dragging)
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_DRAG);
                else 
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_DRAG);
                
                if (isOver && QCHTRayData.GetState(isLeftHand) < RAY_STATE.RAY_STATE_DRAG)
                    QCHTRayData.SetEndPoint(isLeftHand, raycast.worldPosition);
                else if (QCHTRayData.GetState(isLeftHand) >= RAY_STATE.RAY_STATE_DRAG)
                    QCHTRayData.SetEndPoint(data.isLeftHand, data.dragPosition);
            }
            else
            {
                QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_HOVER);
                if (physicsRaycaster != null && physicsRaycaster.enabled)
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_REST);
                else 
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_REST);
            }

            var dragObj = data.pointerDrag;
            if (dragObj)
            {
                var rayData = dragObj.GetComponent<IRayDataProvider>();
                if (!ReferenceEquals(rayData, null))
                {
                    QCHTRayData.SetReticleImage(isLeftHand, rayData.SelectedRayData.ReticleSprite);
                    QCHTRayData.SetReticleColor(isLeftHand, rayData.SelectedRayData.ReticleColor);
                    QCHTRayData.SetRayColor(isLeftHand, rayData.SelectedRayData.RayColor);
                    QCHTRayData.SetReticleScaleFactor(isLeftHand, rayData.SelectedRayData.ReticleScaleFactor);
                    return;
                }
            }

            var enterObj = data.pointerEnter;
            if (enterObj)
            {
                var rayData = enterObj.GetComponent<IRayDataProvider>();
                if (!ReferenceEquals(rayData, null))
                {
                    QCHTRayData.SetReticleImage(isLeftHand, rayData.HoverRayData.ReticleSprite);
                    QCHTRayData.SetReticleColor(isLeftHand, rayData.HoverRayData.ReticleColor);
                    QCHTRayData.SetRayColor(isLeftHand, rayData.HoverRayData.RayColor);
                    QCHTRayData.SetReticleScaleFactor(isLeftHand, rayData.HoverRayData.ReticleScaleFactor);
                    return;
                }
            }

            QCHTRayData.SetReticleImage(isLeftHand, null);
            QCHTRayData.SetReticleColor(isLeftHand, null);
            QCHTRayData.SetRayColor(isLeftHand, null);
            QCHTRayData.SetReticleScaleFactor(isLeftHand, 1.0f);

            #endregion
        }

        /// <summary>
        /// New version of ShouldStartDrag implemented first in PointerInputModule. This version differs in that
        /// for ray based pointers it makes a decision about whether a drag should start based on the angular change
        /// the pointer has made so far, as seen from the camera. This also works when the world space ray is 
        /// translated rather than rotated, since the beginning and end of the movement are considered as angle from
        /// the same point.
        /// </summary>
        private bool ShouldStartDrag(QCHTPointerEventData pointerEvent)
        {
            if (!pointerEvent.useDragThreshold)
                return true;

            if (!pointerEvent.IsQCHTPointerEventData())
            {
                // Same as original behaviour for canvas based pointers
                return (pointerEvent.pressPosition - pointerEvent.position).sqrMagnitude >=
                       eventSystem.pixelDragThreshold * eventSystem.pixelDragThreshold;
            }

            // When it's not a screen space pointer we have to look at the angle it moved rather than the pixels distance
            var hand = pointerEvent.isLeftHand ? QCHTInput.WorldLeftHand : QCHTInput.WorldRightHand;
            var pressDir = (pointerEvent.pointerPressRaycast.worldPosition - hand.GetWristPosition()).normalized;
            var currentDir = pointerEvent.worldSpaceRay.direction.normalized;
            return Vector3.Dot(pressDir, currentDir) < Mathf.Cos(Mathf.Deg2Rad * (angleDragThreshold));
        }

        /// <summary>
        /// The purpose of this function is to allow us to switch between using the standard IsPointerMoving
        /// method for mouse driven pointers, but to always return true when it's a ray based pointer. 
        /// All real-world ray-based input devices are always moving so for simplicity we just return true
        /// for them. 
        /// 
        /// If PointerEventData.IsPointerMoving was virtual we could just override that in
        /// OVRRayPointerEventData.
        /// </summary>
        /// <param name="pointerEvent"></param>
        /// <returns></returns>
        private static bool IsPointerMoving(PointerEventData pointerEvent)
        {
            if (pointerEvent.IsQCHTPointerEventData())
                return true;

            return pointerEvent.IsPointerMoving();
        }

        /// <summary>
        /// Redefinition of IsPointerOverGameObject.
        /// Searching the last pointer enter in qcht pointer data list.  
        /// </summary>
        /// <param name="pointerId"></param>
        /// <returns></returns>
        public override bool IsPointerOverGameObject(int pointerId)
        {
            GetPointerData(pointerId, out var data, false);

            if (data != null)
                return data.pointerEnter != null;

            return false;
        }

        /// <summary>
        /// Get state of button corresponding to gaze pointer
        /// </summary>
        /// <returns></returns>
        private PointerEventData.FramePressState GetButtonState(bool isLeft)
        {
            var pressed = QCHTInput.GestureDown(isLeft, GestureId.PINCH);
            pressed |= selectionGrab && QCHTInput.GestureDown(isLeft, GestureId.GRAB);

            var released = QCHTInput.GestureUp(isLeft, GestureId.PINCH);
            pressed |= selectionGrab && QCHTInput.GestureUp(isLeft, GestureId.GRAB);

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;

            return PointerEventData.FramePressState.NotChanged;
        }
    }
}