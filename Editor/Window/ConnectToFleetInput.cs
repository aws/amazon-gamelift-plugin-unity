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
        
        public string FleetId;
        
        private static readonly List<string> s_fleetNameList = new();
        
        private TextField _fleetNameInput;
        private DropdownField _fleetNameDropdownContainer;
        private VisualElement _fleetCreateFoldout;
        private VisualElement _fleetConnectFoldout;
        private VisualElement _fleetId;
        private Label _fleetIdText;
        private VisualElement _fleetStatus;
        private readonly VisualElement _container;
        private readonly GameLiftFleetManager _fleetManager;
        private readonly StateManager _stateManager;
        private Button _cancelButton;
        private StatusBox _connectToAnywhereErrorBox;
        
        private FleetStatus _fleetState;
        private List<FleetAttributes> _fleetsList;

        public ConnectToFleetInput(VisualElement container, StateManager stateManager, FleetStatus initialState)
        {
            _container = container;
            _fleetState = initialState;
            _stateManager = stateManager;
            _fleetManager = stateManager.FleetManager;
            
            
            AssignUiElements(container);
            PopulateFleetVisualElements();
            RegisterCallBacks(container);
            SetupBootMenu();
            SetupStatusBox();
            
            UpdateGUI();
        }
        
        private async Task OnAnywhereConnectClicked(string text)
        {
            if (_fleetState is FleetStatus.NotCreated or FleetStatus.Creating)
            {
                var response = await _fleetManager?.CreateAnywhereFleet(text)!;
                if (response.Success)
                {
                    _fleetState = FleetStatus.Selected;
                }
                else
                {
                    _connectToAnywhereErrorBox.AddExternalButton(Urls.AwsIAMConsole, "View IAM console");
                    _connectToAnywhereErrorBox.Show(response.ErrorMessage);
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
        
        private void OnSelectFleetDropdown()
        {
            _fleetState = FleetStatus.Selected;
            
            UpdateGUI();
        }

        private void RegisterCallBacks(VisualElement container)
        {
            container.Q<Button>("AnywherePageCreateFleetButton").RegisterCallback<ClickEvent>(async _ => 
                await OnAnywhereConnectClicked(_fleetNameInput.text));
            container.Q<Button>("AnywherePageConnectFleetNewButton").RegisterCallback<ClickEvent>(async _ => 
                await OnCreateNewFleetClicked());
            _fleetNameDropdownContainer.RegisterValueChangedCallback(evt => OnSelectFleetDropdown());
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
        
        private async Task SetupFleetMenu()
        {
            await UpdateFleetMenu();
            _fleetNameDropdownContainer.RegisterValueChangedCallback(_ =>
                {
                    _stateManager.SelectedFleetName = _fleetNameDropdownContainer.value;
                    var currentFleet = _fleetsList.First(fleet => fleet.Name == _fleetNameDropdownContainer.value);
                    _fleetIdText.text = currentFleet.FleetId;
                    FleetId = currentFleet.FleetId;
                    _stateManager.CoreApi.PutSetting(SettingsKeys.SelectedFleetName, currentFleet.Name);
                }
            );
        }
        
        private async Task UpdateFleetMenu()
        {
            if (_stateManager.GameLiftWrapper != null)
            {
                var fleetsListResponse = await _fleetManager?.ListFleetAttributes()!;
                if (fleetsListResponse.Success)
                {
                    _fleetsList = fleetsListResponse.FleetAttributes;
                    s_fleetNameList.Clear();
                    _fleetsList.ForEach(fleet => s_fleetNameList.Add(fleet.Name));
                    _fleetNameDropdownContainer.choices = s_fleetNameList;
                }
                else
                {
                    _connectToAnywhereErrorBox.AddExternalButton(Urls.AwsIAMConsole, "View IAM console");
                    _connectToAnywhereErrorBox.Show(fleetsListResponse.ErrorMessage);
                }
            }
        }
        
        private async void SetupBootMenu()
        {
            await SetupFleetMenu();
            
            if (_fleetsList.Count >= 1)
            {
                _fleetState = FleetStatus.Selecting;
            }
            _fleetNameDropdownContainer.index = _fleetNameDropdownContainer.choices.IndexOf(_stateManager.SelectedFleetName);
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
        
        private void SetupStatusBox()
        {
            _connectToAnywhereErrorBox = new StatusBox(StatusBox.StatusBoxType.Error);
            var errorContainer = _container.Q("AnywherePageConnectFleetErrorContainer");
            errorContainer.Add(_connectToAnywhereErrorBox);
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in _fleetVisualElements)
            {
                if (elements.Contains(element)) {
                    Show(element);
                } else {
                    Hide(element);
                }
            }
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