// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Linq;
using UnityEditor;

public static class UnityPackageExporter
{
    [MenuItem("Assets/Export Sample")]
    public static void Export()
    {
        var exportedPackageAssetList = Directory.EnumerateDirectories("Assets")
            .Except(new string[]
            {
                "Assets\\Editor",
                "Assets\\Tests",
            })
            .ToList();

        exportedPackageAssetList.AddRange(Directory.EnumerateFiles("Assets"));
        exportedPackageAssetList.Add("Assets\\Editor\\Scripts\\GameLiftClientSettingsMenu.cs");
        exportedPackageAssetList.Add("Assets\\Editor\\Scripts\\ClientServerSwitchMenu.cs");

        string sampleFolder = @"..\GameLift-Unity\Assets\com.amazonaws.gamelift\Examples~\SampleGame";

        if (!Directory.Exists(sampleFolder))
        {
            Directory.CreateDirectory(sampleFolder);
        }

        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), Path.Combine(sampleFolder, "sample.unitypackage"),
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }
}
