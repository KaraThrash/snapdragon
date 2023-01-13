/******************************************************************************
 * File: SpacesRuntimeReferenceImageLibrary.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesRuntimeReferenceImageLibrary : RuntimeReferenceImageLibrary
    {
        private readonly List<XRReferenceImage> _images = new List<XRReferenceImage>();

        public SpacesRuntimeReferenceImageLibrary(XRReferenceImageLibrary serializedLibrary) {
            for (int i = 0; i < serializedLibrary.count; i++) {

                if (serializedLibrary[i].texture == null) {
                    Debug.LogWarning("XRReferenceImage '" + serializedLibrary[i].name + "' has no valid texture set.");
                    continue;
                }

                if (!serializedLibrary[i].specifySize || serializedLibrary[i].size == Vector2.zero) {
                    Debug.LogWarning("XRReferenceImage '" + serializedLibrary[i].name + "' does not have a specified physical size.");
                    continue;
                }

                _images.Add(serializedLibrary[i]);
            }
        }

        protected override XRReferenceImage GetReferenceImageAt(int index) {
            return _images[index];
        }

        public override int count => _images.Count;
    }
}