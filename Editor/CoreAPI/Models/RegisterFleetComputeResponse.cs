using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class RegisterFleetComputeResponse : Response
    {
        public string ComputeName { get; set; }
        public string IpAddress { get; set; }
        public string WebSocketUrl { get; set; }
    }
}