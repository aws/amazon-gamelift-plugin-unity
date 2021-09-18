// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal static class EditorMenu
    {
        [MenuItem("GameLift/Import Sample Game", priority = 8000)]
        public static void ImportSampleGame()
        {
            string filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";
            AssetDatabase.ImportPackage(filePackagePath, interactive: true);
        }

        [MenuItem("GameLift/Plugin Settings", priority = 0)]
        public static void ShowPluginSettings()
        {
            Assembly unityEditorAssembly = typeof(UnityEditor.Editor).Assembly;

            Type settingsWindowType = unityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

            if (settingsWindowType == null)
            {
                settingsWindowType = unityEditorAssembly.GetType("UnityEditor.InspectorWindow");
            }

            EditorWindow.GetWindow<SettingsWindow>(settingsWindowType);
        }

        public static void ShowDeployment()
        {
            EditorWindow.GetWindow<DeploymentWindow>();
        }

        public static void ShowLocalTesting()
        {
            EditorWindow.GetWindow<LocalTestWindow>();
        }

        public static void OpenForums()
        {
            Application.OpenURL(Urls.AwsGameTechForums);
        }

        public static void OpenGameLiftServerCSharpSdkIntegrationDoc()
        {
            Application.OpenURL(Urls.GameLiftServerCSharpSdkIntegrationDoc);
        }

        public static void OpenGameLiftServerCSharpSdkApiDoc()
        {
            Application.OpenURL(Urls.GameLiftServerCSharpSdkApiDoc);
        }

        public static void OpenAwsDocumentation()
        {
            Application.OpenURL(Urls.AwsHelpGameLiftUnity);
        }

        public static void ReportSecurity()
        {
            Application.OpenURL(Urls.AwsSecurity);
        }

        public static void ReportBugs()
        {
            Application.OpenURL(Urls.GitHubAwsLabs);
        }

        public static void PingSdk()
        {
            string filePackagePath = $"Packages/{Paths.PackageName}/{Paths.ServerSdkDllInPackage}";
            UnityEngine.Object sdk = AssetDatabase.LoadMainAssetAtPath(filePackagePath);

            if (sdk)
            {
                Selection.activeObject = sdk;
                EditorGUIUtility.PingObject(sdk);
            }
        }
    }
}

