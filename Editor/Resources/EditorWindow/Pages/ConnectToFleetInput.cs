using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class ConnectToFleetInput : StatefulInput
    {
        private readonly VisualElement _fleetNameInput;
        private readonly VisualElement _fleetNameDropdownContainer;
        private readonly VisualElement _fleetID;
        private readonly VisualElement _fleetStatus;

        private FleetStatus _fleetState;
        private readonly Button _cancelButton;

        public ConnectToFleetInput(VisualElement container, FleetStatus initialState)
        {
            _fleetState = initialState;

            _fleetNameInput = container.Q("LabelAnywhereConnectNoFleet");
            _fleetNameDropdownContainer = container.Q("LabelAnywhereConnectFleets");
            _fleetID = container.Q("AnywhereFleetID");
            _fleetStatus = container.Q("AnywhereFleetStatus");

            container.Q<Button>("ButtonAnywhereConnectButton").RegisterCallback<ClickEvent>(_ =>
            {
                if (_fleetState is FleetStatus.CreatingInitial or FleetStatus.Creating)
                {
                    // TODO: Add functionality
                    _fleetState = FleetStatus.Selected;
                }
                UpdateGUI();
            });

            container.Q<Button>("AnywhereFleetCreateNewButton").RegisterCallback<ClickEvent>(_ =>
            {
                if (_fleetState is FleetStatus.Selected or FleetStatus.Selecting)
                {
                    // TODO: Add functionality
                    _fleetState = FleetStatus.Creating;
                }
                UpdateGUI();

            });

            _cancelButton = container.Q<Button>("AnywhereFleetCancelButton");
            _cancelButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (_fleetState is FleetStatus.Creating)
                {
                    // TODO: Add functionality
                    _fleetState = FleetStatus.Selected;
                }
                UpdateGUI();
            });

            UpdateGUI();
        }

        protected sealed override void UpdateGUI()
        {
            switch (_fleetState)
            {
                case FleetStatus.Creating:
                    Show(_fleetNameInput);
                    Show(_cancelButton);
                    Hide(_fleetNameDropdownContainer); ;
                    break;
                case FleetStatus.CreatingInitial:
                    Show(_fleetNameInput);
                    Hide(_cancelButton);
                    Hide(_fleetNameDropdownContainer);
                    break;
                case FleetStatus.Selecting:
                    Show(_fleetNameDropdownContainer);
                    Hide(_fleetNameInput);
                    Hide(_fleetID);
                    Hide(_fleetStatus);
                    break;
                case FleetStatus.Selected:
                    Show(_fleetNameDropdownContainer);
                    Show(_fleetID);
                    Show(_fleetStatus);
                    Hide(_fleetNameInput);
                    break;
            }
        }

        public enum FleetStatus
        {
            CreatingInitial,
            Creating,
            Selecting,
            Selected
        }
    }
}