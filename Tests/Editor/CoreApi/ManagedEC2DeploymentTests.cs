using System;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.CoreAPI;
using Moq;
using NUnit.Framework;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class ManagedEC2DeploymentTests
    {
        private AwsCredentialsTestProvider _awsCredentialsTestProvider;
        private Mock<ManagedEC2FleetParameters> _managedEc2FleetParametersMock;
        private DeploymentSettings _deploymentSettings;
        private Mock<ConfirmChangesRequest> _confirmChangesRequestMock;
        private static Mock<CoreApi> _coreApiMock;

        private const string GameName = "TestGame";
        private const string FleetName = "TestFleet";
        private const string BuildName = "TestBuild";
        private const string GameServerFile = "TestFile";
        private const string GameServerFolder = "TestFolder";
        private static readonly OperatingSystem OperatingSystemName = OperatingSystem.WINDOWS_2012;
        
        [SetUp]
        public void Setup()
        {
            _awsCredentialsTestProvider = new AwsCredentialsTestProvider();
            _managedEc2FleetParametersMock = new Mock<ManagedEC2FleetParameters>();
            _coreApiMock = new Mock<CoreApi>();
        }

        private ManagedEC2Deployment ArrangeEc2DeploymentHappyPath()
        {
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSettingOrClear(It.IsAny<SettingsKeys>(), It.IsAny<string>())).Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), null)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), string.Empty)).Returns(Response.Fail(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.GetSetting(It.IsAny<SettingsKeys>())).Returns(Response.Ok(new GetSettingResponse()));

            _managedEc2FleetParametersMock.Object.GameName = GameName;
            _managedEc2FleetParametersMock.Object.FleetName = FleetName;
            _managedEc2FleetParametersMock.Object.BuildName = BuildName;
            _managedEc2FleetParametersMock.Object.GameServerFile = GameServerFile;
            _managedEc2FleetParametersMock.Object.GameServerFolder = GameServerFolder;
            _managedEc2FleetParametersMock.Object.OperatingSystem = OperatingSystemName;
            
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, _coreApiMock);

            _deploymentSettings = GetUnitUnderTest(scenarioLocator: scenarioLocatorMock);
            
            _deploymentSettings.Refresh();
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
            
            Assert.AreEqual(_deploymentSettings.GameName, GameName);
        }
        
        [Test]
        public void StartDeployment_WhenTaskIsFaulted_ExpectException()
        {
            //Arrange
            var ec2DeploymentHappyPath = ArrangeEc2DeploymentHappyPath();
            
            //Act & Assert
            Assert.Throws<AggregateException>(() => _deploymentSettings.StartDeployment(null).Wait());
        }
        
        [Test]
        public void StartDeployment_WhenTaskIsFaultedWithContinueWith_ExpectFalse()
        {
            //Arrange
            var ec2DeploymentHappyPath = ArrangeEc2DeploymentHappyPath();
            var isErrored = false;
            
            //Act
            _deploymentSettings.StartDeployment(null).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    isErrored = true;
                }
            });
                
            //Assert
            Assert.IsFalse(isErrored);
        }
        
        [Test]
        public void DeleteDeployment_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var ec2DeploymentHappyPath = ArrangeEc2DeploymentHappyPath();
            _coreApiMock.Setup(target => target.DeleteStack(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Response.Ok(new DeleteStackResponse()));
                
            //Act
            ec2DeploymentHappyPath.DeleteDeployment().Wait();
            
            //Assert
            _coreApiMock.Verify(f => f.DeleteStack(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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
        
        private static Mock<ScenarioLocator> SetUpScenarioLocatorToReturnTestDeployer(
            string name, string description, string url, bool hasServer,
            Mock<CoreApi> coreApiMock, Mock<DeployerBase> deployerMock = null,
            Mock<ScenarioLocator> scenarioLocatorMock = null, string scenarioFolder = null)
        {
            Mock<Delay> delayMock = SetUpDelayMock();

            deployerMock = deployerMock ?? new Mock<DeployerBase>(delayMock.Object, coreApiMock.Object);

            deployerMock.SetupGet(target => target.DisplayName)
                .Returns(name);

            deployerMock.SetupGet(target => target.Description)
                .Returns(description);

            deployerMock.SetupGet(target => target.ScenarioFolder)
                .Returns(scenarioFolder);

            deployerMock.SetupGet(target => target.HelpUrl)
                .Returns(url);

            deployerMock.SetupGet(target => target.HasGameServer)
                .Returns(hasServer);

            scenarioLocatorMock = scenarioLocatorMock ?? new Mock<ScenarioLocator>();

            var scenarios = new DeployerBase[] { deployerMock.Object, deployerMock.Object };
            scenarioLocatorMock.Setup(target => target.GetScenarios())
                .Returns(scenarios)
                .Verifiable();

            return scenarioLocatorMock;
        }
    }
}