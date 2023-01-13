/******************************************************************************
 * File: SessionSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    public sealed class SessionSubsystem : XRSessionSubsystem
    {
        public const string ID = "Spaces-SessionSubsystem";

        private class SessionProvider : XRSessionSubsystem.Provider
        {
            private BaseRuntimeFeature _underlyingFeature;
            private BaseRuntimeFeature UnderlyingFeature {
                get {
                    if (_underlyingFeature == null) {
                        _underlyingFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
                    }
                    return _underlyingFeature;
                }
            }

            public override Feature requestedFeatures { get => Feature.PositionAndRotation /* | Feature.ImageTracking | Feature.ObjectTracking | Feature.PlaneTracking */; }

            public override Feature currentTrackingMode { get => UnderlyingFeature?.IsSessionRunning == true ? Feature.PositionAndRotation : Feature.None; }

            public override TrackingState trackingState { get => UnderlyingFeature?.IsSessionRunning == true ? TrackingState.Tracking : TrackingState.None; }

            public override NotTrackingReason notTrackingReason { get => NotTrackingReason.None; }

            public override Feature requestedTrackingMode {
                get => Feature.PositionAndRotation;
                set {
                    if (value != Feature.PositionAndRotation) {
                        Debug.LogWarning("Session's tracking mode must be PositionAndRotation and can't be anything else.");
                    }
                }
            }
            public override int frameRate { get => 0; }

            public override bool matchFrameRateEnabled { get => false; }

            public override bool matchFrameRateRequested { get => false; }

            public override IntPtr nativePtr { get => IntPtr.Zero; }

            public override Guid sessionId {
                get {
                    return UnderlyingFeature != null ?
                        new Guid(UnderlyingFeature.SessionHandle.ToString().PadLeft(32, '0')) : new Guid();
                }
            }

            public override void Start() { }

            public override void Stop() { }

            public override void Destroy() { }

            public override Promise<SessionAvailability> GetAvailabilityAsync() {
                if (UnderlyingFeature != null && UnderlyingFeature.SystemIDHandle != 0) {
                    return Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.Supported | SessionAvailability.Installed);
                }

                return Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.None);
            }

            public override NativeArray<ConfigurationDescriptor> GetConfigurationDescriptors(Allocator allocator) {
                var nativeArray = new NativeArray<ConfigurationDescriptor>(1, allocator, NativeArrayOptions.UninitializedMemory);
                nativeArray[0] = new ConfigurationDescriptor(IntPtr.Zero, Feature.PositionAndRotation, 0);
                return nativeArray;
            }

            public override void OnApplicationPause() { }

            public override void OnApplicationResume() { }

            public override void Update(XRSessionUpdateParams updateParams, Configuration configuration) { }

            public override void Update(XRSessionUpdateParams updateParams) { }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor() {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo {
                id = ID,
                providerType = typeof(SessionProvider),
                subsystemTypeOverride = typeof(SessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }
    }
}