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
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameLiftAnywhereTab : Tab
{
    private GameLiftPlugin GameLiftConfig;
    private string _fleetName;
    private string _computeName;
    private string _ipAddress;
    private string _fleetId;
    private string _fleetLocation = "custom-location-1";
    private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
    private CoreApi _coreApi;
    private AmazonGameLiftWrapper _gameLiftWrapper;
    private List<FleetAttributes> fleetsList;
    private static List<string> fleetNameList = new();
    private DropdownField fleetDropdown;

    public GameLiftAnywhereTab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        GameLiftConfig = gameLiftConfig;
        Root = root;
        TabNumber = 3;
        _coreApi = CoreApi.SharedInstance;
        var credentials = _coreApi.RetrieveAwsCredentials(GameLiftConfig.CurrentState.SelectedProfile);
        AmazonGameLiftClient client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
        _gameLiftWrapper = new AmazonGameLiftWrapper(client);
        SetupTab();
    }


    private async void SetupTab()
    {
        var tabName = "Tab3";
        base.SetupTab(tabName, OnTabButtonClicked);
        
        SetupButtons();
        await SetupFleetMenu();
        SetupBootMenu();
    }
    private void SetupBootMenu()
    {
        VisualElement targetFoldout;
        if (fleetsList.Count < 1)
        {
            targetFoldout = GetFoldout("Connect");
        }
        else
        {
            targetFoldout = GetFoldout("Connected");
        }
        ChangeFoldout(null, targetFoldout);
    }

    private async Task SetupFleetMenu()
    {
        fleetDropdown = Root.Q<DropdownField>("FleetDropdown");
        var fleetIdLabel = Root.Q<Label>("FleetIdLabel");
        
        await UpdateFleetMenu();
        fleetDropdown.RegisterValueChangedCallback(evt =>
            {
                GameLiftConfig.CurrentState.SelectedFleetIndex = fleetDropdown.index;
                var currentFleet = fleetsList[GameLiftConfig.CurrentState.SelectedFleetIndex];
                fleetIdLabel.text = currentFleet.FleetId;
                _fleetId = currentFleet.FleetId;
                _fleetName = currentFleet.Name;
            }
        ); 
        fleetDropdown.index = 0; //TODO Save this index in config to read from
    }
    
    private async Task UpdateFleetMenu()
    {
        fleetsList = await ListFleets();
        fleetNameList.Clear();
        fleetsList.ForEach(fleet => fleetNameList.Add(fleet.Name));
        fleetDropdown.choices = fleetNameList;
    }

    private void SetupComputeMenu()
    {
        
    }

    private void SetupButtons()
    {
        var fleetTextField = Root.Q<TextField>("CreateAnywhereFleetField");
        var fleetTextButton = Root.Q<Button>("CreateAnywhereFleet");
        fleetTextField.RegisterValueChangedCallback(_ =>  SetupFleetButton(fleetTextField,fleetTextButton));
        
        var registerComputeTextButton = Root.Q<Button>("RegisterComputeButton");
        var registerComputeTextField = Root.Q<TextField>("RegisterComputeField");
        var ipTextFields = Root.Query<TextField>("IpAddress").ToList();
        
        registerComputeTextField.RegisterValueChangedCallback(_ =>
            SetupCompute(registerComputeTextField, ipTextFields, registerComputeTextButton));
            
        foreach (var ipField in ipTextFields)
        {
            ipField.RegisterValueChangedCallback(_ =>
                SetupCompute(registerComputeTextField, ipTextFields, registerComputeTextButton));
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
        var ipAddress = ipTextField.ToList();
        var ipTextFieldsValid = ipAddress.All(text => text.value.Length >= 1);
        _ipAddress = String.Join(".", ipAddress);
        _computeName = computeTextName.value;
        ToggleButtons(button, computeTextNameValid && ipTextFieldsValid);
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
                    UpdateFleetMenu();
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
                var success = await RegisterFleetCompute(_computeName, _fleetId, _fleetLocation, _ipAddress);
                if(!success)
                {
                    ToggleButtons(button, true);
                }
                else
                {
                    var launchClientButton = Root.Q<Button>("LaunchClient");
                    ToggleButtons(launchClientButton, true);
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
        await CreateCustomLocationIfNotExists(_fleetLocation);
        var fleetId = await CreateFleet(ComputeType.ANYWHERE, _fleetLocation);
        var fleetNameResponse = _coreApi.PutSetting(SettingsKeys.FleetName, fleetName);
        var fleetIdResponse = _coreApi.PutSetting(SettingsKeys.FleetId, fleetId);
        var customLocationNameResponse = _coreApi.PutSetting(SettingsKeys.CustomLocationName, _fleetLocation);
        //var authTokenResponse = _coreApi.PutSetting(SettingsKeys.AuthToken, authToken); doesn't happen here might not even need
        return fleetNameResponse.Success && customLocationNameResponse.Success && fleetIdResponse.Success;
    }
    
    private async Task<bool> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation, string ipAddress)
    {
        var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
        var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
        var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
        var webSocketResponse = _coreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);

        return computeNameResponse.Success && ipAddressResponse.Success && webSocketResponse.Success;
    }

    private async Task CreateCustomLocationIfNotExists(string fleetLocation)
    {
        try
        {
            var listLocationsResponse = await _gameLiftWrapper.ListLocations(new ListLocationsRequest
            {
                Filters = new List<string>{ "CUSTOM" }
            });
                
            var foundLocation = listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

            if (foundLocation == null)
            {
                var createLocationResponse = await _gameLiftWrapper.CreateLocation(new CreateLocationRequest()
                {
                    LocationName = fleetLocation
                });
                    
                if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Created Custom Location {fleetLocation}");
                }
            }
        }
        catch (Exception ex)
        {
            var errorBox = Root.Q<VisualElement>("FleetErrorInfoBox");
            errorBox.style.display = DisplayStyle.Flex;
            errorBox.Q<Label>().text = ex.Message;
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
            var createFleetResponse = await _gameLiftWrapper.CreateFleet(createFleetRequest);
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
            var listFleetResponse = await _gameLiftWrapper.ListFleets(listFleetRequest);

            var describeFleetRequest = new DescribeFleetAttributesRequest()
            {
                FleetIds = listFleetResponse.FleetIds
            };

            var describeFleetResponse = await _gameLiftWrapper.DescribeFleets(describeFleetRequest);
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
            var registerComputeResponse = await _gameLiftWrapper.RegisterCompute(registerComputeRequest);
                
            return registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
        }
        catch (Exception ex)
        {
            var errorBox = Root.Q<VisualElement>("ComputeErrorInfoBox");
            errorBox.style.display = DisplayStyle.Flex;
            errorBox.Q<Label>().text = ex.Message;
            return null;
        }
    }
}
