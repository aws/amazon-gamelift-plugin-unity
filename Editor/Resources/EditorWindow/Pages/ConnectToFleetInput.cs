using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class ConnectToFleetInput : StatefulInput
    {
        public string FleetId;
        
        private TextField _fleetNameInput;
        private DropdownField _fleetNameDropdownContainer;
        private VisualElement _fleetCreateFoldout;
        private VisualElement _fleetConnectFoldout;
        private VisualElement _fleetId;
        private Label _fleetIdText;
        private VisualElement _fleetStatus;
        private readonly GameLiftRequestAdapter _requestAdapter;
        private readonly GameLiftPlugin _gameLiftPlugin;
        private Button _cancelButton;
        
        private FleetStatus _fleetState;
        private List<FleetAttributes> _fleetsList;
        private static readonly List<string> s_fleetNameList = new();

        public ConnectToFleetInput(VisualElement container, GameLiftPlugin gameLiftPlugin, FleetStatus initialState)
        {
            _fleetState = initialState;
            _gameLiftPlugin = gameLiftPlugin;
            _requestAdapter = new GameLiftRequestAdapter(_gameLiftPlugin);
            _gameLiftPlugin.SetupWrapper();

            AssignUiElements(container);
            RegisterCallBacks(container);
            SetupBootMenu();
            
            UpdateGUI();
        }
        
        private async Task OnAnywhereConnectClicked(string text)
        {
            if (_fleetState is FleetStatus.CreatingInitial or FleetStatus.Creating)
            {
                var success = await _requestAdapter?.CreateAnywhereFleet(text)!;
                if (success)
                {
                    _fleetState = FleetStatus.Selected;
                }
            }
            
            UpdateGUI();
        }

        private void OnCreateNewFleetClicked()
        {
            if (_fleetState is FleetStatus.Selected or FleetStatus.Selecting)
            {
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
            container.Q<Button>("ButtonAnywhereConnectButton").RegisterCallback<ClickEvent>(async _ => 
                await OnAnywhereConnectClicked(_fleetNameInput.text));
            container.Q<Button>("AnywhereFleetCreateNewButton").RegisterCallback<ClickEvent>(_ => 
                OnCreateNewFleetClicked());
            _fleetNameDropdownContainer.RegisterValueChangedCallback(evt => OnSelectFleetDropdown());
            _cancelButton.RegisterCallback<ClickEvent>(_ => OnCancelButtonClicked());
        }

        private void AssignUiElements(VisualElement container)
        {
            _fleetNameInput = container.Q<TextField>("CreateAnywhereFleetField");
            _fleetNameDropdownContainer = container.Q<DropdownField>("DropdownConnectAnywhereFleet");
            _fleetId = container.Q("AnywhereFleetID");
            _fleetIdText = container.Q<Label>("LabelAnywhereConnectedFleetIDValue");
            _fleetStatus = container.Q("AnywhereFleetStatus");
            _fleetCreateFoldout = container.Q("FoldoutCreateFleet");
            _fleetConnectFoldout = container.Q("FoldoutConnectFleet");
            _cancelButton = container.Q<Button>("AnywhereFleetCancelButton");
        }
        
        private async Task SetupFleetMenu()
        {
            await UpdateFleetMenu();
            _fleetNameDropdownContainer.RegisterValueChangedCallback(_ =>
                {
                    _gameLiftPlugin.CurrentState.SelectedFleetIndex = _fleetNameDropdownContainer.index;
                    var currentFleet = _fleetsList[_gameLiftPlugin.CurrentState.SelectedFleetIndex];
                    _fleetIdText.text = currentFleet.FleetId;
                    FleetId = currentFleet.FleetId;
                    _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.SelectedFleetIndex,
                        _gameLiftPlugin.CurrentState.SelectedFleetIndex.ToString());
                }
            );
        }
        
        private async Task UpdateFleetMenu()
        {
            if (_gameLiftPlugin.GameLiftWrapper != null)
            {
                _fleetsList = await _requestAdapter.ListFleets();
                s_fleetNameList.Clear();
                _fleetsList.ForEach(fleet => s_fleetNameList.Add(fleet.Name));
                _fleetNameDropdownContainer.choices = s_fleetNameList;
            }
        }
        
        private async void SetupBootMenu()
        {
            await SetupFleetMenu();
            
            if (_fleetsList.Count >= 1)
            {
                _fleetState = FleetStatus.Selecting;
            }
            _fleetNameDropdownContainer.index = _gameLiftPlugin.CurrentState.SelectedFleetIndex;
            UpdateGUI();
        }

        protected sealed override void UpdateGUI()
        {
            switch (_fleetState)
            {
                case FleetStatus.Creating:
                    Show(_fleetNameInput);
                    Show(_cancelButton);
                    Show(_fleetCreateFoldout);
                    Hide(_fleetNameDropdownContainer);
                    Hide(_fleetConnectFoldout);
                    break;
                case FleetStatus.CreatingInitial:
                    Show(_fleetNameInput);
                    Show(_fleetCreateFoldout);
                    Hide(_cancelButton);
                    Hide(_fleetConnectFoldout);
                    break;
                case FleetStatus.Selecting:
                    Show(_fleetNameDropdownContainer);
                    Show(_fleetConnectFoldout);
                    Hide(_fleetNameInput);
                    Hide(_fleetId);
                    Hide(_fleetStatus);
                    Hide(_fleetCreateFoldout);
                    break;
                case FleetStatus.Selected:
                    Show(_fleetNameDropdownContainer);
                    Show(_fleetId);
                    Show(_fleetStatus);
                    Show(_fleetConnectFoldout);
                    Hide(_fleetNameInput);
                    Hide(_fleetCreateFoldout);
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