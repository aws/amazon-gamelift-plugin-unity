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
        private StatusBox _statusBox;
        private ElementLocalizer _elementLocalizer;

        public AwsUserProfilesPage(VisualElement container, StateManager stateManager)
        {
            var awsCredentials = new AwsCredentialsFactory().Create();
           AwsCredentialsUpdateModel = awsCredentials.Update;
            
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AwsUserProfilesPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _container.Add(uxml);
            SetupStatusBoxes();
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
            
            if (!stateManager.IsBootstrapped)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.UserProfilePageStatusBoxWarningText);
            }
           
            ChooseProfileMenu();
            SetupButtonCallbacks();
        }
        
        private void LocalizeText()
        {
            _elementLocalizer = new ElementLocalizer(_container);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNewAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNewAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNoAccountTitle", Strings.UserProfilePageAccountCardNoAccountTitle);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNoAccountDescription", Strings.UserProfilePageAccountCardNoAccountDescription);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNewAccountTitle", Strings.UserProfilePageAccountCardNewAccountTitle);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNewAccountDescription", Strings.UserProfilePageAccountCardNewAccountDescription);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNoAccountLink", Strings.UserProfilePageAccountCardNoAccountLink);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardNoAccountButtonLabel", Strings.UserProfilePageAccountCardNoAccountButtonLabel);
            _elementLocalizer.SetElementText("UserProfilePageAccountCardHasAccountButton", Strings.UserProfilePageAccountCardHasAccountButton);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapTitle", Strings.UserProfilePageBootstrapTitle);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapDescription", Strings.UserProfilePageBootstrapDescription);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapPricingText", Strings.UserProfilePageBootstrapPricingText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapProfileInputText", Strings.UserProfilePageBootstrapProfileInputText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapBucketText", Strings.UserProfilePageBootstrapBucketText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapBucketUnsetText", Strings.UserProfilePageBootstrapBucketUnsetText);
            _elementLocalizer.SetElementText("LabelBootstrapRegion", Strings.LabelBootstrapRegion);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapStatusText", Strings.UserProfilePageBootstrapStatusText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapWarningText", Strings.UserProfilePageBootstrapWarningText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapProfilePlaceholderText", Strings.UserProfilePageBootstrapProfilePlaceholderText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapPricingInfoText", Strings.UserProfilePageBootstrapPricingInfoText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapPricingFreeTierText", Strings.UserProfilePageBootstrapPricingFreeTierText);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapHelpLink", Strings.UserProfilePageBootstrapHelpLink);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapStartButton", Strings.UserProfilePageBootstrapStartButton);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapAnotherProfileButton", Strings.UserProfilePageBootstrapAnotherProfileButton);
            _elementLocalizer.SetElementText("UserProfilePageBootstrapAnotherBucketButton", Strings.UserProfilePageBootstrapAnotherBucketButton);
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
                _statusBox.Show(StatusBox.StatusBoxType.Success, Strings.UserProfilePageStatusBoxSuccessText);
                _stateManager.SetBucketBootstrap(bucketName);
            }
            else
            {
                _statusBox.Show(StatusBox.StatusBoxType.Error, Strings.UserProfilePageStatusBoxErrorText, bucketResponse.ErrorMessage, string.Format(Urls.AwsGameLiftLogs, _stateManager.Region), Strings.ViewLogsStatusBoxUrlTextButton);
            }
        }

        private void OpenS3Popup(string bucketName)
        {
            var popup = ScriptableObject.CreateInstance<GameLiftPluginBucketPopup>();
            popup.Init(bucketName);
            popup.OnConfirm += BootstrapAccount;
            popup.ShowModalUtility();
        }
        
        private void SetupStatusBoxes()
        {
            _statusBox = new StatusBox();
            var statusBoxContainer = _container.Q("UserProfilePageStatusBoxContainer");
            statusBoxContainer.Add(_statusBox);
        }
    }
}