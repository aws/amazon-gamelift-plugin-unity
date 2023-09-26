using System;
using System.Collections.Generic;
using System.Linq;
using Editor.CoreAPI;
using Editor.Resources.EditorWindow;
using Editor.Window;
using UnityEngine;
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
        private readonly Button _replaceComputeButton;
        private readonly Button _cancelReplaceButton;
        private readonly VisualElement _computeStatus;
        private readonly VisualElement _container;
        private readonly GameLiftComputeManager _computeManager;
        private readonly ConnectToFleetInput _fleetDetails;
        private readonly StateManager _stateManager;

        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _location = "custom-location-1";

        public RegisterComputeInput(VisualElement container, StateManager stateManager,
            ConnectToFleetInput fleetDetails, ComputeStatus initialState)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/RegisterComputeInput");
            container.Add(uxml.Instantiate());
            _container = container;

            _computeState = initialState;
            _fleetDetails = fleetDetails;
            _stateManager = stateManager;
            _computeManager = stateManager.ComputeManager;
            _computeNameInput = container.Q<TextField>("AnywherePageComputeNameInput");
            _ipInputs = container.Query<TextField>("AnywherePageComputeIPAddressInput").ToList();
            _computeStatus = container.Q("AnywherePageComputeStatus");
            _registerButton = container.Q<Button>("AnywherePageComputeRegisterButton");
            _replaceComputeButton = container.Q<Button>("AnywherePageComputeRegisterNewButton");
            _cancelReplaceButton = container.Q<Button>("AnywherePageComputeCancelButton");
            LocalizeText();

            PopulateComputeVisualElements();
            RegisterCallbacks();
            SetupConfigSettings();
            UpdateGUI();
        }

        private void RegisterCallbacks()
        {
            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterComputeButtonClicked());
            _replaceComputeButton.RegisterCallback<ClickEvent>(_ => OnReplaceComputeButtonClicked());
            _cancelReplaceButton.RegisterCallback<ClickEvent>(_ => OnCancelReplaceButtonClicked());

            _computeNameInput.RegisterValueChangedCallback(_ =>
                VerifyComputeTextFields(_computeNameInput, _ipInputs, _registerButton));
            _computeNameInput.value = _computeName;

            var index = 0;
            var currentIp = _ipAddress.Split(".");
            foreach (var ipField in _ipInputs)
            {
                ipField.value = currentIp[index];
                ipField.RegisterValueChangedCallback(_ =>
                    VerifyComputeTextFields(_computeNameInput, _ipInputs, _registerButton));
                index++;
            }
        }

        private async void OnRegisterComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.NotRegistered or ComputeStatus.Registering)
            {
                var previousComputeName = _stateManager.ComputeName;
                var success = await _computeManager.RegisterFleetCompute(_computeName, _fleetDetails.FleetId, _location,
                    _ipAddress, previousComputeName);
                if (success)
                {
                    _computeState = ComputeStatus.Registered;
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

        private void VerifyComputeTextFields(TextField computeTextName, IEnumerable<TextField> ipTextField,
            Button button)
        {
            var computeTextNameValid = computeTextName.value.Length >= 1;
            var ipText = ipTextField.ToList().Select(ipAddressField => ipAddressField.value).ToList();
            _computeName = computeTextName.value;
            var ipTextFieldsValid = ipText.All(text => text.Length >= 1) && ipText.All(s => int.TryParse(s, out _));

            button.SetEnabled(computeTextNameValid && ipTextFieldsValid);
        }

        private void SetupConfigSettings()
        {
            _computeName = _stateManager.ComputeName;
            _ipAddress = _stateManager.IpAddress;
        }

        private void PopulateComputeVisualElements()
        {
            _computeVisualElements = new List<VisualElement>()
            {
                _registerButton,
                _cancelReplaceButton,
                _computeStatus,
                _replaceComputeButton
            };
        }

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
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageComputeNameLabel", Strings.AnywherePageComputeNameLabel);
            l.SetElementText("AnywherePageComputeIPLabel", Strings.AnywherePageComputeIPLabel);
            l.SetElementText("AnywherePageComputeStatusLabel", Strings.AnywherePageComputeStatusLabel);
            l.SetElementText("AnywherePageComputeRegisterButton", Strings.AnywherePageComputeRegisterButton);
            l.SetElementText("AnywherePageComputeRegisterNewButton", Strings.AnywherePageComputeRegisterNewButton);
            l.SetElementText("AnywherePageComputeCancelButton", Strings.AnywherePageComputeCancelButton);
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