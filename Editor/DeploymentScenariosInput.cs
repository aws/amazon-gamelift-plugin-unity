// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class DeploymentScenariosInput : StatefulInput
    {
        private InputState _inputState;
        private bool _enabled;
        private DeploymentScenarios _deploymentScenarios;

        private readonly VisualElement _container;
        private readonly VisualElement _radio2Group;
        private readonly VisualElement _radio3Group;
        private readonly Button _showMoreScenariosButton;

        public Action<DeploymentScenarios> OnValueChanged;

        public static readonly Dictionary<int, DeploymentScenarios> ScenarioIndexMap = new()
        {
            { 1, DeploymentScenarios.SingleRegion },
            { 3, DeploymentScenarios.SpotFleet },
            { 4, DeploymentScenarios.FlexMatch },
        };

        public DeploymentScenariosInput(VisualElement container, DeploymentScenarios initialValue, bool enabled)
        {
            _container = container;
            _inputState = initialValue == DeploymentScenarios.SingleRegion ? InputState.Initial : InputState.Expanded;
            _deploymentScenarios = initialValue;
            _enabled = enabled;
            _container.SetEnabled(_enabled);

            _radio2Group = container.Q("FleetTypeRadioButton2Group");
            _radio3Group = container.Q("FleetTypeRadioButton3Group");

            SetupRadioButton("EC2SingleFleetRadio", DeploymentScenarios.SingleRegion);
            SetupRadioButton("EC2SpotFleetRadio", DeploymentScenarios.SpotFleet);
            SetupRadioButton("EC2FlexFleetRadio", DeploymentScenarios.FlexMatch);
            PopulateFleetTypeVisualElements();
            _showMoreScenariosButton = container.Q<Button>("ShowMoreScenarios");
            _showMoreScenariosButton.RegisterCallback<ClickEvent>(e =>
            {
                _inputState = InputState.Expanded;
                UpdateGUI();
            });

            UpdateGUI();
        }

        private enum InputState
        {
            Initial,
            Expanded,
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            _container.SetEnabled(_enabled);
        }

        private void SetupRadioButton(string elementName, DeploymentScenarios radioValue)
        {
            var radio = _container.Q<RadioButton>(elementName);
            if (radio == default) return;
            radio.value = _deploymentScenarios == radioValue;
            radio.RegisterValueChangedCallback(v =>
            {
                if (_enabled && v.newValue)
                {
                    _deploymentScenarios = radioValue;
                    OnValueChanged?.Invoke(_deploymentScenarios);
                }
            });
        }

        private List<VisualElement> GetExtraScenarioElements()
        {
            return new List<VisualElement>()
            {
                _showMoreScenariosButton,
                _radio2Group,
                _radio3Group
            };
        }

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _inputState switch
            {
                InputState.Initial => new List<VisualElement>() {_showMoreScenariosButton},
                InputState.Expanded => new List<VisualElement>() { _radio2Group, _radio3Group},
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in GetExtraScenarioElements())
            {
                if (elements.Contains(element)) {
                    Show(element);
                } else {
                    Hide(element);
                }
            }
        }
    }
}