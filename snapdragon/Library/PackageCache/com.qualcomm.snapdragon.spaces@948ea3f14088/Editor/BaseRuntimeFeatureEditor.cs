/******************************************************************************
 * File: BaseRuntimeFeatureEditor.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    [CustomEditor(typeof(BaseRuntimeFeature))]
    internal class BaseRuntimeFeatureEditor : UnityEditor.Editor
    {
        private SerializedProperty _checkInstalledRuntime;
        private SerializedProperty _launchAppOnViewer;
        private SerializedProperty _launchControllerOnHost;
        private SerializedProperty _useCustomController;
        private SerializedProperty _exportHeadless;
        private SerializedProperty _alternateStartActivity;
        private SerializedProperty _skipPermissionChecks;

        private bool _controllerFoldoutOpen;
        private string _controllerFoldoutEditorPrefsKey = "Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature.Settings.ControllerFoldoutOpen";

        private bool _advancedFoldoutOpen;
        private string _advancedFoldoutEditorPrefsKey = "Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature.Settings.AdvancedFoldoutOpen";

        void OnEnable() {
            _checkInstalledRuntime = serializedObject.FindProperty("CheckInstalledRuntime");
            _launchAppOnViewer = serializedObject.FindProperty("LaunchAppOnViewer");
            _controllerFoldoutOpen = EditorPrefs.GetBool(_controllerFoldoutEditorPrefsKey, true);
            _launchControllerOnHost = serializedObject.FindProperty("LaunchControllerOnHost");
            _useCustomController = serializedObject.FindProperty("UseCustomController");
            _advancedFoldoutOpen = EditorPrefs.GetBool(_advancedFoldoutEditorPrefsKey, true);
            _exportHeadless = serializedObject.FindProperty("ExportHeadless");
            _alternateStartActivity = serializedObject.FindProperty("AlternateStartActivity");
            _skipPermissionChecks = serializedObject.FindProperty("SkipPermissionChecks");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            /* Because the checkbox is directly appended to the label, a manual spacing is added to the default label width. */
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth + 50;
            EditorGUILayout.PropertyField(_checkInstalledRuntime);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_launchAppOnViewer);

            EditorGUILayout.Space();
            bool tempControllerFoldoutOpen = EditorGUILayout.Foldout(_controllerFoldoutOpen, "Controller Settings", true);
            if (_controllerFoldoutOpen != tempControllerFoldoutOpen) {
                _controllerFoldoutOpen = tempControllerFoldoutOpen;
                EditorPrefs.SetBool(_controllerFoldoutEditorPrefsKey, _controllerFoldoutOpen);
            }
            if (_controllerFoldoutOpen) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_launchControllerOnHost);
                EditorGUI.BeginDisabledGroup(!_launchControllerOnHost.boolValue);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_useCustomController);
                if (_useCustomController.boolValue && _launchControllerOnHost.boolValue) {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("An Android archive including a custom controller implementation should be residing inside the Assets folder in order for this toggle to have any effect. Refer to the documentation on how to create a custom controller.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawLinkButton("Link to documentation", "https://docs.spaces.qualcomm.com/");
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            bool tempAdvancedSettingsFoldoutOpen = EditorGUILayout.Foldout(_advancedFoldoutOpen, "Advanced Settings", true);
            if (_advancedFoldoutOpen != tempAdvancedSettingsFoldoutOpen) {
                _advancedFoldoutOpen = tempAdvancedSettingsFoldoutOpen;
                EditorPrefs.SetBool(_advancedFoldoutEditorPrefsKey, _advancedFoldoutOpen);
            }
            if (_advancedFoldoutOpen) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_exportHeadless);
                EditorGUILayout.PropertyField(_alternateStartActivity);
                EditorGUILayout.PropertyField(_skipPermissionChecks);
                EditorGUI.indentLevel--;
            }

            /* Reset the original Editor label width in order to avoid broken UI. */
            EditorGUIUtility.labelWidth = labelWidth;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLinkButton(string title, string url) {
            var linkButtonStyle = new GUIStyle(GUI.skin.label);
            linkButtonStyle.normal.textColor = new Color(0f, 0.5f, 0.95f, 1f);
            linkButtonStyle.hover.textColor = linkButtonStyle.normal.textColor;
            linkButtonStyle.fixedWidth = EditorStyles.label.CalcSize(new GUIContent(title + " ")).x;
            linkButtonStyle.margin = new RectOffset(50, 0, 0, 0);

            if (GUILayout.Button(title, linkButtonStyle)) {
                Application.OpenURL(url);
            }

            var buttonRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(buttonRect.x, buttonRect.y + buttonRect.height, buttonRect.width, 2), GUIContent.none);
        }
    }
}