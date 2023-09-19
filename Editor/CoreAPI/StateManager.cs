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
            private set => SetupWrapper(value);
        }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftClientFactory = new AmazonGameLiftClientFactory(coreApi);
        }

        private void SetupWrapper(string profileName)
        {
            _selectedProfile = profileName;
            GameLiftWrapper = AmazonGameLiftClientFactory.Get(SelectedProfile);
            FleetManager = new GameLiftFleetManager(CoreApi, GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(CoreApi, GameLiftWrapper);
        }
    }
}