// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Ensures that the AWS profile and region are set up. Primary action is to open the setup window.
    /// </summary>
    internal class CredentialsSetting : Setting
    {
        private readonly CoreApi _coreApi;

        public CredentialsSetting(CoreApi coreApi)
            : base(Strings.LabelSettingsAwsCredentialsTitle, Strings.LabelSettingsAwsCredentialsSetUpButton, Strings.TooltipSettingsAwsCredentials) =>
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));

        internal override void RunPrimaryAction()
        {
            EditorWindow.GetWindow<AwsCredentialsWindow>();
        }

        protected override bool RefreshIsConfigured()
        {
            PrimaryActionMessage = Strings.LabelSettingsAwsCredentialsSetUpButton;
            GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!profileResponse.Success)
            {
                return false;
            }

            RetriveAwsCredentialsResponse credentialsResponse = _coreApi.RetrieveAwsCredentials(profileResponse.Value);

            if (!credentialsResponse.Success)
            {
                return false;
            }

            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!currentRegionResponse.Success || !_coreApi.IsValidRegion(currentRegionResponse.Value))
            {
                return false;
            }

            PrimaryActionMessage = Strings.LabelAwsCredentialsUpdateButton;
            return true;
        }
    }
}
