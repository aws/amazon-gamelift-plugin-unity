// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Editor.Window;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class DeploymentScenariosInput : StatefulInput
    {
        private InputState _inputState;
        private bool _enabled;
        private DeploymentScenarios _deploymentScenarios;

        private readonly VisualElement _container;
        private readonly VisualElement _spotFleetRadioGroup;
        private readonly VisualElement _flexMatchRadioGroup;
        private readonly Button _showMoreScenariosButton;

        public Action<DeploymentScenarios> OnValueChanged;
        
        public DeploymentScenariosInput(VisualElement container, DeploymentScenarios initialValue, bool enabled)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/DeploymentScenarios");
            container.Add(uxml.Instantiate());

            _container = container;
            _inputState = initialValue == DeploymentScenarios.SingleRegion ? InputState.Initial : InputState.Expanded;
            _deploymentScenarios = initialValue;
            _enabled = enabled;
            _container.SetEnabled(_enabled);

            _spotFleetRadioGroup = container.Q("ManagedEC2ScenarioSpotFleet");
            _flexMatchRadioGroup = container.Q("ManagedEC2ScenarioFlexMatch");

            SetupRadioButton("ManagedEC2ScenarioSingleFleetRadio", DeploymentScenarios.SingleRegion);
            SetupRadioButton("ManagedEC2ScenarioSpotFleetRadio", DeploymentScenarios.SpotFleet);
            SetupRadioButton("ManagedEC2ScenarioFlexMatchRadio", DeploymentScenarios.FlexMatch);
            _showMoreScenariosButton = container.Q<Button>("ManagedEC2ScenarioShowMoreButton");
            _showMoreScenariosButton.RegisterCallback<ClickEvent>(e =>
            {
                _inputState = InputState.Expanded;
                UpdateGUI();
            });

            LocalizeText();
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
                _spotFleetRadioGroup,
                _flexMatchRadioGroup
            };
        }

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _inputState switch
            {
                InputState.Initial => new List<VisualElement>() { _showMoreScenariosButton },
                InputState.Expanded => new List<VisualElement>() { _spotFleetRadioGroup, _flexMatchRadioGroup },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected sealed override void UpdateGUI()
        {
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