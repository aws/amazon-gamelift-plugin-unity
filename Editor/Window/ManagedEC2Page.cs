// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2Page
    {
        private readonly VisualElement _container;
        private readonly DeploymentSettings _model;
        private readonly Button _deployButton;
        private readonly Button _redeployButton;
        private readonly Button _deleteButton;
        private readonly Button _launchClientButton;
        private readonly DeploymentScenariosInput _deploymentScenariosInput;
        private readonly FleetParametersInput _fleetParamsInput;
        private readonly Label _ec2DeploymentStatusLabel;

        public ManagedEC2Page(VisualElement container)
        {
            _container = container;
            _model = DeploymentSettingsFactory.Create();
            _model.Restore();
            _model.Refresh();
            var parameters = new ManagedEC2FleetParameters
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

            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/ManagedEC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);

            var ec2Deployment = new ManagedEC2Deployment(_model, parameters);
            var scenarioContainer = container.Q("ManagedEC2ScenarioTitle");
            _deploymentScenariosInput =
                new DeploymentScenariosInput(scenarioContainer, _model.Scenario, true);
            _deploymentScenariosInput.SetEnabled(true);
            _deploymentScenariosInput.OnValueChanged += value => { Debug.Log($"Fleet type changed to {value}"); };
            _ec2DeploymentStatusLabel = _container.Q<Label>("ManagedEC2DeployStatusText");

            var parametersInput = container.Q<Foldout>("ManagedEC2ParametersTitle");
            _fleetParamsInput = new FleetParametersInput(parametersInput, parameters);
            _fleetParamsInput.OnValueChanged += param =>
            {
                ec2Deployment.UpdateModelFromParameters();
                UpdateGUI();
            };

            _deployButton = container.Q<Button>("ManagedEC2CreateStackButton");
            _deployButton.RegisterCallback<ClickEvent>(_ =>
            {
                ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _redeployButton = container.Q<Button>("ManagedEC2RedeployStackButton");
            _redeployButton.RegisterCallback<ClickEvent>(_ =>
            {
                ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _deleteButton = container.Q<Button>("ManagedEC2DeleteStackButton");
            _deleteButton.RegisterCallback<ClickEvent>(async _ =>
            {
                await ec2Deployment.DeleteDeployment();
                UpdateGUI();
            });
            _launchClientButton = container.Q<Button>("ManagedEC2LaunchClientButton");
            _launchClientButton.RegisterCallback<ClickEvent>(_ => EditorApplication.EnterPlaymode());

            _model.CurrentStackInfoChanged += UpdateGUI;
            _model.Scenario = DeploymentScenarios.SingleRegion;
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            LocalizeText();

            _deployButton.SetEnabled(_model.CurrentStackInfo.StackStatus == null && _model.CanDeploy);
            _redeployButton.SetEnabled(_model.CurrentStackInfo.StackStatus != null && _model.CanDeploy);
            _deleteButton.SetEnabled(_model.CurrentStackInfo.StackStatus != null && _model.IsCurrentStackModifiable);
            _launchClientButton.SetEnabled(
                _model.CurrentStackInfo.StackStatus is StackStatus.CreateComplete or StackStatus.UpdateComplete);

            _deploymentScenariosInput.SetEnabled(_model.CanDeploy);
            _fleetParamsInput.SetEnabled(_model.CanDeploy);
            _ec2DeploymentStatusLabel.text = _model.CurrentStackInfo.StackStatus;
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
            l.SetElementText("ManagedEC2DeployStatusIcon", Strings.ManagedEC2DeployStatusIcon);
            l.SetElementText("ManagedEC2DeployStatusText", Strings.ManagedEC2DeployStatusText);
            l.SetElementText("ManagedEC2DeployActionsLabel", Strings.ManagedEC2DeployActionsLabel);
            l.SetElementText("ManagedEC2CreateStackButton", Strings.ManagedEC2CreateStackButton);
            l.SetElementText("ManagedEC2RedeployStackButton", Strings.ManagedEC2RedeployStackButton);
            l.SetElementText("ManagedEC2DeleteStackButton", Strings.ManagedEC2DeleteStackButton);
            l.SetElementText("ManagedEC2LaunchClientTitle", Strings.ManagedEC2LaunchClientTitle);
            l.SetElementText("ManagedEC2LaunchClientLabel", Strings.ManagedEC2LaunchClientLabel);
            l.SetElementText("ManagedEC2LaunchClientButton", Strings.ManagedEC2LaunchClientButton);
        }

        private string GetScenarioType(ElementLocalizer l) => _model.Scenario switch
        {
            DeploymentScenarios.SingleRegion => l.GetText(Strings.ManagedEC2ScenarioSingleFleetLabel),
            DeploymentScenarios.SpotFleet => l.GetText(Strings.ManagedEC2ScenarioSpotFleetLabel),
            DeploymentScenarios.FlexMatch => l.GetText(Strings.ManagedEC2ScenarioFlexMatchLabel),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}