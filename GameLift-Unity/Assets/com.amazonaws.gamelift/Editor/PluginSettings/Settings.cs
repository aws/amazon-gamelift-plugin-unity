// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    internal sealed class Settings
    {
        public static Settings SharedInstance { get; } = new Settings();

        public BootstrapSetting BootstrapSetting { get; }

        public CredentialsSetting CredentialsSetting { get; }

        public DotNetSetting DotNetSetting { get; } = new DotNetSetting();

        public GameLiftLocalSetting GameLiftLocalSetting { get; }

        public JavaSetting JavaSetting { get; }

        public IEnumerable<Setting> AllSettings { get; }

        public event Action AnySettingChanged;

        /// <summary>
        /// Only for testing.
        /// </summary>
        internal Settings(BootstrapSetting bootstrapSetting, CredentialsSetting credentialsSetting,
            GameLiftLocalSetting gameLiftLocalSetting, JavaSetting javaSetting)
        {
            BootstrapSetting = bootstrapSetting ?? throw new ArgumentNullException(nameof(bootstrapSetting));
            CredentialsSetting = credentialsSetting ?? throw new System.ArgumentNullException(nameof(credentialsSetting));
            GameLiftLocalSetting = gameLiftLocalSetting ?? throw new System.ArgumentNullException(nameof(gameLiftLocalSetting));
            JavaSetting = javaSetting ?? throw new System.ArgumentNullException(nameof(javaSetting));
        }

        private Settings()
        {
            CoreApi coreApi = CoreApi.SharedInstance;
            CredentialsSetting = new CredentialsSetting(coreApi);
            BootstrapSetting = new BootstrapSetting(coreApi, CredentialsSetting);
            GameLiftLocalSetting = new GameLiftLocalSetting(coreApi);
            JavaSetting = new JavaSetting(coreApi);
            AllSettings = new List<Setting>
            {
                CredentialsSetting,
                BootstrapSetting,
                DotNetSetting,
                GameLiftLocalSetting,
                JavaSetting
            };
        }

        public void Refresh()
        {
            bool isAnySettingChanged = false;

            foreach (Setting setting in AllSettings)
            {
                bool wasConfigured = setting.IsConfigured;
                setting.Refresh();

                if (setting.IsConfigured != wasConfigured)
                {
                    isAnySettingChanged = true;
                }
            }

            if (isAnySettingChanged)
            {
                AnySettingChanged?.Invoke();
            }
        }
    }
}
