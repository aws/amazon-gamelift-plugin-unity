using System;
using System.Collections.Generic;
using System.Linq;
using Editor.CoreAPI;
using Editor.Window;
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
        private readonly GameLiftComputeManager _computeManager;
        private readonly StateManager _stateManager;

        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _location = "custom-location-1";

        public RegisterComputeInput(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/RegisterComputeInput");
            container.Add(uxml.Instantiate());
            _container = container;

            _stateManager = stateManager;
            _computeManager = stateManager.ComputeManager;
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
            _computeName = _stateManager.ComputeName ?? _computeName;
            _ipAddress = _stateManager.IpAddress ?? _ipAddress;


            RegisterCallbacks();
            SetupConfigSettings();
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
                var response = await _computeManager.RegisterFleetCompute(_computeName,
                    _stateManager.AnywhereFleetId, _location, _ipAddress);
                if (response.Success)
                {
                    _stateManager.ComputeName = response.ComputeName;
                    _stateManager.IpAddress = response.IpAddress;
                    _stateManager.WebSocketUrl = response.WebSocketUrl;
                    _computeState = ComputeStatus.Registered;

                    if (!string.IsNullOrWhiteSpace(previousComputeName))
                    {
                        var deregisterResponse =
                            await _computeManager.DeregisterCompute(previousComputeName, _stateManager.AnywhereFleetId);
                        if (!deregisterResponse)
                        {
                            Debug.LogError(new TextProvider().GetError(ErrorCode.DeregisterComputeFailed));
                        }
                    }
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
            _computeName = _stateManager.ComputeName;
            _ipAddress = _stateManager.IpAddress;
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

        protected sealed override void UpdateGUI()
        {
            SetInputsReadonly(_computeState == ComputeStatus.Registered);
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
                _statusIndicator.Set(State.Success, "Registered"); // TODO
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