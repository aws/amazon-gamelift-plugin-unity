// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using Editor.GameLiftConfigurationUI;
using Editor.Window;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class AwsUserProfilesPage
    {
        public List<TextField> AccountDetailTextFields = new();
        
        public readonly AwsCredentialsUpdate UpdateModel;
        public readonly AwsCredentialsCreation CreationModel;
        public readonly BootstrapSettings BootstrapSettings;
        public readonly UserProfileSelection UserProfileSelection;
        
        private VisualElement _currentElement;
        
        private readonly GameLiftPlugin _gameLiftConfig;
        private readonly VisualElement _container;
        private readonly UserProfileCreation _userProfileCreation;

        public AwsUserProfilesPage(VisualElement container, GameLiftPlugin gameLiftConfig)
        {
            
            var awsCredentials = AwsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
            
            
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AwsUserProfilesPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _container.Add(uxml);
            ApplyText();

            _gameLiftConfig = gameLiftConfig;
            
            _userProfileCreation = new UserProfileCreation(_container, _gameLiftConfig, this);
            BootstrapSettings = _userProfileCreation.SetupBootstrap();
            UserProfileSelection = new UserProfileSelection(_container, _gameLiftConfig, this);
            
            SetupConfigSettings();
            RefreshProfiles();
            
            SetupTab();
            SetupButtonCallbacks();
        }
        
        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("UserProfilePageAccountCardNewAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            l.SetElementText("UserProfilePageAccountCardNewAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            l.SetElementText("UserProfilePageAccountCardNoAccountTitle", Strings.UserProfilePageAccountCardNoAccountTitle);
            l.SetElementText("UserProfilePageAccountCardNoAccountDescription", Strings.UserProfilePageAccountCardNoAccountDescription);
            l.SetElementText("UserProfilePageAccountCardNewAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            l.SetElementText("UserProfilePageAccountCardNewAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            l.SetElementText("UserProfilePageAccountNewProfileTitle", Strings.UserProfilePageAccountNewProfileTitle);
            l.SetElementText("UserProfilePageAccountNewProfileName", Strings.UserProfilePageAccountNewProfileName);
            l.SetElementText("UserProfilePageAccountNewProfileAccessKey", Strings.UserProfilePageAccountNewProfileAccessKey);
            l.SetElementText("UserProfilePageAccountNewProfileSecretKey", Strings.UserProfilePageAccountNewProfileSecretKey);
            l.SetElementText("UserProfilePageAccountNewProfileRegion", Strings.UserProfilePageAccountNewProfileRegion);
            l.SetElementText("UserProfilePageAccountNewProfileRegionPlaceholderDropdown", Strings.UserProfilePageAccountNewProfileRegionPlaceholderDropdown);
            l.SetElementText("UserProfilePageAccountCardNoAccountLink", Strings.UserProfilePageAccountCardNoAccountLink);
            l.SetElementText("UserProfilePageAccountNewProfileHelpLink", Strings.UserProfilePageAccountNewProfileHelpLink);
            l.SetElementText("UserProfilePageAccountCardNoAccountButton", Strings.UserProfilePageAccountCardNoAccountButton);
            l.SetElementText("UserProfilePageAccountCardHasAccountButton", Strings.UserProfilePageAccountCardHasAccountButton);
            l.SetElementText("UserProfilePageAccountNewProfileCreateButton", Strings.UserProfilePageAccountNewProfileCreateButton);
            l.SetElementText("UserProfilePageAccountNewProfileCancelButton", Strings.UserProfilePageAccountNewProfileCancelButton);
            l.SetElementText("UserProfilePageBootstrapTitle", Strings.UserProfilePageBootstrapTitle);
            l.SetElementText("UserProfilePageBootstrapDescription", Strings.UserProfilePageBootstrapDescription);
            l.SetElementText("UserProfilePageBootstrapPricingText", Strings.UserProfilePageBootstrapPricingText);
            l.SetElementText("UserProfilePageBootstrapProfileInputText", Strings.UserProfilePageBootstrapProfileInputText);
            l.SetElementText("UserProfilePageBootstrapBucketText", Strings.UserProfilePageBootstrapBucketText);
            l.SetElementText("UserProfilePageBootstrapBucketUnsetText", Strings.UserProfilePageBootstrapBucketUnsetText);
            l.SetElementText("LabelBootstrapRegion", Strings.LabelBootstrapRegion);
            l.SetElementText("UserProfilePageBootstrapStatusText", Strings.UserProfilePageBootstrapStatusText);
            l.SetElementText("UserProfilePageBootstrapWarningText", Strings.UserProfilePageBootstrapWarningText);
            l.SetElementText("UserProfilePageBootstrapProfilePlaceholderText", Strings.UserProfilePageBootstrapProfilePlaceholderText);
            l.SetElementText("UserProfilePageBootstrapPricingInfoText", Strings.UserProfilePageBootstrapPricingInfoText);
            l.SetElementText("UserProfilePageBootstrapPricingFreeTierText", Strings.UserProfilePageBootstrapPricingFreeTierText);
            l.SetElementText("UserProfilePageBootstrapHelpLink", Strings.UserProfilePageBootstrapHelpLink);
            l.SetElementText("UserProfilePageBootstrapStartButton", Strings.UserProfilePageBootstrapStartButton);
            l.SetElementText("UserProfilePageBootstrapAnotherProfileButton", Strings.UserProfilePageBootstrapAnotherProfileButton);
            l.SetElementText("UserProfilePageBootstrapAnotherBucketButton", Strings.UserProfilePageBootstrapAnotherBucketButton);
        }

        private void SetupButtonCallbacks()
        {
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
                BootstrapSettings.RefreshBucketName();
                OpenS3Popup(BootstrapSettings.BucketName);
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
            UserProfileSelection.AccountSelection(true);
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
            if (!_userProfileCreation.CreateModel())
            {
                return false;
            }
            UserProfileSelection.AccountSelection(false);
            Debug.Log("Saving Profile");
            return true;
        }

        private void ClearCredentials()
        {
            AccountDetailTextFields = _container.Query<TextField>(null, "AccountDetailsInput").ToList();
            foreach (var textField in AccountDetailTextFields)
            {
                textField.value = "";
            }
        }

        private void ToggleHiddenText(TextField hiddenField)
        {
            hiddenField.isPasswordField = !hiddenField.isPasswordField;
        }
        
        public void SetupWrapper()
        {
            var credentials = _gameLiftConfig.CoreApi.RetrieveAwsCredentials(_gameLiftConfig.CurrentState.SelectedProfile);
            var client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
            _gameLiftConfig.GameLiftWrapper = new AmazonGameLiftWrapper(client);
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
        
        public void RefreshProfiles()
        {
            UpdateModel.Refresh();
            _gameLiftConfig.CurrentState.AllProfiles = UpdateModel.AllProlfileNames;
        }
        
        private void BootstrapAccount (string bucketName)
        { 
            _userProfileCreation.BootstrapAccount(bucketName);
        }
        
        public void OpenS3Popup(string bucketName)
        {
            var popup = ScriptableObject.CreateInstance<GameLiftPluginBucketPopup>();
            popup.Init(bucketName);
            popup.OnConfirm += BootstrapAccount;
            popup.ShowModalUtility();
        }
    }
}