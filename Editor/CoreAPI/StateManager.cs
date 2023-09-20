using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.CoreAPI
{
    public class StateManager
    {
        public CoreApi CoreApi { get; }

        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftComputeManager ComputeManager { get; set; }

        public IAmazonGameLiftClientWrapper GameLiftWrapper { get; private set; }

        public IAmazonGameLiftClientFactory AmazonGameLiftClientFactory { get; }

        private string _selectedProfile;

        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProfile(value);
        }

        public IReadOnlyList<string> AllProfiles => CoreApi.ListCredentialsProfiles().Profiles.ToList();
        
        public bool IsBootstrapped { get; set; }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftClientFactory = new AmazonGameLiftClientFactory(coreApi);

            SetProfile(coreApi.GetSetting(SettingsKeys.CurrentProfileName).Value);
            IsBootstrapped = coreApi.GetSetting(SettingsKeys.IsBootstrapped).Value == "true";
        }

        private void SetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;
            _selectedProfile = profileName;
            GameLiftWrapper = AmazonGameLiftClientFactory.Get(SelectedProfile);
            FleetManager = new GameLiftFleetManager(CoreApi, GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(CoreApi, GameLiftWrapper);
        }
    }
}