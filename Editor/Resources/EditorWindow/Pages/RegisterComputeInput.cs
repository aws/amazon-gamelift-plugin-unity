using System;
using System.Collections.Generic;
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

        public RegisterComputeInput(VisualElement container, ComputeStatus initialState)
        {
            _computeState = initialState;

            _computeNameInput = container.Q<TextField>("RegisterComputeField");
            _ipInputs = container.Query<TextField>("IpAddress").ToList();
            _computeStatus = container.Q("AnywhereComputeStatusBox");
            _registerButton = container.Q<Button>("ButtonAnywhereCompute");
            _registerNewButton = container.Q<Button>("ButtonAnywhereNewCompute");
            _cancelButton = container.Q<Button>("ButtonAnywhereCancelCompute");

            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterButtonClicked());
            _registerNewButton.RegisterCallback<ClickEvent>(_ => OnRegisterNewButtonClicked());
            _cancelButton.RegisterCallback<ClickEvent>(_ => OnCancelButtonClicked());

            UpdateGUI();
        }

        private void OnRegisterButtonClicked()
        {
            if (_computeState is ComputeStatus.RegisteringInitial or ComputeStatus.Registering)
            {
                // TODO: Add functionality
                _computeState = ComputeStatus.Registered;
            }

            UpdateGUI();
        }

        private void OnRegisterNewButtonClicked()
        {
            if (_computeState is ComputeStatus.Registered)
            {
                // TODO: Add functionality
                _computeState = ComputeStatus.Registering;
            }

            UpdateGUI();
        }

        private void OnCancelButtonClicked()
        {
            if (_computeState is ComputeStatus.Registering)
            {
                // TODO: Add functionality
                _computeState = ComputeStatus.Registered;
            }

            UpdateGUI();
        }

        private void SetInputsReadonly(bool value)
        {
            _computeNameInput.isReadOnly = value;
            _ipInputs.ForEach(input => input.isReadOnly = value);
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