// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class DeploymentSettingsTests
    {
        private static readonly bool[] s_boolValues = new bool[] { true, false };

        #region Form persistence

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ScenarioIndex_WhenFormFilledAndRestore_IsExpected(bool coreSuccess)
        {
            int testIndex = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            int testIndex1 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentScenarioIndex, coreSuccess, SettingsFormatter.FormatInt(testIndex1));
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentGameName, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFolderPath, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFilePath, false, null);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.ScenarioIndex = testIndex;
            underTest.Restore();

            // Assert
            Assert.AreEqual(coreSuccess ? testIndex1 : 1, underTest.ScenarioIndex);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GameName_WhenFormFilledAndRestore_IsExpected(bool coreSuccess)
        {
            string id = DateTime.UtcNow.Ticks.ToString();
            string testGameName = "test" + id;
            string testGameName1 = "test1" + id;

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentGameName, coreSuccess, testGameName1);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentScenarioIndex, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFolderPath, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFilePath, false, null);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.GameName = testGameName;
            underTest.Restore();

            // Assert
            Assert.AreEqual(coreSuccess ? testGameName1 : null, underTest.GameName);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void BuildFolderPath_WhenFormFilledAndRestore_IsExpected(bool coreSuccess)
        {
            string id = DateTime.UtcNow.Ticks.ToString();
            string testBuildFolderPath = "test path" + id;
            string testBuildFolderPath1 = "test path1" + id;

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFolderPath, coreSuccess, testBuildFolderPath1);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentGameName, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentScenarioIndex, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFilePath, false, null);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.BuildFolderPath = testBuildFolderPath;
            underTest.Restore();

            // Assert
            Assert.AreEqual(coreSuccess ? testBuildFolderPath1 : null, underTest.BuildFolderPath);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void BuildFilePath_WhenFormFilledAndRestore_IsExpected(bool coreSuccess)
        {
            string id = DateTime.UtcNow.Ticks.ToString();
            string testBuildExePath = "test path" + id;
            string testBuildExePath1 = "test path1" + id;

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFilePath, coreSuccess, testBuildExePath1);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentGameName, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentScenarioIndex, false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.DeploymentBuildFolderPath, false, null);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.BuildFolderPath = testBuildExePath;
            underTest.Restore();

            // Assert
            Assert.AreEqual(coreSuccess ? testBuildExePath1 : null, underTest.BuildFilePath);
        }

        #endregion

        #region IsBootstrapped

        [Test]
        public void IsBootstrapped_WhenNewInstance_IsFalse()
        {
            DeploymentSettings underTest = GetUnitUnderTest();

            Assert.IsFalse(underTest.IsBootstrapped);
        }

        [Test]
        [TestCase(false, false, false, false)]
        [TestCase(false, false, false, true)]
        [TestCase(false, false, true, false)]
        [TestCase(false, false, true, true)]
        [TestCase(false, true, false, false)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, true, true)]
        [TestCase(true, false, false, false)]
        [TestCase(true, false, false, true)]
        [TestCase(true, false, true, true)]
        [TestCase(true, true, true, true)]
        public void IsBootstrapped_WhenRefreshAndSettingsSetParam_IsExpected(bool profileSet, bool bucketSet, bool regionSet, bool regionValid)
        {
            bool expected = profileSet && bucketSet && regionSet && regionValid;

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: profileSet);
            coreApiMock.SetUpCoreApiWithBucket(success: bucketSet);
            coreApiMock.SetUpCoreApiWithRegion(success: regionSet, regionValid);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);
            underTest.GameName = null;
            underTest.Refresh();

            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.IsBootstrapped);
        }

        #endregion

        #region Form validation

        [Test]
        public void IsFormFilled_WhenNewInstance_IsFalse()
        {
            DeploymentSettings underTest = GetUnitUnderTest();

            Assert.IsFalse(underTest.IsFormFilled);
        }

        [Test]
        public void IsFormFilled_WhenFieldsFilledAndNoServerBuild_IsTrue()
        {
            DeploymentSettings underTest = TestWhenFieldsFilledAndNoServerBuild();
            Assert.IsTrue(underTest.IsFormFilled);
        }

        [Test]
        public void IsBuildPathFilled_WhenFieldsFilledAndNoServerBuild_IsTrue()
        {
            DeploymentSettings underTest = TestWhenFieldsFilledAndNoServerBuild();
            Assert.IsTrue(underTest.IsBuildFolderPathFilled);
        }

        private DeploymentSettings TestWhenFieldsFilledAndNoServerBuild()
        {
            const string testGameName = "test";
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);
            coreApiMock.Setup(target => target.FolderExists(It.IsAny<string>()))
                .Returns(false);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;
            underTest.BuildFolderPath = "test path";
            underTest.BuildFilePath = "test path/build.exe";
            underTest.GameName = testGameName;

            // Assert
            scenarioLocatorMock.Verify();
            return underTest;
        }

        [Test]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, true)]
        public void IsFormFilled_WhenFieldsFilledAndServerBuildPathFilledParam_IsExpected(bool buildFolderExists, bool buildExeExists, bool expected)
        {
            DeploymentSettings underTest = TestWhenFieldsFilledAndServerBuild(buildFolderExists, buildExeExists);
            Assert.AreEqual(expected, underTest.IsFormFilled);
        }

        [Test]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        public void IsBuildFolderPathFilled_WhenFieldsFilledAndServerBuildPathFilledParam_IsExpected(bool buildFolderExists, bool buildExeExists, bool expected)
        {
            DeploymentSettings underTest = TestWhenFieldsFilledAndServerBuild(buildFolderExists, buildExeExists);
            Assert.AreEqual(expected, underTest.IsBuildFolderPathFilled);
        }

        [Test]
        [TestCase(false, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, true, true)]
        public void IsBuildFilePathFilled_WhenFieldsFilledAndServerBuildPathFilledParam_IsExpected(bool buildFolderExists, bool buildExeExists, bool expected)
        {
            DeploymentSettings underTest = TestWhenFieldsFilledAndServerBuild(buildFolderExists, buildExeExists);
            Assert.AreEqual(expected, underTest.IsBuildFilePathFilled);
        }

        private DeploymentSettings TestWhenFieldsFilledAndServerBuild(bool buildFolderExists, bool buildExeExists)
        {
            const string testGameName = "test";
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);
            coreApiMock.Setup(target => target.FolderExists(It.IsAny<string>()))
                .Returns(buildFolderExists)
                .Verifiable();

            coreApiMock.Setup(target => target.FileExists(It.IsAny<string>()))
                .Returns(buildExeExists)
                .Verifiable();

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: true, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;
            underTest.BuildFolderPath = "test path";
            underTest.BuildFilePath = "test path/build.exe";
            underTest.GameName = testGameName;

            // Assert
            scenarioLocatorMock.Verify();
            return underTest;
        }

        [Test]
        public void IsValidScenarioIndex_WhenRefreshAndScenarioIndexNegative_IsFalse()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);
            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = -1;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.IsFalse(underTest.IsValidScenarioIndex);
        }

        [Test]
        public void IsValidScenarioIndex_WhenRefreshAndScenarioIndexOutOfRange_IsFalse()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 1;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.IsFalse(underTest.IsValidScenarioIndex);
        }

        [Test]
        public void IsValidScenarioIndex_WhenRefreshAndScenarioIndexInRange_IsTrue()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.IsTrue(underTest.IsValidScenarioIndex);
        }

        #endregion

        [Test]
        public void ScenarioName_WhenRefresh_IsExpected()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.AreEqual(testScenarioName, underTest.ScenarioName);
        }

        [Test]
        public void ScenarioPath_WhenNewInstance_IsNull()
        {
            DeploymentSettings underTest = GetUnitUnderTest();

            Assert.IsNull(underTest.ScenarioPath);
        }

        [Test]
        public void ScenarioPath_WhenRefresh_IsExpected()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";
            string expectedFolderPath = "expected";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            var pathConverterMock = new Mock<PathConverter>(coreApiMock.Object);
            pathConverterMock.Setup(target => target.GetScenarioAbsolutePath(testScenarioName))
                .Returns(expectedFolderPath)
                .Verifiable();

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock, scenarioFolder: testScenarioName);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock, pathConverter: pathConverterMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;

            // Assert
            pathConverterMock.Verify();
            scenarioLocatorMock.Verify();
            Assert.AreEqual(expectedFolderPath, underTest.ScenarioPath);
        }

        [Test]
        public void ScenarioDescription_WhenRefresh_IsExpected()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.AreEqual(testScenarioDescription, underTest.ScenarioDescription);
        }

        [Test]
        public void ScenarioHelpUrl_WhenRefresh_IsExpected()
        {
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock);

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock);

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

            // Act
            underTest.Refresh();
            underTest.ScenarioIndex = 0;

            // Assert
            scenarioLocatorMock.Verify();
            Assert.AreEqual(testScenarioUrl, underTest.ScenarioHelpUrl);
        }

        [UnityTest]
        public IEnumerator ScenarioLocator_WhenRefreshAndScenarioSelected_IsExececuted([ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var scenarioLocatorMock = new Mock<ScenarioLocator>();

                DeploymentSettings underTest = SetUpStartDeployment(
                    deployerMock => SetUpDeployerStartDeployment(deployerMock, success: true),
                    waitSuccess: true, hasGameServer, scenarioLocatorMock: scenarioLocatorMock);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                // Assert
                scenarioLocatorMock.Verify();
            }
        }

        #region IsDeploymentRunning

        [Test]
        public void IsDeploymentRunning_WhenNewInstance_IsFalse()
        {
            DeploymentSettings underTest = GetUnitUnderTest();

            Assert.IsFalse(underTest.IsDeploymentRunning);
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenRefreshAndScenarioSelectedAndStartDeploymentSuccess_IsFalse([ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                DeploymentSettings underTest = SetUpStartDeployment(deployerMock =>
                    SetUpDeployerStartDeployment(deployerMock, success: true), waitSuccess: true, hasGameServer);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                Assert.IsFalse(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenRefreshAndScenarioSelectedAndStartDeploymentFailure_IsFalse(
            [ValueSource(nameof(s_boolValues))] bool waitSuccess,
            [ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                DeploymentSettings underTest = SetUpStartDeployment(deployerMock =>
                    SetUpDeployerStartDeployment(deployerMock, success: false), waitSuccess, hasGameServer);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                Assert.IsFalse(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenRefreshAndScenarioSelectedAndStartDeploymentException_IsFalse([ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testMessage = "TestException";
                await TestWhenRefreshAndScenarioSelectedAndStartDeploymentException(testMessage, hasGameServer, underTest => Assert.IsFalse(underTest.IsDeploymentRunning));
            }
        }

        #endregion

        #region CanCancel

        [Test]
        public void CanCancel_WhenNewInstance_IsFalse()
        {
            DeploymentSettings underTest = GetUnitUnderTest();

            Assert.IsFalse(underTest.CanCancel);
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenRefreshAndScenarioSelectedAndStartDeploymentSuccess_IsFalse(
            [ValueSource(nameof(s_boolValues))] bool waitSuccess,
            [ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                DeploymentSettings underTest = SetUpStartDeployment(deployerMock =>
                    SetUpDeployerStartDeployment(deployerMock, success: true), waitSuccess, hasGameServer);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                Assert.IsFalse(underTest.CanCancel);
            }
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenRefreshAndScenarioSelectedAndStartDeploymentFailure_IsFalse(
            [ValueSource(nameof(s_boolValues))] bool waitSuccess,
            [ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                DeploymentSettings underTest = SetUpStartDeployment(deployerMock =>
                    SetUpDeployerStartDeployment(deployerMock, success: false), waitSuccess, hasGameServer);


                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                Assert.IsFalse(underTest.CanCancel);
            }
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenRefreshAndScenarioSelectedAndStartDeploymentException_IsFalse([ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testMessage = "TestException";
                await TestWhenRefreshAndScenarioSelectedAndStartDeploymentException(testMessage, hasGameServer, underTest => Assert.IsFalse(underTest.CanCancel));
            }
        }

        #endregion

        #region CancelDeployment

        [UnityTest]
        public IEnumerator CancelDeployment_WhenCanCancel_CallsDeploymentWaiterCancelDeployment()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                Mock<Delay> delayMock = SetUpDelayMock();
                var deploymentWaiterMock = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
                (DeploymentSettings underTest, Mock<DeployerBase> deployerMock) = SetUpCancelDeploymentWhenCanCancelTrue(coreApiMock, deploymentWaiterMock);

                Task task = underTest.StartDeployment(ConfirmChangeSetTask);
                Assert.IsTrue(underTest.IsDeploymentRunning);
                Assert.IsTrue(underTest.CanCancel);

                underTest.CancelDeployment();
                await task;

                coreApiMock.Verify();
                deployerMock.Verify();
                deploymentWaiterMock.Verify();
            }
        }

        [UnityTest]
        public IEnumerator CancelDeployment_WhenCanCancelAndDeployerCancelSuccess_DescribeStackStatusRefreshed()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                string testStatus = "TestStatus" + DateTime.UtcNow.Ticks.ToString();
                TextProvider textProvider = TextProviderFactory.Create();
                string expectedText = string.Format(textProvider.Get(Strings.StackStatusTemplate), testStatus);

                var coreApiMock = new Mock<CoreApi>();
                Mock<Delay> delayMock = SetUpDelayMock();
                var deploymentWaiterMock = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
                (DeploymentSettings underTest, Mock<DeployerBase> _) = SetUpCancelDeploymentWhenCanCancelTrue(coreApiMock, deploymentWaiterMock);

                var response = new DescribeStackResponse()
                {
                    StackStatus = testStatus,
                    Outputs = new Dictionary<string, string>()
                };
                response = Response.Ok(response);
                coreApiMock.Setup(target => target.DescribeStack(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(response)
                    .Verifiable();

                Task task = underTest.StartDeployment(ConfirmChangeSetTask);
                Assert.IsTrue(underTest.IsDeploymentRunning);
                Assert.IsTrue(underTest.CanCancel);

                // Act
                underTest.CancelDeployment();

                await task;

                // Assert
                coreApiMock.Verify();
                deploymentWaiterMock.Verify();
                Assert.AreEqual(expectedText, underTest.CurrentStackInfo.Status);
            }
        }

        [UnityTest]
        public IEnumerator CancelDeployment_WhenCanCancelAndDeployerCancelSuccess_DescribeStackStatusNotRefreshed()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                Mock<Delay> delayMock = SetUpDelayMock();
                var deploymentWaiterMock = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
                (DeploymentSettings underTest, Mock<DeployerBase> _) = SetUpCancelDeploymentWhenCanCancelTrue(coreApiMock, deploymentWaiterMock);

                // It is called once in StartDeployment
                coreApiMock.Verify(target => target.DescribeStack(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

                Task task = underTest.StartDeployment(ConfirmChangeSetTask);
                Assert.IsTrue(underTest.IsDeploymentRunning);
                Assert.IsTrue(underTest.CanCancel);

                underTest.CancelDeployment();
                await task;

                coreApiMock.Verify();
                deploymentWaiterMock.Verify();
            }
        }

        private static (DeploymentSettings underTest, Mock<DeployerBase> deployerMock) SetUpCancelDeploymentWhenCanCancelTrue(
            Mock<CoreApi> coreApiMock, Mock<DeploymentWaiter> deploymentWaiterMock)
        {
            const string testGameName = "test";
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";
            const string testScenarioPath = "C:/test";

            SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);

            Mock<Delay> delayMock = SetUpDelayMock();
            var deployerMock = new Mock<DeployerBase>(delayMock.Object, coreApiMock.Object);
            DeploymentResponse response = Response.Ok(new DeploymentResponse());

            deployerMock.Setup(target => target.StartDeployment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), ConfirmChangeSetTask))
                .Returns(Task.FromResult(response))
                .Verifiable();

            Mock<ScenarioLocator> scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: false, coreApiMock, deployerMock);

            Mock<PathConverter> pathConverterMock = GetMockPathConverter();
            pathConverterMock.Setup(target => target.GetScenarioAbsolutePath(It.IsAny<string>()))
                .Returns(testScenarioPath);

            pathConverterMock.Setup(target => target.GetParametersFilePath(testScenarioPath))
                .Returns(string.Empty);

            var cancelResponse = Response.Ok(new Response());

            deploymentWaiterMock.Setup(target => target.CancelDeployment())
                .Returns(cancelResponse)
                .Verifiable();

            deploymentWaiterMock.Setup(target => target.CanCancel)
                .Returns(true)
                .Verifiable();

            DeploymentResponse waitResponse = Response.Fail(new DeploymentResponse(AmazonGameLift.Editor.ErrorCode.OperationCancelled));
            deploymentWaiterMock.Setup(target => target.WaitUntilDone(It.IsAny<DeploymentId>()))
                .Returns(Task.Delay(200).ContinueWith(_ => waitResponse))
                .Verifiable();

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock, pathConverter: pathConverterMock, deploymentWaiter: deploymentWaiterMock);

            underTest.Refresh();
            underTest.ScenarioIndex = 0;
            underTest.BuildFolderPath = "test path";
            underTest.BuildFilePath = "test path/build.exe";
            underTest.GameName = testGameName;

            return (underTest, deployerMock);
        }

        #endregion

        #region StartDeployment

        [UnityTest]
        public IEnumerator StartDeployment_WhenRefreshAndScenarioSelected_IsExececuted(
            [ValueSource(nameof(s_boolValues))] bool waitSuccess,
            [ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<DeployerBase> deployerMock = null;

                DeploymentSettings underTest = SetUpStartDeployment(mock =>
                {
                    SetUpDeployerStartDeployment(mock, success: true);
                    deployerMock = mock;
                }, waitSuccess, hasGameServer);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                // Assert
                deployerMock.Verify();
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenRefreshAndScenarioSelectedAndHasNoServerAndInvalidBuildFilePath_IsExececuted()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<DeployerBase> deployerMock = null;

                DeploymentSettings underTest = SetUpStartDeployment(mock =>
                {
                    SetUpDeployerStartDeployment(mock, success: true);
                    deployerMock = mock;
                }, waitSuccess: true, hasGameServer: false);

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                // Assert
                deployerMock.Verify();
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenRefreshAndScenarioSelectedAndHasServerAndInvalidBuildFilePath_IsNotExececuted()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testGameName = "test";
                const string testScenarioName = "test scenario";
                const string testScenarioDescription = "test scenario description";
                const string testScenarioUrl = "test";
                const string testBuildFilePath = "invalid";
                const string testBuildFolderPath = "test path";

                var coreApiMock = new Mock<CoreApi>();
                coreApiMock.Setup(target => target.FileExists(testBuildFilePath))
                    .Returns(true)
                    .Verifiable();

                coreApiMock.Setup(target => target.FolderExists(testBuildFolderPath))
                    .Returns(true)
                    .Verifiable();

                SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);

                Mock<Delay> delayMock = SetUpDelayMock();
                var deployerMock = new Mock<DeployerBase>(delayMock.Object, coreApiMock.Object);
                var scenarioLocatorMock = new Mock<ScenarioLocator>();
                SetUpScenarioLocatorToReturnTestDeployer(
                    testScenarioName, testScenarioDescription, testScenarioUrl, hasServer: true,
                    coreApiMock, deployerMock, scenarioLocatorMock);

                DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock);

                underTest.Refresh();
                underTest.ScenarioIndex = 0;
                underTest.GameName = testGameName;
                underTest.BuildFolderPath = testBuildFolderPath;
                underTest.BuildFilePath = testBuildFilePath;

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                // Assert
                VerifyStartDeploymentNotExecuted(deployerMock);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenRefreshAndScenarioNotSelected_IsNotExececuted([ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<DeployerBase> deployerMock = null;

                DeploymentSettings underTest = SetUpStartDeployment(mock =>
                {
                    SetUpDeployerStartDeployment(mock, success: true);
                    deployerMock = mock;
                }, waitSuccess: false, hasGameServer);

                underTest.Refresh();
                underTest.ScenarioIndex = -1;
                underTest.BuildFolderPath = "test path";
                underTest.BuildFilePath = "test path/build.exe";
                underTest.GameName = "test";

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                VerifyStartDeploymentNotExecuted(deployerMock);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenRefreshAndScenarioSelectedAndGameNameEmpty_IsNotExececuted(
            [ValueSource(nameof(s_boolValues))] bool hasGameServer)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<DeployerBase> deployerMock = null;

                DeploymentSettings underTest = SetUpStartDeployment(mock =>
                {
                    SetUpDeployerStartDeployment(mock, success: true);
                    deployerMock = mock;
                }, waitSuccess: false, hasGameServer);

                underTest.GameName = string.Empty;

                // Act
                await underTest.StartDeployment(ConfirmChangeSetTask);

                VerifyStartDeploymentNotExecuted(deployerMock);
            }
        }

        private static void VerifyStartDeploymentNotExecuted(Mock<DeployerBase> deployerMock)
        {
            try
            {
                deployerMock.Verify(target =>
                    target.StartDeployment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), ConfirmChangeSetTask),
                    Times.Never);
            }
            catch (MockException)
            {
                Assert.IsFalse(true, "The method was executed.");
            }
        }

        #endregion

        #region CurrentStackInfo

        [Test]
        public void CurrentStackInfo_WhenBootstrappedAndNewInstance_IsDefault()
        {
            string key = DateTime.UtcNow.Ticks.ToString();
            string testStatus = "TestStatus" + key;
            string testGameName = "test" + key;
            string testApiGatewayEndpoint = "ApiGatewayEndpoint" + key;
            string testUserPoolClientId = "UserPoolClientId" + key;

            var testOutputs = new Dictionary<string, string>
            {
                {StackOutputKeys.ApiGatewayEndpoint, testApiGatewayEndpoint},
                {StackOutputKeys.UserPoolClientId, testUserPoolClientId},
            };

            TextProvider textProvider = TextProviderFactory.Create();
            string expectedStatusText = textProvider.Get(Strings.StatusNothingDeployed);

            var coreApiMock = new Mock<CoreApi>();
            SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);
            coreApiMock.SetUpCoreApiWithDescribeStack(success: true, stackName: testGameName, result: testStatus,
                outputs: testOutputs);

            DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Assert
            Assert.AreEqual(expectedStatusText, underTest.CurrentStackInfo.Status);
            Assert.IsNull(underTest.CurrentStackInfo.ApiGatewayEndpoint);
            Assert.IsNull(underTest.CurrentStackInfo.UserPoolClientId);
        }

        [UnityTest]
        public IEnumerator CurrentStackInfoChanged_WhenBootstrappedAndGameNameChanged_IsRaised()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testGameName = "test";

                var coreApiMock = new Mock<CoreApi>();
                SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

                underTest.Refresh();
                underTest.ScenarioIndex = 0;

                bool isEventRaised = false;
                underTest.CurrentStackInfoChanged += () =>
                {
                    isEventRaised = true;
                };

                // Act
                await underTest.SetGameNameAsync(testGameName);

                // Assert
                Assert.IsTrue(isEventRaised);
            }
        }

        [UnityTest]
        public IEnumerator CurrentStackInfoChanged_WhenBootstrappedAndGameNameSetAndRefreshCurrentStackInfo_IsRaised()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testGameName = "test";

                var coreApiMock = new Mock<CoreApi>();
                SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

                underTest.Refresh();
                underTest.ScenarioIndex = 0;
                await underTest.SetGameNameAsync(testGameName);

                bool isEventRaised = false;
                underTest.CurrentStackInfoChanged += () =>
                {
                    isEventRaised = true;
                };

                // Act
                underTest.RefreshCurrentStackInfo();

                // Assert
                Assert.IsTrue(isEventRaised);
            }
        }

        [UnityTest]
        public IEnumerator CurrentStackInfo_WhenBootstrappedAndGameNameChanged_IsExpected()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                string key = DateTime.UtcNow.Ticks.ToString();
                string testStatus = "TestStatus" + key;
                string testGameName = "test" + key;
                string testApiGatewayEndpoint = "ApiGatewayEndpoint" + key;
                string testUserPoolClientId = "UserPoolClientId" + key;

                var testOutputs = new Dictionary<string, string>
                {
                    {StackOutputKeys.ApiGatewayEndpoint, testApiGatewayEndpoint},
                    {StackOutputKeys.UserPoolClientId, testUserPoolClientId},
                };

                TextProvider textProvider = TextProviderFactory.Create();
                string expectedStatusText = string.Format(textProvider.Get(Strings.StackStatusTemplate), testStatus);

                var coreApiMock = new Mock<CoreApi>();
                SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);
                coreApiMock.SetUpCoreApiWithDescribeStack(success: true, stackName: testGameName, result: testStatus,
                    outputs: testOutputs);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

                underTest.Refresh();
                underTest.ScenarioIndex = 0;

                // Act
                await underTest.SetGameNameAsync(testGameName);

                // Assert
                Assert.AreEqual(testStatus, underTest.CurrentStackInfo.StackStatus);
                Assert.AreEqual(expectedStatusText, underTest.CurrentStackInfo.Status);
                Assert.AreEqual(testApiGatewayEndpoint, underTest.CurrentStackInfo.ApiGatewayEndpoint);
                Assert.AreEqual(testUserPoolClientId, underTest.CurrentStackInfo.UserPoolClientId);
            }
        }

        [UnityTest]
        public IEnumerator CurrentStackInfo_WhenWaitForCurrentDeploymentAndDeploymentContainerEmpty_IsNotChanged()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                var deploymentIdContainer = new Mock<IDeploymentIdContainer>();
                deploymentIdContainer.SetupGet(target => target.HasValue)
                    .Returns(false)
                    .Verifiable();

                DeploymentSettings underTest = GetUnitUnderTest(deploymentIdContainer: deploymentIdContainer);
                DeploymentStackInfo expectedInfo = underTest.CurrentStackInfo;

                await underTest.WaitForCurrentDeployment();

                Assert.AreEqual(expectedInfo, underTest.CurrentStackInfo);
            }
        }

        [UnityTest]
        public IEnumerator CurrentStackInfo_WhenWaitForCurrentDeploymentAndDeploymentWaiterSuccess_IsExpected()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testProfile = "test";
                const string testScenarioName = "Test Name";
                const string testRegion = "test-region";
                const string testGameName = "test";
                const string testStackName = "test-name";
                var deploymentId = new DeploymentId(testProfile, testRegion, testStackName, testScenarioName);
                var expectedInfo = new DeploymentInfo(testRegion, testGameName, testScenarioName, DateTime.Now, StackStatus.CreateComplete, new Dictionary<string, string>());
                DeploymentStackInfo expectedStackInfo = DeploymentStackInfoFactory.Create(TextProviderFactory.Create(), expectedInfo);

                var coreApiMock = new Mock<CoreApi>();

                Mock<Delay> delayMock = SetUpDelayMock();

                var deploymentWaiter = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
                DeploymentResponse waitResponse = Response.Ok(new DeploymentResponse());

                deploymentWaiter.Setup(target => target.WaitUntilDone(It.IsAny<DeploymentId>()))
                    .Returns(Task.FromResult(waitResponse))
                    .Raises(mock => mock.InfoUpdated += null, expectedInfo)
                    .Verifiable();

                var deploymentIdContainer = new Mock<IDeploymentIdContainer>();
                SetUpContainer(deploymentIdContainer, deploymentId);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, deploymentWaiter: deploymentWaiter, deploymentIdContainer: deploymentIdContainer);

                DeploymentStackInfo unexpectedInfo = underTest.CurrentStackInfo;

                await underTest.WaitForCurrentDeployment();

                Assert.AreNotEqual(unexpectedInfo, underTest.CurrentStackInfo);
                Assert.AreEqual(expectedStackInfo, underTest.CurrentStackInfo);
            }
        }

        [UnityTest]
        public IEnumerator CurrentStackInfo_WhenWaitForCurrentDeploymentAndDeploymentWaiterFails_IsExpected()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testProfile = "test";
                const string testScenarioName = "Test Name";
                const string testRegion = "test-region";
                const string testStackName = "test-name";
                var deploymentId = new DeploymentId(testProfile, testRegion, testStackName, testScenarioName);

                var coreApiMock = new Mock<CoreApi>();

                Mock<Delay> delayMock = SetUpDelayMock();

                var deploymentWaiter = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
                DeploymentResponse waitResponse = Response.Fail(new DeploymentResponse());

                deploymentWaiter.Setup(target => target.WaitUntilDone(It.IsAny<DeploymentId>()))
                    .Returns(Task.FromResult(waitResponse))
                    .Verifiable();

                var deploymentIdContainer = new Mock<IDeploymentIdContainer>();
                SetUpContainer(deploymentIdContainer, deploymentId);
                deploymentIdContainer.Setup(target => target.Clear())
                    .Verifiable();

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, deploymentWaiter: deploymentWaiter, deploymentIdContainer: deploymentIdContainer);

                DeploymentStackInfo expectedInfo = underTest.CurrentStackInfo;

                await underTest.WaitForCurrentDeployment();

                deploymentWaiter.Verify();
                deploymentIdContainer.Verify();
                Assert.AreEqual(expectedInfo, underTest.CurrentStackInfo);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenWaitForCurrentDeploymentAndDeploymentWaiterException_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testMessage = "TestException";
                const string testProfile = "test";
                const string testScenarioName = "Test Name";
                const string testRegion = "test-region";
                const string testStackName = "test-name";
                var deploymentId = new DeploymentId(testProfile, testRegion, testStackName, testScenarioName);

                var coreApiMock = new Mock<CoreApi>();

                Mock<Delay> delayMock = SetUpDelayMock();

                var deploymentWaiter = new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);

                deploymentWaiter.Setup(target => target.WaitUntilDone(It.IsAny<DeploymentId>()))
                    .Throws(new Exception(testMessage))
                    .Verifiable();

                var deploymentIdContainer = new Mock<IDeploymentIdContainer>();
                SetUpContainer(deploymentIdContainer, deploymentId);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, deploymentWaiter: deploymentWaiter, deploymentIdContainer: deploymentIdContainer);

                try
                {
                    await underTest.WaitForCurrentDeployment();
                }
                catch (Exception ex)
                {
                    if (ex.Message != testMessage)
                    {
                        throw;
                    }

                    Assert.IsFalse(underTest.IsDeploymentRunning);
                    return;
                }

                Assert.Fail("Did not throw.");
            }
        }

        #endregion

        [UnityTest]
        [TestCase(null, true, ExpectedResult = null)]
        [TestCase(StackStatus.CreateComplete, true, ExpectedResult = null)]
        [TestCase(StackStatus.CreateFailed, false, ExpectedResult = null)]
        [TestCase(StackStatus.CreateInProgress, false, ExpectedResult = null)]
        [TestCase(StackStatus.DeleteComplete, false, ExpectedResult = null)]
        [TestCase(StackStatus.DeleteFailed, false, ExpectedResult = null)]
        [TestCase(StackStatus.DeleteInProgress, false, ExpectedResult = null)]
        [TestCase(StackStatus.RollbackComplete, false, ExpectedResult = null)]
        [TestCase(StackStatus.RollbackFailed, false, ExpectedResult = null)]
        [TestCase(StackStatus.RollbackInProgress, false, ExpectedResult = null)]
        [TestCase(StackStatus.UpdateComplete, true, ExpectedResult = null)]
        [TestCase(StackStatus.UpdateCompleteCleanUpInProgress, true, ExpectedResult = null)]
        [TestCase(StackStatus.UpdateInProgress, false, ExpectedResult = null)]
        [TestCase(StackStatus.UpdateRollbackComplete, true, ExpectedResult = null)]
        [TestCase(StackStatus.UpdateRollbackInProgress, false, ExpectedResult = null)]
        public IEnumerator IsCurrentStackModifiable_WhenBootstrappedAndGameNameChanged_IsExpected(string testStatus, bool isModifiableExpected)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                string key = DateTime.UtcNow.Ticks.ToString();
                string testGameName = "test" + key;

                var testOutputs = new Dictionary<string, string>();

                var coreApiMock = new Mock<CoreApi>();
                SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);
                coreApiMock.SetUpCoreApiWithDescribeStack(success: true, stackName: testGameName, result: testStatus,
                    outputs: testOutputs);

                DeploymentSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

                underTest.Refresh();
                underTest.ScenarioIndex = 0;

                // Act
                await underTest.SetGameNameAsync(testGameName);

                // Assert
                Assert.AreEqual(isModifiableExpected, underTest.IsCurrentStackModifiable);
            }
        }

        private static void SetUpContainer(Mock<IDeploymentIdContainer> deploymentIdContainer, DeploymentId deploymentId)
        {
            deploymentIdContainer.SetupGet(target => target.HasValue)
                .Returns(true)
                .Verifiable();

            deploymentIdContainer.Setup(target => target.Get())
                .Returns(deploymentId)
                .Verifiable();
        }

        private static async Task TestWhenRefreshAndScenarioSelectedAndStartDeploymentException(string testError, bool hasGameServer, Action<DeploymentSettings> assert)
        {
            var testException = new Exception(testError);
            DeploymentSettings underTest = SetUpStartDeployment(deployerMock =>
            {
                deployerMock.Setup(target => target.StartDeployment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>(), It.IsAny<ConfirmChangesDelegate>()))
                .Throws(testException)
                .Verifiable();
            }, waitSuccess: false, hasGameServer);

            try
            {
                await underTest.StartDeployment(ConfirmChangeSetTask);
            }
            catch (Exception ex)
            {
                if (ex.Message != testError)
                {
                    throw;
                }

                assert(underTest);
            }
        }

        private static void SetUpDeployerStartDeployment(Mock<DeployerBase> deployerMock, bool success)
        {
            var response = new DeploymentResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);

            deployerMock.Setup(target => target.StartDeployment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), ConfirmChangeSetTask))
                .Returns(Task.FromResult(response))
                .Verifiable();
        }

        private static void SetUpcoreApiforRefreshSuccess(Mock<CoreApi> coreApiMock, string profile = "test-profile", string gameName = "game", string result = StackStatus.CreateComplete)
        {
            coreApiMock.SetUpCoreApiWithProfile(success: true, profile);
            coreApiMock.SetUpCoreApiForBootstrapSuccess();
            SetUpcoreApiforRefreshStackStatusSuccess(coreApiMock, profile, gameName, result);
        }

        private static void SetUpcoreApiforRefreshStackStatusSuccess(Mock<CoreApi> coreApiMock, string profile = "test-profile", string gameName = "game", string result = StackStatus.CreateComplete)
        {
            coreApiMock.SetUpGetStackNameAsGameName(gameName);
            coreApiMock.SetUpCoreApiWithDescribeStack(success: true, profile, stackName: gameName, result: result);
        }

        private static DeploymentSettings SetUpStartDeployment(Action<Mock<DeployerBase>> setUpDeployer,
            bool waitSuccess, bool hasGameServer, string buildFilePath = "test path/build.exe",
            Mock<ScenarioLocator> scenarioLocatorMock = null, Mock<CoreApi> coreApiMock = null,
            Mock<DeploymentWaiter> deploymentWaiter = null, Mock<IDeploymentIdContainer> deploymentIdContainer = null)
        {
            const string testGameName = "test";
            const string testScenarioName = "test scenario";
            const string testScenarioDescription = "test scenario description";
            const string testScenarioUrl = "test";
            const string testScenarioPath = "C:/test";
            const string testBuildFolderPath = "test path";

            coreApiMock = coreApiMock ?? new Mock<CoreApi>();
            coreApiMock.Setup(target => target.FileExists(buildFilePath))
                .Returns(hasGameServer)
                .Verifiable();

            coreApiMock.Setup(target => target.FolderExists(testBuildFolderPath))
                .Returns(hasGameServer)
                .Verifiable();

            SetUpcoreApiforRefreshSuccess(coreApiMock, gameName: testGameName);

            Mock<Delay> delayMock = SetUpDelayMock();
            var deployerMock = new Mock<DeployerBase>(delayMock.Object, coreApiMock.Object);
            setUpDeployer(deployerMock);

            scenarioLocatorMock = SetUpScenarioLocatorToReturnTestDeployer(
                testScenarioName, testScenarioDescription, testScenarioUrl, hasGameServer, coreApiMock, deployerMock,
                scenarioLocatorMock);

            Mock<PathConverter> pathConverterMock = GetMockPathConverter();
            pathConverterMock.Setup(target => target.GetScenarioAbsolutePath(It.IsAny<string>()))
                .Returns(testScenarioPath);

            pathConverterMock.Setup(target => target.GetParametersFilePath(testScenarioPath))
                .Returns(string.Empty);

            deploymentWaiter = deploymentWaiter ?? new Mock<DeploymentWaiter>(delayMock.Object, coreApiMock.Object);
            DeploymentResponse waitResponse = waitSuccess
                ? Response.Ok(new DeploymentResponse())
                : Response.Fail(new DeploymentResponse());
            deploymentWaiter.Setup(target => target.WaitUntilDone(It.IsAny<DeploymentId>()))
                .Returns(Task.FromResult(waitResponse))
                .Verifiable();

            DeploymentSettings underTest = GetUnitUnderTest(scenarioLocatorMock, coreApi: coreApiMock,
                pathConverter: pathConverterMock, deploymentWaiter: deploymentWaiter, deploymentIdContainer: deploymentIdContainer);

            underTest.Refresh();
            underTest.ScenarioIndex = 0;
            underTest.GameName = testGameName;

            if (hasGameServer)
            {
                underTest.BuildFolderPath = testBuildFolderPath;
                underTest.BuildFilePath = buildFilePath;
            }

            return underTest;
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

            var scenarios = new DeployerBase[] { deployerMock.Object };
            scenarioLocatorMock.Setup(target => target.GetScenarios())
                .Returns(scenarios)
                .Verifiable();

            return scenarioLocatorMock;
        }

        private static Mock<Delay> SetUpDelayMock()
        {
            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return delayMock;
        }

        private static DeploymentSettings GetUnitUnderTest(Mock<ScenarioLocator> scenarioLocator = null,
            Mock<PathConverter> pathConverter = null, Mock<CoreApi> coreApi = null,
            Mock<ScenarioParametersUpdater> parametersUpdater = null, Mock<DeploymentWaiter> deploymentWaiter = null,
            Mock<IDeploymentIdContainer> deploymentIdContainer = null, Mock<Delay> delay = null)
        {
            coreApi = coreApi ?? new Mock<CoreApi>();
            scenarioLocator = scenarioLocator ?? new Mock<ScenarioLocator>();
            Mock<Delay> delayMock = delay ?? SetUpDelayMock();
            deploymentWaiter = deploymentWaiter ?? new Mock<DeploymentWaiter>(delayMock.Object, coreApi.Object);
            pathConverter = pathConverter ?? GetMockPathConverter(coreApi);
            parametersUpdater = parametersUpdater ?? GetMockScenarioParametersUpdater(coreApi);
            deploymentIdContainer = deploymentIdContainer ?? new Mock<IDeploymentIdContainer>();

            return new DeploymentSettings(scenarioLocator.Object, pathConverter.Object, coreApi.Object,
                parametersUpdater.Object, TextProviderFactory.Create(), deploymentWaiter.Object,
                deploymentIdContainer.Object, delayMock.Object, new MockLogger());
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

        private static Task<bool> ConfirmChangeSetTask(ConfirmChangesRequest _)
        {
            return Task.FromResult(true);
        }
    }
}
