// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
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

        [MenuItem("GameLift/Plugin Settings")]
        public static void ShowPluginSettings() => EditorWindow.GetWindow<SettingsWindow>();

        [MenuItem("GameLift/Deployment")]
        public static void ShowDeployment() => EditorWindow.GetWindow<DeploymentWindow>();

        [MenuItem("GameLift/Local Testing")]
        public static void ShowLocalTesting() => EditorWindow.GetWindow<LocalTestWindow>();

        [MenuItem("GameLift/Help/Open PDF documentation", priority = 1000)]
        public static void OpenUserGuidePdf()
        {
            string filePackagePath = $"Packages/{Paths.PackageName}/{Paths.PdfGuideFileInPackage}";
            string url = Path.GetFullPath(filePackagePath);
            Application.OpenURL(url);
        }

        [MenuItem("GameLift/Help/Open AWS GameTech Forums", priority = 1001)]
        public static void OpenForums()
        {
            Application.OpenURL(Urls.AwsGameTechForums);
        }

        [MenuItem("GameLift/Help/Open AWS documentation", priority = 1002)]
        public static void OpenAwsDocumentation()
        {
            Application.OpenURL(Urls.AwsHelpGameLiftUnity);
        }

        [MenuItem("GameLift/Help/Report security vulnerabilities", priority = 1003)]
        public static void ReportSecurity()
        {
            Application.OpenURL(Urls.AwsSecurity);
        }

        [MenuItem("GameLift/Help/Report bugs \u2215 issues", priority = 1004)]
        public static void ReportBugs()
        {
            Application.OpenURL(Urls.GitHubAwsLabs);
        }
    }
}

