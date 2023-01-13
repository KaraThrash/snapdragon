// /******************************************************************************
//  * File: EditorUtils.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Text;
using UnityEditor;
using UnityEngine;

namespace QCHT.Editor
{
    public static class EditorUtility
    {
        public static void CreateAssetInSettingsFromObj(Object obj, string folder, string fileName)
        {
            const string assetFolder = "Assets";
            const string settingsFolder = "Settings";
            
            var sb = new StringBuilder(assetFolder).Append("/").Append(settingsFolder).Append("/");
            if (!AssetDatabase.IsValidFolder(sb.ToString())) AssetDatabase.CreateFolder(assetFolder, settingsFolder);
            if (!string.IsNullOrEmpty(folder))
            {
                sb.Append(folder);
                Debug.Log($"[EditorUtility:CreateAssetInSettingsFromObj] message = {sb}");
                if (!AssetDatabase.IsValidFolder(sb.ToString()))
                    AssetDatabase.CreateFolder($"{assetFolder}/{settingsFolder}" , folder);
                sb.Append("/");
            }

            sb.Append(fileName).Append(".asset");
            var newFile = AssetDatabase.GenerateUniqueAssetPath(sb.ToString());
            AssetDatabase.CreateAsset(obj, newFile);
            UnityEditor.EditorUtility.FocusProjectWindow();
            var asset = AssetDatabase.LoadAssetAtPath<Object>(newFile);
            EditorGUIUtility.PingObject(asset);
            Debug.LogWarning($"Asset has been created at {newFile}");
        }
    }
}