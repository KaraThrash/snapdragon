// /******************************************************************************
//  * File: HandInfo.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QCHT.Core;
using QCHT.Core.Extensions;

namespace QCHT.Interactions.Hands
{
    public class HandInfo : MonoBehaviour
    {
        public GameObject labelContainer;
        public Text handSideLabel;
        public Text gesture;
        public Text ratio;
        public bool IsLeftHand;

        void Update()
        {
            QCHTHand qchtHand = IsLeftHand ? QCHTInput.WorldLeftHand : QCHTInput.WorldRightHand;

            if (qchtHand.IsDetected)
            {
                transform.position =
                    GetLabelPosition(
                        qchtHand);
                labelContainer.SetActive(true);
                handSideLabel.text = qchtHand.IsLeft ? "LEFT" : "RIGHT";
                gesture.text = qchtHand.gesture.ToString();
                ratio.text = string.Format("RATIO: {0}", qchtHand.gestureRatio.ToString("0.00"));
            }
            else
            {
                labelContainer.SetActive(false);
            }
        }

        private Vector3 GetLabelPosition(QCHTHand hand)
        {
            Vector3 labelposition = Vector3.zero;

            Vector3[] mcps =
            {
                hand.GetPoint(QCHTPointId.INDEX_MCP),
                hand.GetPoint(QCHTPointId.MIDDLE_MCP),
                hand.GetPoint(QCHTPointId.RING_MCP),
                hand.GetPoint(QCHTPointId.PINKY_MCP),
                hand.GetPoint(QCHTPointId.THUMB_DIP)
            };


            float maxX = hand.IsLeft ? Mathf.Infinity : Mathf.NegativeInfinity;
            foreach (Vector3 point in mcps)
            {
                if (hand.IsLeft)
                {
                    if (point.x < maxX)
                    {
                        maxX = point.x;
                        labelposition = point;
                    }
                }
                else
                {
                    if (point.x > maxX)
                    {
                        maxX = point.x;
                        labelposition = point;
                    }
                }
            }

            return labelposition;
        }
    }
}