// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using System.Threading;
using Editor.CoreAPI;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class UserProfileSelection
    {
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;
        private readonly VisualElement _container;
        private readonly AwsUserProfilesPage _awsUserProfilesPage;
        private readonly StateManager _stateManager;
        
        public UserProfileSelection(VisualElement container, StateManager stateManager, AwsUserProfilesPage profilesPage)
        {
            _container = container;
            _stateManager = stateManager;
            _awsUserProfilesPage = profilesPage;
            _bootstrapSettings = profilesPage.BootstrapSettings;
        }

        private void SelectBucket()
        {
            _refreshBucketsCancellation = new CancellationTokenSource();
            _ = _bootstrapSettings.RefreshExistingBuckets(_refreshBucketsCancellation.Token);
            if (_bootstrapSettings.BucketName != null)
            {
                SelectBucket(_bootstrapSettings.BucketName);
                _bootstrapSettings.SaveSelectedBucket();
            }
        }

        public void SelectBucket(string selectedBucket)
        {
            _bootstrapSettings.SelectBucket(selectedBucket);
            _bootstrapSettings.SaveSelectedBucket();
            _container.Q<Label>("S3BucketNameLabel").text = selectedBucket;
        }

        public void SelectProfile(bool isSetup)
        {
            _awsUserProfilesPage.RefreshProfiles();
            var allProfileSelectFields = _container.Query<DropdownField>(null,"AccountSelection").ToList();
            foreach (var profileSelectDropdownField in allProfileSelectFields)
            {
                if (isSetup)
                {
                    profileSelectDropdownField.RegisterValueChangedCallback(_ => { OnAccountSelect(profileSelectDropdownField.index); });
                }
                
                profileSelectDropdownField.choices = _awsUserProfilesPage.UpdateModel.AllProlfileNames.ToList();
                if (profileSelectDropdownField.choices.Contains("default"))
                {
                    profileSelectDropdownField.index = profileSelectDropdownField.choices.IndexOf(_stateManager.SelectedProfile is "default" or null ? "default" : _stateManager.SelectedProfile);
                }
            }
            
        }

        private void OnAccountSelect(int index)
        {
            var accountSelectLabels = _container.Query<Label>(null, "AccountSelectLabel").ToList();
            SelectBucket();
            foreach (var label in accountSelectLabels)
            {
                switch (label.name)
                {
                    case "S3BucketNameLabel":
                        label.text = _bootstrapSettings.BucketName ?? "No Bucket Created";
                        break;
                    case "Region":
                        if (_awsUserProfilesPage.UpdateModel.RegionBootstrap.RegionIndex >= 0)
                        {
                            label.text =
                                _awsUserProfilesPage.UpdateModel.RegionBootstrap.AllRegions[
                                    _awsUserProfilesPage.UpdateModel.RegionBootstrap.RegionIndex];
                        }
                        break;
                    case "BootstrapStatus":
                        label.text = _bootstrapSettings.BucketName != null ? "Active" : "Inactive";
                        break;
                }
            }

            UpdateModel(index);
        }

        private void UpdateModel(int index)
        {
            _awsUserProfilesPage.UpdateModel.SelectedProfileIndex = index;
            _awsUserProfilesPage.UpdateModel.Update();
            _stateManager.SelectedProfile = _awsUserProfilesPage.UpdateModel.AllProlfileNames[index];
            _stateManager.CoreApi.PutSetting(SettingsKeys.CurrentProfileName,
                _stateManager.SelectedProfile);
        }
    }
}