// /******************************************************************************
//  * File: IBeginProximalGrabHandler.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.EventSystems;

namespace QCHT.Interactions.Proximal
{
    public interface IBeginProximalGrabHandler : IEventSystemHandler
    {
        void OnBeginGrab(InteractionData eventData);
    }
}