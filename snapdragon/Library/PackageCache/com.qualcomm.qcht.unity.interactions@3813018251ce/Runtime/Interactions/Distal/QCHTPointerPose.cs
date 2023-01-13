// /******************************************************************************
//  * File: QCHTPointerPose.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    public sealed class QCHTPointerPose : MonoBehaviour
    {
        public enum LaserPosition
        {
            Legacy,
            Palm,
            IndexTip,
            IndexDip,
            MiddleOfIndexTipAndIndexDip,
            MiddleOfIndexTipAndThumbTip,
            MiddleOfIndexTipAndThumbTipAndPalm
        }

        public bool IsLeftHand;
        public LaserPosition Position;

        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffset;

        [Space]
        [SerializeField] private Transform palmTransform;

        [SerializeField] private Transform indexTipTransform;
        [SerializeField] private Transform indexDipTransform;
        [SerializeField] private Transform thumbTipTransform;

        public void LateUpdate()
        {
            var t = transform;
            Vector3 posOffset;
            Vector3 shoulderPoint;

            if (Position == LaserPosition.Legacy)
            {
                var hand = IsLeftHand ? QCHTInput.WorldLeftHand : QCHTInput.WorldRightHand;

                if (!hand.IsDetected)
                    return;
                
                posOffset = QCHTManager.Instance.Head.rotation * hand.GetRotation(QCHTPointId.PALM_CENTER) * positionOffset;
                t.position = hand.GetPalmPosition() + posOffset;
                shoulderPoint = GetShoulderPosition(0.15f); // Legacy offset
            }
            else
            {
                posOffset = palmTransform.localToWorldMatrix * positionOffset;
                t.position = GetHandPoint(Position) + posOffset;
                shoulderPoint = GetShoulderPosition(0.05f);
            }

            var position = t.position;
            var direction = Vector3.Normalize(position - shoulderPoint);
            t.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        private Vector3 GetShoulderPosition(float offset)
        {
            var bottomHead = QCHTManager.Instance.Head.position - QCHTManager.Instance.Head.up * offset;
            var shoulderOffset = IsLeftHand ? -QCHTManager.Instance.Head.right : QCHTManager.Instance.Head.right;
            return bottomHead + shoulderOffset * offset;
        }

        private Vector3 GetHandPoint(LaserPosition laserPosition)
        {
            switch (laserPosition)
            {
                case LaserPosition.Palm:
                    return palmTransform.position;

                case LaserPosition.IndexTip:
                    return indexTipTransform.position;

                case LaserPosition.IndexDip:
                    return indexDipTransform.position;

                case LaserPosition.MiddleOfIndexTipAndIndexDip:
                    return (indexTipTransform.position + indexDipTransform.position) * .5f;

                case LaserPosition.MiddleOfIndexTipAndThumbTip:
                    return (thumbTipTransform.position + indexTipTransform.position) * 0.5f;

                case LaserPosition.MiddleOfIndexTipAndThumbTipAndPalm:
                    return (thumbTipTransform.position + indexTipTransform.position + palmTransform.position) * 0.333f;

                default:
                    return palmTransform.position;
            }
        }
    }
}