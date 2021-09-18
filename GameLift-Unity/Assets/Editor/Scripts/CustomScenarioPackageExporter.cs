// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AWS.GameLift.Editor
{
    public static class CustomScenarioPackageExporter
    {
        [MenuItem("Assets/Export Custom Scenario")]
        public static void Export()
        {
            string[] exportedPackageAssetList = new string[]
            {
                "Assets\\Editor\\Custom Scenario"
            };

            string sampleFolder = @"..\GameLift-Unity\Assets\com.amazonaws.gamelift\Examples~\CustomScenario";

            if (!Directory.Exists(sampleFolder))
            {
                Directory.CreateDirectory(sampleFolder);
            }

            AssetDatabase.ExportPackage(exportedPackageAssetList, Path.Combine(sampleFolder, "sample.unitypackage"),
                ExportPackageOptions.Recurse);
            Debug.Log(nameof(CustomScenarioPackageExporter) + " finished.");
        }
    }
}
