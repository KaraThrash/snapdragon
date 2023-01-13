// /******************************************************************************
//  * File: QCHTControlBox.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core.Extensions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Distal.ControlBox
{
    public class QCHTControlBox : MonoBehaviour
    {
        private const string InteractionGridResourceName = "InteractionGrid";

        [SerializeField] private ControlBoxSettings settings;

        private InteractionGrid _interactionGrid;
        private Rigidbody _rigidBody;
        private bool _hasPhysic;

        private void Start()
        {
            var prefab = Resources.Load(InteractionGridResourceName) as GameObject;
            if (prefab == null)
            {
                Debug.LogError("[QCHTInteractionGizmo:Start] Can't find InteractionGrid in resources");
                return;
            }

            _rigidBody = GetComponent<Rigidbody>();

            var t = transform;
            var bounds = gameObject.GetMeshBoundingBox();
            var grid = Instantiate(prefab, t);
            
            grid.layer = gameObject.layer;
            var children = GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                child.gameObject.layer = gameObject.layer;
            }

            var test = t.rotation * bounds.size;
            grid.transform.localScale = Vector3Extensions.Abs(test.Divide(t.lossyScale)) + new Vector3(settings.ScaleOffset, settings.ScaleOffset, settings.ScaleOffset);
            grid.transform.localPosition = t.InverseTransformPoint(bounds.center);
            //grid.transform.localRotation = Quaternion.Inverse(t.rotation);

            _interactionGrid = grid.GetComponent<InteractionGrid>();
            _interactionGrid.Setup(settings);

            if (_rigidBody)
                _hasPhysic = !_rigidBody.isKinematic;
        }

        public void ToggleGrid(bool on)
        {
            _interactionGrid.gameObject.SetActive(on);

            if (!_hasPhysic || !_rigidBody)
                return;

            _rigidBody.isKinematic = on;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(QCHTControlBox))]
        public class QCHTControlBoxEditor : UnityEditor.Editor
        {
            private SerializedProperty _settingsProperty;
            private UnityEditor.Editor _editor;

            public void OnEnable()
            {
                _settingsProperty = serializedObject.FindProperty("settings");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(_settingsProperty);

                if (_settingsProperty.objectReferenceValue == null)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Create new control box settings", GUILayout.Width(300), GUILayout.Height(25)))
                    {
                        var settings = CreateControlBoxSettings();
                        _settingsProperty.objectReferenceValue = settings;
                        serializedObject.ApplyModifiedProperties();
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    CreateCachedEditor(_settingsProperty.objectReferenceValue, null, ref _editor);
                    _editor.DrawDefaultInspector();
                }

                serializedObject.ApplyModifiedProperties();
            }

            private static ControlBoxSettings CreateControlBoxSettings()
            {
                var settings = CreateInstance<ControlBoxSettings>();
                if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
                var newFile = "Assets/Settings/NewControlBoxSettings.asset";
                newFile = AssetDatabase.GenerateUniqueAssetPath(newFile);
                AssetDatabase.CreateAsset(settings, newFile);
                EditorUtility.FocusProjectWindow();
                var obj = AssetDatabase.LoadAssetAtPath<Object>(newFile);
                EditorGUIUtility.PingObject(obj);
                Debug.LogWarning($"Asset has been created at {newFile}");
                return settings;
            }
        }
#endif
    }
}