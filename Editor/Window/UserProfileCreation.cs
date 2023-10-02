// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class UserProfileCreation
    {
        private readonly VisualElement _container;
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;
        private StateManager _stateManager;
        private AwsUserProfilesPage _profilesPage;


        public UserProfileCreation(VisualElement container, StateManager stateManager, AwsUserProfilesPage profilesPage)
        {
            _container = container;
            _stateManager = stateManager;
            _profilesPage = profilesPage;
        }

        public void BootstrapAccount(string bucketName)
        {
            var bucketResponse = CreateBucket(bucketName);
            if (bucketResponse.Success)
            {
                // TODO: Add success status box
                _stateManager.SetBucketBootstrap(bucketName);
            }
            else
            {
                // TODO: Add error status box
                Debug.Log(bucketResponse.ErrorMessage);
            }
        }

        public BootstrapSettings SetupBootstrap()
        {
            _bootstrapSettings = BootstrapSettingsFactory.Create();
            _refreshBucketsCancellation = new CancellationTokenSource();
            _bootstrapSettings.SetUp(_refreshBucketsCancellation.Token)
                .ContinueWith(_ => { _bootstrapSettings.RefreshCurrentBucket(); },
                    TaskContinuationOptions.ExecuteSynchronously);
            return _bootstrapSettings;
        }

        private Response CreateBucket(string bucketName)
        {
            _refreshBucketsCancellation?.Cancel();
            _bootstrapSettings.BucketName = bucketName;
            return _bootstrapSettings.CreateBucket();
        }

        public bool CreateUserProfile()
        {
            var dropdownField = _container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown");
            var credentials = _profilesPage.AccountDetailTextFields.Select(textField => textField.value).ToList();

            if (credentials.Any(credential => credential == ""))
            {
                return false;
            }

            _profilesPage.AwsCredentialsCreateModel.ProfileName = credentials[0];
            _profilesPage.AwsCredentialsCreateModel.AccessKeyId = credentials[1];
            _profilesPage.AwsCredentialsCreateModel.SecretKey = credentials[2];
            _profilesPage.AwsCredentialsCreateModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _profilesPage.AwsCredentialsCreateModel.Create();

            return true;
        }
    }
}