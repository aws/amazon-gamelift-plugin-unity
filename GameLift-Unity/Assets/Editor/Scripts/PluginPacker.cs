// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace AWS.GameLift.Editor
{
    public static class PluginPacker
    {
        private static readonly int DEFAULT_POLL_INTERVAL_IN_MS = 100;

        [MenuItem("Assets/Pack Plugin")]
        public static void Pack()
        {
            string pluginDirectory = @"..\GameLift-Unity\Assets\com.amazonaws.gamelift";
            string targetDirectory = @"..\build";

            if (!Directory.Exists(pluginDirectory))
            {
                throw new FileNotFoundException($"Unable to find the plugin folder: {pluginDirectory}");
            }

            if (!Directory.Exists(targetDirectory))
            {
                Debug.Log($"Creating a build output directory at: {targetDirectory}");
                Directory.CreateDirectory(targetDirectory);
            }

            // UnityEditor PackageManager Pack doc: https://docs.unity3d.com/ScriptReference/PackageManager.Client.Pack.html
            Debug.Log($"Packing the plugin artifacts into package tarball at: {targetDirectory}");
            var packRequest = UnityEditor.PackageManager.Client.Pack(pluginDirectory, targetDirectory);

            while (!packRequest.IsCompleted)
            {
                Thread.Sleep(DEFAULT_POLL_INTERVAL_IN_MS);
            }

            if (packRequest.Status != UnityEditor.PackageManager.StatusCode.Success)
            {
                throw new System.InvalidOperationException($"Failed to pack package tarball. Error: {packRequest.Error.message}");
            }

            Debug.Log($"Sucessfully packed plugin artifacts into package tarball at: {packRequest.Result.tarballPath}");
        }
    }
}
