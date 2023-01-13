// /******************************************************************************
//  * File: CanBeEmptyDrawer.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using UnityEditor;
using UnityEngine;

namespace QCHT.Editor
{
    public class CanBeEmptyDrawer
    {
        /// <summary>
        /// Adds a [CanBeEmpty] label in the inspector above the serialized field.
        /// </summary>
        [CustomPropertyDrawer(typeof(CanBeEmptyAttribute))]
        public class OptionalDrawer : DecoratorDrawer
        {
            private const float Size = 0.25f;

            public override float GetHeight()
            {
                return base.GetHeight() * ( 1f + Size );
            }

            public override void OnGUI(Rect position)
            {
                position.y += GetHeight() * Size / ( 1f + Size );
                EditorGUI.LabelField(position, "[CanBeEmpty]");
            }
        }
    }
}