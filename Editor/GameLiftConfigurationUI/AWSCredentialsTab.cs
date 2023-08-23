// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftConfigurationUI
{
    public class AwsCredentialsTab : Tab
    {
        private List<TextField> _accountDetailTextFields = new();
        private readonly GameLiftPlugin _gameLiftConfig;

        public AwsCredentialsTab(VisualElement root, GameLiftPlugin gameLiftConfig)
        {
            _gameLiftConfig = gameLiftConfig;
            Root = root;
            SetupConfigSettings();
            SetupBootstrap();
            SetupTab();
            SetUpForSelectedMode();
            SetupAccountSection();
        }

        private void SetupAccountSection()
        {
            SetLabelText(Strings.LabelAccountTitle);
            SetLabelText(Strings.LabelAccountDescription);
            SetLabelText(Strings.LabelAccountCardNoAccountTitle);
            SetLabelText(Strings.LabelAccountCardNoAccountDescription);
            SetLabelText(Strings.LabelAccountHasAccountTitle);
            SetLabelText(Strings.LabelAccountHasAccountDescription);
            SetLabelText(Strings.LabelAccountNewProfileTitle);
            SetLabelText(Strings.LabelAccountNewProfileName);
            SetLabelText(Strings.LabelAccountNewProfileAccessKey);
            SetLabelText(Strings.LabelAccountNewProfileSecretKey);
            SetLabelText(Strings.LabelAccountNewProfileRegion);
            SetLabelText(Strings.LabelAccountNewProfileRegionPlaceholder);
            SetLabelText(Strings.LabelAccountNewProfileSecretKey);
            SetLabelText(Strings.LabelAccountNewProfileSecretKey);

            SetLabelTextAndLink(Strings.LabelAccountCardNoAccountDescriptionLink, "");
            SetLabelTextAndLink(Strings.LabelAccountNewProfileHelpLink, "");

            SetButtonLabelAndAction(Strings.ButtonAccountCardNoAccount,
                _ => { Application.OpenURL(""); }); // TODO: Get final url for this
            SetButtonLabelAndAction(Strings.ButtonAccountCardHasAccount,
                _ => { ChangeWizard(GetWizard("AddNewProfile")); });
            SetButtonLabelAndAction(Strings.ButtonAccountNewProfileCreate, _ =>
            {
                if (SaveProfile())
                {
                    var targetWizard = GetWizard("BootstrapMenu");
                    ChangeWizard(targetWizard);
                }
                else
                {
                    Debug.Log("Error");
                }
            });
            SetButtonLabelAndAction(Strings.ButtonAccountNewProfileCancel, _ =>
            {
                ClearCredentials();
                SetupBootMenu();
            });
        }

        private void SetupTab()
        {
            const string tabName = "Tab2";
            base.SetupTab(tabName, OnTabButtonClicked);
            var dropdownField = Root.Q<DropdownField>(null, "AccountDetailsInput");
            dropdownField.index = 0;
            AccountSelection(true);
            SetupBootMenu();
        }

        private void SetupConfigSettings()
        {
            var selectedProfile = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.SelectedProfile);
            if (selectedProfile.Success)
            {
                _gameLiftConfig.CurrentState.SelectedProfile = selectedProfile.Value;
            }
            else
            {
                _gameLiftConfig.CurrentState.SelectedProfile = _gameLiftConfig.CurrentState.AllProfiles.First();
            }
        }

        private void SetupBootMenu()
        {
            VisualElement targetWizard;
            switch (_gameLiftConfig.CurrentState.AllProfiles.Length)
            {
                case 0:
                    targetWizard = GetWizard("Cards");
                    break;
                case 1:
                    if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                    {
                        EnableInfoBox("Tab2Warning");
                        //TODO Set SelectedProfile and change dropdown to "Choose Profile" and make all the below labels ---
                    }

                    targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                        ? GetWizard("BootstrapMenu")
                        : GetWizard("CompletedProfile");
                    break;
                default:
                {
                    if (_gameLiftConfig.CurrentState.AllProfiles.Any(profile => profile == "default"))
                    {
                        targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false
                            ? GetWizard("BootstrapMenu")
                            : GetWizard("CompletedProfile");
                    }
                    else
                    {
                        targetWizard = GetWizard("BootstrapMenu");
                        if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                        {
                            EnableInfoBox("Tab2Warning");
                        }
                    }

                    break;
                }
            }

            ChangeWizard(targetWizard);
        }

        private bool SaveProfile()
        {
            _accountDetailTextFields = Root.Query<TextField>(null, "AccountDetailsInput").ToList();
            var dropdownField = Root.Q<DropdownField>(null, "AccountDetailsInput");

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
            _accountDetailTextFields = Root.Query<TextField>(null, "AccountDetailsInput").ToList();
            foreach (var textField in _accountDetailTextFields)
            {
                textField.value = "";
            }
        }

        private void AccountSelection(bool isSetup)
        {
            _gameLiftConfig.RefreshProfiles();
            var accountSelectFields = Root.Query<DropdownField>(null, "AccountSelection").ToList();
            foreach (var accountSelect in accountSelectFields)
            {
                if (isSetup)
                {
                    accountSelect.RegisterValueChangedCallback(_ => { OnAccountSelect(accountSelect.index); });
                }

                accountSelect.choices = _gameLiftConfig.CurrentState.AllProfiles.ToList();
                if (accountSelect.choices.Contains("default"))
                {
                    if (_gameLiftConfig.CurrentState.SelectedProfile is "default" or null)
                    {
                        accountSelect.index = accountSelect.choices.IndexOf("default");
                    }
                    else
                    {
                        accountSelect.index =
                            accountSelect.choices.IndexOf(_gameLiftConfig.CurrentState.SelectedProfile);
                    }
                }
            }
        }

        private void OnAccountSelect(int index)
        {
            var accountSelectLabels = Root.Query<Label>(null, "AccountSelectLabel").ToList();
            BucketSelection();
            foreach (var label in accountSelectLabels)
            {
                switch (label.name)
                {
                    case "Bucket":
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
            _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.SelectedProfile,
                _gameLiftConfig.CurrentState.SelectedProfile);
        }

        private void OnTabButtonClicked(ClickEvent evt, Button button)
        {
            switch (button.name)
            {
                case "CreateProfile":
                {
                    var targetWizard = GetWizard("AddNewProfile");
                    ChangeWizard(targetWizard);
                    break;
                }
                case "Cancel":
                {
                    ClearCredentials();
                    SetupBootMenu();
                    break;
                }
                case "CreateAccountProfile":
                {
                    if (SaveProfile())
                    {
                        var targetWizard = GetWizard("BootstrapMenu");
                        ChangeWizard(targetWizard);
                    }
                    else
                    {
                        Debug.Log("Error");
                    }

                    break;
                }
                case "BootstrapProfile":
                {
                    _bootstrapSettings.RefreshBucketName();
                    _gameLiftConfig.OpenS3Popup(_bootstrapSettings.BucketName);
                    break;
                }
                case "AccessKeyToggleReveal":
                {
                    var accessToggle = Root.Q<TextField>("AccessKeyField");
                    ToggleHiddenText(accessToggle);
                    break;
                }
                case "SecretKeyToggleReveal":
                {
                    var secretToggle = Root.Q<TextField>("SecretKeyField");
                    ToggleHiddenText(secretToggle);
                    break;
                }
            }
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
                EnableInfoBox("Tab2Success");
                _gameLiftConfig.CurrentState.SelectedBootstrapped = true;
            }
            else
            {
                _gameLiftConfig.CurrentState.SelectedBootstrapped = false;
                var errorBox = Root.Q<VisualElement>("Tab2Error");
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

            SetLabelText(Strings.LabelBootstrapTitle);
            SetLabelText(Strings.LabelBootstrapDescription);
            SetLabelText(Strings.LabelBootstrapPricing);
            SetLabelText(Strings.LabelBootstrapProfileInput);
            SetLabelText(Strings.LabelBootstrapBucket);
            SetLabelText(Strings.LabelBootstrapBucketUnset);
            SetLabelText(Strings.LabelBootstrapRegion);
            SetLabelText(Strings.LabelBootstrapStatus);
            SetLabelText(Strings.LabelBootstrapWarning);
            SetLabelText(Strings.LabelBootstrapProfilePlaceholder);

            SetLabelTextAndLink(Strings.LabelBootstrapPricingInfo, Urls.GameLiftBootstrapInformation);
            SetLabelTextAndLink(Strings.LabelBootstrapPricingFreeTier, Urls.AwsFreeTier);
            SetLabelTextAndLink(Strings.LabelBootstrapHelpLink, Urls.GameLiftBootstrapInformation);

            SetButtonLabelAndAction(Strings.ButtonBootstrapStart, _ =>
            {
                // _bootstrapSettings.RefreshBucketName();
                _gameLiftConfig.OpenS3Popup(_bootstrapSettings.BucketName);
            });
            SetButtonLabelAndAction(Strings.ButtonBootstrapAnotherProfile,
                _ => { ChangeWizard(GetWizard("AddNewProfile")); });
            SetButtonLabelAndAction(Strings.ButtonBootstrapAnotherBucket,
                _ => { }); // TODO: Add or find functionality for this
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
        }

        public override void OnAccountSelect()
        {
        }
    }
}