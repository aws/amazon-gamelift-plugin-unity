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

            EditorGUILayout.PropertyField(_isAnywhereTest, new GUIContent("GameLift Anywhere", "GameLift Anywhere Mode"));

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
            
            _isAnywhereTest = serializedObject.FindProperty(nameof(GameLiftClientSettings.IsAnywhereTest));
            _computeName = serializedObject.FindProperty(nameof(GameLiftClientSettings.ComputeName));
            _fleetID = serializedObject.FindProperty(nameof(GameLiftClientSettings.FleetID));
            _fleetLocation = serializedObject.FindProperty(nameof(GameLiftClientSettings.FleetLocation));
            _profileName = serializedObject.FindProperty(nameof(GameLiftClientSettings.ProfileName));
        }

        private void EditAnywhereMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_computeName, new GUIContent("GLA Compute Name", "Fleet Compute Name"));
            EditorGUILayout.PropertyField(_fleetID, new GUIContent("GLA FleetID", "This Fleet Id should match the value generated in the amazon console"));
            EditorGUILayout.PropertyField(_fleetLocation, new GUIContent("GLA Fleet Location", "This Location should match the value defined in your amazon console"));
            EditorGUILayout.PropertyField(_profileName, new GUIContent("GLA Profile Name", "This Name should match the value defined by your amazon account"));

            if (string.IsNullOrWhiteSpace(targetSettings.ComputeName))
            {
                EditorGUILayout.HelpBox("Please set the Fleet Compute Name.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.FleetID))
            {
                EditorGUILayout.HelpBox("Please set the FleetID.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.FleetLocation))
            {
                EditorGUILayout.HelpBox("Please set the Fleet Location.", MessageType.Warning);
            }
        }

        private void EditGameLiftMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_remoteUrl, new GUIContent("API Gateway Endpoint", "API Gateway URL"));
            EditorGUILayout.PropertyField(_poolClientId, new GUIContent("Cognito Client ID"));

            if (string.IsNullOrWhiteSpace(targetSettings.ApiGatewayUrl))
            {
                EditorGUILayout.HelpBox("Please set the API Gateway URL.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.UserPoolClientId))
            {
                EditorGUILayout.HelpBox("Please set the User Pool Client ID.", MessageType.Warning);
            }
        }
        
        
    }
}
