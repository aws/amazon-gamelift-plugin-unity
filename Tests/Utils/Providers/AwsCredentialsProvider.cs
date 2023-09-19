using System;
using AmazonGameLift.Editor;
using Moq;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class AwsCredentialsProvider
    {
        
        private readonly TextProvider _textProvider = TextProviderFactory.Create();
        
        
        public AwsCredentials GetAwsCredentialsWithStubComponents(CoreApi coreApi)
        {
            var regionMock = new Mock<RegionBootstrap>(coreApi);
            regionMock.Setup(target => target.Refresh())
                .Verifiable();

            var mockLogger = new MockLogger();
            var creationMock = new Mock<AwsCredentialsCreation>(_textProvider, regionMock.Object, coreApi, mockLogger);
            creationMock.Setup(target => target.Refresh())
                .Verifiable();

            var updateMock = new Mock<AwsCredentialsUpdate>(_textProvider, regionMock.Object, coreApi, mockLogger);
            updateMock.Setup(target => target.Refresh())
                .Verifiable();

            return new AwsCredentials(creationMock.Object, updateMock.Object, coreApi);
        }
    }
}