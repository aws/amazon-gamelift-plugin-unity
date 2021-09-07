// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class CredentialsSettingTests
    {
        [Test]
        public void IsConfigured_WhenRefreshAndProfileNotSaved_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();

            GetSettingResponse profileResponse = Response.Fail(new GetSettingResponse());
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var underTest = new CredentialsSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndCredentialsNotSaved_IsFalse()
        {
            const string testProfileName = "test-profile";
            var coreApiMock = new Mock<CoreApi>();

            var profileResponse = new GetSettingResponse() { Value = testProfileName };
            profileResponse = Response.Ok(profileResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var credentialsResponse = new RetriveAwsCredentialsResponse();
            credentialsResponse = Response.Fail(credentialsResponse);
            coreApiMock.Setup(target => target.RetrieveAwsCredentials(testProfileName))
                .Returns(credentialsResponse)
                .Verifiable();

            var underTest = new CredentialsSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndRegionNotSaved_IsFalse()
        {
            const string testProfileName = "test-profile";
            var coreApiMock = new Mock<CoreApi>();

            var profileResponse = new GetSettingResponse() { Value = testProfileName };
            profileResponse = Response.Ok(profileResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var credentialsResponse = new RetriveAwsCredentialsResponse();
            credentialsResponse = Response.Ok(credentialsResponse);
            coreApiMock.Setup(target => target.RetrieveAwsCredentials(testProfileName))
                .Returns(credentialsResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse();
            regionResponse = Response.Fail(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(It.IsAny<string>()))
                .Returns(false);

            var underTest = new CredentialsSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndRegionSavedInvalid_IsFalse()
        {
            const bool isRegionValid = false;
            const string testProfileName = "test-profile";
            Mock<CoreApi> coreApiMock = SetUpCoreApiForRefreshSuccess(testProfileName, isRegionValid);
            var underTest = new CredentialsSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndAllDataValid_IsTrue()
        {
            const bool isRegionValid = true;
            const string testProfileName = "test-profile";
            Mock<CoreApi> coreApiMock = SetUpCoreApiForRefreshSuccess(testProfileName, isRegionValid);
            var underTest = new CredentialsSetting(coreApiMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(isConfigured);
        }

        private Mock<CoreApi> SetUpCoreApiForRefreshSuccess(string testProfileName, bool isRegionValid)
        {
            var coreApiMock = new Mock<CoreApi>();

            var profileResponse = new GetSettingResponse() { Value = testProfileName };
            profileResponse = Response.Ok(profileResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var credentialsResponse = new RetriveAwsCredentialsResponse();
            credentialsResponse = Response.Ok(credentialsResponse);
            coreApiMock.Setup(target => target.RetrieveAwsCredentials(testProfileName))
                .Returns(credentialsResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse();
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(It.IsAny<string>()))
                .Returns(isRegionValid)
                .Verifiable();

            return coreApiMock;
        }
    }
}
