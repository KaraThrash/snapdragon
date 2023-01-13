// /******************************************************************************
//  * File: ExecuteProximalEventsExtensions.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.EventSystems;

namespace QCHT.Interactions.Proximal
{
    public static class ExecuteProximalEventsExtensions
    {
        /*** IProximalEnterHandler ***/
        private static readonly ExecuteEvents.EventFunction<IProximalEnterHandler> s_proximalEnterHandler =
            ExecuteEnter;

        private static void ExecuteEnter(IProximalEnterHandler handler, BaseEventData eventData)
        {
            handler.OnProximalEnter(ExecuteEvents.ValidateEventData<InteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IProximalEnterHandler> enterHandler => s_proximalEnterHandler;

        /*** IProximalExitHandler ***/
        private static readonly ExecuteEvents.EventFunction<IProximalExitHandler> s_proximalExitHandler =
            ExecuteExit;

        private static void ExecuteExit(IProximalExitHandler handler, BaseEventData eventData)
        {
            handler.OnProximalExit(ExecuteEvents.ValidateEventData<InteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IProximalExitHandler> exitHandler => s_proximalExitHandler;

        /*** IBeginProximalGrabHandler ***/
        private static readonly ExecuteEvents.EventFunction<IBeginProximalGrabHandler> s_proximalGrabHandlerBegin =
            ExecuteBeginGrab;

        private static void ExecuteBeginGrab(IBeginProximalGrabHandler handler, BaseEventData eventData)
        {
            handler.OnBeginGrab(ExecuteEvents.ValidateEventData<InteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IBeginProximalGrabHandler> beginGrabHandler =>
            s_proximalGrabHandlerBegin;

        /*** IProximalGrabHandler ***/
        private static readonly ExecuteEvents.EventFunction<IProximalGrabHandler> s_proximalGrabHandler = ExecuteGrab;

        private static void ExecuteGrab(IProximalGrabHandler handler, BaseEventData eventData)
        {
            handler.OnGrab(ExecuteEvents.ValidateEventData<InteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IProximalGrabHandler> grabHandler => s_proximalGrabHandler;

        /*** IEndProximalGrabHandler ***/
        private static readonly ExecuteEvents.EventFunction<IEndProximalGrabHandler> s_proximalGrabHandlerEnd =
            ExecuteEndGrab;

        private static void ExecuteEndGrab(IEndProximalGrabHandler handler, BaseEventData eventData)
        {
            handler.OnEndGrab(ExecuteEvents.ValidateEventData<InteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IEndProximalGrabHandler> endGrabHandler => s_proximalGrabHandlerEnd;

        /*** IBeginDoubleProximalGrabHandler ***/
        private static readonly ExecuteEvents.EventFunction<IBeginDoubleGrabHandler> s_proximalBeginDoubleGrabHandler =
            ExecuteBeginDoubleGrab;

        private static void ExecuteBeginDoubleGrab(IBeginDoubleGrabHandler handler, BaseEventData eventData)
        {
            handler.OnBeginDoubleGrab(ExecuteEvents.ValidateEventData<DoubleInteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IBeginDoubleGrabHandler> beginDoubleGrabHandler =>
            s_proximalBeginDoubleGrabHandler;

        /*** IDoubleProximalGrabHandler ***/
        private static readonly ExecuteEvents.EventFunction<IDoubleGrabHandler> s_proximalDoubleGrabHandler =
            ExecuteDoubleGrab;

        private static void ExecuteDoubleGrab(IDoubleGrabHandler handler, BaseEventData eventData)
        {
            handler.OnDoubleGrab(ExecuteEvents.ValidateEventData<DoubleInteractionData>(eventData));
        }

        public static ExecuteEvents.EventFunction<IDoubleGrabHandler> doubleGrabHandler => s_proximalDoubleGrabHandler;
    }
}