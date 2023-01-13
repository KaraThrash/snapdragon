// /******************************************************************************
//  * File: IPointerMoveHandler.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal
{
    public interface IPointerMoveHandler : IEventSystemHandler
    {
        public void OnPointerMove(PointerEventData eventData);
    }
}