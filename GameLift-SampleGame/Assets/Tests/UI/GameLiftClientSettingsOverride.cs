// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using UnityEditor;

namespace SampleTests.UI
{
    internal sealed class GameLiftClientSettingsOverride
    {
        private GameLiftClientSettings _settings;
        private GameLiftConfiguration _backupConfig;

        public void SetUp(TestSettings testSettings)
        {
            _settings = AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>(AssetPaths.GameLiftClientSettings);
            _backupConfig = _settings.GetConfiguration();
            _settings.AwsRegion = testSettings.Region;
            _settings.UserPoolClientId = testSettings.UserPoolClientId;
            _settings.ApiGatewayEndpoint = testSettings.ApiGatewayEndpoint;
        }

        public void TearDown()
        {
            if (_settings != null)
            {
                _settings.AwsRegion = _backupConfig.AwsRegion;
                _settings.UserPoolClientId = _backupConfig.UserPoolClientId;
                _settings.ApiGatewayEndpoint = _backupConfig.ApiGatewayEndpoint;
            }
        }
    }
}
