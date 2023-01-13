/******************************************************************************
 * File: ImageTrackingSubsystem.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces
{
    internal class ImageTarget
    {
        public XRTrackedImage SubsystemImageTarget;

        public ulong ImageTargetHandle { get; }

        public ImageTarget(ulong id, XRTrackedImage trackedImage) {
            SubsystemImageTarget = trackedImage;
            ImageTargetHandle = id;
        }
    }

    internal class ImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        public const string ID = "Spaces-ImageTrackingSubsystem";

        private class ImageTrackingProvider : Provider
        {
            private ulong _imageTrackerHandle = 0;
            private ImageTrackingFeature _underlyingFeature;
            private RuntimeReferenceImageLibrary _imageLibrary;
            private int _maxNumberOfMovingImages = 0;

            private List<ImageTarget> _activeTrackedImages = new List<ImageTarget>();

            public override void Start() {
                _underlyingFeature = OpenXRSettings.Instance.GetFeature<ImageTrackingFeature>();
                _underlyingFeature.TryCreateImageTracker(out _imageTrackerHandle, _imageLibrary, _maxNumberOfMovingImages);
            }

            public override void Stop() => DestroyImageTracker();

            public override void Destroy() => DestroyImageTracker();

            public override TrackableChanges<XRTrackedImage> GetChanges(XRTrackedImage defaultTrackedImage, Allocator allocator) {
                // NOTE: Since method is called frequently, move initialization of lists to Start
                var addedTrackedImages = new List<XRTrackedImage>();
                var updatedTrackedImages = new List<XRTrackedImage>();
                var removedTrackableIds = new List<TrackableId>();

                if (!_underlyingFeature.TryLocateImageTargets(_imageTrackerHandle, _maxNumberOfMovingImages, out var newTrackedImages)) {
                    return new TrackableChanges<XRTrackedImage>(0, 0, 0, allocator, defaultTrackedImage);
                }

                foreach (var newImageTarget in newTrackedImages) {
                    bool canAdd = true;
                    foreach (var activeImageTarget in _activeTrackedImages) {
                        /* Check for added list */
                        if (newImageTarget.trackableId == activeImageTarget.SubsystemImageTarget.trackableId) {
                            canAdd = false;
                            /* Check for updated list */
                            if (newImageTarget.pose != activeImageTarget.SubsystemImageTarget.pose) {
                                updatedTrackedImages.Add(newImageTarget);
                                break;
                            }
                        }
                    }

                    if (canAdd) {
                        addedTrackedImages.Add(newImageTarget);
                        _activeTrackedImages.Add(new ImageTarget(0, newImageTarget));
                    }
                }

                /* Check for removed list */
                var toRemoveImages = new List<ImageTarget>();
                foreach (var activeImageTarget in _activeTrackedImages) {
                    bool canRemove = newTrackedImages.All(newImageTarget => activeImageTarget.SubsystemImageTarget.trackableId != newImageTarget.trackableId);

                    if (canRemove) {
                        removedTrackableIds.Add(activeImageTarget.SubsystemImageTarget.trackableId);
                        toRemoveImages.Add(activeImageTarget);
                    }
                }

                foreach (var imageToRemove in toRemoveImages) {
                    _activeTrackedImages.Remove(imageToRemove);
                }

                return TrackableChanges<XRTrackedImage>.CopyFrom(
                    new NativeArray<XRTrackedImage>(addedTrackedImages.ToArray(), allocator),
                    new NativeArray<XRTrackedImage>(updatedTrackedImages.ToArray(), allocator),
                    new NativeArray<TrackableId>(removedTrackableIds.ToArray(), allocator),
                    allocator);
            }

            public override RuntimeReferenceImageLibrary imageLibrary {
                set {
                    if (value != null) {
                        _imageLibrary = value;
                        if (_imageTrackerHandle != 0) {
                            Debug.LogWarning("Can't change the image library while subsystem is running.");
                        }
                    } else {
                        _imageLibrary = null;
                        DestroyImageTracker();
                    }
                }
            }

            private void DestroyImageTracker() {
                if (_imageTrackerHandle != 0 && _underlyingFeature.TryDestroyImageTracker(_imageTrackerHandle)) {
                    _imageTrackerHandle = 0;
                    _activeTrackedImages.Clear();
                }
            }

            public override RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary) {
                return new SpacesRuntimeReferenceImageLibrary(serializedLibrary);
            }

            public override int requestedMaxNumberOfMovingImages {
                get => _maxNumberOfMovingImages;
                set => _maxNumberOfMovingImages = value;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor() {
            XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo {
                id = ID,
                providerType = typeof(ImageTrackingProvider),
                subsystemTypeOverride = typeof(ImageTrackingSubsystem),
                supportsMovingImages = true,
                supportsMutableLibrary = false
            });
        }
    }
}