// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Shows the settings window once; started by Unity.
    /// </summary>
    internal sealed class FirstTimeSettingsLauncher
    {
        private const string NonEmptyValue = "1";

        [InitializeOnLoadMethod]
        private static void Run()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            GetSettingResponse response = CoreApi.SharedInstance.GetSetting(SettingsKeys.WasSettingsWindowShown);

            if (!response.Success)
            {
                EditorMenu.ShowPluginSettings();
            }
        }

        internal static void SetSetting()
        {
            CoreApi.SharedInstance.PutSetting(SettingsKeys.WasSettingsWindowShown, NonEmptyValue);
        }
    }
}
