/******************************************************************************
 * File: XRTrackedHand.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public struct XRTrackedHand : ITrackable, IEquatable<XRTrackedHand>
    {
        private TrackableId _id;
        private Pose _pose;
        private TrackingState _trackingState;
        private static readonly XRTrackedHand _defaultValue = new XRTrackedHand(TrackableId.invalidId);

        public TrackableId trackableId => _id;
        public IntPtr nativePtr => IntPtr.Zero;
        public Pose pose => _pose;
        public TrackingState trackingState => _trackingState;

        public static XRTrackedHand defaultValue => _defaultValue;

        public XRTrackedHand(TrackableId trackableId, TrackingState trackingState = TrackingState.None) {
            _id = trackableId;
            _pose = Pose.identity;
            _trackingState = trackingState;
        }

        internal void UpdatePoseAndTrackedState(Pose pose, TrackingState trackingState) {
            _pose = pose;
            _trackingState = trackingState;
        }

        public override int GetHashCode() => _id.GetHashCode() * 4999559 + ((int)_trackingState).GetHashCode();

        public bool Equals(XRTrackedHand other) => _id.Equals(other._id) && _trackingState == other._trackingState;

        public override bool Equals(object obj) => obj is XRTrackedHand && Equals((XRTrackedHand)obj);

        public static bool operator==(XRTrackedHand lhs, XRTrackedHand rhs) => lhs.Equals(rhs);

        public static bool operator!=(XRTrackedHand lhs, XRTrackedHand rhs) => !lhs.Equals(rhs);
    }
}