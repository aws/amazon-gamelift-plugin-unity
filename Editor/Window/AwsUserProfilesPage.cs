// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class AwsUserProfilesPage
    {
        public List<TextField> AccountDetailTextFields = new();
        
        public readonly AwsCredentialsUpdate AwsCredentialsUpdateModel;
        public readonly AwsCredentialsCreation AwsCredentialsCreateModel;
        public readonly BootstrapSettings BootstrapSettings;
        
        private VisualElement _currentElement;

        private const string hiddenClassName = "hidden";
        private readonly StateManager _stateManager;
        private readonly VisualElement _container;
        private readonly UserProfileCreation _userProfileCreation;
        private readonly List<VisualElement> _allMenus;
        private readonly VisualElement _noAccountMenu;
        private readonly VisualElement _createMenu;
        private readonly VisualElement _bootstrapMenu;
        private readonly VisualElement _completedMenu;

        public AwsUserProfilesPage(VisualElement container, StateManager stateManager)
        {
            var awsCredentials = new AwsCredentialsFactory().Create();
            AwsCredentialsCreateModel = awsCredentials.Creation;
            AwsCredentialsUpdateModel = awsCredentials.Update;
            
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AwsUserProfilesPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _container.Add(uxml);
            LocalizeText();
            
            _noAccountMenu =  _container.Q<VisualElement>("UserProfilePageNoAccountMenu");
            _createMenu = _container.Q<VisualElement>("UserProfilePageCreateMenu");
            _bootstrapMenu = _container.Q<VisualElement>("UserProfilePageBootstrapMenu");
            _completedMenu = _container.Q<VisualElement>("UserProfilePageCompletedMenu");
            _allMenus = new List<VisualElement>()
            {
                _noAccountMenu,
                _createMenu,
                _bootstrapMenu,
                _completedMenu,
            };
            AccountDetailTextFields = _createMenu.Query<TextField>().ToList();

            _stateManager = stateManager;
            AwsCredentialsCreateModel.OnCreated += () => _stateManager.SetProfile(AwsCredentialsCreateModel.ProfileName);
            _stateManager.OnUserProfileUpdated += () =>
            {
                AwsCredentialsUpdateModel.Refresh();
                AwsCredentialsUpdateModel.Update();
            };
            
            _userProfileCreation = new UserProfileCreation(_container, _stateManager, this);
            BootstrapSettings = _userProfileCreation.SetupBootstrap();
                
            RefreshProfiles();

            container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown").choices =
                _stateManager.CoreApi.ListAvailableRegions().ToList();
            
            ChooseProfileMenu();
            SetupButtonCallbacks();
        }
        
        private void LocalizeText()
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
            l.SetElementText("UserProfilePageAccountNewProfileAccessKeyInput", Strings.UserProfilePageAccountNewProfileAccessKeyInput);
            l.SetElementText("UserProfilePageAccountNewProfileSecretKeyLabel", Strings.UserProfilePageAccountNewProfileSecretKeyLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionLabel", Strings.UserProfilePageAccountNewProfileRegionLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionPlaceholderDropdown", Strings.UserProfilePageAccountNewProfileRegionPlaceholderDropdown);
            l.SetElementText("UserProfilePageAccountCardNoAccountLink", Strings.UserProfilePageAccountCardNoAccountLink);
            l.SetElementText("UserProfilePageAccountNewProfileHelpLink", Strings.UserProfilePageAccountNewProfileHelpLink);
            l.SetElementText("UserProfilePageAccountCardNoAccountButtonLabel", Strings.UserProfilePageAccountCardNoAccountButtonLabel);
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
            _container.Q<Button>("UserProfilePageAccountCardNoAccountButton").RegisterCallback<ClickEvent>(_ => OpenLink(""));
            _container.Q<Button>("UserProfilePageBootstrapAnotherProfileButton").RegisterCallback<ClickEvent>(_ =>
            {
                var targetWizard = _container.Q<VisualElement>("UserProfilePageCreateMenu");
                ShowProfileMenu(targetWizard);
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileCreateButton").RegisterCallback<ClickEvent>(_ =>
            {
                if (CreateUserProfile())
                {
                    var targetWizard = _container.Q<VisualElement>("UserProfilePageBootstrapMenu");
                    ShowProfileMenu(targetWizard);
                }
                else
                {
                    Debug.Log("Error");
                }
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileCancelButton").RegisterCallback<ClickEvent>(_ =>
            {
                ClearCredentials();
                ChooseProfileMenu();
            });
            _container.Q<Button>("UserProfilePageBootstrapStartButton").RegisterCallback<ClickEvent>(_ =>
            {
                BootstrapSettings.RefreshBucketName();
                OpenS3Popup(BootstrapSettings.BucketName);
            });
            _container.Q<Button>("UserProfilePageBootstrapAnotherBucketButton").RegisterCallback<ClickEvent>(_ =>
            {
                OpenS3Popup(_stateManager.BucketName);
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileAccessKeyToggleReveal").RegisterCallback<ClickEvent>(_ =>
            {
                var accessToggle = _container.Q<TextField>("UserProfilePageAccountNewProfileAccessKeyInput");
                ToggleHiddenText(accessToggle);
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileSecretKeyToggleReveal").RegisterCallback<ClickEvent>(_ =>
            {
                var secretToggle = _container.Q<TextField>("UserProfilePageAccountNewProfileSecretKeyInput");
                ToggleHiddenText(secretToggle);
            });
            _container.Q<Button>("UserProfilePageAccountAddNewProfileButton").RegisterCallback<ClickEvent>(_ =>
            {
                var targetWizard = _container.Q<VisualElement>("UserProfilePageCreateMenu");
                ShowProfileMenu(targetWizard);
            });
        }

        private void OpenLink(string url)
        {
            Application.OpenURL(url);
        }

        private void ChooseProfileMenu()
        {
            if (_stateManager.AllProfiles.Count == 0)
            {
                ShowProfileMenu(_noAccountMenu);
            }
            else if (_stateManager.SelectedProfile == null)
            {
                ShowProfileMenu(_createMenu);
            } 
            else if (_stateManager.IsBootstrapped)
            {
                ShowProfileMenu(_completedMenu);
            }
            else
            {
                ShowProfileMenu(_bootstrapMenu);
            }
        }

        private bool CreateUserProfile()
        {
            if (!_userProfileCreation.CreateUserProfile())
            {
                return false;
            }
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
        
        private void ShowProfileMenu(VisualElement targetMenu)
        {
            _allMenus.ForEach(menu => menu.AddToClassList(hiddenClassName));
            if (targetMenu != null)
            {
                targetMenu.RemoveFromClassList(hiddenClassName);
            }
        }
        
        public void RefreshProfiles()
        {
            AwsCredentialsUpdateModel.Refresh();
        }
        
        private void BootstrapAccount(string bucketName)
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