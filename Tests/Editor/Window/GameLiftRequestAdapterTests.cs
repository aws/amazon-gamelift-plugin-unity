using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using Moq;
using NUnit.Framework;
using Tests.Utils.Providers;
using AmazonGameLiftPlugin.Core.Shared;

namespace Editor.Window.UnitTests
{
    [TestFixture]
    public class GameLiftRequestAdapterTests
    {
        private Mock<IAmazonGameLiftClientWrapper> _gameLiftWrapperMock;
        private Mock<IAwsCredentialsFactory> _awsCredentialsFactoryMock;
        private Mock<CoreApi> _coreApiMock;
        private Mock<IAmazonGameLiftClientFactory> _amazonGameLiftClientFactoryMock;
        private AwsCredentialsProvider _awsCredentialsProvider;
        private readonly IAmazonGameLiftClientFactory _amazonGameLiftClientFactory;
        
        [SetUp]
        public void Setup()
        {
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();
            _awsCredentialsFactoryMock = new Mock<IAwsCredentialsFactory>(); 
            _coreApiMock  = new Mock<CoreApi>();
            _amazonGameLiftClientFactoryMock = new Mock<IAmazonGameLiftClientFactory>();
            _awsCredentialsProvider = new AwsCredentialsProvider();
        }

        private GameLiftRequestAdapter ArrangeAnywhereFleetHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel
            {
                LocationName = "custom-location-1"
            });

            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<string>(), null)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<string>(), string.Empty)).Returns(Response.Fail(new PutSettingResponse()));
            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>())).Returns(Task.FromResult(new ListLocationsResponse {Locations = listLocationModel}));
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(Task.FromResult(
                new CreateFleetResponse
                {
                    FleetAttributes = new FleetAttributes { FleetId = "test" }, 
                    LocationStates = new List<LocationState>()
                }));

            _amazonGameLiftClientFactoryMock.Setup(f => f.Get(It.IsAny<string>()))
                .Returns(_gameLiftWrapperMock.Object);

            _awsCredentialsFactoryMock.Setup(f => f.Create())
                .Returns(_awsCredentialsProvider.GetAwsCredentialsWithStubComponents(_coreApiMock.Object));

            return new GameLiftRequestAdapter(_coreApiMock.Object, _gameLiftWrapperMock.Object);
        }
        
        [Test]
        public void CreateAnywhereFleet_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsTrue(createFleetResult.Success);
        }
        
        [Test]
        public void CreateAnywhereFleet_WhenNullWrapper_DoesNotCallCreate()
        {
            //Arrange
            ArrangeAnywhereFleetHappyPath();

            var gameLiftRequestAdapter = new GameLiftRequestAdapter(_coreApiMock.Object, null);

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            Assert.IsFalse(createFleetResult.Success);
        }
        
        [Test]
        public void CreateAnywhereFleet_WhenNullFleetName_FleetNotCreated()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet(null).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsFalse(createFleetResult.Success);
        }
        
        [Test]
        public void CreateAnywhereFleet_WhenNullFleetId_FleetNotCreated()
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
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test] 
        public void CreateCustomLocationIfNotExists_WhenThrowErrorOnListLocation_DoesNotCallCreate()
        {
            //Arrange
            var gameLiftRequestAdapter = ArrangeAnywhereFleetHappyPath();
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>())).Throws(new NullReferenceException());

            //Act
            var createFleetResult =  gameLiftRequestAdapter.CreateAnywhereFleet("test").GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Never);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }
    }
}