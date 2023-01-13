/******************************************************************************
 * File: BaseRuntimeFeature.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        Company = "Qualcomm",
        Desc = "Enables base features like session tracking on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.9.0",
        Required = true,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    public partial class BaseRuntimeFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Base Runtime";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.base";
        public const string FeatureExtensions = "XR_QCOM_component_versioning";

        [Tooltip("Check the installation of the runtime and show a dialog if it isn't installed.")]
        public bool CheckInstalledRuntime = true;
        [Tooltip("Start the application on the viewer device.")]
        public bool LaunchAppOnViewer = true;
        [Tooltip("Start the included Spaces Controller on the host device.")]
        public bool LaunchControllerOnHost = true;
        [Tooltip("Use a custom controller included in the asset on the host device instead of the default one in the package.")]
        public bool UseCustomController = false;
        [Tooltip("If this option is set to true, the exported application will be invisible on the device.")]
        public bool ExportHeadless = false;
        [Tooltip("If this option is set to a value, the defined activity will be started instead of the default Unity one.")]
        public string AlternateStartActivity = "";
        [Tooltip("If this option is set to true, no permission checks will be carried out during launch of the application.")]
        public bool SkipPermissionChecks = false;

        private static List<XRSessionSubsystemDescriptor> _sessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();

        protected override bool OnInstanceCreate(ulong instanceHandle) {
            base.OnInstanceCreate(instanceHandle);
            SetFrameStateCallback(OnFrameStateUpdate);

            if (TryRetrieveComponentVersions()) {
                string componentVersionsString = "Enumerating component information from Spaces Services:\n";
                foreach (var componentVersion in _componentVersions) {
                    componentVersionsString += "Component '" + componentVersion.ComponentName +
                                               "'\n    Version Identifier: " + componentVersion.VersionIdentifier +
                                               "'\n    Build Identifier: " + componentVersion.BuildIdentifier +
                                               "'\n    Build Date Time: " + componentVersion.BuildDateTime + "\n\n";
                }
                Debug.Log(componentVersionsString);
            }

            return true;
        }

        protected override void OnHookMethods() {
            HookMethod("xrGetComponentVersionsQCOM", out _xrGetComponentVersionsQCOM);
        }

        protected override void OnSubsystemCreate() => CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(_sessionSubsystemDescriptors, SessionSubsystem.ID);

        protected override void OnSubsystemStart() => StartSubsystem<XRSessionSubsystem>();

        protected override void OnSubsystemStop() => StopSubsystem<XRSessionSubsystem>();

        protected override void OnSubsystemDestroy()  => DestroySubsystem<XRSessionSubsystem>();
    }
}