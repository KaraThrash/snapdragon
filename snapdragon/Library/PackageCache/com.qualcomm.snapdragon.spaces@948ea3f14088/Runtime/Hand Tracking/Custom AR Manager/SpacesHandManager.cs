/******************************************************************************
 * File: SpacesHandManager.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if QCHT_UNITY_CORE
using QCHT.Core;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ARSessionOrigin))]
    public sealed class SpacesHandManager : ARTrackableManager<XRHandTrackingSubsystem, XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider, XRTrackedHand, SpacesHand>
    {

        [SerializeField]
        private GameObject _handPrefab;

        public static SpacesHandManager instance;
        public SpacesHand LeftHand => subsystem != null ? GetSpacesHandByTrackableID(subsystem.LeftHand.trackableId) : null;
        public SpacesHand RightHand => subsystem != null ? GetSpacesHandByTrackableID(subsystem.RightHand.trackableId) : null;

        protected override void OnEnable() {
            base.OnEnable();
            instance = this;
#if QCHT_UNITY_CORE && !UNITY_EDITOR
            Debug.Log("Injecting QCHT SDK with custom SpacesQCHTSDK.");
            QCHTSDK.Instance = new SpacesQCHTSDK();
#endif
        }

        protected override void OnDisable() {
            base.OnDisable();
#if QCHT_UNITY_CORE && !UNITY_EDITOR
            QCHTSDK.Instance = null;
#endif
            instance = null;
        }

        public event Action<SpacesHandsChangedEventArgs> handsChanged;

        protected override GameObject GetPrefab() => _handPrefab;
        
        public GameObject HandPrefab {
            get => _handPrefab;
            set => _handPrefab = value;
        }

        protected override string gameObjectName => nameof(SpacesHand);

        protected override void OnAfterSetSessionRelativeData(SpacesHand spacesHand, XRTrackedHand xrHand) => spacesHand.UpdateHandData(subsystem);

        protected override void OnTrackablesChanged(List<SpacesHand> added, List<SpacesHand> updated, List<SpacesHand> removed) {
            if (handsChanged != null) {
                handsChanged(new SpacesHandsChangedEventArgs(added, updated, removed));
            }
        }

        private SpacesHand GetSpacesHandByTrackableID(TrackableId id) {
            trackables.TryGetTrackable(id, out SpacesHand hand);
            return hand;
        }
    }
}