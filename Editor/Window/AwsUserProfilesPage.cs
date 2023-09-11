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
            l.SetElementText("LabelAccountCardNewAccountTitle", Strings.LabelAccountCardNewAccountTitle);
            l.SetElementText("LabelAccountCardNewAccountDescription", Strings.LabelAccountCardNewAccountDescription);
            l.SetElementText("LabelAccountCardNoAccountTitle", Strings.LabelAccountCardNoAccountTitle);
            l.SetElementText("LabelAccountCardNoAccountDescription", Strings.LabelAccountCardNoAccountDescription);
            l.SetElementText("LabelAccountHasAccountTitle", Strings.LabelAccountCardNewAccountTitle);
            l.SetElementText("LabelAccountHasAccountDescription", Strings.LabelAccountCardNewAccountDescription);
            l.SetElementText("LabelAccountNewProfileTitle", Strings.LabelAccountNewProfileTitle);
            l.SetElementText("LabelAccountNewProfileName", Strings.LabelAccountNewProfileName);
            l.SetElementText("LabelAccountNewProfileAccessKey", Strings.LabelAccountNewProfileAccessKey);
            l.SetElementText("LabelAccountNewProfileSecretKey", Strings.LabelAccountNewProfileSecretKey);
            l.SetElementText("LabelAccountNewProfileRegion", Strings.LabelAccountNewProfileRegion);
            l.SetElementText("DropdownAccountNewProfileRegionPlaceholder", Strings.DropdownAccountNewProfileRegionPlaceholder);
            l.SetElementText("LinkAccountCardNoAccountDescription", Strings.LinkAccountCardNoAccountDescription);
            l.SetElementText("LinkAccountNewProfileHelp", Strings.LinkAccountNewProfileHelp);
            l.SetElementText("ButtonAccountCardNoAccount", Strings.ButtonAccountCardNoAccount);
            l.SetElementText("ButtonAccountCardHasAccount", Strings.ButtonAccountCardHasAccount);
            l.SetElementText("ButtonAccountNewProfileCreate", Strings.ButtonAccountNewProfileCreate);
            l.SetElementText("ButtonAccountNewProfileCancel", Strings.ButtonAccountNewProfileCancel);
            l.SetElementText("LabelBootstrapTitle", Strings.LabelBootstrapTitle);
            l.SetElementText("LabelBootstrapDescription", Strings.LabelBootstrapDescription);
            l.SetElementText("LabelBootstrapPricing", Strings.LabelBootstrapPricing);
            l.SetElementText("LabelBootstrapProfileInput", Strings.LabelBootstrapProfileInput);
            l.SetElementText("LabelBootstrapBucket", Strings.LabelBootstrapBucket);
            l.SetElementText("LabelBootstrapBucketUnset", Strings.LabelBootstrapBucketUnset);
            l.SetElementText("LabelBootstrapRegion", Strings.LabelBootstrapRegion);
            l.SetElementText("LabelBootstrapStatus", Strings.LabelBootstrapStatus);
            l.SetElementText("LabelBootstrapWarning", Strings.LabelBootstrapWarning);
            l.SetElementText("LabelBootstrapProfilePlaceholder", Strings.LabelBootstrapProfilePlaceholder);
            l.SetElementText("LabelBootstrapPricingInfo", Strings.LabelBootstrapPricingInfo);
            l.SetElementText("LabelBootstrapPricingFreeTier", Strings.LabelBootstrapPricingFreeTier);
            l.SetElementText("LinkBootstrapHelp", Strings.LinkBootstrapHelp);
            l.SetElementText("ButtonBootstrapStart", Strings.ButtonBootstrapStart);
            l.SetElementText("ButtonBootstrapAnotherProfile", Strings.ButtonBootstrapAnotherProfile);
            l.SetElementText("ButtonBootstrapAnotherBucket", Strings.ButtonBootstrapAnotherBucket);
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
            switch (_gameLiftConfig.CurrentState.AllProfiles.Length)
            {
                case 0:
                    targetWizard = _container.Q<VisualElement>("Cards");
                    break;
                case 1:
                    if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                    {
                        _container.Q<VisualElement>(null, "Tab2Warning").style.display = DisplayStyle.Flex;
                        //TODO Set SelectedProfile and change dropdown to "Choose Profile" and make all the below labels ---
                    }

                    targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                        ? _container.Q<VisualElement>("BootstrapMenu")
                        : _container.Q<VisualElement>("CompletedProfile");
                    break;
                default:
                {
                    if (_gameLiftConfig.CurrentState.AllProfiles.Any(profile => profile == "default"))
                    {
                        targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                            ? _container.Q<VisualElement>("BootstrapMenu")
                            : _container.Q<VisualElement>("CompletedProfile");
                    }
                    else
                    {
                        targetWizard = _container.Q<VisualElement>("BootstrapMenu");
                        if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                        {
                            _container.Q<VisualElement>(null, "Tab2Warning").style.display = DisplayStyle.Flex;
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
            var bucketResponse = BucketCreation(bucketName);
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

        private Response BucketCreation(string bucketName)
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