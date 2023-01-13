// /******************************************************************************
//  * File: GrabPointEditor.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Interactions.Hands;
using QCHT.Interactions.Proximal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_1_OR_NEWER
using System;
using System.Linq;
using UnityEditor.UIElements;
#endif

namespace QCHT.Interactions.Editor
{
    [CustomEditor(typeof(GrabPoint))]
    public sealed class GrabPointEditor : UnityEditor.Editor
    {
        private sealed class FreezeOption
        {
            public bool X;
            public bool Y;
            public bool Z;
        }

        public VisualTreeAsset m_InspectorXML;
        public VisualTreeAsset m_ListItemXML;

        private GrabPoint _grabPoint;
        private HandGhost _handGhost;

        // Properties
        private SerializedProperty _snapDataProperty;

        // Edition
        private const float HandleSize = 0.005f;
        private bool _editing;
        private int _currentIndex;
        private HandType _currentType;
        private Tool _savedTool;

        private VisualElement _inspector;

        #region Editor

        private void OnEnable()
        {
            _grabPoint = (GrabPoint) target;
            _snapDataProperty = serializedObject.FindProperty("snapData");
        }

        private void OnDisable()
        {
            if (_handGhost)
                DestroyImmediate(_handGhost.gameObject);

            if (_editing)
                Tools.current = _savedTool;
        }

#if UNITY_2021_1_OR_NEWER
        public override VisualElement CreateInspectorGUI()
        {
            _inspector = new VisualElement();
            m_InspectorXML.CloneTree(_inspector);
            var list = _inspector.Q<ListView>("snapDataList");
            var snapGrp = _inspector.Q<VisualElement>("snapGroup");
            var editTools = snapGrp.Q<VisualElement>("editTools");
            var editButton = _inspector.Q<Button>("editButton");
            var flip = editTools.Q<Button>("editFlip");
            var interactionType = _inspector.Q<PropertyField>("interactableType");
            var interactionProperty = serializedObject.FindProperty(interactionType.bindingPath);

            var displayStyle = (InteractableType) interactionProperty.intValue == InteractableType.Snap
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            snapGrp.style.display = displayStyle;
            editTools.style.display = _editing ? DisplayStyle.Flex : DisplayStyle.None;

            interactionType.RegisterValueChangeCallback(evt =>
            {
                displayStyle = (InteractableType) evt.changedProperty.intValue == InteractableType.Snap
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                snapGrp.style.display = displayStyle;
                SceneView.RepaintAll();
            });

            editButton.clicked += () =>
            {
                _editing = !_editing;
                editButton.text = _editing ? "Stop" : "Edit";
                editTools.style.display = _editing ? DisplayStyle.Flex : DisplayStyle.None;
                Tools.hidden = _editing;
                SceneView.RepaintAll();
            };

            flip.clicked += () =>
            {
                if (!_handGhost || !_handGhost.HandPose)
                    return;

                _handGhost.HandPose.Type = _handGhost.HandPose.Type == HandType.Left ? HandType.Right : HandType.Left;
                SceneView.RepaintAll();
            };

            list.makeItem += () =>
            {
                var item = m_ListItemXML.CloneTree();
                item.Q<ObjectField>("handPose").RegisterValueChangedCallback(evt =>
                {
                    RefreshUI();
                });
                return item;
            };

            list.onSelectedIndicesChange += ints =>
            {
                var enumerable = ints as int[] ?? ints.ToArray();
                _currentIndex = enumerable.Any() ? enumerable.First() : 0;
                RefreshUI();
                SceneView.RepaintAll();
            };

            list.itemsAdded += ints =>
            {
                foreach (var i in ints)
                {
                    var element = _snapDataProperty.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("HandPose").objectReferenceValue = null;
                    element.FindPropertyRelative("HandPoseMask").objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                }

                SceneView.RepaintAll();
            };

            RefreshUI();
            return _inspector;
        }

        private void RefreshUI()
        {
            var editGroup = _inspector.Q<VisualElement>("editGroup");

            try
            {
                if (_snapDataProperty.arraySize == 0)
                    throw new Exception();

                var element = _snapDataProperty.GetArrayElementAtIndex(_currentIndex);
                var handPose = element.FindPropertyRelative("HandPose").objectReferenceValue as HandPose;
                editGroup.style.display = handPose != null ? DisplayStyle.Flex : DisplayStyle.None;

                // Focus on HandGhost in scene view
                if (_handGhost)
                {
                    var r = _handGhost.GetComponentInChildren<Renderer>();
                    if (r == null)
                        return;

                    SceneView.lastActiveSceneView.Frame(r.bounds, false);
                }
            }
            catch (Exception)
            {
                editGroup.style.display = DisplayStyle.None;
            }
        }
#else
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(5f);

            var prop = _snapDataProperty.GetArrayElementAtIndex(_currentIndex);
            var handPoseProperty = prop.FindPropertyRelative("HandPose");
            var handPose = handPoseProperty.objectReferenceValue as HandPose;

            // Pose
            if (!handPose)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create New Pose", GUILayout.Width(200), GUILayout.Height(25)))
                {
                    var pose = HandPoseEditor.CreateNewHandPoseAsset();
                    handPoseProperty.objectReferenceValue = pose;

                    if (_handGhost)
                    {
                        ApplyGhostToPose(_handGhost, pose);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                return;
            }

            EditorGUILayout.LabelField($"Visualizing Element: {_currentIndex}");

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (!_editing && GUILayout.Button("Previous Pose", GUILayout.Width(200), GUILayout.Height(25)))
            {
                _currentIndex--;

                if (_currentIndex < 0)
                    _currentIndex = _snapDataProperty.arraySize - 1;
                SceneView.RepaintAll();
            }
            else if (!_editing && GUILayout.Button("Next Pose", GUILayout.Width(200), GUILayout.Height(25)))
            {
                _currentIndex++;

                if (_currentIndex > _snapDataProperty.arraySize - 1)
                    _currentIndex = 0;
                SceneView.RepaintAll();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (!_editing && GUILayout.Button("Edit Hand Pose", GUILayout.Width(200), GUILayout.Height(25)))
            {
                _editing = true;
                SceneView.RepaintAll();
            }
            else if (_editing)
            {
                if (GUILayout.Button("Flip", GUILayout.Width(200), GUILayout.Height(25)))
                {
                    _handGhost.HandPose.Type =
                        _handGhost.HandPose.Type == HandType.Left ? HandType.Right : HandType.Left;
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Stop Editing", GUILayout.Width(200), GUILayout.Height(25)))
                {
                    _editing = false;
                    SceneView.RepaintAll();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

#endif

        private void OnSceneGUI()
        {
            if (_grabPoint.Type != InteractableType.Snap)
            {
                if (_handGhost)
                    DestroyImmediate(_handGhost.gameObject);
                return;
            }

            if (_snapDataProperty.arraySize == 0)
                return;

            if (_currentIndex >= _snapDataProperty.arraySize)
                _currentIndex = _snapDataProperty.arraySize - 1;

            var prop = _snapDataProperty.GetArrayElementAtIndex(_currentIndex);

            var snapData = SnapData.DeserializeSnapData(prop);
            var grabPointTransform = _grabPoint.transform;

            // Delete wrong ghost 
            if (_handGhost && _handGhost.HandPose && _handGhost.HandPose.Type != _currentType)
                DestroyImmediate(_handGhost.gameObject);

            // Ghost
            if (!_handGhost && snapData.HandPose && snapData.HandPose.Ghost)
            {
                _handGhost = InstantiateGhost(snapData.HandPose.Ghost, snapData.HandPose.Type);
                _currentType = snapData.HandPose.Type;
            }
            else if (_handGhost && (!snapData.HandPose || !snapData.HandPose.Ghost))
                DestroyImmediate(_handGhost.gameObject);

            if (!_handGhost)
                return;

            // Update Ghost
            _handGhost.HandPose = snapData.HandPose;
            ApplyGhostRootPose(_handGhost, grabPointTransform, snapData.LocalOffsetPosition,
                snapData.LocalOffsetRotation);

            if (!_editing)
                return;

            UpdateRootHandle();
            UpdateHandles(_handGhost);
            ApplyGhostToPose(_handGhost, _handGhost.HandPose);
        }

        #endregion

        private void UpdateRootHandle()
        {
            var prop = _snapDataProperty.GetArrayElementAtIndex(_currentIndex);
            var offsetPositionProperty = prop.FindPropertyRelative("LocalOffsetPosition");
            var offsetRotationProperty = prop.FindPropertyRelative("LocalOffsetRotation");
            var offsetPosition = offsetPositionProperty.vector3Value;
            var offsetRotation = offsetRotationProperty.vector3Value;
            var grabPointTransform = _grabPoint.transform;
            var position = grabPointTransform.position;
            var rotation = grabPointTransform.rotation;

            var posOffset = position + rotation * offsetPosition;
            var rotOffset = Tools.pivotRotation == PivotRotation.Local
                ? rotation * Quaternion.Euler(offsetRotation)
                : Quaternion.identity;

            Handles.TransformHandle(ref posOffset, ref rotOffset);
            var o = rotation * Quaternion.Euler(offsetRotation);
            var newPos = Quaternion.Inverse(rotation) * (posOffset - position);
            var newRot = Tools.pivotRotation == PivotRotation.Local
                ? (Quaternion.Inverse(rotation) * rotOffset).eulerAngles
                : (Quaternion.Inverse(rotation) * (rotOffset * o)).eulerAngles;

            var valueChanged = false;
            if (newPos != offsetPositionProperty.vector3Value)
            {
                offsetPositionProperty.vector3Value = newPos;
                valueChanged = true;
            }

            if (newRot != offsetRotationProperty.vector3Value)
            {
                offsetRotationProperty.vector3Value = newRot;
                valueChanged = true;
            }

            if (valueChanged)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_grabPoint);
            }
        }

        private static void UpdateHandles(HandGhost handGhost)
        {
            // Thumb 
            UpdateHandle(handGhost.thumbBase, new FreezeOption {Z = true});
            UpdateHandle(handGhost.thumbMiddle, new FreezeOption {Y = true, Z = true});
            UpdateHandle(handGhost.thumbTop, new FreezeOption {Y = true, Z = true});

            // Index 
            UpdateHandle(handGhost.indexBase, new FreezeOption {Z = true});
            UpdateHandle(handGhost.indexMiddle, new FreezeOption {Y = true, Z = true});
            UpdateHandle(handGhost.indexTop, new FreezeOption {Y = true, Z = true});

            // Middle
            UpdateHandle(handGhost.middleBase, new FreezeOption {Z = true});
            UpdateHandle(handGhost.middleMiddle, new FreezeOption {Y = true, Z = true});
            UpdateHandle(handGhost.middleTop, new FreezeOption {Y = true, Z = true});

            // Ring
            UpdateHandle(handGhost.ringBase, new FreezeOption {Z = true});
            UpdateHandle(handGhost.ringMiddle, new FreezeOption {Y = true, Z = true});
            UpdateHandle(handGhost.ringTop, new FreezeOption {Y = true, Z = true});

            // Pinky
            UpdateHandle(handGhost.pinkyBase, new FreezeOption {Z = true});
            UpdateHandle(handGhost.pinkyMiddle, new FreezeOption {Y = true, Z = true});
            UpdateHandle(handGhost.pinkyTop, new FreezeOption {Y = true, Z = true});
        }

        private static void UpdateHandle(Transform transform, FreezeOption freeze = null)
        {
            if (freeze == null || !freeze.X)
            {
                Handles.color = Handles.xAxisColor;
                transform.rotation = Handles.Disc(transform.rotation, transform.position, transform.right,
                    HandleSize,
                    false, 0f);
            }

            if (freeze == null || !freeze.Y)
            {
                Handles.color = Handles.yAxisColor;
                transform.rotation = Handles.Disc(transform.rotation, transform.position, transform.forward,
                    HandleSize,
                    false, 0f);
            }

            if (freeze == null || !freeze.Z)
            {
                Handles.color = Handles.zAxisColor;
                transform.rotation = Handles.Disc(transform.rotation, transform.position, transform.up, HandleSize,
                    false,
                    0f);
            }
        }

        private static HandGhost InstantiateGhost(HandGhostSO handGhost, HandType type)
        {
            var handObj = type == HandType.Left ? handGhost.LeftGhost.gameObject : handGhost.RightGhost.gameObject;
            var ghost = Instantiate(handObj).GetComponent<HandGhost>();
            ghost.gameObject.hideFlags = HideFlags.HideAndDontSave;
            StageUtility.PlaceGameObjectInCurrentStage(ghost.gameObject);
            return ghost;
        }

        private static void ApplyGhostRootPose(HandGhost handGhost, Transform grabPointTransform,
            Vector3 offsetPosition,
            Vector3 offsetRotation)
        {
            var position = grabPointTransform.position;
            var rotation = grabPointTransform.rotation;
            var posOffset = position + rotation * offsetPosition;
            var rotOffset = rotation * Quaternion.Euler(offsetRotation);
            var ghostTransform = handGhost.transform;
            ghostTransform.position = posOffset;
            ghostTransform.rotation = rotOffset;
        }

        private static void ApplyGhostToPose(HandGhost handGhost, HandPose pose)
        {
            // Thumb
            pose.Thumb.BaseData.Position = handGhost.thumbBase.localPosition;
            pose.Thumb.BaseData.Rotation = handGhost.thumbBase.localRotation;
            pose.Thumb.MiddleData.Position = handGhost.thumbMiddle.localPosition;
            pose.Thumb.MiddleData.Rotation = handGhost.thumbMiddle.localRotation;
            pose.Thumb.TopData.Position = handGhost.thumbTop.localPosition;
            pose.Thumb.TopData.Rotation = handGhost.thumbTop.localRotation;

            // Index
            pose.Index.BaseData.Position = handGhost.indexBase.localPosition;
            pose.Index.BaseData.Rotation = handGhost.indexBase.localRotation;
            pose.Index.MiddleData.Position = handGhost.indexMiddle.localPosition;
            pose.Index.MiddleData.Rotation = handGhost.indexMiddle.localRotation;
            pose.Index.TopData.Position = handGhost.indexTop.localPosition;
            pose.Index.TopData.Rotation = handGhost.indexTop.localRotation;

            // Middle
            pose.Middle.BaseData.Position = handGhost.middleBase.localPosition;
            pose.Middle.BaseData.Rotation = handGhost.middleBase.localRotation;
            pose.Middle.MiddleData.Position = handGhost.middleMiddle.localPosition;
            pose.Middle.MiddleData.Rotation = handGhost.middleMiddle.localRotation;
            pose.Middle.TopData.Position = handGhost.middleTop.localPosition;
            pose.Middle.TopData.Rotation = handGhost.middleTop.localRotation;

            // Ring
            pose.Ring.BaseData.Position = handGhost.ringBase.localPosition;
            pose.Ring.BaseData.Rotation = handGhost.ringBase.localRotation;
            pose.Ring.MiddleData.Position = handGhost.ringMiddle.localPosition;
            pose.Ring.MiddleData.Rotation = handGhost.ringMiddle.localRotation;
            pose.Ring.TopData.Position = handGhost.ringTop.localPosition;
            pose.Ring.TopData.Rotation = handGhost.ringTop.localRotation;

            // Pinky
            pose.Pinky.BaseData.Position = handGhost.pinkyBase.localPosition;
            pose.Pinky.BaseData.Rotation = handGhost.pinkyBase.localRotation;
            pose.Pinky.MiddleData.Position = handGhost.pinkyMiddle.localPosition;
            pose.Pinky.MiddleData.Rotation = handGhost.pinkyMiddle.localRotation;
            pose.Pinky.TopData.Position = handGhost.pinkyTop.localPosition;
            pose.Pinky.TopData.Rotation = handGhost.pinkyTop.localRotation;

            EditorUtility.SetDirty(pose);
        }
    }
}