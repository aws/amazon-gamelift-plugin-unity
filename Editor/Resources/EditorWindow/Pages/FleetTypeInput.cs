using System;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    internal class FleetTypeInput : StatefulInput
    {
        private InputState _inputState;
        private bool _enabled;

        private readonly VisualElement _container;
        private readonly VisualElement _radio2Group;
        private readonly VisualElement _radio3Group;
        private readonly Button _button;

        public Action<FleetType> OnValueChanged;
        public FleetType FleetType { get; private set; }

        public FleetTypeInput(VisualElement container, InputState initialState, FleetType initialValue, bool enabled)
        {
            _container = container;
            _inputState = initialState;
            FleetType = initialValue;
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

        internal enum InputState
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
                    FleetType = radioValue;
                    OnValueChanged?.Invoke(FleetType);
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