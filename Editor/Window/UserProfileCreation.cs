// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class UserProfileCreation
    {
        private readonly VisualElement _container;
        private readonly Button _createProfileButton;
        private readonly DropdownField _regionDropDown;
        private readonly AwsCredentialsCreation _awsCredentialsCreateModel;
        private readonly List<TextField> _textFields;
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;

        public Action OnProfileCreated;

        public UserProfileCreation(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/UserProfileCreation");
            container.Add(uxml.Instantiate());

            _container = container;
            _textFields = _container.Query<TextField>().ToList();
            
            _awsCredentialsCreateModel = AwsCredentialsFactory.Create().Creation;
            _awsCredentialsCreateModel.OnCreated += () =>
            {
                stateManager.SetProfile(_awsCredentialsCreateModel.ProfileName);
                OnProfileCreated?.Invoke();
            };
            
            _createProfileButton = _container.Q<Button>("UserProfilePageAccountNewProfileCreateButton");
            _regionDropDown = _container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown");
            
            SetupCallBacks();
            ValidateInputs();
            LocalizeText();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("UserProfilePageAccountNewProfileTitle", Strings.UserProfilePageAccountNewProfileTitle);
            l.SetElementText("UserProfilePageAccountNewProfileName", Strings.UserProfilePageAccountNewProfileName);
            l.SetElementText("UserProfilePageAccountNewProfileAccessKeyInput", Strings.UserProfilePageAccountNewProfileAccessKeyInput);
            l.SetElementText("UserProfilePageAccountNewProfileSecretKeyLabel", Strings.UserProfilePageAccountNewProfileSecretKeyLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionLabel", Strings.UserProfilePageAccountNewProfileRegionLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionPlaceholderDropdown", Strings.UserProfilePageAccountNewProfileRegionPlaceholderDropdown);
            l.SetElementText("UserProfilePageAccountNewProfileHelpLink", Strings.UserProfilePageAccountNewProfileHelpLink);
            l.SetElementText("UserProfilePageAccountNewProfileCreateButton", Strings.UserProfilePageAccountNewProfileCreateButton);
            l.SetElementText("UserProfilePageAccountNewProfileCancelButton", Strings.UserProfilePageAccountNewProfileCancelButton);
        }

        public void Reset()
        {
            foreach (var field in _textFields)
            {
                field.value = "";
            }
        }

        private bool CreateUserProfile()
        {
            var dropdownField = _container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown");
            var credentials = _container.Query<TextField>().ToList().Select(textField => textField.value).ToList();

            if (credentials.Any(string.IsNullOrWhiteSpace))
            {
                return false;
            }

            _awsCredentialsCreateModel.ProfileName = credentials[0];
            _awsCredentialsCreateModel.AccessKeyId = credentials[1];
            _awsCredentialsCreateModel.SecretKey = credentials[2];
            _awsCredentialsCreateModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _awsCredentialsCreateModel.Create();

            return true;
        }
        

        private void SetupCallBacks()
        {
            _container.Q<TextField>("UserProfilePageAccountNewProfileNameInput").RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            _container.Q<TextField>("UserProfilePageAccountNewProfileAccessKeyInput").RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            _container.Q<TextField>("UserProfilePageAccountNewProfileSecretKeyInput").RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            _regionDropDown.RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });

            _createProfileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var result = CreateUserProfile();
                if (result)
                {
                    OnProfileCreated?.Invoke();
                }
            });
        }

        private void ValidateInputs()
        {
            var anyEmptyFields = _textFields.Any(target => string.IsNullOrWhiteSpace(target.value));
            var isRegionSelected = _regionDropDown.index >= 0 ;

            _createProfileButton.SetEnabled(!anyEmptyFields && isRegionSelected);
        }
    }
}
