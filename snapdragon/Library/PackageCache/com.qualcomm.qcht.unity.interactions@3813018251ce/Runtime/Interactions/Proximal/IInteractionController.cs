// /******************************************************************************
//  * File: IInteractionController.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

namespace QCHT.Interactions.Proximal
{
    public interface IInteractionController
    {
        public InteractionResult Process();
        public void OnBeginInteraction();
        public void OnEndInteraction();
    }
}