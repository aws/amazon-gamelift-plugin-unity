using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        
        
        //var textFieldsList = textFields.Select(textField => textField.value).ToList();
        
    }

    private void SetupFleetButton(Button button) //TODO add to textfield check
    {
        var textFields = Root.Query<TextField>(null, "FleetText").ToList();
        
        var fleetNameValid = textFields.Select(textField => textField.value)
            .ToList()
            .All(CheckValidFleetName);

        ToggleButtons(button, fleetNameValid);
    }

    private static bool CheckValidFleetName(string text)
    {
        const string testPattern = "^[a-zA-Z_\\-]+$";
        var regex = new Regex(testPattern);
        var match = regex.Match(text);
        return match.Success && text.Length is >= 1 and <= 1024;
    }

    private void SetupCompute(Button button)
    {
        var computeTextNameValid = Root.Q<TextField>(null, "ComputeText").value.Length >= 1;
        var ipTextFieldsValid = Root.Query<TextField>(null, "IpAddress")
            .ToList()
            .All(textField => textField.value.Length >= 1);

        if (computeTextNameValid && ipTextFieldsValid)
        {
            ToggleButtons(button, true);
        }
        else
        {
            ToggleButtons(button, false);
        }
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
                ToggleButtons(button, false);
                var success = await RegisterCompute(_computeName,_ipAddress);
                if(!success)
                {
                    ToggleButtons(button, true);
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
        var fleetLocation = "custom-location-1";
        var fleetNameResponse = _coreApi.PutSetting(SettingsKeys.FleetName, fleetName);
        var customLocationNameResponse = _coreApi.PutSetting(SettingsKeys.CustomLocationName, fleetLocation);
        //var authTokenResponse = _coreApi.PutSetting(SettingsKeys.AuthToken, authToken); doesn't happen here might not even need
        //TODO Call Create location, create fleet 
        
        await _gameLiftWrapper.CreateCustomLocationIfNotExists(fleetLocation);
        //await _gameLiftWrapper.CreateFleet()
        return true;
    }
    
    private async Task<bool> RegisterCompute(string computeName, string ipAddress)
    {
        await _gameLiftWrapper.SetupCompute(ipAddress);
        var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
        var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);

        return true;
    }
}
