﻿using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using Editor.CoreAPI;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class RegisterComputeInput : StatefulInput
    {
        private static IReadOnlyCollection<VisualElement> _computeVisualElements;

        private ComputeStatus _computeState;

        private readonly List<TextField> _ipInputs;
        private readonly TextField _computeNameInput;
        private readonly Button _registerButton;
        private readonly Button _registerNewButton;
        private readonly Button _cancelButton;
        private readonly VisualElement _computeStatus;
        private readonly VisualElement _container;
        private readonly GameLiftComputeManager _computeManager;
        private readonly StateManager _stateManager;

        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _location = "custom-location-1";

        public RegisterComputeInput(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            _computeManager = stateManager.ComputeManager;
            _computeNameInput = container.Q<TextField>("AnywherePageComputeNameInput");
            _ipInputs = container.Query<TextField>("AnywherePageComputeIPAddressInput").ToList();
            _computeStatus = container.Q("AnywherePageComputeStatus");
            _registerButton = container.Q<Button>("AnywherePageComputeRegisterButton");
            _registerNewButton = container.Q<Button>("AnywherePageComputeRegisterNewButton");
            _cancelButton = container.Q<Button>("AnywherePageComputeCancelButton");

            _computeState = !string.IsNullOrWhiteSpace(_stateManager.ComputeName) &&
                            !string.IsNullOrWhiteSpace(_stateManager.IpAddress)
                ? ComputeStatus.Registered
                : ComputeStatus.NotRegistered;
            _computeName = _stateManager.ComputeName ?? _computeName;
            _ipAddress = _stateManager.IpAddress ?? _ipAddress;


            PopulateComputeVisualElements();
            RegisterCallbacks();
            SetupConfigSettings();
            UpdateGUI();
        }

        private void RegisterCallbacks()
        {
            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterComputeButtonClicked());
            _registerNewButton.RegisterCallback<ClickEvent>(_ => OnNewComputeButtonClicked());
            _cancelButton.RegisterCallback<ClickEvent>(_ => OnCancelButtonClicked());

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
                var response = await _computeManager.RegisterFleetCompute(_computeName,
                    _stateManager.SelectedProfile.AnywhereFleetId, _location, _ipAddress);
                if (response.Success)
                {
                    _stateManager.ComputeName = response.ComputeName;
                    _stateManager.IpAddress = response.IpAddress;
                    _stateManager.WebSocketUrl = response.WebSocketUrl;
                    
                    _computeState = ComputeStatus.Registered;
                }
            }

            UpdateGUI();
        }

        private void OnNewComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.Registered)
            {
                _computeState = ComputeStatus.Registering;
            }

            UpdateGUI();
        }

        private void OnCancelButtonClicked()
        {
            if (_computeState is ComputeStatus.Registering)
            {
                _computeState = ComputeStatus.Registered;
            }

            UpdateGUI();
        }

        private void SetInputsReadonly(bool value)
        {
            _computeNameInput.isReadOnly = value;
            _ipInputs.ForEach(input => input.isReadOnly = value);
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
            _computeName = _stateManager.SelectedProfile.ComputeName;
            _ipAddress = _stateManager.SelectedProfile.IpAddress;
        }

        private void PopulateComputeVisualElements()
        {
            _computeVisualElements = new List<VisualElement>()
            {
                _registerButton,
                _cancelButton,
                _computeStatus,
                _registerNewButton
            };
        }

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _computeState switch
            {
                ComputeStatus.Disabled => new List<VisualElement>(),
                ComputeStatus.NotRegistered => new List<VisualElement>() { _registerButton, },
                ComputeStatus.Registering => new List<VisualElement>() { _registerButton, _cancelButton },
                ComputeStatus.Registered => new List<VisualElement>() { _computeStatus, _registerNewButton },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected sealed override void UpdateGUI()
        {
            SetInputsReadonly(_computeState == ComputeStatus.Registered);
            var elements = GetVisibleItemsByState();
            foreach (var element in _computeVisualElements)
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

            _container.SetEnabled(_stateManager.IsBootstrapped && !string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId));
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