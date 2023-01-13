// /******************************************************************************
//  * File: QCHTRayData.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Distal
{
    public sealed class QCHTRayData
    {
        #region Private Properties

        private QCHTRayVisualData _leftRayData = QCHTRayVisualData.Default;
        private QCHTRayVisualData _rightRayData = QCHTRayVisualData.Default;

        #endregion

        #region Singleton

        private static QCHTRayData s_instance;
        private static QCHTRayData Instance => s_instance ??= new QCHTRayData();

        #endregion

        #region Helpers

        private static void SaveRayData(QCHTRayVisualData data, bool isLeftHand)
        {
            if (isLeftHand) Instance._leftRayData = data;
            else Instance._rightRayData = data;
        }

        #endregion

        #region Static Data accessors

        public static QCHTRayVisualData GetRayData(bool isLeftHand)
        {
            return isLeftHand ? Instance._leftRayData : Instance._rightRayData;
        }

        public static void SetState(bool isLeftHand, RAY_STATE state)
        {
            var data = GetRayData(isLeftHand);
            data.State = state;
            SaveRayData(data, isLeftHand);
        }

        public static void AddState(bool isLeftHand, RAY_STATE state)
        {
            var data = GetRayData(isLeftHand);
            data.State |= state;
            SaveRayData(data, isLeftHand);
        }

        public static void RemoveState(bool isLeftHand, RAY_STATE state)
        {
            var data = GetRayData(isLeftHand);
            data.State &= ~state;
            SaveRayData(data, isLeftHand);
        }

        public static RAY_STATE GetState(bool isLeftHand)
        {
            return GetRayData(isLeftHand).State;
        }

        public static void SetStartPoint(bool isLeftHand, Vector3 startPoint)
        {
            var data = GetRayData(isLeftHand);
            data.StartPoint = startPoint;
            SaveRayData(data, isLeftHand);
        }

        public static void SetEndPoint(bool isLeftHand, Vector3 endPoint)
        {
            var data = GetRayData(isLeftHand);
            data.EndPoint = endPoint;
            SaveRayData(data, isLeftHand);
        }

        public static void SetReticleImage(bool isLeftHand, Sprite reticleImage)
        {
            var data = GetRayData(isLeftHand);
            data.ReticleImage = reticleImage;
            SaveRayData(data, isLeftHand);
        }

        public static void SetReticleScaleFactor(bool isLeftHand, float scale)
        {
            var data = GetRayData(isLeftHand);
            data.ReticleScaleFactor = scale;
            SaveRayData(data, isLeftHand);
        }

        public static void SetReticleColor(bool isLeftHand, Color? reticleColor)
        {
            var data = GetRayData(isLeftHand);
            data.ReticleColor = reticleColor;
            SaveRayData(data, isLeftHand);
        }

        public static void SetRayColor(bool isLeftHand, Color? rayColor)
        {
            var data = GetRayData(isLeftHand);
            data.RayColor = rayColor;
            SaveRayData(data, isLeftHand);
        }

        #endregion
    }
}