// // /******************************************************************************
// //  * File: IHandInput.cs
// //  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
// //  *
// //  * Confidential and Proprietary - Qualcomm Technologies, Inc.
// //  *
// //  ******************************************************************************/

using UnityEngine.InputSystem;

namespace QCHT.Core
{
    public interface IHandInputHandler
    {
        public void OnLeftHandInsert(InputAction.CallbackContext context);
        public void OnRightHandInsert(InputAction.CallbackContext context);
        public void OnHandsRemove(InputAction.CallbackContext context);
        public void OnHandsReleased(InputAction.CallbackContext context);
        public void OnPinch(InputAction.CallbackContext context);
        public void OnGrab(InputAction.CallbackContext context);
        public void OnFlip(InputAction.CallbackContext context);
        public void OnMousePosition(InputAction.CallbackContext context);
        public void OnMouseScroll(InputAction.CallbackContext context);
    }
}