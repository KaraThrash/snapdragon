/******************************************************************************
 * File: ImageTrackingSampleController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class ImageTrackingSampleController : SampleController
    {
        public float timer;
        public Vector3 anchorPoint;
        private float panelSpeed = 1.0f;
        public List<GameObject> profiles;
        public ARTrackedImageManager arImageManager;

        [Serializable]
        public struct TrackableInfo
        {
            public Text TrackingStatusText;
            public Text[] PositionTexts;
        }
        public TrackableInfo[] trackableInfos;

        private Dictionary<TrackableId, TrackableInfo> _trackedImages = new Dictionary<TrackableId, TrackableInfo>();

        public override void OnEnable()
        {
            base.OnEnable();
            arImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            arImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var trackedImage in args.added)
            {
                _trackedImages.Add(trackedImage.trackableId, trackableInfos[0]);
                if (trackedImage.referenceImage.name == "Pizza")
                {
                    profiles[0].SetActive(true);
                }
            }

            foreach (var trackedImage in args.updated)
            {
                Vector3 position = trackedImage.transform.position;
                TrackableInfo info = _trackedImages[trackedImage.trackableId];

                var step = panelSpeed * Time.deltaTime; //calculate distance to move
                if (trackedImage.referenceImage.name == "HappyFace")
                {
                    //profiles[0].transform.position = Vector3.MoveTowards(profiles[0].transform.position, position, step);
                }
                //profiles[0].transform.position = Vector3.MoveTowards(profiles[0].transform.position, position, step);
                anchorPoint = position;
                info.TrackingStatusText.text = trackedImage.trackingState.ToString();
                info.PositionTexts[0].text = position.x.ToString("#0.00");
                info.PositionTexts[1].text = position.y.ToString("#0.00");
                info.PositionTexts[2].text = position.z.ToString("#0.00");
            }

            foreach (var trackedImage in args.removed)
            {
                StartCoroutine(SecondsCountdown(3f));
                TrackableInfo info = _trackedImages[trackedImage.trackableId];
                info.TrackingStatusText.text = "None";
                info.PositionTexts[0].text = "0.00";
                info.PositionTexts[1].text = "0.00";
                info.PositionTexts[2].text = "0.00";
                _trackedImages.Remove(trackedImage.trackableId);
            }
        }
        public Vector3 getAnchor()
        {

            //profiles[0].transform.position = anchorPoint;
            return anchorPoint;
        }
        IEnumerator SecondsCountdown(float delay)
        {
            yield return new WaitForSeconds(delay);
            profiles[0].SetActive(false);
        }

    }
}
