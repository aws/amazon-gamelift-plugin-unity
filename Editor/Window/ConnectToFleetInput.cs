// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ConnectToFleetInput : StatefulInput
    {
        private TextField _fleetNameInput;
        private DropdownField _fleetNameDropdownContainer;
        private VisualElement _fleetCreateContainer;
        private VisualElement _fleetConnectContainer;
        private VisualElement _fleetId;
        private Label _fleetIdText;
        private VisualElement _fleetStatus;
        private StatusIndicator _statusIndicator;
        private GameLiftFleetManager _fleetManager => _stateManager.FleetManager;
        private readonly StateManager _stateManager;
        private readonly VisualElement _container;
        private Button _cancelButton;

        private FleetStatus _fleetState;
        private List<FleetAttributes> _fleetAttributes = new List<FleetAttributes>();
        private StatusBox _connectToAnywhereStatusBox;

        public ConnectToFleetInput(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/ConnectToFleetInput");
            container.Add(uxml.Instantiate());
            _container = container;

            _stateManager = stateManager;
            _fleetState = FleetStatus.NotCreated;

            AssignUiElements(container);
            RegisterCallBacks(container);
            SetupPage();
            SetupStatusBox();
            LocalizeText();
            _stateManager.OnUserProfileUpdated += () => UpdateFleetMenu();

            UpdateGUI();
        }

        private async Task OnAnywhereConnectClicked(string fleetName)
        {
            if (_fleetState is FleetStatus.NotCreated or FleetStatus.Creating)
            {
                _connectToAnywhereStatusBox.Close();

                var response = await _fleetManager?.CreateFleet(fleetName)!;
                if (response.Success)
                {
                    _stateManager.AnywhereFleetName = response.FleetName;
                    _stateManager.AnywhereFleetId = response.FleetId;
                    await UpdateFleetMenu();
                    _fleetNameDropdownContainer.value = fleetName;
                    _fleetState = FleetStatus.Selected;
                }
                else
                {
                    var url = string.Format(Urls.AwsGameLiftLogs, _stateManager.Region);
                    _connectToAnywhereStatusBox.Show(StatusBox.StatusBoxType.Error,
                        Strings.AnywherePageStatusBoxDefaultErrorText, response.ErrorMessage, url,
                        Strings.ViewLogsStatusBoxUrlTextButton);
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
            var currentFleet = _fleetAttributes.FirstOrDefault(fleet => fleet.Name == fleetName);
            if (currentFleet != null)
            {
                _fleetIdText.text = currentFleet.FleetId;
                _stateManager.AnywhereFleetName = currentFleet.Name;
                _stateManager.AnywhereFleetId = currentFleet.FleetId;
                _fleetState = FleetStatus.Selected;
            }

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
            _fleetNameInput = container.Q<TextField>("AnywherePageCreateFleetNameInput");
            _fleetNameDropdownContainer = container.Q<DropdownField>("AnywherePageConnectFleetNameDropdown");
            _fleetId = container.Q("AnywherePageConnectFleetID");
            _fleetIdText = container.Q<Label>("AnywherePageConnectFleetIDDisplay");
            _fleetStatus = container.Q("AnywherePageConnectFleetStatus");
            _fleetCreateContainer = container.Q("AnywherePageCreateFleet");
            _fleetConnectContainer = container.Q("AnywherePageConnectFleet");
            _cancelButton = container.Q<Button>("AnywherePageCreateFleetCancelButton");
            _statusIndicator = container.Q<StatusIndicator>();
        }

        private async Task UpdateFleetMenu()
        {
            if (_stateManager.GameLiftWrapper != null)
            {
                _fleetAttributes = await _fleetManager.DescribeFleetAttributes(ComputeType.ANYWHERE);
                if (_fleetAttributes == null)
                {
                    _fleetAttributes = new List<FleetAttributes>();
                }

                _fleetNameDropdownContainer.choices = _fleetAttributes.Select(fleet => fleet.Name).ToList();
                _fleetNameDropdownContainer.value = _stateManager.AnywhereFleetName;
                _fleetIdText.text = _stateManager.AnywhereFleetId;

                var fleet = _fleetAttributes.FirstOrDefault(fleet => fleet.Name == _stateManager.AnywhereFleetName);
                if (fleet != null)
                {
                    var textProvider = new TextProvider();
                    if (fleet.Status == Amazon.GameLift.FleetStatus.ERROR)
                    {
                        _statusIndicator.Set(State.Failed,
                            textProvider.Get(Strings.AnywherePageConnectFleetStatusError));
                    }
                    else
                    {
                        _statusIndicator.Set(State.Success,
                            textProvider.Get(Strings.AnywherePageConnectFleetStatusActive));
                    }
                }
            }
        }

        private async void SetupPage()
        {
            await UpdateFleetMenu();

            if (_fleetAttributes.Count >= 1 && string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetName))
            {
                _fleetState = FleetStatus.Selecting;
            }

            UpdateGUI();
        }

        private List<VisualElement> GetFleetVisualElements() => new List<VisualElement>()
        {
            _fleetNameInput,
            _fleetCreateContainer,
            _fleetNameDropdownContainer,
            _cancelButton,
            _fleetConnectContainer,
            _fleetId,
            _fleetStatus,
        };

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _fleetState switch
            {
                FleetStatus.NotCreated => new List<VisualElement>() { _fleetNameInput, _fleetCreateContainer },
                FleetStatus.Creating => new List<VisualElement>()
                {
                    _fleetNameInput, _cancelButton, _fleetCreateContainer
                },
                FleetStatus.Selecting => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetConnectContainer
                },
                FleetStatus.Selected => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetId, _fleetStatus, _fleetConnectContainer
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SetupStatusBox()
        {
            _connectToAnywhereStatusBox = new StatusBox();
            var statusBoxContainer = _container.Q("AnywherePageConnectFleetStatusBoxContainer");
            statusBoxContainer.Add(_connectToAnywhereStatusBox);
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in GetFleetVisualElements())
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

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageCreateFleetNameLabel", Strings.AnywherePageCreateFleetNameLabel);
            l.SetElementText("AnywherePageConnectFleetName", Strings.AnywherePageConnectFleetName);
            l.SetElementText("AnywherePageConnectFleetNameLabel", Strings.AnywherePageConnectFleetNameLabel);
            l.SetElementText("AnywherePageConnectFleetIDLabel", Strings.AnywherePageConnectFleetIDLabel);
            l.SetElementText("AnywherePageConnectFleetStatusLabel", Strings.AnywherePageConnectFleetStatusLabel);
            l.SetElementText("AnywherePageConnectFleetNewButton", Strings.AnywherePageConnectFleetNewButton);
        }
    }
}