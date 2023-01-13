// /******************************************************************************
//  * File: HandPose.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Hands
{
    [Serializable]
    public class BoneData
    {
        public bool IsRoot;
        public bool IsThumb;
        public bool UpdatePosition;
        public bool UpdateRotation = true;
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;

        public static BoneData DefaultRoot => new BoneData
        {
            IsRoot = true,
            IsThumb = false,
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        public static BoneData Default => new BoneData
        {
            IsRoot = false,
            IsThumb = false,
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };

        public void Flip()
        {
            if (IsRoot)
            {
                // Ignored
            }
            else if (IsThumb)
            {
                Rotation = Rotation.FlipZAxis();
                Rotation = Rotation.FlipXZAxis();
            }
            else
            {
                Rotation = Rotation.FlipXAxis();
                Rotation = Rotation.FlipZXAxis();
            }
        }
    }

    [Serializable]
    public class FingerData
    {
        public BoneData BaseData;
        public BoneData MiddleData;
        public BoneData TopData;

        public static FingerData Default => new FingerData
        {
            BaseData = BoneData.Default,
            MiddleData = BoneData.Default,
            TopData = BoneData.Default
        };

        public FingerData(bool isThumb = false)
        {
            BaseData = new BoneData {IsThumb = isThumb};
            MiddleData = new BoneData {IsThumb = isThumb};
            TopData = new BoneData {IsThumb = isThumb};
        }

        public void Flip()
        {
            BaseData.Flip();
            MiddleData.Flip();
            TopData.Flip();
        }
    }

    /// <summary>
    /// Stores hand pose.
    /// All position and rotation are stored local.
    /// </summary>
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPose")]
    public sealed class HandPose : ScriptableObject, ICloneable
    {
        [SerializeField] private HandType _type;

        public HandType Type
        {
            set
            {
                if (_type == value)
                    return;

                _type = value;
                Flip();
            }
            get => _type;
        }

        public Vector3 Scale = Vector3.one;

        public bool IsLeft => Type == HandType.Left;

        [Space]
        public BoneData Root;

        public FingerData Thumb;
        public FingerData Index;
        public FingerData Middle;
        public FingerData Ring;
        public FingerData Pinky;

        [CanBeEmpty, HideInInspector]
        public HandPose HandPoseRefLeft;

        [CanBeEmpty, HideInInspector]
        public HandPose HandPoseRefRight;

        [CanBeEmpty]
        public HandGhostSO Ghost;

        public HandPose()
        {
            Root = new BoneData {IsRoot = true, UpdatePosition = true};
            Thumb = new FingerData(isThumb: true);
            Index = new FingerData();
            Middle = new FingerData();
            Ring = new FingerData();
            Pinky = new FingerData();
        }

        public object Clone()
        {
            var handPose = CreateInstance<HandPose>();
            handPose.CopyFrom(this);
            return handPose;
        }

        public void Reset()
        {
            var handPoseRef = IsLeft ? HandPoseRefLeft : HandPoseRefRight;

            if (handPoseRef)
            {
                Root.CopyFrom(handPoseRef.Root);
                Thumb.CopyFrom(handPoseRef.Thumb);
                Index.CopyFrom(handPoseRef.Index);
                Middle.CopyFrom(handPoseRef.Middle);
                Ring.CopyFrom(handPoseRef.Ring);
                Pinky.CopyFrom(handPoseRef.Pinky);

                return;
            }

            Root = BoneData.DefaultRoot;
            Thumb = FingerData.Default;
            Index = FingerData.Default;
            Middle = FingerData.Default;
            Ring = FingerData.Default;
            Pinky = FingerData.Default;
        }

        private void Flip()
        {
            Root.Flip();
            Thumb.Flip();
            Index.Flip();
            Middle.Flip();
            Ring.Flip();
            Pinky.Flip();
        }
    }

    public static class BoneDataExtensions
    {
        public static void CopyFrom(this BoneData boneData, BoneData data)
        {
            boneData.IsRoot = data.IsRoot;
            boneData.IsThumb = data.IsThumb;
            boneData.Position = data.Position;
            boneData.Rotation = data.Rotation;
        }
    }

    public static class FingerDataExtensions
    {
        public static void CopyFrom(this FingerData fingerData, FingerData finger)
        {
            fingerData.BaseData.CopyFrom(finger.BaseData);
            fingerData.MiddleData.CopyFrom(finger.MiddleData);
            fingerData.TopData.CopyFrom(finger.TopData);
        }
    }

    public static class HandPoseExtensions
    {
        public static void CopyFrom(this HandPose handPose, HandPose pose)
        {
            handPose.Root.CopyFrom(pose.Root);
            handPose.Thumb.CopyFrom(pose.Thumb);
            handPose.Index.CopyFrom(pose.Index);
            handPose.Middle.CopyFrom(pose.Middle);
            handPose.Ring.CopyFrom(pose.Ring);
            handPose.Pinky.CopyFrom(pose.Pinky);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(HandPose))]
    public sealed class HandPoseEditor : UnityEditor.Editor
    {
        private HandPose _handPose;
        private HandGhost _handGhost;

        private Vector2 _previewDir;

        private void OnEnable()
        {
            _handPose = target as HandPose;
        }

        private void OnDisable()
        {
            CleanupPreviewRenderUtility();
        }

        #region Inspector

        public override void OnInspectorGUI()
        {
            var handSide = (HandType) EditorGUILayout.EnumPopup("Hand Side", _handPose.Type);
            if (handSide != _handPose.Type)
            {
                _handPose.Type = handSide;
                EditorUtility.SetDirty(_handPose);
                SetDirty(true);
                AssetDatabase.SaveAssetIfDirty(_handPose);
            }
        }

        #endregion

        #region Preview

        private const string PreviewGhostInstanceName = "ghostPreview";

        private PreviewRenderUtility _previewRenderUtility;

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            base.OnInteractivePreviewGUI(r, background);

            if (_previewRenderUtility == null || _isDirty)
            {
                CleanupPreviewRenderUtility();

                _previewRenderUtility = new PreviewRenderUtility();
                InitPreviewCamera();
                ReloadGhostPreview();
                SetDirty(false);
            }

            _previewRenderUtility.BeginPreview(r, background);
            DoRenderPreview(r);
            _previewRenderUtility.EndAndDrawPreview(r);
        }

        private void InitPreviewCamera()
        {
            var camera = _previewRenderUtility.camera;
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(0f, .07f, -1f);
            cameraTransform.LookAt(new Vector3(0f, .07f, 0f));
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = new Color(.2f, .2f, .2f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
        }

        private void DoRenderPreview(Rect r)
        {
            _previewDir = PreviewGUI.Drag2D(_previewDir, r);

            if (_handGhost)
            {
                _handGhost.transform.rotation = Quaternion.Euler(_previewDir.y, 0.0f, 0.0f) *
                                                Quaternion.Euler(0.0f, _previewDir.x, 0.0f);

                _handGhost.UpdatePose();
            }

            _previewRenderUtility.Render();
        }

        private void ReloadGhostPreview()
        {
            if (!_handPose.Ghost)
                return;

            var obj = _handPose.Type == HandType.Left ? _handPose.Ghost.LeftGhost : _handPose.Ghost.RightGhost;
            if (!obj)
                return;

            var ghost = _previewRenderUtility.InstantiatePrefabInScene(obj.gameObject);
            ghost.name = PreviewGhostInstanceName;
            ghost.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            if (!ghost)
            {
                OnDisable();
                return;
            }

            _handGhost = ghost.GetComponent<HandGhost>();
            _handGhost.HandPose = _handPose;

            var ghostTransform = _handGhost.transform;
            ghostTransform.position = Vector3.zero;
            ghostTransform.rotation = _handGhost.HandPose.Root.Rotation;
        }

        private void CleanupPreviewRenderUtility()
        {
            if (_previewRenderUtility == null)
                return;

            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
        }

        #endregion

        #region Dirty

        private bool _isDirty = true;

        private void SetDirty(bool value)
        {
            _isDirty = value;
        }

        #endregion

        #region Asset

        public static HandPose CreateNewHandPoseAsset()
        {
            var handPose = CreateInstance<HandPose>();
            Editor.EditorUtility.CreateAssetInSettingsFromObj(handPose, "HandPoses", "NewHandPose");
            return handPose;
        }

        public static HandPose DuplicateHandPoseAsset(HandPose handPose)
        {
            var newPose = Instantiate(handPose);
            Editor.EditorUtility.CreateAssetInSettingsFromObj(newPose, "HandPoses", handPose.name);
            return newPose;
        }

        #endregion
    }

    internal static class PreviewGUI
    {
        private static readonly int s_sliderHash = "Slider".GetHashCode();

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            var idControl = GUIUtility.GetControlID(s_sliderHash, FocusType.Passive);
            var cEvent = Event.current;

            switch (cEvent.GetTypeForControl(idControl))
            {
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == idControl)
                    {
                        scrollPosition -= cEvent.delta * (cEvent.shift ? 3f : 1f) /
                            Mathf.Min(position.width, position.height) * 140f;
                        cEvent.Use();
                        GUI.changed = true;
                    }

                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == idControl)
                    {
                        GUIUtility.hotControl = 0;
                    }

                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDown:
                    if (position.Contains(cEvent.mousePosition) && position.width > 50.0f)
                    {
                        GUIUtility.hotControl = idControl;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        cEvent.Use();
                    }

                    break;
            }

            return scrollPosition;
        }
    }

#endif
}