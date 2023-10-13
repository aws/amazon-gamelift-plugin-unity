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

        private readonly AwsCredentialsUpdate _awsCredentialsUpdateModel;
        private readonly BootstrapSettings _bootstrapSettings;
        
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
        private StatusBox _statusBox;

        public AwsUserProfilesPage(VisualElement container, StateManager stateManager)
        {
            var awsCredentials = AwsCredentialsFactory.Create(); 
            _awsCredentialsUpdateModel = awsCredentials.Update;
            
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
                _awsCredentialsUpdateModel.Refresh();
                _awsCredentialsUpdateModel.Update();
            };
            
            var createProfileContainer = _container.Q("UserProfilePageCreateMenu");
            _userProfileCreation = new UserProfileCreation(createProfileContainer, _stateManager);
            _userProfileCreation.OnProfileCreated += () =>
            {
                ShowProfileMenu(_bootstrapMenu);
            };
            _bootstrapSettings = BootstrapSettingsFactory.Create(_stateManager);
                
            RefreshProfiles();

            var regions = _stateManager.CoreApi.ListAvailableRegions().ToList();
            var filteredList = regions
                .Where(item => !item.Contains("cn-"))
                .ToList();
            
            container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown").choices = filteredList;
                
            
            if (!stateManager.IsBootstrapped)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.UserProfilePageStatusBoxWarningText);
            }
           
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
            l.SetElementText("UserProfilePageCompletedBootstrapHelpLink", Strings.UserProfilePageCompletedBootstrapHelpLink);
        }

        private void SetupButtonCallbacks()
        {
            _container.Q<Button>("UserProfilePageAccountCardNoAccountButton")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.CreateAwsAccountLearnMore));
            _container.Q<VisualElement>("UserProfilePageBootstrapHelpLink")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.S3BootstrapHelp));
            _container.Q<VisualElement>("UserProfilePageCompletedBootstrapHelpLink")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.S3BootstrapHelp));
            _container.Q<VisualElement>("UserProfilePageAccountNewProfileHelpLink")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsIamDocumentation));

            _container.Q<Button>("UserProfilePageAccountCardHasAccountButton").RegisterCallback<ClickEvent>(_ =>
            {
                var targetWizard = _container.Q<VisualElement>("UserProfilePageCreateMenu");
                ShowProfileMenu(targetWizard);
            });
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
                _bootstrapSettings.RefreshBucketName();
                OpenS3Popup(_bootstrapSettings.BucketName);
            });
            _container.Q<Button>("UserProfilePageBootstrapAnotherBucketButton").RegisterCallback<ClickEvent>(_ =>
            {
                _bootstrapSettings.RefreshBucketName();
                OpenS3Popup(_bootstrapSettings.BucketName);
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
            _awsCredentialsUpdateModel.Refresh();
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