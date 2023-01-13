/******************************************************************************
 * File: SpacesMicrosoftMixedRealityMotionControllerProfile.cs
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Microsoft Mixed Reality Motion Controller Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Allows for mapping input to the Microsoft Mixed Reality Motion Controller interaction profile.",
        OpenxrExtensionStrings = "",
        Version = "0.9.0",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class SpacesMicrosoftMixedRealityMotionControllerProfile : MicrosoftMotionControllerProfile { }
}