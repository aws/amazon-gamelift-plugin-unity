using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using Moq;
using NUnit.Framework;

namespace Editor.Window.UnitTests
{
    [TestFixture]
    public class AnywherePageTests
    {

        private Mock<GameLiftPlugin> _gameLiftConfigMock;
        private Mock<IAmazonGameLiftClientWrapper> _gameLiftWrapperMock;
        private Mock<IAwsCredentialsFactory> _awsCredentialsFactory;
        private Mock<CoreApi> _coreApi;
        private Mock<IAmazonGameLiftClientFactory> _amazonGameLiftClientFactory;
        
        [SetUp]
        public void Setup()
        {
            _gameLiftConfigMock = new Mock<GameLiftPlugin>();
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();
            _awsCredentialsFactory = new Mock<IAwsCredentialsFactory>(); 
            _coreApi  = new Mock<CoreApi>();
            _amazonGameLiftClientFactory = new Mock<IAmazonGameLiftClientFactory>();
        }
        
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        private AwsCredentials GetAwsCredentialsWithStubComponents(CoreApi coreApi)
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

        private GameLiftRequestAdapter ArrangeAnywhereFleetHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel()
            {
                LocationName = "custom-location-1"
            });

            _coreApi.Setup(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>())).Returns( AmazonGameLiftPlugin.Core.Shared.Response.Ok<PutSettingResponse>(new PutSettingResponse()));
            _coreApi.Setup(f => f.PutSetting(It.IsAny<string>(), null)).Returns( AmazonGameLiftPlugin.Core.Shared.Response.Fail<PutSettingResponse>(new PutSettingResponse()));
            _coreApi.Setup(f => f.PutSetting(It.IsAny<string>(), string.Empty)).Returns( AmazonGameLiftPlugin.Core.Shared.Response.Fail<PutSettingResponse>(new PutSettingResponse()));
            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>())).Returns(Task.FromResult(new ListLocationsResponse(){Locations = listLocationModel}));
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(Task.FromResult(
                new CreateFleetResponse()
                {
                    FleetAttributes = new FleetAttributes() { FleetId = "test" }, 
                    LocationStates = new List<LocationState>()
                }));

            _amazonGameLiftClientFactory.Setup(f => f.Get(It.IsAny<string>()))
                .Returns(_gameLiftWrapperMock.Object);

            _awsCredentialsFactory.Setup(f => f.Create())
                .Returns(GetAwsCredentialsWithStubComponents(_coreApi.Object));
            
            var gameLiftPlugin = new GameLiftPlugin(_awsCredentialsFactory.Object, _coreApi.Object, _amazonGameLiftClientFactory.Object);
            gameLiftPlugin.CurrentState = new GameLiftPlugin.State() { SelectedProfile = "unitTest" };
            gameLiftPlugin.SetupWrapper();

            return new GameLiftRequestAdapter(gameLiftPlugin);
        }
        
        [Test]
        public void CreateAnywhereFleet_CorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApi.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsTrue(createFleetResult);
        }
        
        [Test]
        public void CreateAnywhereFleet_NullWrapper_ExpectFalse()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            var gameLiftPlugin = new GameLiftPlugin(_awsCredentialsFactory.Object, _coreApi.Object, _amazonGameLiftClientFactory.Object);
            gameLiftPlugin.CurrentState = new GameLiftPlugin.State() { SelectedProfile = "unitTest" };
            gameLiftRequestAdapter = new GameLiftRequestAdapter(gameLiftPlugin);

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            Assert.IsFalse(createFleetResult);
        }
        
        [Test]
        public void CreateAnywhereFleet_NullFleetName_ExpectFalse()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet(null).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApi.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.False(createFleetResult);
        }
        
        [Test]
        public void CreateAnywhereFleet_NullFleetId_ExpectFalse()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(Task.FromResult(
                new CreateFleetResponse()
                {
                    FleetAttributes = new FleetAttributes() { FleetId = null }, 
                    LocationStates = new List<LocationState>()
                }));

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApi.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.False(createFleetResult);
        }

        [Test] 
        public void CreateCustomLocationIfNotExists_ThrowErrorOnListLocation_ExpectFalse()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>())).Throws(new NullReferenceException());

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Never);
            _coreApi.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            
            Assert.False(createFleetResult);
        }
    }
}