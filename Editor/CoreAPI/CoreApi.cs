// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.AccountManagement;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.BucketManagement;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.DeploymentManagement;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;
using AmazonGameLiftPlugin.Core.JavaCheck;
using AmazonGameLiftPlugin.Core.JavaCheck.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.FileZip;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;

namespace AmazonGameLift.Editor
{
    public class CoreApi
    {
        private const int MinJavaMajorVersion = 8;
        private static readonly ProcessWrapper s_process = new ProcessWrapper();

        public static CoreApi SharedInstance { get; } = new CoreApi();

        internal CoreApi()
        {
            _settingsStore = new SettingsStore(_fileWrapper, settingsFilePath: Paths.PluginSettingsFile);
            Bootstrapper.Initialize();
        }

        #region File system

        private readonly IFileWrapper _fileWrapper = new FileWrapper();

        public virtual bool FolderExists(string path)
        {
            return _fileWrapper.DirectoryExists(path);
        }

        public virtual string GetUniqueTempFilePath()
        {
            return _fileWrapper.GetUniqueTempFilePath();
        }

        public virtual bool FileExists(string path)
        {
            return _fileWrapper.FileExists(path);
        }

        public virtual void FileDelete(string path)
        {
            _fileWrapper.Delete(path);
        }

        public virtual FileReadAllTextResponse FileReadAllText(string path)
        {
            try
            {
                string text = _fileWrapper.ReadAllText(path);
                return Response.Ok(new FileReadAllTextResponse(text));
            }
            catch (Exception ex)
            {
                var response = new FileReadAllTextResponse()
                {
                    ErrorCode = ErrorCode.ReadingFileFailed,
                    ErrorMessage = ex.Message
                };
                return Response.Fail(response);
            }
        }

        public virtual Response FileWriteAllText(string path, string text)
        {
            try
            {
                _fileWrapper.WriteAllText(path, text);
                return Response.Ok(new Response());
            }
            catch (Exception ex)
            {
                var response = new Response()
                {
                    ErrorCode = ErrorCode.ReadingFileFailed,
                    ErrorMessage = ex.Message
                };
                return Response.Fail(response);
            }
        }

        #endregion

        #region Compression

        private readonly IFileZip _fileZip = new FileZip();

        public virtual void Zip(string sourceFolderPath, string targetFilePath)
        {
            _fileZip.Zip(sourceFolderPath, targetFilePath);
        }

        #endregion

        #region Credentials

        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());

        public virtual GetProfilesResponse ListCredentialsProfiles()
        {
            var request = new GetProfilesRequest();
            return _credentialsStore.GetProfiles(request);
        }

        public virtual RetriveAwsCredentialsResponse RetrieveAwsCredentials(string profileName)
        {
            var request = new RetriveAwsCredentialsRequest() { ProfileName = profileName };
            return _credentialsStore.RetriveAwsCredentials(request);
        }

        public virtual SaveAwsCredentialsResponse SaveAwsCredentials(string profileName, string accessKey, string secretKey)
        {
            var request = new SaveAwsCredentialsRequest()
            {
                ProfileName = profileName,
                AccessKey = accessKey,
                SecretKey = secretKey
            };
            return _credentialsStore.SaveAwsCredentials(request);
        }

        public virtual UpdateAwsCredentialsResponse UpdateAwsCredentials(string profileName, string accessKey, string secretKey)
        {
            var request = new UpdateAwsCredentialsRequest()
            {
                ProfileName = profileName,
                AccessKey = accessKey,
                SecretKey = secretKey
            };
            return _credentialsStore.UpdateAwsCredentials(request);
        }

        #endregion

        #region Settings

        private readonly ISettingsStore _settingsStore;

        public virtual GetSettingResponse GetSetting(string key)
        {
            var request = new GetSettingRequest() { Key = key };
            return _settingsStore.GetSetting(request);
        }

        public virtual PutSettingResponse PutSetting(string key, string value)
        {
            var request = new PutSettingRequest()
            {
                Key = key,
                Value = value
            };
            return _settingsStore.PutSetting(request);
        }

        public virtual ClearSettingResponse ClearSetting(string key)
        {
            var request = new ClearSettingRequest() { Key = key };
            return _settingsStore.ClearSetting(request);
        }

        public virtual Response PutSettingOrClear(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return _settingsStore.ClearSetting(new ClearSettingRequest() { Key = key });
            }

            return _settingsStore.PutSetting(new PutSettingRequest()
            {
                Key = key,
                Value = value
            });
        }

        #endregion

        #region S3

        public virtual IEnumerable<string> ListAvailableRegions()
        {
            return AwsRegionMapper.AvailableRegions();
        }

        public virtual bool IsValidRegion(string region)
        {
            return AwsRegionMapper.IsValidRegion(region);
        }

        public virtual RetrieveAccountIdByCredentialsResponse RetrieveAccountId(string profileName)
        {
            RetriveAwsCredentialsResponse credentialsResponse = RetrieveAwsCredentials(profileName);
            var request = new RetrieveAccountIdByCredentialsRequest()
            {
                AccessKey = credentialsResponse.AccessKey,
                SecretKey = credentialsResponse.SecretKey
            };
            var accountManager = new AccountManager(new AmazonSecurityTokenServiceClientWrapper());
            return accountManager.RetrieveAccountIdByCredentials(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual GetBucketsResponse ListBuckets(string profileName, string region)
        {
            var bucketStore = new BucketStore(CreateS3Wrapper(profileName, region));
            var request = new GetBucketsRequest()
            {
                Region = region
            };
            return bucketStore.GetBuckets(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual CreateBucketResponse CreateBucket(string profileName, string region, string bucketName)
        {
            var bucketStore = new BucketStore(CreateS3Wrapper(profileName, region));
            var request = new CreateBucketRequest()
            {
                BucketName = bucketName,
                Region = region
            };
            return bucketStore.CreateBucket(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual PutLifecycleConfigurationResponse PutBucketLifecycleConfiguration(
            string profileName, string region, string bucketName, BucketPolicy expirationPolicy)
        {
            var bucketStore = new BucketStore(CreateS3Wrapper(profileName, region));
            var request = new PutLifecycleConfigurationRequest()
            {
                BucketName = bucketName,
                BucketPolicy = expirationPolicy
            };
            return bucketStore.PutLifecycleConfiguration(request);
        }

        internal virtual IAmazonS3Wrapper CreateS3Wrapper(string profileName, string region)
        {
            RetriveAwsCredentialsResponse response = RetrieveAwsCredentials(profileName);

            if (!response.Success)
            {
                return CreateDefaultS3Wrapper();
            }

            string accessKey = response.AccessKey;
            string secretKey = response.SecretKey;
            return new AmazonS3Wrapper(accessKey, secretKey, region);
        }

        private static IAmazonS3Wrapper CreateDefaultS3Wrapper()
        {
            return new AmazonS3Wrapper(string.Empty, string.Empty, string.Empty);
        }

        #endregion

        #region Deployment

        private static readonly DeploymentFormatter s_deploymentFormatter = new DeploymentFormatter();

        public virtual string GetBuildS3Key()
        {
            return s_deploymentFormatter.GetBuildS3Key();
        }

        public virtual string GetStackName(string gameName)
        {
            return s_deploymentFormatter.GetStackName(gameName);
        }

        public virtual string GetServerGamePath(string gameFilePathInBuild)
        {
            return s_deploymentFormatter.GetServerGamePath(gameFilePathInBuild);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual ValidateCfnTemplateResponse ValidateCfnTemplate(string profileName, string region, string templateFilePath)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new ValidateCfnTemplateRequest()
            {
                TemplateFilePath = templateFilePath
            };
            return deploymentManager.ValidateCfnTemplate(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual StackExistsResponse StackExists(string profileName, string region, string stackName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new StackExistsRequest()
            {
                StackName = stackName
            };
            return deploymentManager.StackExists(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual CreateChangeSetResponse CreateChangeSet(string profileName, string region,
            string bucketName, string stackName, string templateFilePath, string parametersFilePath,
            string gameName, string lambdaFolderPath, string buildS3Key = null)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new CreateChangeSetRequest()
            {
                StackName = stackName,
                TemplateFilePath = templateFilePath,
                ParametersFilePath = parametersFilePath,
                BootstrapBucketName = bucketName,
                GameName = gameName,
                LambdaSourcePath = lambdaFolderPath,
                BuildS3Key = buildS3Key,
            };
            return deploymentManager.CreateChangeSet(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual DescribeChangeSetResponse DescribeChangeSet(string profileName, string region,
            string stackName, string changeSetName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new DescribeChangeSetRequest()
            {
                StackName = stackName,
                ChangeSetName = changeSetName,
            };
            return deploymentManager.DescribeChangeSet(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual ExecuteChangeSetResponse ExecuteChangeSet(string profileName, string region, string stackName, string changeSetName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new ExecuteChangeSetRequest
            {
                StackName = stackName,
                ChangeSetName = changeSetName
            };
            return deploymentManager.ExecuteChangeSet(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual DeleteChangeSetResponse DeleteChangeSet(string profileName, string region,
            string stackName, string changeSetName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new DeleteChangeSetRequest()
            {
                StackName = stackName,
                ChangeSetName = changeSetName,
            };
            return deploymentManager.DeleteChangeSet(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual DescribeStackResponse DescribeStack(string profileName, string region, string stackName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new DescribeStackRequest
            {
                StackName = stackName,
                OutputKeys = new List<string>
                {
                    StackOutputKeys.ApiGatewayEndpoint,
                    StackOutputKeys.UserPoolClientId,
                }
            };
            return deploymentManager.DescribeStack(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual DeleteStackResponse DeleteStack(string profileName, string region, string stackName)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new DeleteStackRequest
            {
                StackName = stackName
            };
            return deploymentManager.DeleteStack(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>.
        /// </summary>
        public virtual UploadServerBuildResponse UploadServerBuild(string profileName, string region,
            string bucketName, string bucketKey, string filePath)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new UploadServerBuildRequest()
            {
                BucketName = bucketName,
                BuildS3Key = bucketKey,
                FilePath = filePath,
            };
            return deploymentManager.UploadServerBuild(request);
        }

        /// <summary>
        /// Needs AWS credentials set up for <paramref name="profileName"/>. <paramref name="clientRequestToken"/> can be null.
        /// </summary>
        public virtual CancelDeploymentResponse CancelDeployment(string profileName, string region,
            string stackName, string clientRequestToken)
        {
            IDeploymentManager deploymentManager = CreateDeploymentManager(profileName, region);
            var request = new CancelDeploymentRequest()
            {
                StackName = stackName,
                ClientRequestToken = clientRequestToken,
            };
            return deploymentManager.CancelDeployment(request);
        }

        internal virtual IDeploymentManager CreateDeploymentManager(string profileName, string region)
        {
            RetriveAwsCredentialsResponse response = RetrieveAwsCredentials(profileName);
            string accessKey = response.Success ? response.AccessKey : string.Empty;
            string secretKey = response.Success ? response.SecretKey : string.Empty;
            var amazonS3Wrapper = new AmazonS3Wrapper(accessKey, secretKey, region);
            IAmazonCloudFormationWrapper cloudFormationWrapper = new AmazonCloudFormationWrapper(accessKey, secretKey, region);
            return new DeploymentManager(cloudFormationWrapper, amazonS3Wrapper, _fileWrapper, new FileZip());
        }

        #endregion

        #region JRE

        private readonly InstalledJavaVersionProvider _installedJavaVersionProvider =
            new InstalledJavaVersionProvider(s_process);

        public virtual bool CheckInstalledJavaVersion()
        {
            var request = new CheckInstalledJavaVersionRequest()
            {
                ExpectedMinimumJavaMajorVersion = MinJavaMajorVersion
            };
            CheckInstalledJavaVersionResponse response = _installedJavaVersionProvider.CheckInstalledJavaVersion(request);
            return response.IsInstalled;
        }

        #endregion

        #region Local Testing

        private readonly GameLiftProcess _gameLiftProcess = new GameLiftProcess(s_process);

        public virtual StartResponse StartGameLiftLocal(string gameLiftLocalFilePath, int port,
                LocalOperatingSystem localOperatingSystem = LocalOperatingSystem.WINDOWS)
        {
            var request = new StartRequest()
            {
                GameLiftLocalFilePath = gameLiftLocalFilePath,
                Port = port,
                LocalOperatingSystem = localOperatingSystem
            };
            return _gameLiftProcess.Start(request);
        }

        public virtual StopResponse StopProcess(int processId,
                LocalOperatingSystem localOperatingSystem = LocalOperatingSystem.WINDOWS)
        {
            var request = new StopRequest()
            {
                ProcessId = processId,
                LocalOperatingSystem = localOperatingSystem
            };
            return _gameLiftProcess.Stop(request);
        }

        public virtual RunLocalServerResponse RunLocalServer(string exeFilePath, string applicationProductName,
                LocalOperatingSystem localOperatingSystem = LocalOperatingSystem.WINDOWS)
        {
            var request = new RunLocalServerRequest()
            {
                FilePath = exeFilePath,
                ShowWindow = true,
                ApplicationProductName = applicationProductName,
                LocalOperatingSystem = localOperatingSystem
            };
            return _gameLiftProcess.RunLocalServer(request);
        }

        #endregion
    }
}
