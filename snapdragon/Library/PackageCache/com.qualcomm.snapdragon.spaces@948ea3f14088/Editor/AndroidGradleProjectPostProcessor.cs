/******************************************************************************
 * File: AndroidGradleProjectPostProcessor.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Editor
{
    public class AndroidGradleProjectPostProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path) {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            var isOpenXRLoaderActive = settings.Manager.activeLoaders?.Any(loader => loader.GetType() == typeof(OpenXRLoader));
            var openXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var baseRuntimeFeature = openXRSettings?.GetFeature<BaseRuntimeFeature>();

            if (isOpenXRLoaderActive == true && baseRuntimeFeature != null && baseRuntimeFeature.enabled) {
                ModifyArchiveInBuildGradle(true, path, "UnityServicesHelper");
                ModifyArchiveInBuildGradle(true, path, "libopenxr_loader");
                ModifyUnityPlayerActivityLaunchCategory(false, path, "com.unity3d.player.UnityPlayerActivity");

                SetMetaData(!baseRuntimeFeature.LaunchAppOnViewer, path, "SKIP_LAUNCH_ON_VIEWER", "true");
                ModifySpacesLauncherLaunchCategory(!baseRuntimeFeature.ExportHeadless, path);

                /* Determine if the controller archive should be replaced or overwritten by a possible same-name archive from the Assets folder. */
                var projectArchives = new DirectoryInfo(Application.dataPath).GetFiles("*.aar");
                var addController = !baseRuntimeFeature.UseCustomController || (baseRuntimeFeature.UseCustomController && projectArchives.Any(fileInfo => fileInfo.Name == "SpacesController.aar"));

                ModifyArchiveInBuildGradle(baseRuntimeFeature.LaunchControllerOnHost && addController, path, "SpacesController");
                ModifyGradleProjectForHostController(baseRuntimeFeature.LaunchControllerOnHost, path);
                ModifyPlayerActivityWindowFocusChangeBehaviour(baseRuntimeFeature.LaunchControllerOnHost, path);
                SetMetaData(baseRuntimeFeature.AlternateStartActivity != "", path, "ALTERNATE_LAUNCH_ACTIVITY", baseRuntimeFeature.AlternateStartActivity);
                SetMetaData(baseRuntimeFeature.SkipPermissionChecks, path, "SKIP_PERMISSION_CHECKS", "true");
            } else {
                /* Roll back every change that was made if the Base Runtime Feature is not enabled. */
                ModifyArchiveInBuildGradle(false, path, "UnityServicesHelper");
                ModifyArchiveInBuildGradle(false, path, "UnityCustomLauncher");
                ModifyArchiveInBuildGradle(false, path, "SpacesController");
                ModifyArchiveInBuildGradle(false, path, "libopenxr_loader");
                ModifyGradleProjectForHostController(false, path);
                ModifyUnityPlayerActivityLaunchCategory(true, path, "com.unity3d.player.UnityPlayerActivity");
                ModifyPlayerActivityWindowFocusChangeBehaviour(false, path);
                SetMetaData(false, path, "ALTERNATE_LAUNCH_ACTIVITY");
                SetMetaData(false, path, "SKIP_LAUNCH_ON_VIEWER");
                SetMetaData(false, path, "SKIP_PERMISSION_CHECKS");
            }
        }

        private void ModifyArchiveInBuildGradle(bool add, string path, string archiveName) {
            var gradlePropertiesPath = Path.Combine(path, "build.gradle");
            var lines = new List<string>(File.ReadAllLines(gradlePropertiesPath));
            var regex = "(?:[^A-Za-z])(" + archiveName + ")(?:[^A-Za-z],)";

            if (add) {
                if (!lines.Any(line => Regex.Match(line, regex).Success)) {
                    int index = GetIndexForMatch(ref lines,  new [] {"dependencies" , "{"});
                    ModifyBuildScript(true,"    implementation(name: '" + archiveName + "', ext:'aar')", ref lines, index);
                }
            } else {
                lines.RemoveAll(line => Regex.Match(line, regex).Success);
            }

            File.WriteAllText(gradlePropertiesPath, String.Join(Environment.NewLine, lines));
        }

        private void ModifyUnityPlayerActivityLaunchCategory(bool add, string path, string activity) {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");

            var xmlDocument = new XmlDocument();
            using (var reader = new XmlTextReader(path)) {
                reader.Read();
                xmlDocument.Load(reader);
            }

            var launcherSelector = "/manifest/application/activity[@android:name='" + activity + "']/";
            launcherSelector += "intent-filter/category[@android:name='android.intent.category.LAUNCHER']";

            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            var launcherNode = xmlDocument.SelectSingleNode(launcherSelector, namespaceManager);

            bool overwriteFile = false;

            if (add && launcherNode == null) {
                launcherSelector = "/manifest/application/activity[@android:name='" + activity + "']/intent-filter";
                var intentNode = xmlDocument.SelectSingleNode(launcherSelector, namespaceManager);
                if (intentNode == null) {
                    return;
                }
                launcherNode = xmlDocument.CreateNode(XmlNodeType.Element, "category", namespaceManager.DefaultNamespace);
                var launcherAttribute = xmlDocument.CreateAttribute("android", "name", namespaceManager.LookupNamespace("android"));
                launcherAttribute.Value = "android.intent.category.LAUNCHER";
                launcherNode.Attributes.Append(launcherAttribute);
                intentNode.AppendChild(launcherNode);
                overwriteFile = true;
            }

            if (!add && launcherNode != null && launcherNode.ParentNode != null) {
                launcherNode.ParentNode.RemoveChild(launcherNode);
                overwriteFile = true;
            }

            if (!overwriteFile) {
                return;
            }

            using (var writer = new XmlTextWriter(path, new System.Text.UTF8Encoding(false))) {
                writer.Formatting = Formatting.Indented;
                xmlDocument.Save(writer);
            }
        }

        private void ModifySpacesLauncherLaunchCategory(bool add, string path) {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");

            var xmlDocument = new XmlDocument();
            using (var reader = new XmlTextReader(path)) {
                reader.Read();
                xmlDocument.Load(reader);
            }

            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            namespaceManager.AddNamespace("tools", "http://schemas.android.com/tools");

            var activityNode = xmlDocument.SelectSingleNode("/manifest/application/activity[@android:name='com.qualcomm.snapdragon.spaces.unitycustomlauncher.SpacesLauncher']", namespaceManager);
            if (activityNode == null) {
                var applicationNode = xmlDocument.SelectSingleNode("/manifest/application", namespaceManager);
                activityNode = xmlDocument.CreateNode(XmlNodeType.Element, "activity", namespaceManager.DefaultNamespace);
                var activityAttribute = xmlDocument.CreateAttribute("android", "name", namespaceManager.LookupNamespace("android"));
                activityAttribute.Value = "com.qualcomm.snapdragon.spaces.unitycustomlauncher.SpacesLauncher";
                activityNode.Attributes.Append(activityAttribute);
                var mergeAttribute = xmlDocument.CreateAttribute("tools", "node", namespaceManager.LookupNamespace("tools"));
                mergeAttribute.Value = add ? "merge" : "remove";
                activityNode.Attributes.Append(mergeAttribute);
                applicationNode.AppendChild(activityNode);

                var intentNode = xmlDocument.CreateNode(XmlNodeType.Element, "intent-filter", namespaceManager.DefaultNamespace);
                activityNode.AppendChild(intentNode);
                var launcherNode = xmlDocument.CreateNode(XmlNodeType.Element, "category", namespaceManager.DefaultNamespace);
                var launcherAttribute = xmlDocument.CreateAttribute("android", "name", namespaceManager.LookupNamespace("android"));
                launcherAttribute.Value = "android.intent.category.LAUNCHER";
                launcherNode.Attributes.Append(launcherAttribute);
                intentNode.AppendChild(launcherNode);
            } else {
                foreach (XmlAttribute attribute in activityNode.Attributes) {
                    if (!attribute.Prefix.Equals("tools") || !attribute.LocalName.Equals("node")) {
                        continue;
                    }

                    activityNode.Attributes.Remove(attribute);
                    var mergeAttribute = xmlDocument.CreateAttribute("tools", "node", namespaceManager.LookupNamespace("tools"));
                    mergeAttribute.Value = add ? "merge" : "remove";
                    activityNode.Attributes.Append(mergeAttribute);
                    break;
                }
            }

            using (var writer = new XmlTextWriter(path, new System.Text.UTF8Encoding(false))) {
                writer.Formatting = Formatting.Indented;
                xmlDocument.Save(writer);
            }
        }

        private void ModifyGradleProjectForHostController(bool applyPatch, string path) {
            var gradlePropertiesPath = Path.Combine(path, "..", "gradle.properties");
            var lines = new List<string>(File.ReadAllLines(gradlePropertiesPath));
            ModifyBuildScript(true, "android.useAndroidX=true", ref lines);
            ModifyBuildScript(applyPatch, "android.enableJetifier=true", ref lines);
            File.WriteAllText(gradlePropertiesPath, String.Join(Environment.NewLine, lines));

            var launcherBuildGradlePath = Path.Combine(path, "..", "launcher", "build.gradle");
            lines = new List<string>(File.ReadAllLines(launcherBuildGradlePath));
            int index = GetIndexForMatch(ref lines,  new [] {"implementation", "project", "unityLibrary"});
            ModifyBuildScript(applyPatch,"    implementation 'com.android.support.constraint:constraint-layout:1.1.3'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'com.google.android.material:material:1.3.0'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'androidx.navigation:navigation-fragment-ktx:2.4.2'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'androidx.navigation:navigation-ui-ktx:2.4.2'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'androidx.lifecycle:lifecycle-livedata-ktx:2.4.1'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'androidx.lifecycle:lifecycle-viewmodel-ktx:2.4.1'", ref lines, index);
            ModifyBuildScript(applyPatch,"    implementation 'androidx.databinding:viewbinding:7.2.1'", ref lines, index);
            index = GetIndexForMatch(ref lines,  new [] {"apply", "plugin", "com.android.application"});
            ModifyBuildScript(applyPatch,"apply plugin: 'kotlin-android'", ref lines, index);
            index = GetIndexForMatch(ref lines,  new [] {"android", "{"});
            ModifyBuildScript(applyPatch,"    buildFeatures { viewBinding true }", ref lines, index);

            File.WriteAllText(launcherBuildGradlePath, String.Join(Environment.NewLine, lines));

            var mainBuildGradlePath = Path.Combine(path, "..", "build.gradle");
            lines = new List<string>(File.ReadAllLines(mainBuildGradlePath));
            index = GetIndexForMatch(ref lines,  new [] {"classpath", "android", "build", "gradle"});
            ModifyBuildScript(applyPatch,"            classpath 'org.jetbrains.kotlin:kotlin-gradle-plugin:1.6.10'", ref lines, index);
            File.WriteAllText(mainBuildGradlePath, String.Join(Environment.NewLine, lines));
        }

        private void ModifyPlayerActivityWindowFocusChangeBehaviour(bool add, string path) {
            /* Because Spaces applications launch on glasses (secondary display) we can ignore focus lost events which allow for
             * keyevents to still be processed by unity for remote OpenXR controllers to function. */
            var unityPlayerActivityPath = Path.Combine(path, "..", "unityLibrary", "src", "main", "java", "com", "unity3d", "player", "UnityPlayerActivity.java");
            var lines = new List<string>(File.ReadAllLines(unityPlayerActivityPath));
            int index = GetIndexForMatch(ref lines,  new [] {"super", "onWindowFocusChanged", "hasFocus"});
            ModifyBuildScript(add,"        if (!hasFocus) return;", ref lines, index - 1);
            File.WriteAllText(unityPlayerActivityPath, String.Join(Environment.NewLine, lines));
        }

        private void SetMetaData(bool add, string path, string key, string data = "") {
            path = Path.Combine(path, "src", "main", "AndroidManifest.xml");

            key = "com.qualcomm.snapdragon.spaces." + key;

            var xmlDocument = new XmlDocument();
            using (var reader = new XmlTextReader(path)) {
                reader.Read();
                xmlDocument.Load(reader);
            }

            var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            namespaceManager.AddNamespace("tools", "http://schemas.android.com/tools");

            var metaDataNode = xmlDocument.SelectSingleNode("/manifest/application/meta-data[@android:name='" + key + "']", namespaceManager);
            if (metaDataNode != null) {
                metaDataNode.ParentNode.RemoveChild(metaDataNode);
            }
            if (add && data != "") {
                var applicationNode = xmlDocument.SelectSingleNode("/manifest/application", namespaceManager);
                metaDataNode = xmlDocument.CreateNode(XmlNodeType.Element, "meta-data", namespaceManager.DefaultNamespace);
                var metaDataKeyAttribute = xmlDocument.CreateAttribute("android", "name", namespaceManager.LookupNamespace("android"));
                metaDataKeyAttribute.Value = key;
                metaDataNode.Attributes.Append(metaDataKeyAttribute);
                var metaDataValueAttribute = xmlDocument.CreateAttribute("android", "value", namespaceManager.LookupNamespace("android"));
                metaDataValueAttribute.Value = data;
                metaDataNode.Attributes.Append(metaDataValueAttribute);

                applicationNode.AppendChild(metaDataNode);
            }

            using (var writer = new XmlTextWriter(path, new System.Text.UTF8Encoding(false))) {
                writer.Formatting = Formatting.Indented;
                xmlDocument.Save(writer);
            }
        }

        private void ModifyBuildScript(bool add, string newLine, ref List<string> lines, int index = -1) {
            if (add) {
                if (!lines.Any(line => line.Replace(" ", "").Contains(newLine.Replace(" ", "")))) {
                    lines.Insert(index == -1 ? lines.Count : index + 1, newLine);
                }
            } else {
                lines.RemoveAll(line => line.Replace(" ", "").Contains(newLine.Replace(" ", "")));
            }
        }

        private int GetIndexForMatch(ref List<string> lines, string[] matchPhrases) {
            try {
                var index = lines.FindIndex(line => matchPhrases.All(line.Contains));
                return index;
            } catch {
                return -1;
            }
        }
    }
}