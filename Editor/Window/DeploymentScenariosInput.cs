// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class DeploymentScenariosInput : StatefulInput
    {
        private bool _isExpanded;
        private bool _enabled;
        private DeploymentScenarios _deploymentScenarios;

        private readonly VisualElement _container;
        private readonly VisualElement _spotFleetRadioGroup;
        private readonly VisualElement _flexMatchRadioGroup;
        private readonly Button _showMoreScenariosButton;

        private readonly StateManager _stateManager;

        public Action<DeploymentScenarios> OnValueChanged;
        private Dictionary<DeploymentScenarios , RadioButton> _radioButtons = new Dictionary<DeploymentScenarios, RadioButton>();

        public DeploymentScenariosInput(VisualElement container, DeploymentScenarios deploymentScenario, bool enabled, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/DeploymentScenarios");
            container.Add(uxml.Instantiate());

            _container = container;
            _isExpanded = deploymentScenario != DeploymentScenarios.SingleRegion;
            _deploymentScenarios = deploymentScenario;
            _enabled = enabled;
            _stateManager = stateManager;

            _container.SetEnabled(_enabled);

            _spotFleetRadioGroup = container.Q("ManagedEC2ScenarioSpotFleet");
            _flexMatchRadioGroup = container.Q("ManagedEC2ScenarioFlexMatch");

            SetupRadioButton("ManagedEC2ScenarioSingleFleetRadio", DeploymentScenarios.SingleRegion);
            SetupRadioButton("ManagedEC2ScenarioSpotFleetRadio", DeploymentScenarios.SpotFleet);
            SetupRadioButton("ManagedEC2ScenarioFlexMatchRadio", DeploymentScenarios.FlexMatch);

            _showMoreScenariosButton = container.Q<Button>("ManagedEC2ScenarioShowMoreButton");
            _showMoreScenariosButton.RegisterCallback<ClickEvent>(e =>
            {
                _isExpanded = true;
                UpdateGUI();
            });
            
            stateManager.OnUserProfileUpdated += UpdateGUI;
            
            _container.Q<VisualElement>("ManagedEC2ScenarioSingleFleetLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.ManagedEc2FleetLearnMore));
            _container.Q<VisualElement>("ManagedEC2ScenarioSpotFleetLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.ManagedEc2FleetLearnMore));
            _container.Q<VisualElement>("ManagedEC2ScenarioFlexMatchLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.ManagedEc2FleetLearnMore));

            LocalizeText();
            UpdateGUI();
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            _container.SetEnabled(_enabled);
        }

        private void SetupRadioButton(string elementName, DeploymentScenarios deploymentScenario)
        {
            var radio = _container.Q<RadioButton>(elementName);
            if (radio == default) return;
            _radioButtons.Add(deploymentScenario, radio);
            radio.value = _deploymentScenarios == deploymentScenario;
            radio.RegisterValueChangedCallback(v =>
            {
                if (_enabled && v.newValue)
                {
                    _deploymentScenarios = deploymentScenario;
                    OnValueChanged?.Invoke(_deploymentScenarios);
                }
            });
        }

        private List<VisualElement> GetExtraScenarioElements()
        {
            return new List<VisualElement>()
            {
                _showMoreScenariosButton,
                _spotFleetRadioGroup,
                _flexMatchRadioGroup
            };
        }

        private List<VisualElement> GetVisibleItemsByState() =>
            _isExpanded
                ? new List<VisualElement>()
                {
                    _spotFleetRadioGroup, _flexMatchRadioGroup
                }
                : new List<VisualElement>() { _showMoreScenariosButton };

        protected sealed override void UpdateGUI()
        {
            var deploymentScenario = _stateManager.DeploymentScenario;
            _radioButtons[deploymentScenario].value = true;
            if (deploymentScenario != DeploymentScenarios.SingleRegion)
            {
                _isExpanded = true;
            }

            var elements = GetVisibleItemsByState();
            foreach (var element in GetExtraScenarioElements())
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
            l.SetElementText("ManagedEC2ScenarioSingleFleetLabel", Strings.ManagedEC2ScenarioSingleFleetLabel);
            l.SetElementText("ManagedEC2ScenarioSingleFleetRadio", Strings.ManagedEC2ScenarioSingleFleetRadio);
            l.SetElementText("ManagedEC2ScenarioSingleFleetLink", Strings.ManagedEC2ScenarioSingleFleetLink);
            l.SetElementText("ManagedEC2ScenarioSpotFleetLabel", Strings.ManagedEC2ScenarioSpotFleetLabel);
            l.SetElementText("ManagedEC2ScenarioSpotFleetRadio", Strings.ManagedEC2ScenarioSpotFleetRadio);
            l.SetElementText("ManagedEC2ScenarioSpotFleetLink", Strings.ManagedEC2ScenarioSpotFleetLink);
            l.SetElementText("ManagedEC2ScenarioFlexMatchLabel", Strings.ManagedEC2ScenarioFlexMatchLabel);
            l.SetElementText("ManagedEC2ScenarioFlexMatchRadio", Strings.ManagedEC2ScenarioFlexMatchRadio);
            l.SetElementText("ManagedEC2ScenarioFlexMatchLink", Strings.ManagedEC2ScenarioFlexMatchLink);
            l.SetElementText("ManagedEC2ScenarioShowMoreButton", Strings.ManagedEC2ScenarioShowMoreButton);
        }
    }
}
