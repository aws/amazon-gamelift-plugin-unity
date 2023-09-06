using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class RegisterComputeInput : StatefulInput
    {
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
            _computeNameInput = container.Q<TextField>("RegisterComputeField");
            _ipInputs = container.Query<TextField>("IpAddress").ToList();
            _computeStatus = container.Q("AnywhereComputeStatusBox");
            _registerButton = container.Q<Button>("ButtonAnywhereCompute");
            _registerNewButton = container.Q<Button>("ButtonAnywhereNewCompute");
            _cancelButton = container.Q<Button>("ButtonAnywhereCancelCompute");

            RegisterCallbacks();
            SetupConfigSettings();
            UpdateGUI();
        }

        private void RegisterCallbacks()
        {
            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterButtonClicked());
            _registerNewButton.RegisterCallback<ClickEvent>(_ => OnRegisterNewButtonClicked());
            _cancelButton.RegisterCallback<ClickEvent>(_ => OnCancelButtonClicked());
            
            _computeNameInput.RegisterValueChangedCallback(_ =>
                SetupCompute(_computeNameInput, _ipInputs, _registerButton));
            _computeNameInput.value = _computeName;
            
            var index = 0;
            var currentIp = _ipAddress.Split(".");
            foreach (var ipField in _ipInputs)
            {
                ipField.value = currentIp[index];
                ipField.RegisterValueChangedCallback(_ =>
                    SetupCompute(_computeNameInput, _ipInputs, _registerButton));
                index++;
            }
        }

        private async void OnRegisterButtonClicked()
        {
            if (_computeState is ComputeStatus.RegisteringInitial or ComputeStatus.Registering)
            {
                if (CheckCompute())
                {
                    _computeState = ComputeStatus.Registered;
                }
                else
                {
                    var success = await _requestAdapter.RegisterFleetCompute(_computeName, _fleetDetails.FleetId, GameLiftRequestAdapter.FleetLocation, _ipAddress);
                    if (success)
                    {
                        _computeState = ComputeStatus.Registered;
                    }
                }
            }

            UpdateGUI();
        }

        private void OnRegisterNewButtonClicked()
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
        
        private void SetupCompute(TextField computeTextName, IEnumerable<TextField> ipTextField, Button button)
        {
             var computeTextNameValid = computeTextName.value.Length >= 1;
            var ipText = ipTextField.ToList().Select(ipAddressField => ipAddressField.value).ToList();
            _computeName = computeTextName.value;
            var ipTextFieldsValid = ipText.All(text => text.Length >= 1) && ipText.All(s=> int.TryParse(s, out _));

            button.SetEnabled(computeTextNameValid && ipTextFieldsValid);
        }

        private bool CheckCompute()
        {
            if (_fleetDetails != null && _computeName != null && _fleetDetails.FleetId != null)
            {
                if (_requestAdapter.DescribeCompute(_computeName, _fleetDetails.FleetId).Result)
                {
                    _computeState = ComputeStatus.Registered;
                    return true;
                }
            }

            return false;
        }
        
        private void SetupConfigSettings()
        {
            var computeName = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.ComputeName);
            if (computeName.Success)
            {
                _computeName = computeName.Value;
            }

            var ip = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.IpAddress);
            if (ip.Success)
            {
                _ipAddress = ip.Value;
            }

            var selectedProfile = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.SelectedProfile);
            if (selectedProfile.Success)
            {
                _gameLiftPlugin.CurrentState.SelectedProfile = selectedProfile.Value;
            }

            var fleetIndex = _gameLiftPlugin.CoreApi.GetSetting(SettingsKeys.SelectedFleetIndex);
            if (fleetIndex.Success)
            {
                _gameLiftPlugin.CurrentState.SelectedFleetIndex = int.Parse(fleetIndex.Value);
            }
        }

        protected sealed override void UpdateGUI()
        {
            switch (_computeState)
            {
                case ComputeStatus.Disabled:
                    break;
                case ComputeStatus.RegisteringInitial:
                    SetInputsReadonly(false);
                    Hide(_computeStatus);
                    Show(_registerButton);
                    Hide(_registerNewButton);
                    Hide(_cancelButton);
                    break;
                case ComputeStatus.Registering:
                    SetInputsReadonly(false);
                    Hide(_computeStatus);
                    Show(_registerButton);
                    Show(_cancelButton);
                    Hide(_registerNewButton);
                    break;
                case ComputeStatus.Registered:
                    SetInputsReadonly(true);
                    Show(_computeStatus);
                    Hide(_registerButton);
                    Hide(_cancelButton);
                    Show(_registerNewButton);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum ComputeStatus
        {
            Disabled,
            RegisteringInitial,
            Registering,
            Registered,
        }
    }
}