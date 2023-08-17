using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftPlugin.Scripts
{
    public class GameLiftAnywhereTab : Tab
    {
        private readonly GameLiftPlugin _gameLiftConfig;
        private string _fleetName;
        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _fleetId;
        private const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
        private List<FleetAttributes> _fleetsList;
        private static List<string> fleetNameList = new();
        private DropdownField _fleetDropdown;

        public GameLiftAnywhereTab(VisualElement root, GameLiftPlugin gameLiftConfig)
        {
            _gameLiftConfig = gameLiftConfig;
            Root = root;
            TabNumber = 3;
            if (_gameLiftConfig.CurrentState.SelectedProfile != null)
            {
                _gameLiftConfig.SetupWrapper();
            }
            SetupTab();
        }

        private async void SetupTab()
        {
            var tabName = "Tab3";
            base.SetupTab(tabName, OnTabButtonClicked);
            SetupConfigSettings();
            SetupButtons();
            await SetupFleetMenu();
            SetupBootMenu();
        }

        private void SetupConfigSettings()
        {
            var computeName = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.ComputeName);
            if (computeName.Success)
            {
                _computeName = computeName.Value;
            }
            
            var ip =  _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.IpAddress);
            if (ip.Success)
            {
                _ipAddress = ip.Value;
            }
            
            var selectedProfile = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.SelectedProfile);
            if (selectedProfile.Success)
            {
                _gameLiftConfig.CurrentState.SelectedProfile = selectedProfile.Value;
            }
            
            var fleetIndex = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.SelectedFleetIndex);
            if (fleetIndex.Success)
            {
                _gameLiftConfig.CurrentState.SelectedFleetIndex = int.Parse(fleetIndex.Value);
            }
        }
        
        private void SetupBootMenu()
        {
            var targetFoldout = GetFoldout(_fleetsList.Count < 1 ? "Connect" : "Connected");
            ChangeFoldout(null, targetFoldout);
        }

        public override async void OnAccountSelect()
        {
            await UpdateFleetMenu();
        }

        private async Task SetupFleetMenu()
        {
            _fleetDropdown = Root.Q<DropdownField>("FleetDropdown");
            var fleetIdLabel = Root.Q<Label>("FleetIdLabel");
        
            await UpdateFleetMenu();
            _fleetDropdown.RegisterValueChangedCallback(_ =>
                {
                    _gameLiftConfig.CurrentState.SelectedFleetIndex = _fleetDropdown.index;
                    var currentFleet = _fleetsList[_gameLiftConfig.CurrentState.SelectedFleetIndex];
                    fleetIdLabel.text = currentFleet.FleetId;
                    _fleetId = currentFleet.FleetId;
                    _fleetName = currentFleet.Name;
                    _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.SelectedFleetIndex,
                        _gameLiftConfig.CurrentState.SelectedFleetIndex.ToString());
                }
            ); 
            _fleetDropdown.index = _gameLiftConfig.CurrentState.SelectedFleetIndex;
        }

        private async Task UpdateFleetMenu()
        {
            if (_gameLiftConfig.GameLiftWrapper != null)
            {
                _fleetsList = await ListFleets();
                if (_fleetsList.Count >= 1)
                {
                    var currentFoldout = GetFoldout("Connect");
                    var targetFoldout = GetFoldout("Connected");
                    ChangeFoldout(currentFoldout, targetFoldout);
                }
                fleetNameList.Clear();
                _fleetsList.ForEach(fleet => fleetNameList.Add(fleet.Name));
                _fleetDropdown.choices = fleetNameList;
            }
        }

        private void SetupButtons()
        {
            var fleetTextField = Root.Q<TextField>("CreateAnywhereFleetField");
            var fleetTextButton = Root.Q<Button>("CreateAnywhereFleet");
            fleetTextField.RegisterValueChangedCallback(_ =>  SetupFleetButton(fleetTextField,fleetTextButton));
        
            var registerComputeTextButton = Root.Q<Button>("RegisterCompute");
            var registerComputeTextField = Root.Q<TextField>("RegisterComputeField");
            var ipTextFields = Root.Query<TextField>("IpAddress").ToList();
        
            registerComputeTextField.RegisterValueChangedCallback(_ =>
                SetupCompute(registerComputeTextField, ipTextFields, registerComputeTextButton));
            registerComputeTextField.value = _computeName;
            var index = 0;
            var currentIp = _ipAddress.Split(".");
            foreach (var ipField in ipTextFields)
            {
                ipField.value = currentIp[index];
                ipField.RegisterValueChangedCallback(_ =>
                    SetupCompute(registerComputeTextField, ipTextFields, registerComputeTextButton));
                index++;
            }
        }

        private void SetupFleetButton(TextField textField, Button button)
        {
            var fleetNameValid = CheckValidFleetName(textField.value) && textField.value != "GameName-AnywhereFleet";
            _fleetName = textField.value;
            ToggleButtons(button, fleetNameValid);
        }

        private static bool CheckValidFleetName(string text)
        {
            const string testPattern = "^[a-zA-Z_\\-]+$";
            var regex = new Regex(testPattern);
            var match = regex.Match(text);
            var correctComposition = match.Success && text.Length is >= 1 and <= 1024;
        
            return correctComposition && fleetNameList.All(fleet => fleet != text);
        }

        private void SetupCompute(TextField computeTextName, IEnumerable<TextField> ipTextField, Button button)
        {
            var computeTextNameValid = computeTextName.value.Length >= 1;
            var ipText = ipTextField.ToList().Select(ipAddressField => ipAddressField.value).ToList();
            _computeName = computeTextName.value;
            var ipTextFieldsValid = ipText.All(text => text.Length >= 1);
            ToggleButtons(button, computeTextNameValid && ipTextFieldsValid);
            CheckCompute();
        }

        private void CheckCompute()
        {
            if (DescribeCompute(_computeName, _fleetId).Result)
            {
                ComputeSuccess();
            }
        }

        private void ComputeSuccess()
        {
            var cancelButton = Root.Q<Button>("CancelCompute");
            cancelButton.style.display = DisplayStyle.Flex;
            var statusBox = Root.Q<VisualElement>("StatusBox");
            statusBox.style.display = DisplayStyle.Flex;
            var launchClientButton = Root.Q<Button>("LaunchClient");
            ToggleButtons(launchClientButton, true);
        }
    
        private async void OnTabButtonClicked(ClickEvent evt, Button button)
        {
            switch (button.name)
            {
                case "CreateAnywhereFleet":
                {
                    ToggleButtons(button, false);
                    var success = await CreateAnywhereFleet(_fleetName);
                    if (success)
                    {
                        await UpdateFleetMenu();
                        var currentFoldout = GetFoldout("Connect");
                        var targetFoldout = GetFoldout("Connected");
                        ChangeFoldout(currentFoldout, targetFoldout);
                    }
                    else
                    {
                        ToggleButtons(button, true);
                        Debug.Log("Error creating fleet");
                    }
                
                    break;
                }
                case "CreateAnotherFleet":
                {
                    var currentFoldout = GetFoldout("Connected");
                    var targetFoldout = GetFoldout("Connect");
                    ChangeFoldout(currentFoldout, targetFoldout);
                    break;
                }
                case "RegisterCompute":
                {
                    var success = await RegisterFleetCompute(_computeName, _fleetId, FleetLocation, _ipAddress);
                    if(!success)
                    {
                        ToggleButtons(button, true);
                    }
                    else
                    {
                        ComputeSuccess();
                    }
                    break;
                }
                case "CancelCompute":
                {
                    var success = await DeregisterFleetCompute(_computeName, _fleetId);
                    if(success)
                    {
                        var launchClientButton = Root.Q<Button>("LaunchClient");
                        ToggleButtons(launchClientButton, false);
                        var cancelButton = Root.Q<Button>("CancelCompute");
                        cancelButton.style.display = DisplayStyle.None;
                        var statusBox = Root.Q<VisualElement>("StatusBox");
                        statusBox.style.display = DisplayStyle.None;
                    }
                    break;
                }
                case "LaunchClient":
                {
                    //TODO Launch with Anywhere Mode On
                    EditorApplication.EnterPlaymode();
                    break;
                }
            }
        }

        private async Task<bool> CreateAnywhereFleet(string fleetName)
        {
            if (_gameLiftConfig.GameLiftWrapper != null)
            {
                var success = await CreateCustomLocationIfNotExists(FleetLocation);
                if (success)
                {
                    var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation);
                    var fleetNameResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.FleetName, fleetName);
                    var fleetIdResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.FleetId, fleetId);
                    var customLocationNameResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.CustomLocationName, FleetLocation);
                    return fleetNameResponse.Success && customLocationNameResponse.Success && fleetIdResponse.Success;
                }
                return false;
            }
            Debug.Log("Error: Missing Account Profile");
            return false;
        }
    
        private async Task<bool> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation, string ipAddress)
        {
            var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            if (webSocketUrl != null)
            {
                var computeNameResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                var ipAddressResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                var webSocketResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);
            
                return computeNameResponse.Success && ipAddressResponse.Success && webSocketResponse.Success;
            }
        
            return false;
        }
    
        private async Task<bool> DeregisterFleetCompute(string computeName, string fleetId)
        {
            var success = await DeregisterCompute(computeName, fleetId);
            if (success)
            {
                var computeNameResponse = _gameLiftConfig.CoreApi.ClearSetting(SettingsKeys.ComputeName);
                var ipAddressResponse = _gameLiftConfig.CoreApi.ClearSetting(SettingsKeys.IpAddress);
                var webSocketResponse = _gameLiftConfig.CoreApi.ClearSetting(SettingsKeys.WebSocketUrl);
                return computeNameResponse.Success && ipAddressResponse.Success && webSocketResponse.Success;
            }
        
            return false;
        }

        private async Task<bool> CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await _gameLiftConfig.GameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string>{ "CUSTOM" }
                });
                
                var foundLocation = listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await _gameLiftConfig.GameLiftWrapper.CreateLocation(new CreateLocationRequest()
                    {
                        LocationName = fleetLocation
                    });
                    
                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Created Custom Location {fleetLocation}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var errorBox = Root.Q<VisualElement>("FleetErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                return false;
            }
        }

        private async Task<string> CreateFleet(ComputeType computeType, string fleetLocation)
        {
            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    Name = _fleetName,
                    ComputeType = computeType,
                    Description = FleetDescription,
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = fleetLocation
                        }
                    }
                };
                var createFleetResponse = await _gameLiftConfig.GameLiftWrapper.CreateFleet(createFleetRequest);
                return createFleetResponse.FleetAttributes.FleetId;
            }
            catch (Exception ex)
            {
                var errorBox = Root.Q<VisualElement>("FleetErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }
    
        private async Task<List<FleetAttributes>> ListFleets()
        {
            try
            {
                var listFleetRequest = new ListFleetsRequest();
                var listFleetResponse = await _gameLiftConfig.GameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _gameLiftConfig.GameLiftWrapper.DescribeFleets(describeFleetRequest);
                return describeFleetResponse.FleetAttributes;
            }
            catch (Exception ex)
            {
                var errorBox = Root.Q<VisualElement>("FleetErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }

        private async Task<string> RegisterCompute(string computeName, string fleetId, string fleetLocation, string ipAddress)
        {
            try
            {
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                    IpAddress = ipAddress,
                    Location = fleetLocation
                };
                var registerComputeResponse = await _gameLiftConfig.GameLiftWrapper.RegisterCompute(registerComputeRequest);
                
                return registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
            }
            catch (Exception ex)
            {
                var errorBox = Root.Q<VisualElement>("ComputeErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }
    
        private async Task<bool> DeregisterCompute(string computeName, string fleetId)
        {
            try
            {
                var deregisterComputeRequest = new DeregisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId
                };
                var deregisterComputeResponse = await _gameLiftConfig.GameLiftWrapper.DeregisterCompute(deregisterComputeRequest);

                return deregisterComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
        
        private async Task<bool> DescribeCompute(string computeName, string fleetId)
        {
            try
            {
                var deregisterComputeRequest = new DescribeComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId
                };
                var deregisterComputeResponse = await _gameLiftConfig.GameLiftWrapper.DescribeCompute(deregisterComputeRequest);

                return deregisterComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
    }
}
