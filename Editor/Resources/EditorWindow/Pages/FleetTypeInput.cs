using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    internal class FleetTypeInput : StatefulInput
    {
        private InputState _inputState;
        private bool _enabled;
        private FleetType _fleetType;

        private readonly VisualElement _container;
        private readonly VisualElement _radio2Group;
        private readonly VisualElement _radio3Group;
        private readonly Button _button;

        public Action<FleetType> OnValueChanged;

        public static readonly Dictionary<int, FleetType> ScenarioIndexMap = new()
        {
            { 1, FleetType.SingleRegion },
            { 4, FleetType.SpotFleet },
            { 5, FleetType.FlexMatch },
        };

        public FleetTypeInput(VisualElement container, FleetType initialValue, bool enabled)
        {
            _container = container;
            _inputState = initialValue == FleetType.SingleRegion ? InputState.Initial : InputState.Expanded;
            _fleetType = initialValue;
            _enabled = enabled;
            _container.SetEnabled(_enabled);

            _radio2Group = container.Q("FleetTypeRadioButton2Group");
            _radio3Group = container.Q("FleetTypeRadioButton3Group");

            SetupRadioButton("EC2SingleFleetRadio", true, FleetType.SingleRegion);
            SetupRadioButton("EC2SpotFleetRadio", false, FleetType.SpotFleet);
            SetupRadioButton("EC2FlexFleetRadio", false, FleetType.FlexMatch);

            _button = container.Q<Button>("ShowMoreScenarios");
            _button.RegisterCallback<ClickEvent>(e =>
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

        private void SetupRadioButton(string elementName, bool value, FleetType radioValue)
        {
            var radio = _container.Q<RadioButton>(elementName);
            if (radio == default) return;
            radio.value = value;
            radio.RegisterValueChangedCallback(v =>
            {
                if (_enabled && v.newValue)
                {
                    _fleetType = radioValue;
                    OnValueChanged?.Invoke(_fleetType);
                }
            });
        }

        protected sealed override void UpdateGUI()
        {
            switch (_inputState)
            {
                case InputState.Initial:
                    Show(_button);
                    Hide(_radio2Group);
                    Hide(_radio3Group);
                    break;
                case InputState.Expanded:
                    Show(_radio2Group);
                    Show(_radio3Group);
                    Hide(_button);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}