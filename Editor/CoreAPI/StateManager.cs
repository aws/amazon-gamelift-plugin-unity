using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.CoreAPI
{
    public class StateManager
    {
        public CoreApi CoreApi { get; set; }
        
        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftComputeManager ComputeManager  { get; set; }
        
        public IAmazonGameLiftClientWrapper GameLiftWrapper { get; private set; }
        
        public IAmazonGameLiftClientFactory AmazonGameLiftClientFactory { get; set; }
        
        public string SelectedProfile { get; set; }
        
        public int SelectedFleetIndex { get; set; }
        
        public void SetupClientFactory()
        {
            AmazonGameLiftClientFactory = new AmazonGameLiftClientFactory(CoreApi);
        }
        
        public void SetupWrapper()
        {
            GameLiftWrapper = AmazonGameLiftClientFactory.Get(SelectedProfile);
            FleetManager = new GameLiftFleetManager(CoreApi, GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(CoreApi, GameLiftWrapper);
        }
    }
}