using Amazon;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace Editor.CoreAPI
{
    
    public interface IAmazonGameLiftWrapperFactory
    {
        IAmazonGameLiftWrapper Get(string profile);
    }
    
    public class AmazonGameLiftWrapperFactory : IAmazonGameLiftWrapperFactory
    {
        private readonly CoreApi _coreApi;

        public AmazonGameLiftWrapperFactory(CoreApi coreApi)
        {
            _coreApi = coreApi;
        }

        public IAmazonGameLiftWrapper Get(string profile) 
        {
            var credentials = _coreApi.RetrieveAwsCredentials(profile);
            var client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey, RegionEndpoint.GetBySystemName(credentials.Region));
            return new AmazonGameLiftWrapper(client);
        }
    }
}