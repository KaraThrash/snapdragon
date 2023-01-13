/******************************************************************************
 * File: BaseRuntimeFeature.FeatureValidation.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup) {
            rules.Add(new ValidationRule(this) {
                message = "Only the \"Microsoft Mixed Reality Motion Controller Profile\" is fully supported by the included samples and recommended therefore right now. The \"Khronos Simple Controller Profile\" is also supported but some sample features might not fully work with it.",
                checkPredicate = () => {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings) {
                        return false;
                    }
                    return settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature.enabled && feature is SpacesMicrosoftMixedRealityMotionControllerProfile);
                },
                fixIt = () => {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (!settings) {
                        return;
                    }

                    var feature = settings.GetFeatures<OpenXRInteractionFeature>().SingleOrDefault(feature => feature is SpacesMicrosoftMixedRealityMotionControllerProfile);
                    if (feature) {
                        feature.enabled = true;
                    }
                },
                error = false,
            });
            rules.Add(new ValidationRule(this) {
                message = "Only the OpenGLES3 graphics API is supported at the moment.",
                checkPredicate = () => {
                    return PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).SequenceEqual(new []{ GraphicsDeviceType.OpenGLES3 });
                },
                fixIt = () => {
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new []{ GraphicsDeviceType.OpenGLES3 });
                },
                error = true,
            });
            rules.Add(new ValidationRule(this) {
                message = "Multihreaded rendering is not supported.",
                checkPredicate = () => {
                    return !PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
                },
                fixIt = () => {
                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);
                },
                error = true,
            });
            rules.Add(new ValidationRule(this) {
                message = "Minimum Android SDK version has to be equal or greater than 29.",
                checkPredicate = () => {
                    return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel29;
                },
                fixIt = () => {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                },
                error = true,
            });
            rules.Add(new ValidationRule(this) {
                message = "The Scripting backend has to be set to IL2CPP for arm64.",
                checkPredicate = () => {
                    var isUsingIIL2CPP = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
                    var isTargetingARM64 = PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64;

                    return isUsingIIL2CPP && isTargetingARM64;
                },
                fixIt = () => {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                },
                error = true,
            });
            rules.Add(new ValidationRule(this) {
                message = "Device rotation set to AutoRotation is not supported, when launching the application straight to the Viewer. Change the default orientation to \"Landscape Left\".",
                checkPredicate = () => {
                    if (LaunchAppOnViewer) {
                        return PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation;
                    }
                    return true;
                },
                fixIt = () => {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                },
                error = true,
            });
        }
    }
}
#endif