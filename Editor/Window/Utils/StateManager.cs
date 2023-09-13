using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.Window.Utils
{
    public class StateManager
    {
        private State _state;
        public CoreApi CoreApi { get; set; }
        public GameLiftRequestAdapter RequestAdapter { get; set; }
        public State State { get; set; }
        public IAmazonGameLiftClientWrapper GameLiftWrapper { get; private set; }
        
        public IAmazonGameLiftClientFactory AmazonGameLiftClientFactory { get; set; }
        
        public void SetupClientFactory()
        {
            AmazonGameLiftClientFactory = new AmazonGameLiftClientFactory(CoreApi);
        }
        
        public void SetupWrapper()
        {
            GameLiftWrapper = AmazonGameLiftClientFactory.Get(_state.SelectedProfile);
        }

        public void SetupRequestAdapter()
        {
            RequestAdapter = new GameLiftRequestAdapter(CoreApi, (AmazonGameLiftWrapper)GameLiftWrapper);
        }
    }
    
    public struct State
    {
        public string SelectedProfile;
        public int SelectedFleetIndex;
    }
}