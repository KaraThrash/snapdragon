// /******************************************************************************
//  * File: Billboard.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using UnityEngine;

namespace QCHT.Interactions.UI
{
    public class Billboard : MonoBehaviour
    {
        public Transform Target;

        public bool LockY;

        #region MonoBehaviour Functions

        private void LateUpdate()
        {
            Target = Target ? Target : QCHTManager.Instance.Head;

            if (ReferenceEquals(Target, null))
                return;

            if (LockY)
            {
                var upVector = Vector3.up;
                var wPos = Vector3.Normalize(transform.position - Target.transform.position);
                var euler = Quaternion.LookRotation(wPos, upVector).eulerAngles;
                var rot = Quaternion.Euler(0, euler.y, 0);
                transform.rotation = rot;
            }
            else
            {
                var camRot = Target.transform.rotation;
                var upVector = camRot * Vector3.up;
                var wPos = transform.position + camRot * Vector3.forward;
                transform.LookAt(wPos, upVector);
            }
        }

        #endregion
    }
}