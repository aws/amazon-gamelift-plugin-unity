using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using UnityEngine.UIElements;

namespace Editor.Window
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
        private readonly GameLiftRequestAdapter _requestAdapter;
        private readonly ConnectToFleetInput _fleetDetails;
        private readonly GameLiftPlugin _gameLiftPlugin;

        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";

        public RegisterComputeInput(VisualElement container, GameLiftPlugin gameLiftPlugin, ConnectToFleetInput  fleetDetails, ComputeStatus initialState)
        {
            _computeState = initialState;
            _fleetDetails = fleetDetails;
            _gameLiftPlugin = gameLiftPlugin;
            _requestAdapter = new GameLiftRequestAdapter(_gameLiftPlugin);
            _gameLiftPlugin.SetupWrapper();
            _computeNameInput = container.Q<TextField>("AnywherePageComputeNameInput");
            _ipInputs = container.Query<TextField>("AnywherePageComputeIPAddressInput").ToList();
            _computeStatus = container.Q("AnywherePageComputeStatus");
            _registerButton = container.Q<Button>("AnywherePageComputeRegisterButton");
            _registerNewButton = container.Q<Button>("AnywherePageComputeRegisterNewButton");
            _cancelButton = container.Q<Button>("AnywherePageComputeCancelButton");

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
                var success = await _requestAdapter.RegisterFleetCompute(_computeName, _fleetDetails.FleetId, GameLiftRequestAdapter.FleetLocation, _ipAddress);
                if (success)
                {
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
        
        private void VerifyComputeTextFields(TextField computeTextName, IEnumerable<TextField> ipTextField, Button button)
        {
             var computeTextNameValid = computeTextName.value.Length >= 1;
            var ipText = ipTextField.ToList().Select(ipAddressField => ipAddressField.value).ToList();
            _computeName = computeTextName.value;
            var ipTextFieldsValid = ipText.All(text => text.Length >= 1) && ipText.All(s=> int.TryParse(s, out _));

            button.SetEnabled(computeTextNameValid && ipTextFieldsValid);
        }

        private void SetupConfigSettings()
        {
            _computeName = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.ComputeName).Value;
            _ipAddress = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.IpAddress).Value;
            _gameLiftPlugin.CurrentState.SelectedProfile = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.SelectedProfile).Value;
            _gameLiftPlugin.CurrentState.SelectedFleetName = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.SelectedFleetName).Value;
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
                if (elements.Contains(element)) {
                    Show(element);
                } else {
                    Hide(element);
                }
            }
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