// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Runs the periodic settings checks; started by Unity.
    /// </summary>
    internal sealed class SettingsChecker
    {
        private const int CheckPeriodMs = 3000;

        [InitializeOnLoadMethod]
        private static async void StartPeriodicChecks()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            Settings settings = Settings.SharedInstance;

            while (true)
            {
                if (SettingsWindow.IsOpen)
                {
                    settings.Refresh();
                }

                await Task.Delay(CheckPeriodMs);
            }
        }
    }
}
