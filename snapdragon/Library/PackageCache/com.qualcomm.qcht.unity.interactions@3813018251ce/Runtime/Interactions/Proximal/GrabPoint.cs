// /******************************************************************************
//  * File: GrabPoint.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using QCHT.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Proximal
{
    public sealed class GrabPoint : MonoBehaviour, IInteractionDataProvider
    {
        public enum SnapGestureId
        {
            None = GestureId.UNKNOWN,
            Pinch = GestureId.PINCH,
            Grab = GestureId.GRAB,
            Point = GestureId.POINT,
            OpenHand = GestureId.OPEN_HAND,
        }

        [SerializeField] private InteractableType interactionType = InteractableType.Free;
        [SerializeField] private SnapGestureId mainGesture = SnapGestureId.Grab;
        [SerializeField] private SnapGestureId secondaryGesture = SnapGestureId.None;
        [SerializeField, NonReorderable] private SnapData[] snapData;
        public IEnumerable<SnapData> SnapData => snapData;
        public InteractableType Type => interactionType;
        public GestureId GrabGesture => (GestureId) mainGesture;
        public GestureId SecondGrabGesture => (GestureId) secondaryGesture;

#if UNITY_EDITOR
        
        #region Menu

        [MenuItem("GameObject/QCHT/GrabPoint", false)]
        private static void CreateGrabPoint(MenuCommand menuCommand)
        {
            var go = new GameObject("GrabPoint");
            go.AddComponent<GrabPoint>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        #endregion

#endif
    }
}