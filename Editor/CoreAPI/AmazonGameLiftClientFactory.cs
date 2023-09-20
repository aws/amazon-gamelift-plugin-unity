using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.CoreAPI
{
    
    public interface IAmazonGameLiftClientFactory
    {
        IAmazonGameLiftClientWrapper Get(string profile);
    }
    
    public class AmazonGameLiftClientFactory : IAmazonGameLiftClientFactory
    {
        private readonly CoreApi _coreApi;

        public AmazonGameLiftClientFactory(CoreApi coreApi)
        {
            _coreApi = coreApi;
        }

        public IAmazonGameLiftClientWrapper Get(string profile)
        {
            var credentials = _coreApi.RetrieveAwsCredentials(profile);
            var client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
            return new AmazonGameLiftWrapper(client);
        }
    }
}