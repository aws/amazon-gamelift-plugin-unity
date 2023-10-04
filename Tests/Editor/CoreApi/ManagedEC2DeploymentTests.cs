using System;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.CoreAPI;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class ManagedEC2DeploymentTests
    {
        private AwsCredentialsTestProvider _awsCredentialsTestProvider;
        private Mock<ManagedEC2FleetParameters> _managedEc2FleetParametersMock;
        private DeploymentSettings _deploymentSettings;
        private Mock<ConfirmChangesRequest> _confirmChangesRequestMock;
        private static Mock<CoreApi> _coreApiMock;
        
        [SetUp]
        public void Setup()
        {
            _awsCredentialsTestProvider = new AwsCredentialsTestProvider();
            _managedEc2FleetParametersMock = new Mock<ManagedEC2FleetParameters>();
            //_confirmChangesRequestMock = new Mock<ConfirmChangesRequest>();
            _coreApiMock = new Mock<CoreApi>();
        }

        private ManagedEC2Deployment ArrangeEc2DeploymentHappyPath()
        {
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSettingOrClear(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), null)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), string.Empty)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.GetSetting(It.IsAny<SettingsKeys>())).Returns(Response.Ok(new GetSettingResponse()));
            
            _deploymentSettings = GetUnitUnderTest();
            return new ManagedEC2Deployment(_deploymentSettings, _managedEc2FleetParametersMock.Object);
        }
        
        [Test]
        public void StartDeployment_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var ec2DeploymentHappyPath = ArrangeEc2DeploymentHappyPath();
            
            //Act
            ec2DeploymentHappyPath.StartDeployment();
            
            //Assert
            _coreApiMock.Verify(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()), Times.Exactly(8));
            
            Assert.IsTrue(_deploymentSettings.GameName == Application.productName.Substring(0,12) );
        }
        
        [Test]
        public void StartDeployment_WhenTaskIsFaulted_ExpectException()
        {
            //Arrange
            var ec2DeploymentHappyPath = ArrangeEc2DeploymentHappyPath();
            
            //Act
            var startDeployment = _deploymentSettings.StartDeployment(MockEc2Delegate);
            
            //Assert
            Assert.IsFalse(_deploymentSettings.GameName == Application.productName.Substring(0,12) );
        }
        
        private static DeploymentSettings GetUnitUnderTest(Mock<ScenarioLocator> scenarioLocator = null,
            Mock<PathConverter> pathConverter = null, Mock<CoreApi> coreApi = null,
            Mock<ScenarioParametersUpdater> parametersUpdater = null, Mock<DeploymentWaiter> deploymentWaiter = null,
            Mock<IDeploymentIdContainer> deploymentIdContainer = null, Mock<Delay> delay = null, Mock<StateManager> stateManager = null)
        {
            coreApi = _coreApiMock ?? new Mock<CoreApi>();
            scenarioLocator = scenarioLocator ?? new Mock<ScenarioLocator>();
            Mock<Delay> delayMock = delay ?? SetUpDelayMock();
            deploymentWaiter = deploymentWaiter ?? new Mock<DeploymentWaiter>(delayMock.Object, coreApi.Object);
            pathConverter = pathConverter ?? GetMockPathConverter(coreApi);
            parametersUpdater = parametersUpdater ?? GetMockScenarioParametersUpdater(coreApi);
            deploymentIdContainer = deploymentIdContainer ?? new Mock<IDeploymentIdContainer>();
            stateManager = stateManager ?? new Mock<StateManager>(coreApi.Object);

            return new DeploymentSettings(scenarioLocator.Object, pathConverter.Object, coreApi.Object,
                parametersUpdater.Object, TextProviderFactory.Create(), deploymentWaiter.Object,
                deploymentIdContainer.Object, delayMock.Object, new MockLogger(), stateManager.Object);
        }
        
        private static Mock<Delay> SetUpDelayMock()
        {
            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return delayMock;
        }
        
        private static Mock<PathConverter> GetMockPathConverter(Mock<CoreApi> coreApi = null)
        {
            coreApi = coreApi ?? new Mock<CoreApi>();
            return new Mock<PathConverter>(coreApi.Object);
        }

        private static Mock<ScenarioParametersUpdater> GetMockScenarioParametersUpdater(Mock<CoreApi> coreApi = null)
        {
            coreApi = coreApi ?? new Mock<CoreApi>();
            var scenarioParametersEditor = new Mock<ScenarioParametersEditor>();
            var factory = (Func<ScenarioParametersEditor>)(() => scenarioParametersEditor.Object);
            return new Mock<ScenarioParametersUpdater>(coreApi.Object, factory);
        }

        private static Task<bool> MockEc2Delegate(ConfirmChangesRequest request)
        {
            return null;
        }
    }
}