// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.GameLiftConfigurationUI;
using Editor.Window;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class AwsUserProfilesPage
    {
        private List<TextField> _accountDetailTextFields = new();
        private readonly GameLiftPlugin _gameLiftConfig;
        private readonly VisualElement _container;

        private VisualElement _currentElement;

        public AwsUserProfilesPage(VisualElement container, GameLiftPlugin gameLiftConfig)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AwsUserProfilesPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _container.Add(uxml);
            ApplyText();

            _gameLiftConfig = gameLiftConfig;
            SetupConfigSettings();
            SetupBootstrap();
            SetupTab();
            SetUpForSelectedMode();
            SetupButtonCallbacks();
        }
        
        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("UserProfilePageAccountCardNewAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            l.SetElementText("UserProfilePageAccountCardNewAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            l.SetElementText("UserProfilePageAccountCardNoAccountTitle", Strings.UserProfilePageAccountCardNoAccountTitle);
            l.SetElementText("LabelAccountCardNoAccountDescription", Strings.UserProfilePageAccountCardNoAccountDescription);
            l.SetElementText("LabelAccountHasAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            l.SetElementText("LabelAccountHasAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            l.SetElementText("LabelAccountNewProfileTitle", Strings.UserProfilePageAccountNewProfileTitle);
            l.SetElementText("LabelAccountNewProfileName", Strings.UserProfilePageAccountNewProfileName);
            l.SetElementText("LabelAccountNewProfileAccessKey", Strings.UserProfilePageAccountNewProfileAccessKey);
            l.SetElementText("LabelAccountNewProfileSecretKey", Strings.UserProfilePageAccountNewProfileSecretKey);
            l.SetElementText("LabelAccountNewProfileRegion", Strings.UserProfilePageAccountNewProfileRegion);
            l.SetElementText("DropdownAccountNewProfileRegionPlaceholder", Strings.UserProfilePageAccountNewProfileRegionPlaceholderDropdown);
            l.SetElementText("LinkAccountCardNoAccountDescription", Strings.UserProfilePageAccountCardNoAccountLink);
            l.SetElementText("LinkAccountNewProfileHelp", Strings.UserProfilePageAccountNewProfileHelpLink);
            l.SetElementText("ButtonAccountCardNoAccount", Strings.UserProfilePageAccountCardNoAccountButton);
            l.SetElementText("ButtonAccountCardHasAccount", Strings.UserProfilePageAccountCardHasAccountButton);
            l.SetElementText("ButtonAccountNewProfileCreate", Strings.UserProfilePageAccountNewProfileCreateButton);
            l.SetElementText("ButtonAccountNewProfileCancel", Strings.UserProfilePageAccountNewProfileCancelButton);
            l.SetElementText("LabelBootstrapTitle", Strings.UserProfilePageBootstrapTitle);
            l.SetElementText("LabelBootstrapDescription", Strings.UserProfilePageBootstrapDescription);
            l.SetElementText("LabelBootstrapPricing", Strings.UserProfilePageBootstrapPricingText);
            l.SetElementText("LabelBootstrapProfileInput", Strings.UserProfilePageBootstrapProfileInputText);
            l.SetElementText("LabelBootstrapBucket", Strings.UserProfilePageBootstrapBucketText);
            l.SetElementText("LabelBootstrapBucketUnset", Strings.UserProfilePageBootstrapBucketUnsetText);
            l.SetElementText("LabelBootstrapRegion", Strings.LabelBootstrapRegion);
            l.SetElementText("LabelBootstrapStatus", Strings.UserProfilePageBootstrapStatusText);
            l.SetElementText("LabelBootstrapWarning", Strings.UserProfilePageBootstrapWarningText);
            l.SetElementText("LabelBootstrapProfilePlaceholder", Strings.UserProfilePageBootstrapProfilePlaceholderText);
            l.SetElementText("LabelBootstrapPricingInfo", Strings.UserProfilePageBootstrapPricingInfoText);
            l.SetElementText("LabelBootstrapPricingFreeTier", Strings.UserProfilePageBootstrapPricingFreeTierText);
            l.SetElementText("LinkBootstrapHelp", Strings.UserProfilePageBootstrapHelpLink);
            l.SetElementText("ButtonBootstrapStart", Strings.UserProfilePageBootstrapStartButton);
            l.SetElementText("ButtonBootstrapAnotherProfile", Strings.UserProfilePageBootstrapAnotherProfileButton);
            l.SetElementText("ButtonBootstrapAnotherBucket", Strings.UserProfilePageBootstrapAnotherBucketButton);
        }

        private void SetupButtonCallbacks()
        {
            // _container.Q<Button>("LabelAccountCardNoAccountDescriptionLink").RegisterCallback<ClickEvent>(_ => OpenLink(""));
            // _container.Q<Button>("LabelAccountNewProfileHelpLink").RegisterCallback<ClickEvent>(_ => OpenLink(""));
            _container.Q<Button>("ButtonAccountCardNoAccount").RegisterCallback<ClickEvent>(_ => OpenLink(""));
            _container.Q<Button>("ButtonBootstrapAddAnotherProfile").RegisterCallback<ClickEvent>(_ =>
            {
                ChangeWizard(_container.Q<VisualElement>("AddNewProfile"));
            });
            _container.Q<Button>("CreateAccountProfile").RegisterCallback<ClickEvent>(_ =>
            {
                if (SaveProfile())
                {
                    var targetWizard = _container.Q<VisualElement>("BootstrapMenu");
                    ChangeWizard(targetWizard);
                }
                else
                {
                    Debug.Log("Error");
                }
            });
            _container.Q<Button>("Cancel").RegisterCallback<ClickEvent>(_ =>
            {
                ClearCredentials();
                SetupBootMenu();
            });
            _container.Q<Button>("BootstrapProfile").RegisterCallback<ClickEvent>(_ =>
            {
                _bootstrapSettings.RefreshBucketName();
                _gameLiftConfig.OpenS3Popup(_bootstrapSettings.BucketName);
            });
            _container.Q<Button>("AddProfile").RegisterCallback<ClickEvent>(_ =>
            {
                ChangeWizard(_container.Q<VisualElement>("AddNewProfile"));
            });
            _container.Q<Button>("BootstrapNewBucket").RegisterCallback<ClickEvent>(_ =>
            {
                // TODO: Add or find functionality for this
            });
            _container.Q<Button>("AccessKeyToggleReveal").RegisterCallback<ClickEvent>(_ =>
            {
                var accessToggle = _container.Q<TextField>("AccessKeyField");
                ToggleHiddenText(accessToggle);
            });
            _container.Q<Button>("SecretKeyToggleReveal").RegisterCallback<ClickEvent>(_ =>
            {
                var secretToggle = _container.Q<TextField>("SecretKeyField");
                ToggleHiddenText(secretToggle);
            });
        }

        private void OpenLink(string url)
        {
            Application.OpenURL(url);
        }

        private void SetupTab()
        {
            var dropdownField = _container.Q<DropdownField>(null, "AccountDetailsInput");
            dropdownField.index = 0;
            AccountSelection(true);
            SetupBootMenu();
        }

        private void SetupConfigSettings()
        {
            var selectedProfile = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.CurrentProfileName);
            _gameLiftConfig.CurrentState.SelectedProfile = selectedProfile.Success ? selectedProfile.Value : _gameLiftConfig.CurrentState.AllProfiles.First();
        }

        private void SetupBootMenu()
        {
            VisualElement targetWizard;
            var tab2WarningBox = _container.Q<VisualElement>(null, "Tab2Warning");
            var bootStrapMenu = _container.Q<VisualElement>("BootstrapMenu");
            var completedMenu = _container.Q<VisualElement>("CompletedProfile");
            switch (_gameLiftConfig.CurrentState.AllProfiles.Length)
            {
                case 0:
                    targetWizard = _container.Q<VisualElement>("Cards");
                    break;
                case 1:
                    if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                    {
                        tab2WarningBox.style.display = DisplayStyle.Flex;
                        //TODO Set SelectedProfile and change dropdown to "Choose Profile" and make all the below labels ---
                    }

                    targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                        ? bootStrapMenu
                        : completedMenu;
                    break;
                default:
                {
                    if (_gameLiftConfig.CurrentState.AllProfiles.Any(profile => profile == "default"))
                    {
                        targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                            ? bootStrapMenu
                            : completedMenu;
                    }
                    else
                    {
                        targetWizard = bootStrapMenu;
                        if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                        {
                            tab2WarningBox.style.display = DisplayStyle.Flex;
                        }
                    }

                    break;
                }
            }

            ChangeWizard(targetWizard);
        }

        private bool SaveProfile()
        {
            _accountDetailTextFields = _container.Query<TextField>(null, "AccountDetailsInput").ToList();
            var dropdownField = _container.Q<DropdownField>(null, "AccountDetailsInput");

            var credentials = _accountDetailTextFields.Select(textField => textField.value).ToList();

            if (credentials.Any(credential => credential == ""))
            {
                return false;
            }

            _gameLiftConfig.CreationModel.ProfileName = credentials[0];
            _gameLiftConfig.CreationModel.AccessKeyId = credentials[1];
            _gameLiftConfig.CreationModel.SecretKey = credentials[2];
            _gameLiftConfig.CreationModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _gameLiftConfig.CreationModel.Create();
            AccountSelection(false);
            Debug.Log("Saving Profile");
            return true;
        }

        private void ClearCredentials()
        {
            _accountDetailTextFields = _container.Query<TextField>(null, "AccountDetailsInput").ToList();
            foreach (var textField in _accountDetailTextFields)
            {
                textField.value = "";
            }
        }

        private void AccountSelection(bool isSetup)
        {
            _gameLiftConfig.RefreshProfiles();
            var accountSelectFields = _container.Query<DropdownField>(null, "AccountSelection").ToList();
            foreach (var accountSelect in accountSelectFields)
            {
                if (isSetup)
                {
                    accountSelect.RegisterValueChangedCallback(_ => { OnAccountSelect(accountSelect.index); });
                }

                accountSelect.choices = _gameLiftConfig.CurrentState.AllProfiles.ToList();
                if (accountSelect.choices.Contains("default"))
                {
                    accountSelect.index = accountSelect.choices.IndexOf(_gameLiftConfig.CurrentState.SelectedProfile is "default" or null ? "default" : _gameLiftConfig.CurrentState.SelectedProfile);
                }
            }
        }

        private void OnAccountSelect(int index)
        {
            var accountSelectLabels = _container.Query<Label>(null, "AccountSelectLabel").ToList();
            BucketSelection();
            foreach (var label in accountSelectLabels)
            {
                switch (label.name)
                {
                    case "S3BucketNameLabel":
                        label.text = _bootstrapSettings.BucketName ?? "No Bucket Created";
                        break;
                    case "Region":
                        if (_gameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex >= 0)
                        {
                            label.text =
                                _gameLiftConfig.UpdateModel.RegionBootstrap.AllRegions[
                                    _gameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex];
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
            _gameLiftConfig.UpdateModel.SelectedProfileIndex = index;
            _gameLiftConfig.SetupWrapper();
            _gameLiftConfig.UpdateModel.Update();
            _gameLiftConfig.CurrentState.SelectedProfile = _gameLiftConfig.UpdateModel.AllProlfileNames[index];
            _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.CurrentProfileName,
                _gameLiftConfig.CurrentState.SelectedProfile);
        }

        private void ToggleHiddenText(TextField hiddenField)
        {
            hiddenField.isPasswordField = !hiddenField.isPasswordField;
        }

        private CancellationTokenSource _refreshBucketsCancellation;
        private BootstrapSettings _bootstrapSettings;

        public void BootstrapAccount(string bucketName)
        {
            var bucketResponse = CreateBucket(bucketName);
            if (bucketResponse.Success)
            {
                _container.Q<VisualElement>(null, "Tab2Success").style.display = DisplayStyle.Flex;
                _gameLiftConfig.CurrentState.SelectedBootstrapped = true;
                BucketSelection(bucketName);
            }
            else
            {
                _gameLiftConfig.CurrentState.SelectedBootstrapped = false;
                var errorBox = _container.Q<VisualElement>("Tab2Error");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = bucketResponse.ErrorMessage;
                Debug.Log(bucketResponse.ErrorMessage);
            }
        }

        private void SetupBootstrap()
        {
            _bootstrapSettings = BootstrapSettingsFactory.Create();
            _refreshBucketsCancellation = new CancellationTokenSource();
            _bootstrapSettings.SetUp(_refreshBucketsCancellation.Token)
                .ContinueWith(_ => { _bootstrapSettings.RefreshCurrentBucket(); },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        private void SetUpForSelectedMode()
        {
            _bootstrapSettings.RefreshCurrentBucket();
        }

        private Response CreateBucket(string bucketName)
        {
            _refreshBucketsCancellation?.Cancel();
            _bootstrapSettings.BucketName = bucketName;
            _bootstrapSettings.LifeCyclePolicyIndex = 0;
            return _bootstrapSettings.CreateBucket();
        }

        private void BucketSelection()
        {
            _refreshBucketsCancellation = new CancellationTokenSource();
            _ = _bootstrapSettings.RefreshExistingBuckets(_refreshBucketsCancellation.Token);
            if (_bootstrapSettings.BucketName != null)
            {
                BucketSelection(_bootstrapSettings.BucketName);
                _bootstrapSettings.SaveSelectedBucket();
            }
        }

        private void BucketSelection(string selectedBucket)
        {
            _bootstrapSettings.SelectBucket(selectedBucket);
            _bootstrapSettings.SaveSelectedBucket();
            _container.Q<Label>("S3BucketNameLabel").text = selectedBucket;
        }

        private void ChangeWizard(VisualElement targetWizard)
        {
            if (_currentElement != null)
            {
                _currentElement.style.display = DisplayStyle.None;
            }

            _currentElement = targetWizard;
            if (_currentElement != null)
            {
                _currentElement.style.display = DisplayStyle.Flex;
            }
        }
    }
}