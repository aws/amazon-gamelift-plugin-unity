using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace Editor.Resources.EditorWindow.Pages
{
    public class FleetParametersInput
    {
        private static readonly Dictionary<string, OperatingSystem> OSMappings = new()
        {
            { "Amazon Linux 2 (AL2)", OperatingSystem.AMAZON_LINUX_2 },
            { "Amazon Linux 2023 (AL2023)", OperatingSystem.AMAZON_LINUX_2023 },
            { "Windows Server 2016", OperatingSystem.WINDOWS_2016 },
        };

        private readonly FleetParameters _parameters;
        private readonly VisualElement _container;
        private readonly TextField _fleetNameInput;
        private readonly TextField _buildNameInput;
        private readonly TextField _launchParamsInput;
        private readonly TextField _serverFolderInput;
        private readonly TextField _serverFileInput;
        private readonly DropdownField _osDropdown;
        private readonly Button _serverFolderButton;
        private readonly Button _serverFileButton;

        public Action<FleetParameters> OnValueChanged;

        public FleetParametersInput(VisualElement container, FleetParameters parameters)
        {
            _container = container;
            _parameters = parameters;

            _fleetNameInput = SetupInput("EC2FleetNameInput", parameters.FleetName,
                value => parameters.FleetName = value);
            _buildNameInput = SetupInput("EC2BuildNameInput", parameters.BuildName,
                value => parameters.BuildName = value);
            _launchParamsInput = SetupInput("EC2LaunchParamsInput", parameters.LaunchParameters,
                value => parameters.LaunchParameters = value);
            _serverFolderInput = SetupInput("EC2ServerFolderInput", parameters.GameServerFolder,
                value => parameters.GameServerFolder = value);
            _serverFileInput = SetupInput("EC2ServerFileInput", parameters.GameServerFile,
                value => parameters.GameServerFile = value);

            _osDropdown = container.Q<DropdownField>("EC2OperatingSystemDropdown");
            _osDropdown.choices = OSMappings.Keys.ToList();
            _osDropdown.index = OSMappings.Values.ToList().IndexOf(parameters.OperatingSystem);
            _osDropdown.RegisterValueChangedCallback(e =>
            {
                _parameters.OperatingSystem = OSMappings[e.newValue];
                OnValueChanged(_parameters);
            });

            _serverFolderButton = container.Q<Button>("EC2ServerFolderButton");
            _serverFolderButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFolderPanel("Game Server Build Folder Path", Application.dataPath,
                    _parameters.GameServerFolder);
                parameters.GameServerFolder = value;
                _serverFolderInput.value = value;
                OnValueChanged(_parameters);
            });

            _serverFileButton = container.Q<Button>("EC2ServerFileButton");
            _serverFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFilePanel("Game Server Build File Path (exe)",
                    _parameters.GameServerFolder,
                    _parameters.GameServerFile);
                parameters.GameServerFile = value;
                _serverFileInput.value = value;
                OnValueChanged(_parameters);
            });
        }

        public static OperatingSystem GetOperatingSystem(string operatingSystem)
        {
            if (operatingSystem == null)
            {
                return null;
            }

            return OSMappings.TryGetValue(operatingSystem, out var mapping) ? mapping : null;
        }

        private TextField SetupInput(string inputName, string initialValue, Action<string> onChangeEvent)
        {
            var input = _container.Q<TextField>(inputName);
            input.value = initialValue;
            input.RegisterValueChangedCallback(e =>
            {
                onChangeEvent(e.newValue);
                OnValueChanged(_parameters);
            });
            return input;
        }

        public void SetEnabled(bool value)
        {
            _fleetNameInput.SetEnabled(value);
            _buildNameInput.SetEnabled(value);
            _launchParamsInput.SetEnabled(value);
            _serverFolderInput.SetEnabled(value);
            _serverFileInput.SetEnabled(value);
            _osDropdown.SetEnabled(value);
            _serverFolderButton.SetEnabled(value);
            _serverFileButton.SetEnabled(value);
        }
    }
}