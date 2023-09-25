using System;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.CoreAPI
{
    public class StateManager
    {
        public CoreApi CoreApi { get; }

        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftComputeManager ComputeManager { get; set; }

        public IAmazonGameLiftWrapper GameLiftWrapper { get; private set; }

        public IAmazonGameLiftWrapperFactory AmazonGameLiftWrapperFactory { get; }

        private string _selectedProfile;

        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProfile(value);
        }
        
        public Action OnProfileSelected { get; set; }
        
        public bool IsBootstrapped { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string SelectedFleetName { get; set; }
        public string FleetLocation { get; set; }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftWrapperFactory = new AmazonGameLiftWrapperFactory(coreApi);

            SetProfile(coreApi.GetSetting(SettingsKeys.CurrentProfileName).Value);
        }

        private void SetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;
            _selectedProfile = profileName;
            var credentials = CoreApi.RetrieveAwsCredentials(profileName);
            Region = credentials.Region;
            GameLiftWrapper = AmazonGameLiftClientFactory.Get(SelectedProfile);
            FleetManager = new GameLiftFleetManager(CoreApi, GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(CoreApi, GameLiftWrapper);
            OnProfileSelected?.Invoke();
        }
    }
}