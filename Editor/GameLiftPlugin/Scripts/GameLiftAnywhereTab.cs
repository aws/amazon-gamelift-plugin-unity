using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
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
    private CoreApi _coreApi;
    private AmazonGameLiftWrapper _gameLiftWrapper;

    public GameLiftAnywhereTab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        GameLiftConfig = gameLiftConfig;
        Root = root;
        TabNumber = 3;
        _coreApi = CoreApi.SharedInstance;
        SetupTab();
    }


    private void SetupTab()
    {
        var tabName = "Tab3";
        base.SetupTab(tabName, OnTabButtonClicked);

        SetupButtons();
        //var textFieldsList = textFields.Select(textField => textField.value).ToList();
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
        ToggleButtons(button, fleetNameValid);
    }

    private static bool CheckValidFleetName(string text)
    {
        const string testPattern = "^[a-zA-Z_\\-]+$";
        var regex = new Regex(testPattern);
        var match = regex.Match(text);
        return match.Success && text.Length is >= 1 and <= 1024;
    }

    private void SetupCompute(TextField computeTextName, IEnumerable<TextField> ipTextField, Button button)
    {
        var computeTextNameValid = computeTextName.value.Length >= 1;
        var ipTextFieldsValid = ipTextField.All(text => text.value.Length >= 1);
        
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
                    var currentFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "Connect");
                    var targetFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "Connected");
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
                var currentFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "Connected");
                var targetFoldout = AllFoldoutElements.FirstOrDefault(element => element.name == "Connect");
                ChangeFoldout(currentFoldout, targetFoldout);
                break;
            }
            case "RegisterCompute":
            {
                var success = await RegisterCompute(_computeName, _fleetId, _fleetLocation, _ipAddress);
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
                EditorApplication.EnterPlaymode();
                break;
            }
        }
    }

    private async Task<bool> CreateAnywhereFleet(string fleetName)
    {
        await _gameLiftWrapper.CreateCustomLocationIfNotExists(_fleetLocation);
        await _gameLiftWrapper.CreateFleet(ComputeType.ANYWHERE, _fleetLocation);

        var fleetNameResponse = _coreApi.PutSetting(SettingsKeys.FleetName, fleetName);
        var customLocationNameResponse = _coreApi.PutSetting(SettingsKeys.CustomLocationName, _fleetLocation);
        //var authTokenResponse = _coreApi.PutSetting(SettingsKeys.AuthToken, authToken); doesn't happen here might not even need
        
        return fleetNameResponse.Success && customLocationNameResponse.Success;
    }
    
    private async Task<bool> RegisterCompute(string computeName, string fleetId, string fleetLocation, string ipAddress)
    {
        await _gameLiftWrapper.RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
        var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
        var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);

        return computeNameResponse.Success && ipAddressResponse.Success;
    }
}
