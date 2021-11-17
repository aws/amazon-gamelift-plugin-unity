// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    [Serializable]
    internal sealed class SettingsState
    {
        public const int TabSdk = 0;
        public const int TabTest = 1;
        public const int TabDeploy = 2;
        public const int TabHelp = 3;

        private Status _status;
        private TextProvider _textProvider;
        private Settings _settings;

        public IReadStatus Status => _status;

        [field:SerializeField]
        public int ActiveTab { get; set; }

        public bool CanDeploy =>
            _settings.CredentialsSetting.IsConfigured
            && _settings.BootstrapSetting.IsConfigured;

        public bool CanRunLocalTest => _settings.GameLiftLocalSetting.IsConfigured
            && _settings.JavaSetting.IsConfigured;

        public SettingsState(Settings settings, TextProvider textProvider)
        {
            Restore(settings, textProvider);
        }

        public void Restore(Settings settings, TextProvider textProvider)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (textProvider is null)
            {
                throw new ArgumentNullException(nameof(textProvider));
            }

            _textProvider = textProvider;
            _settings = settings;
            _status = new Status();
        }

        public void Refresh()
        {
            bool allConfigured = _settings.AllSettings.All(setting => setting.IsConfigured);

            if (allConfigured == _status.IsDisplayed)
            {
                return;
            }

            _status.IsDisplayed = allConfigured;

            if (allConfigured)
            {
                _status.SetMessage(_textProvider.Get(Strings.LabelSettingsAllConfigured), UnityEditor.MessageType.Info);
            }
        }
    }
}
