// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2Page
    {
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private readonly DeploymentSettings _deploymentSettings;
        private readonly Button _deployButton;
        private readonly Button _redeployButton;
        private readonly Button _deleteButton;
        private readonly Button _launchClientButton;
        private readonly DeploymentScenariosInput _deploymentScenariosInput;
        private readonly FleetParametersInput _fleetParamsInput;
        private readonly StatusIndicator _statusIndicator;
        private readonly ManagedEC2Deployment _ec2Deployment;
        private StatusBox _statusBox;

        public ManagedEC2Page(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            _deploymentSettings = DeploymentSettingsFactory.Create(stateManager);
            if (_stateManager.IsBootstrapped)
            {
                _deploymentSettings.Restore();
            }
            _deploymentSettings.Refresh();
            var parameters = GetManagedEC2Parameters(_deploymentSettings);

            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/ManagedEC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            SetupStatusBoxes();

            _stateManager.OnUserProfileUpdated += UpdateStatusBoxes;
            _ec2Deployment = new ManagedEC2Deployment(_deploymentSettings);
            var scenarioContainer = container.Q("ManagedEC2ScenarioTitle");
            _deploymentScenariosInput =
                new DeploymentScenariosInput(scenarioContainer, _deploymentSettings.Scenario,
                    _stateManager.IsBootstrapped);
            _deploymentScenariosInput.OnValueChanged += value => { Debug.Log($"Fleet type changed to {value}"); };
            _statusIndicator = _container.Q<StatusIndicator>();
            var parametersContainer = container.Q<Foldout>("ManagedEC2ParametersTitle");
            _fleetParamsInput = new FleetParametersInput(parametersContainer, parameters);
            _fleetParamsInput.OnValueChanged += fleetParameters =>
            {
                _ec2Deployment.UpdateModelFromParameters(fleetParameters);
                UpdateGUI();
            };

            _stateManager.OnUserProfileUpdated += UpdateDeploymentSettings;

            _deployButton = container.Q<Button>("ManagedEC2CreateStackButton");
            _deployButton.RegisterCallback<ClickEvent>(_ =>
            {
                _ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _redeployButton = container.Q<Button>("ManagedEC2RedeployStackButton");
            _redeployButton.RegisterCallback<ClickEvent>(_ =>
            {
                _ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _deleteButton = container.Q<Button>("ManagedEC2DeleteStackButton");
            _deleteButton.RegisterCallback<ClickEvent>(async _ =>
            {
                await _ec2Deployment.DeleteDeployment();
                UpdateGUI();
            });
            _launchClientButton = container.Q<Button>("ManagedEC2LaunchClientButton");
            _launchClientButton.RegisterCallback<ClickEvent>(_ => EditorApplication.EnterPlaymode());


            _container.Q<VisualElement>("ManagedEC2IntegrateLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.ManagedEc2IntegrateLink));

            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;
            _deploymentSettings.Scenario = DeploymentScenarios.SingleRegion;
            UpdateGUI();
            UpdateStatusBoxes();
        }

        private ManagedEC2FleetParameters GetManagedEC2Parameters(DeploymentSettings deploymentSettings)
        {
            return new ManagedEC2FleetParameters
            {
                GameName = deploymentSettings.GameName ?? Application.productName,
                FleetName = deploymentSettings.FleetName ?? $"{Application.productName}-ManagedFleet",
                LaunchParameters = deploymentSettings.LaunchParameters ?? $"",
                BuildName = deploymentSettings.BuildName ??
                            $"{Application.productName}-{deploymentSettings.ScenarioName.Replace(" ", "_")}-Build",
                GameServerFile = deploymentSettings.BuildFilePath,
                GameServerFolder = deploymentSettings.BuildFolderPath,
                OperatingSystem = OperatingSystem.FindValue(deploymentSettings.BuildOperatingSystem) ??
                                  OperatingSystem.AMAZON_LINUX_2
            };           
        }

        private void UpdateDeploymentSettings()
        {
            if (_stateManager.IsBootstrapped)
            {
                _deploymentSettings.Refresh();
                _deploymentSettings.Restore();
                _ec2Deployment.UpdateModelFromParameters(GetManagedEC2Parameters(_deploymentSettings));
            }

            UpdateGUI();
        }

        private void UpdateGUI()
        {
            LocalizeText();

            _deployButton.SetEnabled(_deploymentSettings.CurrentStackInfo.StackStatus == null &&
                                     _deploymentSettings.CanDeploy);
            _redeployButton.SetEnabled(_deploymentSettings.CurrentStackInfo.StackStatus != null &&
                                       _deploymentSettings.CanDeploy);
            _deleteButton.SetEnabled(_deploymentSettings.CurrentStackInfo.StackStatus != null &&
                                     _deploymentSettings.IsCurrentStackModifiable);
            _launchClientButton.SetEnabled(
                _deploymentSettings.CurrentStackInfo.StackStatus is StackStatus.CreateComplete
                    or StackStatus.UpdateComplete);

            _deploymentScenariosInput.SetEnabled(_deploymentSettings.CanEdit);
            _fleetParamsInput.SetEnabled(_deploymentSettings.CanEdit);

            var stackStatus = _deploymentSettings.CurrentStackInfo.StackStatus;
            var textProvider = new TextProvider();
            if (stackStatus == null)
            {
                _statusIndicator.Set(State.Inactive, textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }
            else if (stackStatus.IsStackStatusFailed())
            {
                _statusIndicator.Set(State.Failed, textProvider.Get(Strings.ManagedEC2DeployStatusFailed));
            }
            else if (stackStatus == StackStatus.DeleteInProgress)
            {
                _statusIndicator.Set(State.InProgress, textProvider.Get(Strings.ManagedEC2DeployStatusDeleting));
            }
            else if (stackStatus.IsStackStatusInProgress())
            {
                _statusIndicator.Set(State.InProgress, textProvider.Get(Strings.ManagedEC2DeployStatusDeploying));
            }
            else if (stackStatus.IsStackStatusOperationDone())
            {
                _statusIndicator.Set(State.Success, textProvider.Get(Strings.ManagedEC2DeployStatusDeployed));
            }
            else
            {
                _statusIndicator.Set(State.Inactive, textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }
        }
        
        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("ManagedEC2StatusBox");
        }
        
        private void UpdateStatusBoxes()
        {
            if (!_stateManager.IsBootstrapped)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.ManagedEC2StatusBoxNotBootstrappedWarning);
            }
            else
            {
                _statusBox.Close();
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var replacements = new Dictionary<string, string>()
            {
                { "GameName", Application.productName },
                { "ScenarioType", GetScenarioType(l) }
            };
            l.SetElementText("ManagedEC2Title", Strings.ManagedEC2Title);
            l.SetElementText("ManagedEC2Description", Strings.ManagedEC2Description);
            l.SetElementText("ManagedEC2IntegrateTitle", Strings.ManagedEC2IntegrateTitle);
            l.SetElementText("ManagedEC2IntegrateDescription", Strings.ManagedEC2IntegrateDescription);
            l.SetElementText("ManagedEC2IntegrateLink", Strings.ManagedEC2IntegrateLink);
            l.SetElementText("ManagedEC2ScenarioTitle", Strings.ManagedEC2ScenarioTitle);
            l.SetElementText("ManagedEC2ParametersTitle", Strings.ManagedEC2ParametersTitle, replacements);
            l.SetElementText("ManagedEC2DeployTitle", Strings.ManagedEC2DeployTitle, replacements);
            l.SetElementText("ManagedEC2DeployDescription", Strings.ManagedEC2DeployDescription);
            l.SetElementText("ManagedEC2DeployStatusLabel", Strings.ManagedEC2DeployStatusLabel);
            l.SetElementText("ManagedEC2DeployActionsLabel", Strings.ManagedEC2DeployActionsLabel);
            l.SetElementText("ManagedEC2CreateStackButton", Strings.ManagedEC2CreateStackButton);
            l.SetElementText("ManagedEC2RedeployStackButton", Strings.ManagedEC2RedeployStackButton);
            l.SetElementText("ManagedEC2DeleteStackButton", Strings.ManagedEC2DeleteStackButton);
            l.SetElementText("ManagedEC2LaunchClientTitle", Strings.ManagedEC2LaunchClientTitle);
            l.SetElementText("ManagedEC2LaunchClientLabel", Strings.ManagedEC2LaunchClientLabel);
            l.SetElementText("ManagedEC2LaunchClientButton", Strings.ManagedEC2LaunchClientButton);
        }

        private string GetScenarioType(ElementLocalizer l) => _deploymentSettings.Scenario switch
        {
            DeploymentScenarios.SingleRegion => l.GetText(Strings.ManagedEC2ScenarioSingleFleetLabel),
            DeploymentScenarios.SpotFleet => l.GetText(Strings.ManagedEC2ScenarioSpotFleetLabel),
            DeploymentScenarios.FlexMatch => l.GetText(Strings.ManagedEC2ScenarioFlexMatchLabel),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}