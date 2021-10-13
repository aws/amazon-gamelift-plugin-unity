// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEditor;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BootstrapSettingsTests
    {
        private readonly BucketUrlFormatter _bucketUrlFormatter = new BucketUrlFormatter();

        #region CreateBucket

        [Test]
        public void CreateBucket_WhenCanCreateIsFalse_NotCalling()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Verify(target => target.CreateBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            coreApiMock.Verify(target => target.PutBucketLifecycleConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BucketPolicy>()), Times.Never());

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);
            bootstrapUtilityMock.Verify(target => target.GetBootstrapData(), Times.Never());

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, bootstrapUtility: bootstrapUtilityMock);

            // Act
            underTest.CreateBucket();

            // Assert
            coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
        }

        [Test]
        public void CreateBucket_WhenCanCreateAndBootstrapDataNotFound_IsStatusError()
        {
            const string testBucketName = "test-bucket";
            var coreApiMock = new Mock<CoreApi>();

            var currentResponse = new GetSettingResponse();
            currentResponse = Response.Fail(currentResponse);
            coreApiMock.Setup(target => target.GetSetting(It.IsAny<string>()))
                .Returns(currentResponse)
                .Verifiable();

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);

            GetBootstrapDataResponse bootstrapResponse = Response.Fail(new GetBootstrapDataResponse());
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);

            // Act
            underTest.CreateBucket();

            // Assert
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Error, underTest.Status.Type);
        }

        [Test]
        public void CreateBucket_WhenCanCreateAndBootstrapDataFoundAndCreationFailed_IsStatusError()
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";
            const string testBucketName = "test-bucket";
            var coreApiMock = new Mock<CoreApi>();

            var createResponse = new CreateBucketResponse();
            createResponse = Response.Fail(createResponse);
            coreApiMock.Setup(target => target.CreateBucket(testProfile, testRegion, It.IsAny<string>()))
                .Returns(createResponse)
                .Verifiable();

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);

            var bootstrapResponse = new GetBootstrapDataResponse(testProfile, testRegion);
            bootstrapResponse = Response.Ok(bootstrapResponse);
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);

            // Act
            underTest.CreateBucket();

            // Assert
            coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Error, underTest.Status.Type);
        }

        [Test]
        public void CreateBucket_WhenCanCreateAndBootstrapDataFoundAndCreationSuccessAndPutSettingFails_IsStatusError()
        {
            var coreApiMock = new Mock<CoreApi>();
            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);
            BootstrapSettings underTest = SetUpCreateBucketSuccess(isPutSettingSuccess: false, coreApiMock: coreApiMock, bootstrapUtilityMock: bootstrapUtilityMock);

            // Act
            underTest.CreateBucket();

            // Assert
            coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Error, underTest.Status.Type);
        }

        [Test]
        public void CreateBucket_WhenCreationSuccessAndHasNoPolicy_IsStatusInfo()
        {
            var coreApiMock = new Mock<CoreApi>();
            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);

            coreApiMock.Verify(target => target.PutBucketLifecycleConfiguration(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BucketPolicy>()), Times.Never());

            BootstrapSettings underTest = SetUpCreateBucketSuccess(isPutSettingSuccess: true, coreApiMock: coreApiMock, bootstrapUtilityMock: bootstrapUtilityMock);

            // Act
            underTest.LifeCyclePolicyIndex = 0;
            underTest.CreateBucket();

            // Assert
            coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Info, underTest.Status.Type);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreateBucket_WhenCreationSuccessAndHasPolicyAndPutPolicySuccessParam_IsStatusInfo(bool isPutPolicySuccess)
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";
            const string testBucketName = "test-bucket";
            var coreApiMock = new Mock<CoreApi>();
            var bootstrapUtilityMock = new Mock<BootstrapUtility>(coreApiMock.Object);

            var putPolicyResponse = new PutLifecycleConfigurationResponse();
            putPolicyResponse = isPutPolicySuccess
                ? Response.Ok(putPolicyResponse)
                : Response.Fail(putPolicyResponse);
            coreApiMock.Setup(target => target.PutBucketLifecycleConfiguration(testProfile, testRegion, testBucketName, It.IsAny<BucketPolicy>()))
                .Returns(putPolicyResponse)
                .Verifiable();

            BootstrapSettings underTest = SetUpCreateBucketSuccess(isPutSettingSuccess: true, testProfile, testRegion, testBucketName, coreApiMock, bootstrapUtilityMock);

            // Act
            underTest.LifeCyclePolicyIndex = 1;
            underTest.CreateBucket();

            // Assert
            coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Info, underTest.Status.Type);
        }

        private static BootstrapSettings SetUpCreateBucketSuccess(bool isPutSettingSuccess,
            string testProfile = "test-profile", string testRegion = "test-region", string testBucketName = "test-bucket",
            Mock<CoreApi> coreApiMock = null, Mock<BootstrapUtility> bootstrapUtilityMock = null)
        {
            coreApiMock = coreApiMock ?? new Mock<CoreApi>();
            
            // GetSettings is only called if PutSettings is successful
            if (isPutSettingSuccess)
            {
                var currentResponse = new GetSettingResponse();
                currentResponse = Response.Fail(currentResponse);
                coreApiMock.Setup(target => target.GetSetting(It.IsAny<string>()))
                    .Returns(currentResponse)
                    .Verifiable();
            }

            var saveBucketResponse = new PutSettingResponse();
            saveBucketResponse = isPutSettingSuccess
                ? Response.Ok(saveBucketResponse)
                : Response.Fail(saveBucketResponse);
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentBucketName, testBucketName))
                .Returns(saveBucketResponse)
                .Verifiable();

            var createResponse = new CreateBucketResponse();
            createResponse = Response.Ok(createResponse);
            coreApiMock.Setup(target => target.CreateBucket(testProfile, testRegion, It.IsAny<string>()))
                .Returns(createResponse)
                .Verifiable();

            bootstrapUtilityMock = bootstrapUtilityMock ?? new Mock<BootstrapUtility>(coreApiMock.Object);
            var bootstrapResponse = new GetBootstrapDataResponse(testProfile, testRegion);
            bootstrapResponse = Response.Ok(bootstrapResponse);
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock, bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);
            return underTest;
        }

        #endregion

        #region Current Bucket

        #region HasCurrentBucket

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketNotSavedAndRegionSaved_IsFalse()
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

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketSavedAndRegionNotSaved_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: false, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketSavedAndRegionSavedInvalid_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsTrue()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess();
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(underTest.HasCurrentBucket);
        }

        #endregion

        [Test]
        public void CurrentBucketName_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testBucketName = "test-bucket";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess(testBucketName);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testBucketName, underTest.CurrentBucketName);
        }

        [Test]
        public void CurrentBucketUrl_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess(testBucketName, testRegion);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            string expectedUrl = _bucketUrlFormatter.Format(testBucketName, testRegion);
            Assert.AreEqual(expectedUrl, underTest.CurrentBucketUrl);
        }

        [Test]
        public void CurrentRegion_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testRegion = "test-region";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess(testRegion: testRegion);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testRegion, underTest.CurrentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshCurrentBucketAndBucketSavedAndRegionSavedInvalid_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshCurrentBucket();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        private static void AssertNoCurrentBucket(BootstrapSettings underTest)
        {
            Assert.IsFalse(underTest.HasCurrentBucket);
            Assert.IsNull(underTest.CurrentBucketName);
            Assert.IsNull(underTest.CurrentBucketUrl);
        }

        #endregion

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionNotSaved_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();

            var regionResponse = new GetSettingResponse();
            regionResponse = Response.Fail(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.IsValidRegion(It.IsAny<string>()))
                .Returns(true);

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionSavedInvalid_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionAndAccountIdAndProfileSaved_IsExpected()
        {
            const string testRegion = "test-region";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true, testRegion: testRegion);
            coreApiMock.SetUpCoreApiWithAccountId(success: true);
            coreApiMock.SetUpCoreApiWithProfile(success: true);

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testRegion, currentRegion);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionNotSaved_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithRegion(success: false, valid: false);

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionSavedAndInvalid_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndCurrentProfileInvalid_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess();
            coreApiMock.SetUpCoreApiWithProfile(success: false);

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndAccountIdInvalid_IsNull()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiForBootstrapSuccess();
            coreApiMock.SetUpCoreApiWithProfile(success: true);
            coreApiMock.SetUpCoreApiWithAccountId(success: false);

            BootstrapSettings underTest = GetUnitUnderTest(coreApi: coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionAndAccountIdAndProfileSaved_IsExpected()
        {
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true, testRegion: testRegion);
            coreApiMock.SetUpCoreApiWithAccountId(success: true);
            coreApiMock.SetUpCoreApiWithProfile(success: true);

            var bucketNameFormatterMock = new Mock<IBucketNameFormatter>();
            bucketNameFormatterMock.Setup(target => target.FormatBucketName(It.IsAny<string>(), testRegion))
                .Returns(testBucketName)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(bucketNameFormatterMock, coreApiMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testBucketName, name);
        }

        [Test]
        public void BucketName_WhenNewInstance_IsNull()
        {
            var bucketNameFormatterMock = new Mock<IBucketNameFormatter>();

            BootstrapSettings underTest = GetUnitUnderTest(bucketNameFormatterMock);
            Assert.IsNull(underTest.BucketName);
        }

        private static BootstrapSettings GetUnitUnderTest(Mock<IBucketNameFormatter> bucketNameFormatterMock = null,
            Mock<CoreApi> coreApi = null,
            Mock<BootstrapUtility> bootstrapUtility = null)
        {
            bucketNameFormatterMock = bucketNameFormatterMock ?? new Mock<IBucketNameFormatter>();
            coreApi = coreApi ?? new Mock<CoreApi>();
            bootstrapUtility = bootstrapUtility ?? new Mock<BootstrapUtility>(coreApi.Object);
            TextProvider textProvider = TextProviderFactory.Create();

            string[] policyNames = new string[] { "none", "30 days" };
            var lifecyclePolicies = new BucketPolicy[] { BucketPolicy.None, BucketPolicy.ThirtyDaysLifecycle };
            return new BootstrapSettings(lifecyclePolicies, policyNames, textProvider, bucketNameFormatterMock.Object, new MockLogger(), coreApi.Object, bootstrapUtility.Object);
        }
    }
}
