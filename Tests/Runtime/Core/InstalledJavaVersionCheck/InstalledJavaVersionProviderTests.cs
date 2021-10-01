// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using AmazonGameLiftPlugin.Core.JavaCheck;
using AmazonGameLiftPlugin.Core.JavaCheck.Models;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.InstalledJavaVersionCheck
{
    [TestFixture]
    public class InstalledJavaVersionProviderTests
    {
        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionIsInstalled_IsSuccessFul()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(x => x.GetProcessOutput(
                It.IsAny<ProcessStartInfo>())
            ).Returns("java version \"1.8.0_291\"");

            IInstalledJavaVersionProvider installedJavaVersionProvider =
                InstalledJavaVersionProviderFactory.Create(processWrapperMock.Object);

            CheckInstalledJavaVersionResponse response =
                installedJavaVersionProvider.CheckInstalledJavaVersion(new CheckInstalledJavaVersionRequest
                {
                    ExpectedMinimumJavaMajorVersion = 8
                });

            processWrapperMock.Verify();
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionIsNotInstalled_IsNotSuccessFul()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(x => x.GetProcessOutput(
                It.IsAny<ProcessStartInfo>())
            ).Returns("java version \"9.0.1\"");

            IInstalledJavaVersionProvider installedJavaVersionProvider =
                InstalledJavaVersionProviderFactory.Create(processWrapperMock.Object);

            CheckInstalledJavaVersionResponse response =
                installedJavaVersionProvider.CheckInstalledJavaVersion(new CheckInstalledJavaVersionRequest
                {
                    ExpectedMinimumJavaMajorVersion = 8
                });

            processWrapperMock.Verify();
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsFalse(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenJavaIsNotInstalled_IsNotSuccessFul()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(x => x.GetProcessOutput(
                It.IsAny<ProcessStartInfo>())
            ).Returns("java \"0.1\"");

            IInstalledJavaVersionProvider installedJavaVersionProvider =
                InstalledJavaVersionProviderFactory.Create(processWrapperMock.Object);

            CheckInstalledJavaVersionResponse response =
                installedJavaVersionProvider.CheckInstalledJavaVersion(new CheckInstalledJavaVersionRequest
                {
                    ExpectedMinimumJavaMajorVersion = 8
                });

            processWrapperMock.Verify();
            Assert.IsFalse(response.IsInstalled);
        }
    }
}
