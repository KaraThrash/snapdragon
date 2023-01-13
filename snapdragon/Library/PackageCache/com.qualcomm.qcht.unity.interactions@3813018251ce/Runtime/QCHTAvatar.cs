// /******************************************************************************
//  * File: QCHTAvatar.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.VFF
{
    public sealed class QCHTAvatar : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the avatar follow the HMD?")]
        private bool relativeToHead = true;

        #region MonoBehaviour Functions

        public void Start()
        {
            IgnoreAllCollisions();
        }

        public void Update()
        {
            if (relativeToHead)
            {
                var t = transform;
                t.position = QCHTManager.Instance.Head.position;
                t.rotation = QCHTManager.Instance.Head.rotation;
            }
        }

        /// <summary>
        /// Removes all collisions detections between parts from the same avatar
        /// </summary>
        private void IgnoreAllCollisions()
        {
            var colliders = GetComponentsInChildren<Collider>();

            foreach (var colA in colliders)
            {
                foreach (var colB in colliders)
                {
                    Physics.IgnoreCollision(colA, colB);
                }
            }
        }

        #endregion

#if UNITY_EDITOR

        #region Menu

        [MenuItem("GameObject/QCHT/Avatar", false)]
        private static void LoadAvatar(MenuCommand menuCommand)
        {
            LoadPrefab(menuCommand, "QCHTAvatar");
        }

        private static void LoadPrefab(MenuCommand menuCommand, string prefabName)
        {
            var results = AssetDatabase.FindAssets(prefabName);

            if (results == null || results.Length <= 0)
            {
                Debug.LogError($"[LoadPrefab] Can't find the prefab {prefabName} in assets");
                return;
            }

            var prefabPath = AssetDatabase.GUIDToAssetPath(results[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var go = PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            Debug.Log($"[LoadPrefab] Instantiated a prefab {prefabName} from {prefabPath}!");
        }

        #endregion

#endif
    }
}