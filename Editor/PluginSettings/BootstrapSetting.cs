// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    internal class BootstrapSetting : Setting
    {
        private readonly CoreApi _coreApi;
        private readonly CredentialsSetting _credentialsSetting;

        public BootstrapSetting(CoreApi coreApi, CredentialsSetting credentialsSetting)
            : base(Strings.LabelSettingsBootstrapTitle, Strings.LabelSettingsBootstrapButton,
                  Strings.TooltipSettingsBootstrap, Strings.LabelSettingsBootstrapWarning)
        {
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _credentialsSetting = credentialsSetting ?? throw new ArgumentNullException(nameof(credentialsSetting));
            IsPrimaryActionEnabled = false;
        }

        internal override void RunPrimaryAction()
        {
            EditorWindow.GetWindow<BootstrapWindow>();
        }

        internal override void Refresh()
        {
            base.Refresh();
            IsPrimaryActionEnabled = _credentialsSetting.IsConfigured;
        }

        protected override bool RefreshIsConfigured()
        {
            GetSettingResponse bucketNameResponse = _coreApi.GetSetting(SettingsKeys.CurrentBucketName);
            string currentBucketName = bucketNameResponse.Success ? bucketNameResponse.Value : null;
            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);
            string currentRegion = currentRegionResponse.Success ? currentRegionResponse.Value : null;

            return !string.IsNullOrEmpty(currentBucketName) && _coreApi.IsValidRegion(currentRegion);
        }
    }
}
