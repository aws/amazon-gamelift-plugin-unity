// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

namespace AmazonGameLift.Editor
{
    internal sealed class SettingsStatus
    {
        private readonly Status _status = new Status();
        private readonly TextProvider _textProvider;
        private readonly Settings _settings;

        public IReadStatus Status => _status;

        public SettingsStatus(Settings settings, TextProvider textProvider)
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
