// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class EC2Page
    {
        private readonly VisualElement _container;
        private string _fleetName;
        private FleetType _fleetType;
        private readonly FleetParameters _parameters;
        private DeploymentSettings _model;

        public EC2Page(VisualElement container)
        {
            _container = container;
            _model = DeploymentSettingsFactory.Create();
            _model.Restore();
            _model.Refresh();
            _parameters = new FleetParameters
            {
                FleetName = _model.FleetName ?? $"{Application.productName}-ManagedFleet",
                LaunchParameters = _model.LaunchParameters ?? $"",
                BuildName = _model.BuildName ??
                            $"{Application.productName}-{_model.ScenarioName.Replace(" ", "_")}-Build",
                GameServerFile = _model.BuildFilePath,
                GameServerFolder = _model.BuildFolderPath,
                OperatingSystem = FleetParametersInput.GetOperatingSystem(_model.BuildOperatingSystem) ??
                                  OperatingSystem.AMAZON_LINUX_2
            };

            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/EC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            _fleetType = FleetType.SingleRegion; // TODO: Read from storage
            var fleetTypeInput = new FleetTypeInput(container,  FleetTypeInput.ScenarioIndexMap[_model.ScenarioIndex], true);
            fleetTypeInput.SetEnabled(true);
            fleetTypeInput.OnValueChanged += value => { Debug.Log($"Fleet type changed to {value}"); };

            container.Q<Foldout>("EC2ParametersSection").text = $"{Application.productName} parameters";
            var fleetParamsInput = new FleetParametersInput(container, _parameters);

            container.Q<Button>("EC2CreateResourceButton").RegisterCallback<ClickEvent>(_ => { StartDeployment(); });
        }

        private void StartDeployment()
        {
            _model.FleetName = _parameters.FleetName;
            _model.BuildName = _parameters.BuildName;
            _model.LaunchParameters = _parameters.LaunchParameters;
            _model.BuildFolderPath = _parameters.GameServerFolder;
            _model.BuildFilePath = _parameters.GameServerFile;
            _model.BuildOperatingSystem = _parameters.OperatingSystem;
            _model.GameName = Application.productName.Substring(0, 12);

            _model.Save();
            _model.StartDeployment(ConfirmChanges).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
            });
        }

        private Task<bool> ConfirmChanges(ConfirmChangesRequest request)
        {
            var stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());

            StackUpdateDialog dialog = UnityEditor.EditorWindow.GetWindow<StackUpdateDialog>();
            return dialog.SetUp(stackUpdateModelFactory.Create(request));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
        }
    }
}