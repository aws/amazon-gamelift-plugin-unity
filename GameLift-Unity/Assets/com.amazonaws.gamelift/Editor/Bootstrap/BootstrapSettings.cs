// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int DefaultLifeCyclePolicyIndex = 0;
        private readonly Status _status = new Status();
        private readonly BucketUrlFormatter _bucketUrlFormatter = new BucketUrlFormatter();
        private readonly TextProvider _textProvider;
        private readonly IBucketNameFormatter _bucketFormatter;
        private readonly CoreApi _coreApi;
        private readonly BootstrapUtility _bootstrapUtility;
        private List<string> _existingBuckets = new List<string>();

        public IReadStatus Status => _status;

        public IReadOnlyList<string> ExistingBuckets => _existingBuckets;

        public string[] AllLifecyclePolicies { get; }

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
        public string BucketName { get; private set; }

        public int LifeCyclePolicyIndex { get; set; }

        public bool CanCreate => !string.IsNullOrEmpty(BucketName);

        public bool IsBucketListLoaded { get; private set; }

        public bool CanSaveSelectedBucket =>
            _existingBuckets.Count != 0
            && CurrentRegion != null
            && !string.IsNullOrEmpty(BucketName);

        public int SelectedMode { get; set; }

        public event Action<IReadOnlyList<string>> OnBucketsLoaded = default;

        public BootstrapSettings(IEnumerable<string> policyLifecycles, TextProvider textProvider,
            IBucketNameFormatter bucketFormatter, CoreApi coreApi = null,
            BootstrapUtility bootstrapUtility = null)
        {
            if (policyLifecycles is null)
            {
                throw new ArgumentNullException(nameof(policyLifecycles));
            }

            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _bucketFormatter = bucketFormatter ?? throw new ArgumentNullException(nameof(bucketFormatter));
            _coreApi = coreApi ?? CoreApi.SharedInstance;
            _bootstrapUtility = bootstrapUtility ?? BootstrapUtility.SharedInstance;
            LifeCyclePolicyIndex = DefaultLifeCyclePolicyIndex;
            AllLifecyclePolicies = policyLifecycles.ToArray();
        }

        public void SetUp()
        {
            RefreshExistingBuckets();
            SelectedMode = _existingBuckets.Count != 0 ? SelectionMode : CreationMode;
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
                OnBucketCreated(BucketName);
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
                Debug.LogError(DevStrings.OperationInvalid);
                return;
            }

            PutSettingResponse bucketNameResponse = _coreApi.PutSetting(SettingsKeys.CurrentBucketName, BucketName);

            if (!bucketNameResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, bucketNameResponse);
                bucketNameResponse.LogError(_textProvider);
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
        public void RefreshExistingBuckets()
        {
            IsBucketListLoaded = false;

            GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!profileResponse.Success)
            {
                SetErrorStatus(Strings.StatusGetProfileFailed);
                profileResponse.LogError(_textProvider);
                return;
            }

            GetSettingResponse regionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!regionResponse.Success || !_coreApi.IsValidRegion(regionResponse.Value))
            {
                SetErrorStatus(Strings.StatusGetRegionFailed);
                regionResponse.LogError(_textProvider);
                return;
            }

            GetBucketsResponse bucketsResponse = _coreApi.ListBuckets(profileResponse.Value, regionResponse.Value);

            if (!bucketsResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, bucketsResponse);
                bucketsResponse.LogError(_textProvider);
                return;
            }

            _existingBuckets = bucketsResponse.Buckets.ToList();
            IsBucketListLoaded = true;
            OnBucketsLoaded?.Invoke(_existingBuckets);
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
                currentRegionResponse.LogError(_textProvider);
                return;
            }

            CurrentRegion = currentRegionResponse.Value;
            GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!profileResponse.Success)
            {
                SetErrorStatus(Strings.StatusGetProfileFailed);
                profileResponse.LogError(_textProvider);
                return;
            }

            RetrieveAccountIdByCredentialsResponse accountIdResponse = _coreApi.RetrieveAccountId(profileResponse.Value);

            if (!accountIdResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, accountIdResponse);
                accountIdResponse.LogError(_textProvider);
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

        private void OnBucketCreated(string bucketName)
        {
            RefreshExistingBuckets();

            PutSettingResponse bucketNameResponse = _coreApi.PutSetting(SettingsKeys.CurrentBucketName, bucketName);

            if (!bucketNameResponse.Success)
            {
                OnBucketCreationFailure(bucketNameResponse);
                return;
            }

            SetInfoStatus(Strings.StatusBootstrapComplete);
            RefreshCurrentBucket();
        }

        private void OnBucketCreationFailure(Response response)
        {
            SetErrorStatus(Strings.StatusBootstrapFailedTemplate, response);
            response.LogError(_textProvider);
        }
    }
}
