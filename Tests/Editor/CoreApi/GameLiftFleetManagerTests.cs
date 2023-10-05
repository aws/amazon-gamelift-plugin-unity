// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

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
    public class GameLiftFleetManagerTests
    {
        private Mock<IAmazonGameLiftWrapper> _gameLiftWrapperMock;
        private Mock<IAwsCredentialsFactory> _awsCredentialsFactoryMock;
        private Mock<CoreApi> _coreApiMock;
        private Mock<IAmazonGameLiftWrapperFactory> _amazonGameLiftClientFactoryMock;
        private AwsCredentialsTestProvider _awsCredentialsTestProvider;
        
        [SetUp]
        public void Setup()
        {
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftWrapper>();
            _awsCredentialsFactoryMock = new Mock<IAwsCredentialsFactory>(); 
            _coreApiMock  = new Mock<CoreApi>();
            _amazonGameLiftClientFactoryMock = new Mock<IAmazonGameLiftWrapperFactory>();
            _awsCredentialsTestProvider = new AwsCredentialsTestProvider();
        }

        private GameLiftFleetManager ArrangeAnywhereFleetHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel
            {
                LocationName = "custom-location-1"
            });

            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), null)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), string.Empty)).Returns(Response.Fail(new PutSettingResponse()));
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
                .Returns(_awsCredentialsTestProvider.GetAwsCredentialsWithStubComponents(_coreApiMock.Object));

            return new GameLiftFleetManager(_gameLiftWrapperMock.Object);
        }

        [Test]
        public void CreateAnywhereFleet_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsTrue(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullWrapper_DoesNotCallCreate()
        {
            //Arrange
            ArrangeAnywhereFleetHappyPath();

            var gameLiftFleetManager = new GameLiftFleetManager(null);

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test").GetAwaiter().GetResult();

            //Assert
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullFleetName_FleetNotCreated()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet(null).GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullFleetId_FleetNotCreated()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(
                Task.FromResult(
                    new CreateFleetResponse()
                    {
                        FleetAttributes = new FleetAttributes() { FleetId = null },
                        LocationStates = new List<LocationState>()
                    }));

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(2));
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateCustomLocationIfNotExists_WhenThrowErrorOnListLocation_DoesNotCallCreate()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>()))
                .Throws(new NullReferenceException());

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Never);
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }
    }
}