using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using Moq;
using NUnit.Framework;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.CoreAPI;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    [TestFixture]
    public class GameLiftComputeManagerTests
    {
        private Mock<IAmazonGameLiftWrapper> _gameLiftWrapperMock;
        private Mock<IAwsCredentialsFactory> _awsCredentialsFactoryMock;
        private Mock<CoreApi> _coreApiMock;
        private Mock<IAmazonGameLiftWrapperFactory> _amazonGameLiftClientFactoryMock;
        private AwsCredentialsTestProvider _awsCredentialsTestProvider;

        private string _fleetId = "fleetId-12345-12345-12345-12345";
        private string _computeName = "TestComputeName";
        private string _location = "TestLocation";
        private string _ipAddress = "120.120.120.120";
        private string _endpoint = "wss://test.com";
        
        [SetUp]
        public void Setup()
        {
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftWrapper>();
            _awsCredentialsFactoryMock = new Mock<IAwsCredentialsFactory>(); 
            _coreApiMock  = new Mock<CoreApi>();
            _amazonGameLiftClientFactoryMock = new Mock<IAmazonGameLiftWrapperFactory>();
            _awsCredentialsTestProvider = new AwsCredentialsTestProvider();
        }

        private GameLiftComputeManager ArrangeAnywhereFleetHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel
            {
                LocationName = "custom-location-1"
            });

            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), null)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), string.Empty)).Returns(Response.Fail(new PutSettingResponse()));
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>())).Returns(Task.FromResult(
                new RegisterComputeResponse()
                {
                    Compute = new Compute()
                    {
                        FleetId = _fleetId,
                        ComputeName = _computeName,
                        Location = _location,
                        IpAddress = _ipAddress,
                        GameLiftServiceSdkEndpoint = _endpoint
                    }
                }));

            _amazonGameLiftClientFactoryMock.Setup(f => f.Get(It.IsAny<string>()))
                .Returns(_gameLiftWrapperMock.Object);

            _awsCredentialsFactoryMock.Setup(f => f.Create())
                .Returns(_awsCredentialsTestProvider.GetAwsCredentialsWithStubComponents(_coreApiMock.Object));

            return new GameLiftComputeManager(_gameLiftWrapperMock.Object);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftComputeManager = ArrangeAnywhereFleetHappyPath();
            
            //Act
            var registerComputeResponse =  gameLiftComputeManager.RegisterFleetCompute(_computeName, _fleetId, _location, _ipAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(3));
            
            Assert.IsTrue(registerComputeResponse.Success);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenNullWrapper_DoesNotCallRegister()
        {
            //Arrange
            ArrangeAnywhereFleetHappyPath();
        
            var gameLiftFleetManager = new GameLiftComputeManager(null);
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(_computeName, _fleetId, _location, _ipAddress).GetAwaiter().GetResult();
            
            //Assert
            Assert.IsFalse(createFleetResult.Success);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenNullComputeName_ComputeNotRegistered()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(null, _fleetId, _location, _ipAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(3));
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void RegisterFleetCompute_WhenNullIpAddress_ComputeNotRegistered()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(_computeName, _fleetId, _location, null).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(3));
            
            Assert.IsFalse(createFleetResult.Success);
        }
        
        
        [Test] 
        public void RegisterFleetCompute_WhenThrowErrorOnListLocation_DoesNotRegister()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>())).Throws(new NullReferenceException());
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(_computeName, _fleetId, _location, _ipAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }
    }
}