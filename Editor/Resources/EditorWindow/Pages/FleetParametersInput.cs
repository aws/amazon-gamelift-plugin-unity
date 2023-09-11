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
        private readonly Dictionary<string, OperatingSystem> _osMappings = new()
        {
            { "Amazon Linux 2 (AL2)", OperatingSystem.AMAZON_LINUX_2 },
            { "Amazon Linux 2023 (AL2023)", OperatingSystem.AMAZON_LINUX_2023 },
            { "Windows Server 2016", OperatingSystem.WINDOWS_2016 },
        };

        private readonly FleetParameters _parameters;
        private readonly VisualElement _container;

        public FleetParametersInput(VisualElement container, FleetParameters parameters)
        {
            _container = container;
            _parameters = parameters;

            SetupInput("EC2FleetNameInput", parameters.FleetName, value => parameters.FleetName = value);
            SetupInput("EC2BuildNameInput", parameters.BuildName, value => parameters.BuildName = value);
            SetupInput("EC2LaunchParamsInput", parameters.LaunchParameters,
                value => parameters.LaunchParameters = value);
            var serverFolderInput = SetupInput("EC2ServerFolderInput", parameters.GameServerFolder,
                value => parameters.GameServerFolder = value);
            var serverFileInput = SetupInput("EC2ServerFileInput", parameters.GameServerFile,
                value => parameters.GameServerFile = value);

            var osDropdown = container.Q<DropdownField>("EC2OperatingSystemDropdown");
            osDropdown.choices = _osMappings.Keys.ToList();
            osDropdown.RegisterValueChangedCallback(e => { _parameters.OperatingSystem = _osMappings[e.newValue]; });

            var serverFolderButton = container.Q<Button>("EC2ServerFolderButton");
            serverFolderButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFolderPanel("Game Server Build Folder Path", Application.dataPath,
                    _parameters.GameServerFolder);
                parameters.GameServerFolder = value;
                serverFolderInput.value = value;
            });

            var serverFileButton = container.Q<Button>("EC2ServerFileButton");
            serverFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFilePanel("Game Server Build File Path (exe)", _parameters.GameServerFolder,
                    _parameters.GameServerFile);
                parameters.GameServerFile = value;
                serverFileInput.value = value;
            });
        }

        private TextField SetupInput(string inputName, string initialValue, Action<string> onChangeEvent)
        {
            var input = _container.Q<TextField>(inputName);
            input.value = initialValue;
            input.RegisterValueChangedCallback(e => onChangeEvent(e.newValue));
            return input;
        }
    }
}