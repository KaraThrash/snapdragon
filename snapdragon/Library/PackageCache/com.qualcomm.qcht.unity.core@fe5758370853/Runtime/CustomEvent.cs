// /******************************************************************************
//  * File: CustomEvent.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace QCHT.Core
{
    [CreateAssetMenu(menuName = "QCHT/CustomEvent")]
    public sealed class CustomEvent : ScriptableObject
    {
        public UnityEvent Event = new UnityEvent();

        public void SendEvent()
        {
            Event?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomEvent))]
    public sealed class CustomEventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Event"))
            {
                var customEvent = target as CustomEvent;

                if (customEvent != null)
                    customEvent.SendEvent();
            }
        }
    }
#endif
}