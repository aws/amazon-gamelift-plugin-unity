// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    [CustomEditor(typeof(GameLiftClientSettings))]
    public sealed class GameLiftClientSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _isLocalTest;
        private SerializedProperty _localPort;
        private SerializedProperty _remoteUrl;
        private SerializedProperty _region;
        private SerializedProperty _poolClientId;

        private void OnEnable()
        {
            _isLocalTest = serializedObject.FindProperty(nameof(GameLiftClientSettings.IsLocalTest));
            _localPort = serializedObject.FindProperty(nameof(GameLiftClientSettings.LocalPort));
            _remoteUrl = serializedObject.FindProperty(nameof(GameLiftClientSettings.ApiGatewayUrl));
            _region = serializedObject.FindProperty(nameof(GameLiftClientSettings.AwsRegion));
            _poolClientId = serializedObject.FindProperty(nameof(GameLiftClientSettings.UserPoolClientId));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var targetSettings = (GameLiftClientSettings)target;

            EditorGUILayout.PropertyField(_isLocalTest, new GUIContent("Local Testing Mode", "Enabling this to make sample game client connect to game server running on localhost"));

            try
            {
                if (targetSettings.IsLocalTest)
                {
                    EditLocalMode(targetSettings);
                }
                else
                {
                    EditGameLiftMode(targetSettings);
                }
            }
            finally
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void EditLocalMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.LabelField("GameLift Local URL", targetSettings.LocalUrl);
            EditorGUILayout.PropertyField(_localPort, new GUIContent("GameLift Local Port", "This port should match the port value defined in Local Testing"));

            if (targetSettings.LocalPort == 0)
            {
                EditorGUILayout.HelpBox("Please set the GameLift Local port.", MessageType.Warning);
            }
        }

        private void EditGameLiftMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_remoteUrl, new GUIContent("API Gateway Endpoint", "API Gateway URL"));
            EditorGUILayout.PropertyField(_region, new GUIContent("AWS Region", "AWS region used for communicating with Cognito and API Gateway"));
            EditorGUILayout.PropertyField(_poolClientId, new GUIContent("Cognito Client ID"));

            if (string.IsNullOrWhiteSpace(targetSettings.ApiGatewayUrl))
            {
                EditorGUILayout.HelpBox("Please set the API Gateway URL.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.AwsRegion))
            {
                EditorGUILayout.HelpBox("Please set the AWS Region.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.UserPoolClientId))
            {
                EditorGUILayout.HelpBox("Please set the User Pool Client ID.", MessageType.Warning);
            }
        }
    }
}
