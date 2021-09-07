// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using AmazonGameLift.Editor;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class JavaSettingTests
    {
        [Test]
        public void IsConfigured_WhenRefreshAndApiCheckIsFalseAndCancelled_IsFalse()
        {
            // API mock
            var coreApiMock = new Mock<CoreApi>();
            var cancellationTokenSource = new CancellationTokenSource();
            coreApiMock.Setup(target => target.CheckInstalledJavaVersion())
                .Returns(false)
                .Verifiable();

            var underTest = new JavaSetting(coreApiMock.Object);

            // Act
            cancellationTokenSource.Cancel();
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            // Assert
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndApiCheckIsTrueAndCancelled_IsTrue()
        {
            // API mock
            var coreApiMock = new Mock<CoreApi>();
            var cancellationTokenSource = new CancellationTokenSource();
            coreApiMock.Setup(target => target.CheckInstalledJavaVersion())
                .Returns(true)
                .Verifiable();

            var underTest = new JavaSetting(coreApiMock.Object);

            // Act
            cancellationTokenSource.Cancel();
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndApiCheckIsTrue_IsTrue()
        {
            // API mock
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.CheckInstalledJavaVersion())
                .Returns(true)
                .Verifiable();

            var underTest = new JavaSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(isConfigured);
        }
    }
}
