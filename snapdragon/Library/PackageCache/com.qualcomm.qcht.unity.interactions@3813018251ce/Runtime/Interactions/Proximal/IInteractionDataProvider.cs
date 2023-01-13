// /******************************************************************************
//  * File: IInteractionDataProvider.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using QCHT.Core;

namespace QCHT.Interactions.Proximal
{
    public enum InteractableType
    {
        Free,
        Snap
    }
    
    public interface IInteractionDataProvider
    {
        public InteractableType Type { get; }
        public IEnumerable<SnapData> SnapData { get; }
        public GestureId GrabGesture { get; }
        public GestureId SecondGrabGesture { get; }
    }
}