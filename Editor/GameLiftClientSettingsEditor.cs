// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    [CustomEditor(typeof(GameLiftClientSettings))]
    public sealed class GameLiftClientSettingsEditor : UnityEditor.Editor
    {
        
        private SerializedProperty _remoteUrl;
        private SerializedProperty _region;
        private SerializedProperty _poolClientId;
        private SerializedProperty _isAnywhereTest;
        private SerializedProperty _computeName;
        private SerializedProperty _fleetID;
        private SerializedProperty _fleetLocation;
        private SerializedProperty _profileName;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var targetSettings = (GameLiftClientSettings)target;

            EditorGUILayout.PropertyField(_isAnywhereTest, new GUIContent("GameLift Anywhere", "Enabling this to make sample game client connect to game server running on an Anywhere fleet"));

            try
            {
                if (targetSettings.IsAnywhereTest)
                {
                    EditAnywhereMode(targetSettings);
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

        private void OnEnable()
        {
            _remoteUrl = serializedObject.FindProperty(nameof(GameLiftClientSettings.ApiGatewayUrl));
            _poolClientId = serializedObject.FindProperty(nameof(GameLiftClientSettings.UserPoolClientId));
            _region = serializedObject.FindProperty(nameof(GameLiftClientSettings.AwsRegion));
            _isAnywhereTest = serializedObject.FindProperty(nameof(GameLiftClientSettings.IsAnywhereTest));
            _computeName = serializedObject.FindProperty(nameof(GameLiftClientSettings.computeName));
            _fleetID = serializedObject.FindProperty(nameof(GameLiftClientSettings.fleetID));
            _fleetLocation = serializedObject.FindProperty(nameof(GameLiftClientSettings.fleetLocation));
            _profileName = serializedObject.FindProperty(nameof(GameLiftClientSettings.profileName));
        }

        private void EditAnywhereMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_computeName, new GUIContent("Compute Name", "Fleet Compute Name"));
            EditorGUILayout.PropertyField(_fleetID, new GUIContent("FleetID", "This Fleet Id should match the value generated in the AWS GameLift Console"));
            EditorGUILayout.PropertyField(_fleetLocation, new GUIContent("Fleet Location", "This Location should match the value defined in the AWS GameLift Console"));
            EditorGUILayout.PropertyField(_profileName, new GUIContent("Profile Name", "This Name should match the value defined on the users AWS Account"));

            if (string.IsNullOrWhiteSpace(targetSettings.computeName))
            {
                EditorGUILayout.HelpBox("Please set the Fleet Compute Name.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.fleetID))
            {
                EditorGUILayout.HelpBox("Please set the FleetID.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.fleetLocation))
            {
                EditorGUILayout.HelpBox("Please set the Fleet Location.", MessageType.Warning);
            }
            
            if (string.IsNullOrWhiteSpace(targetSettings.profileName))
            {
                EditorGUILayout.HelpBox("Please set the AWS Profile Name.", MessageType.Warning);
            }
        }

        private void EditGameLiftMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_remoteUrl, new GUIContent("API Gateway Endpoint", "API Gateway URL"));
            EditorGUILayout.PropertyField(_poolClientId, new GUIContent("Cognito Client ID", "AWS region used for communicating with Cognito and API Gateway"));
            EditorGUILayout.PropertyField(_region, new GUIContent("AWS Region"));

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
