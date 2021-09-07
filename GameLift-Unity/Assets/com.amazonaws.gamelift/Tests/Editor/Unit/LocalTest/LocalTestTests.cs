// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class LocalTestTests
    {
        #region GameLiftLocalPath

        [Test]
        public void GameLiftLocalPath_WhenNewInstance_IsNull()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsNull(underTest.GameLiftLocalPath);
        }

        [Test]
        public void GameLiftLocalPath_WhenRefreshAndGameLiftLocalPathNotSet_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: false);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            underTest.Refresh();

            coreApiMock.Verify();
            Assert.IsNull(underTest.GameLiftLocalPath);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void GameLiftLocalPath_WhenRefreshAndGameLiftLocalPathSetAndExistsParam_IsExpected(bool glFileExists)
        {
            string testGlPath = DateTime.UtcNow.Ticks.ToString();

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: true, testGlPath);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);

            coreApiMock.Setup(target => target.FileExists(testGlPath))
                .Returns(glFileExists);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);

            underTest.Refresh();

            coreApiMock.Verify();
            Assert.AreEqual(glFileExists ? testGlPath : null, underTest.GameLiftLocalPath);
        }

        #endregion

        #region IsBootstrapped

        [Test]
        public void IsBootstrapped_WhenNewInstance_IsFalse()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsFalse(underTest.IsBootstrapped);
        }

        [Test]
        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, true)]
        public void IsBootstrapped_WhenRefreshAndGameLiftLocalPathSetParam_IsTrue(bool glPathSet, bool glFileExists, bool expected)
        {
            string testGlPath = "GL" + DateTime.UtcNow.Ticks.ToString();

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.FileExists(testGlPath))
                .Returns(glFileExists);

            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: glPathSet, testGlPath);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);
            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            underTest.Refresh();

            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.IsBootstrapped);
        }

        #endregion

        [Test]
        [TestCase(false, -1, false)]
        [TestCase(false, 0, false)]
        [TestCase(false, 1, false)]
        [TestCase(false, 100, false)]
        [TestCase(true, -1, true)]
        [TestCase(true, 0, true)]
        [TestCase(true, 1, true)]
        [TestCase(true, 100, true)]
        public void IsFormFilled_WhenBuildExePathExistsAndPortParams_IsExpected(bool exeExists, int port, bool expected)
        {
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.FileExists(testBuildExePath))
                .Returns(exeExists);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            underTest.BuildExePath = testBuildExePath;
            underTest.GameLiftLocalPort = port;

            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.IsFormFilled);
        }

        #region CanStart

        [Test]
        public void CanStart_WhenNewInstance_IsFalse()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsFalse(underTest.CanStart);
        }

        [Test]
        public void CanStart_WhenAnyFileExistsIsFalse_IsFalse()
        {
            Mock<CoreApi> coreApiMock = SetUpCoreApiAnyFileExistsIsFalse();

            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            Assert.IsFalse(underTest.CanStart);
        }

        [Test]
        [TestCase(false, false, false, -1, false)]
        [TestCase(false, false, false, 0, false)]
        [TestCase(false, false, false, 1, false)]
        [TestCase(false, false, false, 100, false)]
        [TestCase(false, false, true, -1, false)]
        [TestCase(false, false, true, 0, false)]
        [TestCase(false, false, true, 1, false)]
        [TestCase(false, false, true, 100, false)]
        [TestCase(true, false, false, -1, false)]
        [TestCase(true, false, false, 0, false)]
        [TestCase(true, false, false, 1, false)]
        [TestCase(true, false, false, 100, false)]
        [TestCase(true, false, true, -1, false)]
        [TestCase(true, false, true, 0, false)]
        [TestCase(true, false, true, 1, false)]
        [TestCase(true, false, true, 100, false)]
        [TestCase(true, true, false, -1, false)]
        [TestCase(true, true, false, 0, false)]
        [TestCase(true, true, false, 1, false)]
        [TestCase(true, true, false, 100, false)]
        [TestCase(true, true, true, -1, true)]
        [TestCase(true, true, true, 0, true)]
        [TestCase(true, true, true, 1, true)]
        [TestCase(true, true, true, 100, true)]
        public void CanStart_WhenRefreshAndIsBootstrappedAndIsFormFilledParams_IsExpected(bool glPathSet, bool glFileExists, bool exeExists, int port, bool expected)
        {
            string testGlPath = "GL" + DateTime.UtcNow.Ticks.ToString();
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.FileExists(testGlPath))
                .Returns(glFileExists);

            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: glPathSet, testGlPath);

            coreApiMock.Setup(target => target.FileExists(testBuildExePath))
                .Returns(exeExists);

            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);

            underTest.Refresh();
            underTest.BuildExePath = testBuildExePath;
            underTest.GameLiftLocalPort = port;

            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.CanStart);
        }

        #endregion

        #region CanStop

        [Test]
        public void CanStop_WhenNewInstance_IsFalse()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsFalse(underTest.CanStop);
        }

        [UnityTest]
        public IEnumerator CanStop_WhenNewInstanceAndStart_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                LocalTest underTest = GetUnitUnderTest();

                await underTest.Start();

                Assert.IsFalse(underTest.CanStop);
            }
        }

        [UnityTest]
        public IEnumerator CanStop_WhenCanStartAndStartSucceeds_IsTrue()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                LocalTest underTest = await TestWhenCanStartAndStartSucceeds(coreApiMock);
                coreApiMock.Verify();
                Assert.IsTrue(underTest.CanStop);
            }
        }

        #endregion

        #region Start

        [UnityTest]
        public IEnumerator Start_WhenAnyFileExistsIsFalse_NotCallingCore()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<CoreApi> coreApiMock = SetUpCoreApiAnyFileExistsIsFalse();
                coreApiMock.Verify(target => target.StartGameLiftLocal(It.IsAny<string>(), It.IsAny<int>()), Times.Never());

                LocalTest underTest = GetUnitUnderTest(coreApiMock);

                await underTest.Start();

                coreApiMock.Verify();
            }
        }

        [Test]
        public void Start_WhenCanStartAndStartAndCancelledWhileWaiting_ThrowsTaskCanceledException()
        {
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
            int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);

            var coreApiMock = new Mock<CoreApi>();
            SetUpWithCoreStartGameLiftLocal(coreApiMock, success: true, testBuildExePath, testPort);

            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new TaskCanceledException())
                .Verifiable();

            LocalTest underTest = GetUnitUnderTest(coreApiMock, delayMock);
            underTest.Refresh();
            underTest.BuildExePath = testBuildExePath;
            underTest.GameLiftLocalPort = testPort;

            AssertAsync.ThrowsAsync<TaskCanceledException>(async () => await underTest.Start());
        }

        #endregion

        #region IsDeploymentRunning

        [Test]
        public void IsDeploymentRunning_WhenNewInstance_IsFalse()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsFalse(underTest.IsDeploymentRunning);
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenAnyFileExistsIsFalseAndStart_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                Mock<CoreApi> coreApiMock = SetUpCoreApiAnyFileExistsIsFalse();
                LocalTest underTest = GetUnitUnderTest(coreApiMock);

                await underTest.Start();

                Assert.IsFalse(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenCanStartAndStartAndRunLocalServerFails_IsTrue()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                LocalTest underTest = await TestWhenCanStartAndStartAndRunLocalServerFails(coreApiMock);

                // Assert
                coreApiMock.Verify();
                Assert.IsTrue(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenCanStartAndStartAndRunLocalServerFailsAndStop_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                SetUpCoreApiWithStopProcess(coreApiMock, success: true);
                LocalTest underTest = await TestWhenCanStartAndStartAndRunLocalServerFails(coreApiMock);
                underTest.Stop();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenCanStartAndStartSucceedsAndStop_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                SetUpCoreApiWithStopProcess(coreApiMock, success: true);
                LocalTest underTest = await TestWhenCanStartAndStartSucceeds(coreApiMock);
                underTest.Stop();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(underTest.IsDeploymentRunning);
            }
        }

        [UnityTest]
        public IEnumerator IsDeploymentRunning_WhenCanStartAndStartSucceeds_IsTrue()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                LocalTest underTest = await TestWhenCanStartAndStartSucceeds(coreApiMock);

                // Assert
                coreApiMock.Verify();
                Assert.IsTrue(underTest.IsDeploymentRunning);
            }
        }

        #endregion

        #region Status

        [Test]
        public void Status_WhenNewInstance_IsNotDisplayed()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsFalse(underTest.Status.IsDisplayed);
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartAndStartGameLiftLocalFails_IsDisplayedError()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
                int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);

                var coreApiMock = new Mock<CoreApi>();
                SetUpWithCoreStartGameLiftLocal(coreApiMock, success: false, testBuildExePath, testPort);

                LocalTest underTest = GetUnitUnderTest(coreApiMock);
                underTest.Refresh();
                underTest.BuildExePath = testBuildExePath;
                underTest.GameLiftLocalPort = testPort;

                // Act
                await underTest.Start();

                // Assert
                coreApiMock.Verify();
                Assert.AreEqual(MessageType.Error, underTest.Status.Type);
            }
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartAndRunLocalServerFails_IsDisplayedError()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                LocalTest underTest = await TestWhenCanStartAndStartAndRunLocalServerFails(coreApiMock);

                // Assert
                coreApiMock.Verify();
                Assert.AreEqual(MessageType.Error, underTest.Status.Type);
            }
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartAndStopWhileWaiting_IsNotDisplayed()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
                int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);

                var coreApiMock = new Mock<CoreApi>();
                SetUpWithCoreStartGameLiftLocal(coreApiMock, success: true, testBuildExePath, testPort);
                SetUpCoreApiWithStopProcess(coreApiMock, success: true);

                var delayMock = new Mock<Delay>();

                LocalTest underTest = GetUnitUnderTest(coreApiMock, delayMock);

                delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Callback(() => underTest.Stop());

                underTest.Refresh();
                underTest.BuildExePath = testBuildExePath;
                underTest.GameLiftLocalPort = testPort;

                // Act
                await underTest.Start();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(underTest.Status.IsDisplayed);
            }
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartSucceeds_IsDisplayedInfo()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                LocalTest underTest = await TestWhenCanStartAndStartSucceeds(coreApiMock);

                // Assert
                coreApiMock.Verify();
                Assert.AreEqual(MessageType.Info, underTest.Status.Type);
            }
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartAndRunLocalServerFailsAndStop_IsNotDisplayed()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                SetUpCoreApiWithStopProcess(coreApiMock, success: true);
                LocalTest underTest = await TestWhenCanStartAndStartAndRunLocalServerFails(coreApiMock);
                underTest.Stop();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(underTest.Status.IsDisplayed);
            }
        }

        [UnityTest]
        public IEnumerator Status_WhenCanStartAndStartSucceedsAndStop_IsNotDisplayed()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                SetUpCoreApiWithStopProcess(coreApiMock, success: true);
                LocalTest underTest = await TestWhenCanStartAndStartSucceeds(coreApiMock);
                underTest.Stop();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(underTest.Status.IsDisplayed);
            }
        }

        #endregion

        #region Form fields

        [Test]
        public void BuildExePath_WhenNewInstance_IsNull()
        {
            LocalTest underTest = GetUnitUnderTest();
            Assert.IsNull(underTest.BuildExePath);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void BuildExePath_WhenRefreshAndPathSavedParam_IsExpected(bool saved)
        {
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, saved, testBuildExePath);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);
            LocalTest underTest = GetUnitUnderTest(coreApiMock);

            underTest.Refresh();

            coreApiMock.Verify();

            if (saved)
            {
                Assert.AreEqual(testBuildExePath, underTest.BuildExePath);
            }
            else
            {
                Assert.IsNull(underTest.BuildExePath);
            }
        }

        [Test]
        [TestCase(int.MinValue, 1)]
        [TestCase(int.MinValue + 12345, 1)]
        [TestCase(-1000, 1)]
        [TestCase(0, 1)]
        [TestCase(ushort.MaxValue + 1, ushort.MaxValue)]
        [TestCase(int.MaxValue, ushort.MaxValue)]
        [TestCase(int.MaxValue - 123, ushort.MaxValue)]
        public void GameLiftLocalPort_WhenSetOutOfRangeParam_IsExpected(int testPort, int expected)
        {
            LocalTest underTest = GetUnitUnderTest();
            underTest.GameLiftLocalPort = testPort;
            Assert.AreEqual(expected, underTest.GameLiftLocalPort);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GameLiftLocalPort_WhenRefreshAndPortSavedParam_IsExpected(bool saved)
        {
            int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);
            string testPortSetting = SettingsFormatter.FormatInt(testPort);

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, saved, testPortSetting);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);

            underTest.Refresh();

            coreApiMock.Verify();

            if (saved)
            {
                Assert.AreEqual(testPort, underTest.GameLiftLocalPort);
            }
        }

        #endregion

        private static async Task<LocalTest> TestWhenCanStartAndStartAndRunLocalServerFails(Mock<CoreApi> coreApiMock)
        {
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
            int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);

            SetUpWithCoreStartGameLiftLocal(coreApiMock, success: true, testBuildExePath, testPort);
            SetUpCoreApiWithRunLocalServer(coreApiMock, success: false, testBuildExePath);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            underTest.Refresh();
            underTest.BuildExePath = testBuildExePath;
            underTest.GameLiftLocalPort = testPort;

            // Act
            await underTest.Start();
            return underTest;
        }

        private static async Task<LocalTest> TestWhenCanStartAndStartSucceeds(Mock<CoreApi> coreApiMock)
        {
            string testBuildExePath = "build" + DateTime.UtcNow.Ticks.ToString();
            int testPort = UnityEngine.Random.Range(1, ushort.MaxValue);

            SetUpWithCoreStartGameLiftLocal(coreApiMock, success: true, testBuildExePath, testPort);
            SetUpCoreApiWithRunLocalServer(coreApiMock, success: true, testBuildExePath);

            LocalTest underTest = GetUnitUnderTest(coreApiMock);
            underTest.Refresh();
            underTest.BuildExePath = testBuildExePath;
            underTest.GameLiftLocalPort = testPort;

            // Act
            await underTest.Start();
            return underTest;
        }

        private static void SetUpWithCoreStartGameLiftLocal(Mock<CoreApi> coreApiMock, bool success, string testBuildExePath, int testPort)
        {
            string testGlPath = "GL" + DateTime.UtcNow.Ticks.ToString();

            coreApiMock.Setup(target => target.FileExists(testBuildExePath))
                    .Returns(true)
                    .Verifiable();

            coreApiMock.Setup(target => target.FileExists(testGlPath))
                    .Returns(true)
                    .Verifiable();

            coreApiMock.SetUpCoreApiWithGameLiftLocalPath(success: true, testGlPath);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.LocalServerPath, success: false, null);
            coreApiMock.SetUpCoreApiWithSetting(SettingsKeys.GameLiftLocalPort, success: false, null);
            SetUpCoreApiWithStartGameLiftLocal(coreApiMock, success: success, testGlPath, testPort);
        }

        private static Mock<CoreApi> SetUpCoreApiAnyFileExistsIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.FileExists(It.IsAny<string>()))
                .Returns(false);
            return coreApiMock;
        }

        private static void SetUpCoreApiWithStartGameLiftLocal(Mock<CoreApi> coreApiMock, bool success,
            string gameLiftLocalFilePath, int port)
        {
            var response = new StartResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.StartGameLiftLocal(gameLiftLocalFilePath, port))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpCoreApiWithRunLocalServer(Mock<CoreApi> coreApiMock, bool success, string exeFilePath)
        {
            var response = new RunLocalServerResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.RunLocalServer(exeFilePath))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpCoreApiWithStopProcess(Mock<CoreApi> coreApiMock, bool success)
        {
            var response = new StopResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.StopProcess(It.IsAny<int>()))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpDelayMockWait(Mock<Delay> delayMock)
        {
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private static LocalTest GetUnitUnderTest(Mock<CoreApi> coreApi = null, Mock<Delay> delayMock = null)
        {
            coreApi = coreApi ?? new Mock<CoreApi>();
            TextProvider textProvider = TextProviderFactory.Create();

            if (delayMock == null)
            {
                delayMock = new Mock<Delay>();
                SetUpDelayMockWait(delayMock);
            }

            return new LocalTest(coreApi.Object, textProvider, delayMock.Object, new MockLogger());
        }
    }
}
