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
        private List<TextField> AccountDetailTextFields = new();

        private readonly AwsCredentialsUpdate AwsCredentialsUpdateModel;
        private readonly BootstrapSettings BootstrapSettings;
        
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
        private BootstrapSettings _bootstrapSettings;

        public AwsUserProfilesPage(VisualElement container, StateManager stateManager)
        {
            var awsCredentials = new AwsCredentialsFactory().Create();
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
            _stateManager.OnUserProfileUpdated += () =>
            {
                AwsCredentialsUpdateModel.Refresh();
                AwsCredentialsUpdateModel.Update();
            };

            var createProfileContainer = _container.Q("UserProfilePageCreateMenu");
            _userProfileCreation = new UserProfileCreation(createProfileContainer, _stateManager);
            _userProfileCreation.OnProfileCreated += () =>
            {
                ShowProfileMenu(_bootstrapMenu);
            };
            BootstrapSettings = BootstrapSettingsFactory.Create();
                
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
            l.SetElementText("UserProfilePageAccountCardNoAccountLink", Strings.UserProfilePageAccountCardNoAccountLink);
            l.SetElementText("UserProfilePageAccountCardNoAccountButtonLabel", Strings.UserProfilePageAccountCardNoAccountButtonLabel);
            l.SetElementText("UserProfilePageAccountCardHasAccountButton", Strings.UserProfilePageAccountCardHasAccountButton);
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
            _container.Q<Button>("UserProfilePageAccountCardNoAccountButton").RegisterCallback<ClickEvent>(_ => OpenLink("")); // TODO: Investigate correct URL
            _container.Q<Button>("UserProfilePageBootstrapAnotherProfileButton").RegisterCallback<ClickEvent>(_ =>
            {
                var targetWizard = _container.Q<VisualElement>("UserProfilePageCreateMenu");
                ShowProfileMenu(targetWizard);
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileCancelButton").RegisterCallback<ClickEvent>(_ =>
            {
                _userProfileCreation.Reset();
                ChooseProfileMenu();
            });
            _container.Q<Button>("UserProfilePageBootstrapStartButton").RegisterCallback<ClickEvent>(_ =>
            {
                BootstrapSettings.RefreshBucketName();
                OpenS3Popup(_stateManager.BucketName);
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

        private void RefreshProfiles()
        {
            AwsCredentialsUpdateModel.Refresh();
        }
        
        private void BootstrapAccount(string bucketName)
        { 
            var bucketResponse = _bootstrapSettings.CreateBucket(bucketName);
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

        private void OpenS3Popup(string bucketName)
        {
            var popup = ScriptableObject.CreateInstance<GameLiftPluginBucketPopup>();
            popup.Init(bucketName);
            popup.OnConfirm += BootstrapAccount;
            popup.ShowModalUtility();
        }
    }
}