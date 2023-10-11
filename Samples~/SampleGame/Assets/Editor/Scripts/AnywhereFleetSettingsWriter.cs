// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using AmazonGameLift.Editor;
using AmazonGameLift.Runtime;
using Editor.CoreAPI;
using UnityEditor;

namespace Editor.Scripts
{
    public static class AnywhereFleetSettingsWriter
    {
        public static void WriteServerSettings(string directory = "")
        {
#if UNITY_SERVER
            var profile = GetProfile();
            var path = Path.Join(directory, GameLiftServer.configFilePath);
            var serverSettings = new Settings<SettingsKeys>(path);
            serverSettings.PutSetting(SettingsKeys.CurrentRegion, profile.Region);
            serverSettings.PutSetting(SettingsKeys.FleetId, profile.AnywhereFleetId);
            serverSettings.PutSetting(SettingsKeys.ComputeName, profile.ComputeName);
            serverSettings.PutSetting(SettingsKeys.WebSocketUrl, profile.WebSocketUrl);
#endif
        }

        public static void WriteClientSettings(string directory = "")
        {
#if !UNITY_SERVER
            var profile = GetProfile();
            var path = Path.Join(directory, GameLiftCoreApi.ConfigFilePath);
            var clientSettings = new Settings<ClientSettingsKeys>(path);
            clientSettings.PutSetting(ClientSettingsKeys.CurrentRegion, profile.Region);
            clientSettings.PutSetting(ClientSettingsKeys.FleetId, profile.AnywhereFleetId);
            clientSettings.PutSetting(ClientSettingsKeys.FleetLocation, profile.AnywhereFleetLocation);
            clientSettings.PutSetting(ClientSettingsKeys.CurrentProfileName, profile.Region);
#endif
        }

        private static UserProfile GetProfile()
        {
            var settings =
                AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>("Assets/Settings/GameLiftClientSettings.asset");
            if (settings == null || !settings.IsGameLiftAnywhere)
            {
                return null;
            }

            var stateManager = new StateManager(new CoreApi());
            return stateManager.SelectedProfile;
        }
    }
}
