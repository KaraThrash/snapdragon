/******************************************************************************
 * File: XRHandTrackingSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public class XRHandTrackingSubsystem : TrackingSubsystem<XRTrackedHand, XRHandTrackingSubsystem, XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider>
    {
        public XRTrackedHand LeftHand => provider.GetLeftHand();
        public XRTrackedHand RightHand => provider.GetRightHand();

        public override TrackableChanges<XRTrackedHand> GetChanges(Allocator allocator)
            => provider.GetChanges(XRTrackedHand.defaultValue, allocator);

        public void GetJoints(TrackableId trackableId, ref Pose[] joints)
            => provider.GetJoints(trackableId, ref joints);

        public void GetGestureData(TrackableId trackableId, ref int gesture, ref float gestureRatio, ref float flipRatio)
            => provider.GetGestureData(trackableId, ref gesture, ref gestureRatio, ref flipRatio);

        public abstract class Provider : SubsystemProvider<XRHandTrackingSubsystem>
        {
            public abstract XRTrackedHand GetLeftHand();
            public abstract XRTrackedHand GetRightHand();

            public abstract TrackableChanges<XRTrackedHand> GetChanges(XRTrackedHand defaultHand, Allocator allocator);

            public abstract void GetJoints(TrackableId trackableId, ref Pose[] joints);

            public abstract void GetGestureData(TrackableId trackableId, ref int gesture, ref float gestureRatio, ref float flipRatio);
        }

    }
}