// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Ensures that GameLift Local is available. Primary action is to open a web browser with the download page.
    /// </summary>
    internal class GameLiftLocalSetting : Setting
    {
        private readonly CoreApi _coreApi;

        public GameLiftLocalSetting(CoreApi coreApi)
          : base(Strings.LabelSettingsLocalTestingTitle, Strings.LabelSettingsLocalTestingButton, Strings.TooltipSettingsLocalTesting) =>
            _coreApi = coreApi ?? throw new System.ArgumentNullException(nameof(coreApi));

        internal override void RunPrimaryAction()
        {
            Application.OpenURL(Urls.AwsGameLiftLocal);
        }

        protected override bool RefreshIsConfigured()
        {
            GetSettingResponse response = _coreApi.GetSetting(SettingsKeys.GameLiftLocalPath);
            return response.Success && _coreApi.FileExists(response.Value);
        }

        internal virtual void SetPath(string path)
        {
            _coreApi.PutSetting(SettingsKeys.GameLiftLocalPath, path);
            RefreshIsConfigured();
        }
    }
}
