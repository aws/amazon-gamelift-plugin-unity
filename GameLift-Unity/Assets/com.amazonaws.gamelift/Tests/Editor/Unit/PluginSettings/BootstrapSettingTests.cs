// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BootstrapSettingTests
    {
        [Test]
        public void IsPrimaryActionEnabled_WhenNewInstance_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);
            bool actual = underTest.IsPrimaryActionEnabled;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(actual);
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(true, true)]
        public void IsPrimaryActionEnabled_WhenRefreshAndCredentialsSettingIsConfiguredParam_IsExpected(bool credentialsSettingSet, bool expected)
        {
            const bool isRegionValid = true;
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";
            Mock<CoreApi> coreApiMock = SetUpCurrentBucketForSuccessSettings(testBucketName, testRegion, isRegionValid);

            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            credentialsSettingMock.Setup(target => target.IsConfigured)
                .Returns(credentialsSettingSet)
                .Verifiable();

            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);

            // Act
            underTest.Refresh();
            bool actual = underTest.IsPrimaryActionEnabled;

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndBucketNotSavedAndRegionSaved_IsFalse()
        {
            const string testRegion = "test-region";
            var coreApiMock = new Mock<CoreApi>();

            GetSettingResponse bucketResponse = Response.Fail(new GetSettingResponse());
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentBucketName))
                .Returns(bucketResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse() { Value = testRegion };
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(testRegion))
                .Returns(true);

            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndBucketSavedAndRegionNotSaved_IsFalse()
        {
            const string testBucketName = "test-bucket";
            var coreApiMock = new Mock<CoreApi>();

            var bucketResponse = new GetSettingResponse() { Value = testBucketName };
            bucketResponse = Response.Ok(bucketResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentBucketName))
                .Returns(bucketResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse();
            regionResponse = Response.Fail(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(It.IsAny<string>()))
                .Returns(false);

            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndBucketSavedAndRegionSavedInvalid_IsFalse()
        {
            const bool isRegionValid = false;
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";
            Mock<CoreApi> coreApiMock = SetUpCurrentBucketForSuccessSettings(testBucketName, testRegion, isRegionValid);
            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(isConfigured);
        }

        [Test]
        public void IsConfigured_WhenRefreshAndBucketAndRegionSaved_IsTrue()
        {
            const bool isRegionValid = true;
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";
            Mock<CoreApi> coreApiMock = SetUpCurrentBucketForSuccessSettings(testBucketName, testRegion, isRegionValid);
            var credentialsSettingMock = new Mock<CredentialsSetting>(coreApiMock.Object);
            var underTest = new BootstrapSetting(coreApiMock.Object, credentialsSettingMock.Object);

            // Act
            underTest.Refresh();
            bool isConfigured = underTest.IsConfigured;

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(isConfigured);
        }

        private Mock<CoreApi> SetUpCurrentBucketForSuccessSettings(string testBucketName, string testRegion, bool isRegionValid)
        {
            var coreApiMock = new Mock<CoreApi>();

            var bucketResponse = new GetSettingResponse() { Value = testBucketName };
            bucketResponse = Response.Ok(bucketResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentBucketName))
                .Returns(bucketResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse() { Value = testRegion };
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(testRegion))
                .Returns(isRegionValid)
                .Verifiable();

            return coreApiMock;
        }
    }
}
