// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    internal static class SettingsFactory
    {
        public static List<SettingPanel> CreateSettingPanels(TextProvider textProvider)
        {
            if (textProvider is null)
            {
                throw new ArgumentNullException(nameof(textProvider));
            }

            Settings settings = Settings.SharedInstance;
            return new List<SettingPanel>
            {
                new SettingPanel(settings.DotNetSetting, textProvider),
                new GameLiftSettingPanel(settings.GameLiftLocalSetting, textProvider),
                new SettingPanel(settings.JavaSetting, textProvider),
                new SettingPanel(settings.CredentialsSetting, textProvider),
                new SettingPanel(settings.BootstrapSetting, textProvider)
            };
        }
    }
}
