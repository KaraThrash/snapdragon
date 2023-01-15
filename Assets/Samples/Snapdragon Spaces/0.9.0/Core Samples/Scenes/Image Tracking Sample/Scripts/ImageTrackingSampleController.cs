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
        public List<float> timer = new List <float>(3);

        public GameObject testObject;
        public Vector3 anchorPoint;

        public List<GameObject> profiles;
        public ARTrackedImageManager arImageManager;

        public bool testParenting;
        public bool deletePrefab;
        public bool startTimer;
        public bool stopTimer;


        void FixedUpdate()
        {
            if (testParenting == true)
            {
                testParentingFunction();
                testParenting = false;
            }
            if (deletePrefab)
            {
                StartCoroutine(SecondsCountdown(1f));
                deletePrefab = false;
            }
            if (startTimer)
            {
                StartCoroutine(runSocialTimer0());
            }
            if (stopTimer)
            {
                StopCoroutine(runSocialTimer0());
            }
        }

        public void testParentingFunction()
        {
            //var testObjectInstance = Instantiate(testObject);
            var panelInstance = Instantiate(profiles[0]);

            //Find ar session component
            var cam = GameObject.Find("AR Session Origin");
            panelInstance.transform.parent = cam.transform;
            panelInstance.transform.localPosition = new Vector3(0,0,1) * 1.3f;
        }

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
                    StartCoroutine(runSocialTimer0());
                    testParentingFunction();
                }
            }

            foreach (var trackedImage in args.updated)
            {
                Vector3 position = trackedImage.transform.position;
                TrackableInfo info = _trackedImages[trackedImage.trackableId];

                //var cam = Camera.main.transform;
                //panelInstance.transform.position = cam.position + cam.forward;
                //testObjectInstance.transform.position = cam.position + cam.forward;

                if (trackedImage.referenceImage.name == "HappyFace")
                {
                    //profiles[0].transform.position = Vector3.MoveTowards(profiles[0].transform.position, position, step);
                }
                //profiles[0].transform.position = Vector3.MoveTowards(profiles[0].transform.position, position, step);
                anchorPoint = position;
                profiles[0].SetActive(true);
                info.TrackingStatusText.text = trackedImage.trackingState.ToString();
                info.PositionTexts[0].text = position.x.ToString("#0.00");
                info.PositionTexts[1].text = position.y.ToString("#0.00");
                info.PositionTexts[2].text = position.z.ToString("#0.00");
            }

            foreach (var trackedImage in args.removed)
            {
                if (trackedImage.referenceImage.name == "Pizza")
                {
                    StartCoroutine(SecondsCountdown(3f));
                    StopCoroutine(runSocialTimer0());
                }

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
        // seperate coroutine timers
        IEnumerator runSocialTimer0()
        {
            while (true)
            {
                timer[0] += Time.deltaTime;
            }
        }

        IEnumerator runSocialTimer1()
        {
            while (true)
            {
                timer[1] += Time.deltaTime;
            }
        }

        IEnumerator runSocialTimer2()
        {
            while (true)
            {
                timer[2] += Time.deltaTime;
            }
        }
    }
}