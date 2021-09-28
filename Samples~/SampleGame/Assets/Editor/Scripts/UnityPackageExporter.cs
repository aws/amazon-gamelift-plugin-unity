// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UnityPackageExporter
{
    public static void Export()
    {
        var exportedPackageAssetList = Directory.EnumerateDirectories("Assets")
            .Except(new string[]
            {
                "Assets\\Editor",
                "Assets\\Tests",
            })
            .ToList();


        Debug.Log("Exporting Sample Game...");

        exportedPackageAssetList.AddRange(Directory.EnumerateFiles("Assets"));
        exportedPackageAssetList.Add("Assets\\Editor\\Scripts\\GameLiftClientSettingsMenu.cs");
        exportedPackageAssetList.Add("Assets\\Editor\\Scripts\\ClientServerSwitchMenu.cs");

        string outputFolder = @"..";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string outputPath = Path.Combine(outputFolder, "SampleGame.unitypackage");

        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), outputPath,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

        Debug.Log("Sample Game exported to " + outputPath);
    }
}
