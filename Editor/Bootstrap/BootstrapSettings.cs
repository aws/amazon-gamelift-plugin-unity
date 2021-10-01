// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// A view model for <see cref="BootstrapWindow"/>.
    /// </summary>
    internal class BootstrapSettings
    {
        public const int CreationMode = 0;
        public const int SelectionMode = 1;
        public const int NoneLifeCyclePolicyIndex = 0;
        private const int DefaultLifeCyclePolicyIndex = 0;
        private readonly Status _status = new Status();
        private readonly BucketUrlFormatter _bucketUrlFormatter = new BucketUrlFormatter();
        private readonly BucketPolicy[] _lifecyclePolicies;
        private readonly TextProvider _textProvider;
        private readonly IBucketNameFormatter _bucketFormatter;
        private readonly ILogger _logger;
        private readonly CoreApi _coreApi;
        private readonly BootstrapUtility _bootstrapUtility;
        private List<string> _existingBuckets = new List<string>();

        public IReadStatus Status => _status;

        public IReadOnlyList<string> ExistingBuckets => _existingBuckets;

        public string[] AllLifecyclePolicyNames { get; }

        /// <summary>
        /// Is set by <see cref="Refresh"/>, <see cref="RefreshBucketName"/> or <see cref="RefreshCurrentBucket"/>.
        /// </summary>
        public string CurrentRegion { get; private set; }

        /// <summary>
        /// Updated in <see cref="CreateBucket"/> or <see cref="SaveSelectedBucket"/>.
        /// </summary>
        public string CurrentBucketName { get; private set; }

        public string CurrentBucketUrl { get; private set; }

        public bool HasCurrentBucket { get; private set; }

        /// <summary>
        /// Generated from <see cref="GameName"/>.
        /// </summary>
        public string BucketName { get; set; }

        public int LifeCyclePolicyIndex { get; set; }

        public bool CanCreate => !string.IsNullOrEmpty(BucketName);

        public bool IsBucketListLoaded { get; private set; }

        public bool CanSaveSelectedBucket =>
            _existingBuckets.Count != 0
            && CurrentRegion != null
            && !string.IsNullOrEmpty(BucketName);

        public int SelectedMode { get; set; }

        public event Action<IReadOnlyList<string>> OnBucketsLoaded = default;

        public BootstrapSettings(IEnumerable<BucketPolicy> lifecyclePolicies,
            IEnumerable<string> lifecyclePolicyNames, TextProvider textProvider,
            IBucketNameFormatter bucketFormatter, ILogger logger, CoreApi coreApi = null,
            BootstrapUtility bootstrapUtility = null)
        {
            if (lifecyclePolicies is null)
            {
                throw new ArgumentNullException(nameof(lifecyclePolicies));
            }

            if (lifecyclePolicyNames is null)
            {
                throw new ArgumentNullException(nameof(lifecyclePolicyNames));
            }

            _lifecyclePolicies = lifecyclePolicies.ToArray();
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _bucketFormatter = bucketFormatter ?? throw new ArgumentNullException(nameof(bucketFormatter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coreApi = coreApi ?? CoreApi.SharedInstance;
            _bootstrapUtility = bootstrapUtility ?? BootstrapUtility.SharedInstance;
            LifeCyclePolicyIndex = DefaultLifeCyclePolicyIndex;
            AllLifecyclePolicyNames = lifecyclePolicyNames.ToArray();
        }

        public async Task SetUp(CancellationToken cancellationToken = default)
        {
            await RefreshExistingBuckets(cancellationToken);
        }

        public void SelectBucket(string name)
        {
            if (BucketName == name)
            {
                return;
            }

            BucketName = name;

            // Reset if we select a new valid bucket
            _status.IsDisplayed &= string.IsNullOrEmpty(BucketName);
        }

        public void CreateBucket()
        {
            if (!CanCreate)
            {
                return;
            }

            GetBootstrapDataResponse bootstrapResponse = _bootstrapUtility.GetBootstrapData();

            if (!bootstrapResponse.Success)
            {
                OnBucketCreationFailure(bootstrapResponse);
                return;
            }

            CreateBucketResponse createResponse = _coreApi.CreateBucket(bootstrapResponse.Profile, bootstrapResponse.Region, BucketName);

            if (createResponse.Success)
            {
                OnBucketCreated(bootstrapResponse.Profile, bootstrapResponse.Region, BucketName);
            }
            else
            {
                OnBucketCreationFailure(createResponse);
            }
        }

        public void SaveSelectedBucket()
        {
            if (!CanSaveSelectedBucket)
            {
                _logger.Log(DevStrings.OperationInvalid, LogType.Error);
                return;
            }

            PutSettingResponse bucketNameResponse = _coreApi.PutSetting(SettingsKeys.CurrentBucketName, BucketName);

            if (!bucketNameResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, bucketNameResponse);
                _logger.LogResponseError(bucketNameResponse);
                return;
            }

            HasCurrentBucket = true;
            CurrentBucketName = BucketName;
            CurrentBucketUrl = _bucketUrlFormatter.Format(CurrentBucketName, CurrentRegion);
            SetInfoStatus(Strings.StatusBootstrapUpdateComplete);
        }

        /// <summary>
        /// Sets <see cref="IsBucketListLoaded"/> to <c>true</c> when complete.
        /// </summary>
        public async Task RefreshExistingBuckets(CancellationToken cancellationToken = default)
        {
            IsBucketListLoaded = false;
            _status.IsDisplayed = false;
            _existingBuckets.Clear();

            try
            {
                GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

                if (!profileResponse.Success)
                {
                    SetErrorStatus(Strings.StatusGetProfileFailed);
                    _logger.LogResponseError(profileResponse);
                    return;
                }

                GetSettingResponse regionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

                if (!regionResponse.Success || !_coreApi.IsValidRegion(regionResponse.Value))
                {
                    SetErrorStatus(Strings.StatusGetRegionFailed);
                    _logger.LogResponseError(regionResponse);
                    return;
                }

                GetBucketsResponse bucketsResponse = await Task.Run(() => _coreApi.ListBuckets(profileResponse.Value, regionResponse.Value), cancellationToken);

                if (!bucketsResponse.Success)
                {
                    SetErrorStatus(Strings.StatusBootstrapFailedTemplate, bucketsResponse);
                    _logger.LogResponseError(bucketsResponse);
                    return;
                }

                _existingBuckets = bucketsResponse.Buckets.ToList();
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
            finally
            {
                IsBucketListLoaded = true;
                OnBucketsLoaded?.Invoke(_existingBuckets);
            }
        }

        public void Refresh()
        {
            RefreshBucketName();
            RefreshCurrentBucket();
        }

        public void RefreshCurrentBucket()
        {
            GetSettingResponse bucketNameResponse = _coreApi.GetSetting(SettingsKeys.CurrentBucketName);
            CurrentBucketName = bucketNameResponse.Success ? bucketNameResponse.Value : null;
            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);
            bool isRegionValid = _coreApi.IsValidRegion(currentRegionResponse.Value);
            CurrentRegion = currentRegionResponse.Success && isRegionValid ? currentRegionResponse.Value : null;
            HasCurrentBucket = !string.IsNullOrEmpty(CurrentBucketName) && isRegionValid;

            if (HasCurrentBucket)
            {
                CurrentBucketUrl = _bucketUrlFormatter.Format(CurrentBucketName, CurrentRegion);
                return;
            }

            CurrentBucketName = null;
            CurrentBucketUrl = null;

            if (!isRegionValid)
            {
                CurrentRegion = null;
            }
        }

        public void RefreshBucketName()
        {
            CurrentRegion = null;
            BucketName = null;
            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!currentRegionResponse.Success || !_coreApi.IsValidRegion(currentRegionResponse.Value))
            {
                SetErrorStatus(Strings.StatusGetRegionFailed);
                _logger.LogResponseError(currentRegionResponse);
                return;
            }

            CurrentRegion = currentRegionResponse.Value;
            GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!profileResponse.Success)
            {
                SetErrorStatus(Strings.StatusGetProfileFailed);
                _logger.LogResponseError(profileResponse);
                return;
            }

            RetrieveAccountIdByCredentialsResponse accountIdResponse = _coreApi.RetrieveAccountId(profileResponse.Value);

            if (!accountIdResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, accountIdResponse);
                _logger.LogResponseError(accountIdResponse);
                return;
            }

            BucketName = _bucketFormatter.FormatBucketName(accountIdResponse.AccountId, CurrentRegion);
        }

        private void SetInfoStatus(string statusKey)
        {
            SetStatus(statusKey, MessageType.Info);
        }

        private void SetErrorStatus(string statusKey)
        {
            SetStatus(statusKey, MessageType.Error);
        }

        private void SetErrorStatus(string statusKey, Response errorResponse)
        {
            string errorTemplate = _textProvider.Get(statusKey);
            string message = string.Format(errorTemplate, _textProvider.GetError(errorResponse.ErrorCode), errorResponse.ErrorMessage);
            _status.SetMessage(message, MessageType.Error);
            _status.IsDisplayed = true;
        }

        private void SetStatus(string statusKey, MessageType messageType)
        {
            _status.SetMessage(_textProvider.Get(statusKey), messageType);
            _status.IsDisplayed = true;
        }

        private void OnBucketCreated(string profileName, string region, string bucketName)
        {
            PutSettingResponse bucketNameResponse = _coreApi.PutSetting(SettingsKeys.CurrentBucketName, bucketName);

            if (!bucketNameResponse.Success)
            {
                OnBucketCreationFailure(bucketNameResponse);
                return;
            }

            BucketPolicy policy = _lifecyclePolicies[LifeCyclePolicyIndex];

            if (policy != BucketPolicy.None)
            {
                PutLifecycleConfigurationResponse putPolicyResponse = _coreApi.PutBucketLifecycleConfiguration(profileName, region, bucketName, policy);

                if (!putPolicyResponse.Success)
                {
                    _logger.LogResponseError(putPolicyResponse);
                }
            }

            SetInfoStatus(Strings.StatusBootstrapComplete);
            RefreshCurrentBucket();
        }

        private void OnBucketCreationFailure(Response response)
        {
            SetErrorStatus(Strings.StatusBootstrapFailedTemplate, response);
            _logger.LogResponseError(response);
        }
    }
}
