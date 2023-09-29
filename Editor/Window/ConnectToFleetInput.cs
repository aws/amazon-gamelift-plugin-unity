using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using Editor.CoreAPI;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class ConnectToFleetInput : StatefulInput
    {
        private static IReadOnlyCollection<VisualElement> _fleetVisualElements;

        // public string FleetId;

        private TextField _fleetNameInput;
        private DropdownField _fleetNameDropdownContainer;
        private VisualElement _fleetCreateFoldout;
        private VisualElement _fleetConnectFoldout;
        private VisualElement _fleetId;
        private Label _fleetIdText;
        private VisualElement _fleetStatus;
        private GameLiftFleetManager _fleetManager => _stateManager.FleetManager;
        private readonly StateManager _stateManager;
        private Button _cancelButton;

        private readonly VisualElement _container;
        private FleetStatus _fleetState;
        private List<FleetAttributes> _fleetsList = new List<FleetAttributes>();

        public ConnectToFleetInput(VisualElement container, StateManager stateManager, FleetStatus initialState)
        {
            _container = container;
            _fleetState = initialState;
            _stateManager = stateManager;

            AssignUiElements(container);
            PopulateFleetVisualElements();
            RegisterCallBacks(container);
            SetupBootMenu();
            _stateManager.OnProfileSelected += () => UpdateFleetMenu();

            UpdateGUI();
        }

        private async Task OnAnywhereConnectClicked(string fleetName)
        {
            if (_fleetState is FleetStatus.NotCreated or FleetStatus.Creating)
            {
                var response = await _fleetManager?.CreateAnywhereFleet(fleetName)!;
                if (response.Success)
                {
                    _stateManager.SelectedProfile.FleetName = response.FleetName;
                    _stateManager.SelectedProfile.FleetId = response.FleetId;
                    await UpdateFleetMenu();
                    _fleetNameDropdownContainer.value = fleetName;
                    _fleetState = FleetStatus.Selected;
                }
            }

            UpdateGUI();
        }

        private async Task OnCreateNewFleetClicked()
        {
            if (_fleetState is FleetStatus.Selected or FleetStatus.Selecting)
            {
                await UpdateFleetMenu();
                _fleetState = FleetStatus.Creating;
            }

            UpdateGUI();
        }

        private void OnCancelButtonClicked()
        {
            if (_fleetState is FleetStatus.Creating)
            {
                _fleetState = FleetStatus.Selected;
            }

            UpdateGUI();
        }

        private void OnSelectFleetDropdown(string fleetName)
        {
            var currentFleet = _fleetsList.First(fleet => fleet.Name == fleetName);
            _fleetIdText.text = currentFleet.FleetId;
            _stateManager.FleetName = currentFleet.Name;
            _stateManager.FleetId = currentFleet.FleetId;

            _fleetState = FleetStatus.Selected;

            UpdateGUI();
        }

        private void RegisterCallBacks(VisualElement container)
        {
            container.Q<Button>("AnywherePageCreateFleetButton").RegisterCallback<ClickEvent>(async _ =>
                await OnAnywhereConnectClicked(_fleetNameInput.text));
            container.Q<Button>("AnywherePageConnectFleetNewButton").RegisterCallback<ClickEvent>(async _ =>
                await OnCreateNewFleetClicked());
            _fleetNameDropdownContainer.RegisterValueChangedCallback(evt => OnSelectFleetDropdown(evt.newValue));
            _cancelButton.RegisterCallback<ClickEvent>(_ => OnCancelButtonClicked());
        }

        private void AssignUiElements(VisualElement container)
        {
            _fleetNameInput = container.Q<TextField>("AnywherePageCreateFleetInput");
            _fleetNameDropdownContainer = container.Q<DropdownField>("AnywherePageConnectFleetDropdown");
            _fleetId = container.Q("AnywherePageConnectFleetID");
            _fleetIdText = container.Q<Label>("AnywherePageConnectFleetIDDisplay");
            _fleetStatus = container.Q("AnywherePageConnectFleetStatus");
            _fleetCreateFoldout = container.Q("AnywherePageCreateFleetTitle");
            _fleetConnectFoldout = container.Q("AnywherePageConnectFleetTitle");
            _cancelButton = container.Q<Button>("AnywherePageCreateFleetCancelButton");
        }

        private async Task UpdateFleetMenu()
        {
            if (_stateManager.GameLiftWrapper != null)
            {
                _fleetsList = await _fleetManager.ListFleetAttributes() ?? new List<FleetAttributes>();
                _fleetNameDropdownContainer.choices = _fleetsList.Select(fleet => fleet.Name).ToList();
                _fleetNameDropdownContainer.value = _stateManager.FleetName;
                _fleetIdText.text = _stateManager.FleetId;
            }
        }

        private async void SetupBootMenu()
        {
            await UpdateFleetMenu();

            if (_fleetsList.Count >= 1 && string.IsNullOrWhiteSpace(_stateManager.FleetName))
            {
                _fleetState = FleetStatus.Selecting;
            }

            UpdateGUI();
        }

        private void PopulateFleetVisualElements()
        {
            _fleetVisualElements = new List<VisualElement>()
            {
                _fleetNameInput,
                _fleetCreateFoldout,
                _fleetNameDropdownContainer,
                _cancelButton,
                _fleetConnectFoldout,
                _fleetId,
                _fleetStatus,
            };
        }

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _fleetState switch
            {
                FleetStatus.NotCreated => new List<VisualElement>() { _fleetNameInput, _fleetCreateFoldout },
                FleetStatus.Creating => new List<VisualElement>()
                {
                    _fleetNameInput, _cancelButton, _fleetCreateFoldout
                },
                FleetStatus.Selecting => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetConnectFoldout
                },
                FleetStatus.Selected => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetId, _fleetStatus, _fleetConnectFoldout
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in _fleetVisualElements)
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

            _container.SetEnabled(_stateManager.IsBootstrapped);
        }

        public enum FleetStatus
        {
            NotCreated,
            Creating,
            Selecting,
            Selected
        }
    }
}