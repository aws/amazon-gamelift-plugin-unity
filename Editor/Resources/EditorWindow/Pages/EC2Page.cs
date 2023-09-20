﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class EC2Page
    {
        private readonly VisualElement _container;
        private string _fleetName;
        private readonly DeploymentSettings _model;
        private readonly Button _deployButton;
        private readonly Button _redeployButton;
        private readonly Button _deleteButton;
        private readonly Button _launchClientButton;
        private readonly FleetTypeInput _fleetTypeInput;
        private readonly FleetParametersInput _fleetParamsInput;

        public EC2Page(VisualElement container)
        {
            _container = container;
            _model = DeploymentSettingsFactory.Create();
            _model.Restore();
            _model.Refresh();
            var parameters = new FleetParameters
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

            var ec2Deployment = new EC2Deployment(_model, parameters);

            _fleetTypeInput =
                new FleetTypeInput(container, FleetTypeInput.ScenarioIndexMap[_model.ScenarioIndex], true);
            _fleetTypeInput.SetEnabled(true);
            _fleetTypeInput.OnValueChanged += value => { Debug.Log($"Fleet type changed to {value}"); };

            container.Q<Foldout>("EC2ParametersSection").text = $"{Application.productName} parameters";
            _fleetParamsInput = new FleetParametersInput(container, parameters);
            _fleetParamsInput.OnValueChanged += param =>
            {
                ec2Deployment.UpdateModelFromParameters();
                UpdateGUI();
            };

            _deployButton = container.Q<Button>("EC2CreateStackButton");
            _deployButton.RegisterCallback<ClickEvent>(_ =>
            {
                ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _redeployButton = container.Q<Button>("EC2RedeployStackButton");
            _redeployButton.RegisterCallback<ClickEvent>(_ =>
            {
                ec2Deployment.StartDeployment();
                UpdateGUI();
            });
            _deleteButton = container.Q<Button>("EC2DeleteStackButton");
            _deleteButton.RegisterCallback<ClickEvent>(async _ =>
            {
                await ec2Deployment.DeleteDeployment();
                UpdateGUI();
            });
            _launchClientButton = container.Q<Button>("EC2LaunchClientButton");
            _launchClientButton.RegisterCallback<ClickEvent>(_ => EditorApplication.EnterPlaymode());

            _model.CurrentStackInfoChanged += UpdateGUI;
            _model.ScenarioIndex = 1;
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            _deployButton.SetEnabled(_model.CurrentStackInfo.StackStatus == null && _model.CanDeploy);
            _redeployButton.SetEnabled(_model.CurrentStackInfo.StackStatus != null && _model.CanDeploy);
            _deleteButton.SetEnabled(_model.CurrentStackInfo.StackStatus != null && _model.IsCurrentStackModifiable);
            _launchClientButton.SetEnabled(_model.CurrentStackInfo.StackStatus is StackStatus.CreateComplete or StackStatus.UpdateComplete);

            _fleetTypeInput.SetEnabled(_model.CanDeploy);
            _fleetParamsInput.SetEnabled(_model.CanDeploy);
            _container.Q<Label>("EC2DeploymentStatusLabel").text = _model.CurrentStackInfo.StackStatus;
        }
        
        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
        }
    }
}