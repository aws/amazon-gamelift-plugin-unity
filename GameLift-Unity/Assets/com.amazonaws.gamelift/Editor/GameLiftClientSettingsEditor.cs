// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    [CustomEditor(typeof(GameLiftClientSettings))]
    public sealed class GameLiftClientSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targetSettings = (GameLiftClientSettings)target;

            if (string.IsNullOrWhiteSpace(targetSettings.ApiGatewayEndpoint))
            {
                EditorGUILayout.HelpBox("Please set API Gateway Endpoint.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.AwsRegion))
            {
                EditorGUILayout.HelpBox("Please set AWS Region.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.UserPoolClientId))
            {
                EditorGUILayout.HelpBox("Please set User Pool Client ID.", MessageType.Warning);
            }
        }
    }
}
