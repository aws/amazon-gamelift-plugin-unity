using System;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;

namespace Editor.CoreAPI
{
    public class StateManager
    {
        private CoreApi CoreApi { get; }

        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftComputeManager ComputeManager { get; set; }

        public IAmazonGameLiftWrapper GameLiftWrapper { get; private set; }

        public IAmazonGameLiftWrapperFactory AmazonGameLiftWrapperFactory { get; }

        public string SelectedProfile
        {
            get => GetSetting(SettingsKeys.CurrentProfileName);
            set => SetProfile(value);
        }

        public string BucketName
        {
            get => GetSetting(SettingsKeys.CurrentProfileName);
            set => SetProfile(value);
        }

        public string Region
        {
            get => GetSetting(SettingsKeys.CurrentProfileName);
            set => SetProfile(value);
        }

        public string SelectedFleetName
        {
            get => GetSetting(SettingsKeys.FleetName);
            set => PutSetting(SettingsKeys.FleetName, value);
        }

        public string FleetId
        {
            get => GetSetting(SettingsKeys.FleetId);
            set => PutSetting(SettingsKeys.FleetId, value);
        }

        public string FleetLocation
        {
            get => GetSetting(SettingsKeys.FleetLocation);
            set => PutSetting(SettingsKeys.FleetLocation, value);
        }

        public string ComputeName
        {
            get => GetSetting(SettingsKeys.ComputeName);
            set => PutSetting(SettingsKeys.ComputeName, value);
        }

        public string IpAddress
        {
            get => GetSetting(SettingsKeys.IpAddress);
            set => PutSetting(SettingsKeys.IpAddress, value);
        }

        public string WebSocketUrl
        {
            get => GetSetting(SettingsKeys.WebSocketUrl);
            set => PutSetting(SettingsKeys.WebSocketUrl, value);
        }

        public bool IsBootstrapped => !string.IsNullOrWhiteSpace(SelectedProfile) &&
                                      !string.IsNullOrWhiteSpace(Region) & !string.IsNullOrWhiteSpace(BucketName);

        public Action OnProfileSelected { get; set; }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftWrapperFactory = new AmazonGameLiftWrapperFactory(coreApi);

            SetProfile(coreApi.GetSetting(SettingsKeys.CurrentProfileName).Value);
        }

        private void SetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName) || profileName == SelectedProfile) return;
            SelectedProfile = profileName;
            var credentials = CoreApi.RetrieveAwsCredentials(profileName);
            Region = credentials.Region;
            GameLiftWrapper = AmazonGameLiftWrapperFactory.Get(SelectedProfile);
            FleetManager = new GameLiftFleetManager(CoreApi, GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(CoreApi, GameLiftWrapper);
            OnProfileSelected?.Invoke();
        }

        private string GetSetting(string key) => CoreApi.GetSetting(key).Value;
        private void PutSetting(string key, string value) => CoreApi.PutSetting(key, value);
    }
}