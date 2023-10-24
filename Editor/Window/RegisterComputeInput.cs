// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class RegisterComputeInput : StatefulInput
    {
        private ComputeStatus _computeState;

        private readonly List<TextField> _ipInputs;
        private readonly TextField _computeNameInput;
        private readonly Button _registerButton;
        private readonly Button _replaceComputeButton;
        private readonly Button _cancelReplaceButton;
        private readonly VisualElement _computeStatus;
        private readonly VisualElement _container;
        private readonly StatusIndicator _statusIndicator;
        private readonly StateManager _stateManager;
        private StatusBox _registerComputeStatusBox;

        private readonly string _defaultComputeName = "ComputerName-ProfileName";
        private readonly string _defaultIpAddress = "127.0.0.1";
        private string _computeName;
        private string _ipAddress;

        public RegisterComputeInput(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/RegisterComputeInput");
            container.Add(uxml.Instantiate());
            _container = container;

            _stateManager = stateManager;
            _computeNameInput = container.Q<TextField>("AnywherePageComputeNameInput");
            _ipInputs = container.Query<TextField>("AnywherePageComputeIPAddressInput").ToList();
            _computeStatus = container.Q("AnywherePageComputeStatus");
            _registerButton = container.Q<Button>("AnywherePageComputeRegisterButton");
            _replaceComputeButton = container.Q<Button>("AnywherePageComputeReplaceComputeButton");
            _cancelReplaceButton = container.Q<Button>("AnywherePageComputeCancelReplaceButton");
            _statusIndicator = container.Q<StatusIndicator>();
            LocalizeText();

            _computeState = !string.IsNullOrWhiteSpace(_stateManager.ComputeName) &&
                            !string.IsNullOrWhiteSpace(_stateManager.IpAddress)
                ? ComputeStatus.Registered
                : ComputeStatus.NotRegistered;

            _stateManager.OnUserProfileUpdated += UpdateGUI;

            SetupConfigSettings();
            RegisterCallbacks();
            SetupStatusBox();

            _stateManager.OnFleetChanged += CheckCompute;

            UpdateGUI();
        }

        private void RegisterCallbacks()
        {
            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterComputeButtonClicked());
            _replaceComputeButton.RegisterCallback<ClickEvent>(_ => OnReplaceComputeButtonClicked());
            _cancelReplaceButton.RegisterCallback<ClickEvent>(_ => OnCancelReplaceButtonClicked());

            _computeNameInput.value = _computeName;
            _computeNameInput.RegisterValueChangedCallback(_ =>
                UpdateComputeTextFields(_computeNameInput, _ipInputs));

            var index = 0;
            var currentIp = _ipAddress.Split(".");
            foreach (var ipField in _ipInputs)
            {
                ipField.value = currentIp[index];
                ipField.RegisterValueChangedCallback(_ =>
                    UpdateComputeTextFields(_computeNameInput, _ipInputs));
                index++;
            }
        }

        private async void OnRegisterComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.NotRegistered or ComputeStatus.Registering)
            {
                var previousComputeName = _stateManager.ComputeName;
                var registerResponse = await _stateManager.ComputeManager.RegisterFleetCompute(_computeName,
                    _stateManager.AnywhereFleetId, _stateManager.AnywhereFleetLocation, _ipAddress);
                if (registerResponse.Success)
                {
                    _stateManager.ComputeName = registerResponse.ComputeName;
                    _stateManager.IpAddress = registerResponse.IpAddress;
                    _stateManager.WebSocketUrl = registerResponse.WebSocketUrl;
                    _computeState = ComputeStatus.Registered;

                    if (!string.IsNullOrWhiteSpace(previousComputeName))
                    {
                        var deregisterResponse =
                            await _stateManager.ComputeManager.DeregisterCompute(previousComputeName,
                                _stateManager.AnywhereFleetId);
                                
                        if (!deregisterResponse)
                        {
                            Debug.LogError(new TextProvider().GetError(ErrorCode.DeregisterComputeFailed));
                        }
                    }
                }
                else
                {
                    var url = string.Format(Urls.AwsGameLiftLogs, _stateManager.Region);
                    _registerComputeStatusBox.Show(StatusBox.StatusBoxType.Error,
                        Strings.AnywherePageStatusBoxDefaultErrorText, 
                        registerResponse.ErrorMessage, 
                        url,
                        Strings.ViewLogsStatusBoxUrlTextButton);
                }
            }
            
            UpdateGUI();
        }

        private async void CheckCompute()
        {
            if (_computeState is ComputeStatus.Registered)
            {
                var computes = await _stateManager.ComputeManager.ListCompute(_stateManager.AnywhereFleetId);
                if (computes.Success)
                {
                    var matchingCompute =
                        computes.ComputeList.FirstOrDefault(compute =>
                            compute.ComputeName == _stateManager.ComputeName);
                    if (matchingCompute == null)
                    {
                        _computeNameInput.value = _defaultComputeName;
                        var defaultIpAddress = _defaultIpAddress.Split(".");
                        for (var i = 0; i < defaultIpAddress.Length; i++)
                        {
                            _ipInputs[i].value = defaultIpAddress[i];
                        }
                        _computeState = ComputeStatus.NotRegistered;
                    }
                    else
                    {
                        _stateManager.ComputeName = matchingCompute.ComputeName;
                        _stateManager.IpAddress = matchingCompute.IpAddress;
                        _stateManager.WebSocketUrl = matchingCompute.GameLiftServiceSdkEndpoint;

                        var ipAddress = matchingCompute.IpAddress.Split(".");
                        for (var i = 0; i < ipAddress.Length; i++)
                        {
                            _ipInputs[i].value = ipAddress[i];
                        }

                        _computeState = ComputeStatus.Registered;
                    }
                }
                else
                {
                    _computeState = ComputeStatus.NotRegistered;
                }
            }

            UpdateGUI();
        }

        private void OnReplaceComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.Registered)
            {
                _computeState = ComputeStatus.Registering;
            }

            UpdateGUI();
        }

        private void OnCancelReplaceButtonClicked()
        {
            if (_computeState is ComputeStatus.Registering)
            {
                _computeState = ComputeStatus.Registered;
            }

            UpdateGUI();
        }

        private void UpdateComputeTextFields(TextField computeTextField, IEnumerable<TextField> ipTextField)
        {
            var computeTextNameValid = computeTextField.value.Length >= 1;
            var ipText = ipTextField.ToList().Select(ipAddressField => ipAddressField.value).ToList();
            _computeName = computeTextField.value;
            var ipTextFieldsValid = ipText.All(text => text.Length >= 1) && ipText.All(s => int.TryParse(s, out _));
            _ipAddress = string.Join(".", ipText);

            _registerButton.SetEnabled(computeTextNameValid && ipTextFieldsValid);
        }

        private void SetupConfigSettings()
        {
            _computeName = !string.IsNullOrWhiteSpace(_stateManager.ComputeName) ? _stateManager.ComputeName : _defaultComputeName;
            _ipAddress = !string.IsNullOrWhiteSpace(_stateManager.IpAddress) ? _stateManager.IpAddress : _defaultIpAddress;
        }

        private List<VisualElement> GetComputeVisualElements() =>
            new List<VisualElement>()
            {
                _registerButton,
                _cancelReplaceButton,
                _computeStatus,
                _replaceComputeButton
            };

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _computeState switch
            {
                ComputeStatus.Disabled => new List<VisualElement>(),
                ComputeStatus.NotRegistered => new List<VisualElement>() { _registerButton, },
                ComputeStatus.Registering => new List<VisualElement>() { _registerButton, _cancelReplaceButton },
                ComputeStatus.Registered => new List<VisualElement>() { _computeStatus, _replaceComputeButton },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SetupStatusBox()
        {
            _registerComputeStatusBox =  new StatusBox();
            var statusBoxContainer = _container.Q("AnywherePageComputeStatusBoxContainer");
            statusBoxContainer.Add(_registerComputeStatusBox);
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in GetComputeVisualElements())
            {
                if (elements.Contains(element))
                {
                    Show(element);
                }
                else
                {
                    Hide(element);
                }
            }

            _container.SetEnabled(_stateManager.IsBootstrapped &&
                                  !string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId));

            if (!string.IsNullOrWhiteSpace(_stateManager.ComputeName))
            {
                var textProvider = new TextProvider();
                _statusIndicator.Set(State.Success,
                    textProvider.Get(Strings.AnywherePageComputeStatusRegistered));
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageComputeNameLabel", Strings.AnywherePageComputeNameLabel);
            l.SetElementText("AnywherePageComputeIPLabel", Strings.AnywherePageComputeIPLabel);
            l.SetElementText("AnywherePageComputeStatusLabel", Strings.AnywherePageComputeStatusLabel);
            l.SetElementText("AnywherePageComputeRegisterButton", Strings.AnywherePageComputeRegisterButton);
            l.SetElementText("AnywherePageComputeReplaceComputeButton",
                Strings.AnywherePageComputeReplaceComputeButton);
            l.SetElementText("AnywherePageComputeCancelReplaceButton", Strings.AnywherePageComputeCancelReplaceButton);
        }

        public enum ComputeStatus
        {
            Disabled,
            NotRegistered,
            Registering,
            Registered,
        }
    }
}
