using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;

public class AWSCredentialsTab : Tab
{
    private GameLiftPlugin GameLiftConfig;
    private List<TextField> _accountDetailTextFields = new();
    

    public AWSCredentialsTab(VisualElement root, GameLiftPlugin gameLiftConfig)
    {
        GameLiftConfig = gameLiftConfig;
        Root = root;
        TabNumber = 2;
        SetupBootstrap();
        SetupTab();
        SetUpForSelectedMode();
    }
    
    private void SetupTab()
    {
        var tabName = "Tab2";
        base.SetupTab(tabName, OnTabButtonClicked);
        var dropdownField = Root.Q<DropdownField>(null, "AccountDetailsInput");
        dropdownField.index = 0;
        AccountSelection();
        SetupBootMenu();
    }

    private void SetupBootMenu()
    {
        VisualElement targetWizard;
        switch (GameLiftConfig.CurrentState.AllProfiles.Length)
        {
            case 0:
                targetWizard = GetWizard("Cards");
                break;
            case 1:
                if (GameLiftConfig.CurrentState.SelectedProfile == null)
                {
                    EnableInfoBox("Tab2Warning");
                    //TODO Set SelectedProfile and change dropdown to "Choose Profile" and make all the below labels ---
                }
                targetWizard = GameLiftConfig.CurrentState.SelectedBootstrapped == false ? GetWizard("BootstrapMenu") : GetWizard("CompletedProfile");
                break;
            default:
            {
                if (GameLiftConfig.CurrentState.AllProfiles.Any(profile => profile == "default"))
                {
                    targetWizard = GameLiftConfig.CurrentState.SelectedBootstrapped == false ? GetWizard("BootstrapMenu") : GetWizard("CompletedProfile");
                }
                else
                {
                    targetWizard = GetWizard("BootstrapMenu");
                    if (GameLiftConfig.CurrentState.SelectedProfile == null)
                    {
                        EnableInfoBox("Tab2Warning");
                    }
                }
                break;
            }
        }
        ChangeWizard(targetWizard);
    }

    private bool SaveProfile()
    {
        _accountDetailTextFields = Root.Query<TextField>(null, "AccountDetailsInput").ToList();
        var dropdownField = Root.Q<DropdownField>(null, "AccountDetailsInput");
        
        var credentials = _accountDetailTextFields.Select(textField => textField.value).ToList();

        if (credentials.Any(credential => credential == ""))
        {
            return false;
        }

        GameLiftConfig.CreationModel.ProfileName = credentials[0];
        GameLiftConfig.CreationModel.AccessKeyId = credentials[1];
        GameLiftConfig.CreationModel.SecretKey = credentials[2];
        GameLiftConfig.CreationModel.RegionBootstrap.RegionIndex = dropdownField.index;
        GameLiftConfig.CreationModel.Create();
        Debug.Log("Saving Profile");
        return true;
    }

    private void CLearCredentials()
    {
        _accountDetailTextFields = Root.Query<TextField>(null, "AccountDetailsInput").ToList();
        foreach (var textField in _accountDetailTextFields)
        {
            textField.value = "";
        }
        
    }
    
    private void AccountSelection()
    {
        var accountSelectFields = Root.Query<DropdownField>(null, "AccountSelection").ToList();
        foreach (var accountSelect in accountSelectFields)
        {
            accountSelect.RegisterValueChangedCallback((dropdown) =>
            {
                OnAccountSelect(accountSelect.index);
            });

            accountSelect.choices = GameLiftConfig.CurrentState.AllProfiles.ToList();
            if (accountSelect.choices.Contains("default"))
            {
                accountSelect.index = accountSelect.choices.IndexOf("default");
            }
        }
    }

    private void OnAccountSelect(int index)
    {
        var accountSelectLabels = Root.Query<Label>(null, "AccountSelectLabel").ToList();
        BucketSelection();
        foreach (var label in accountSelectLabels)
        {
            switch (label.name)
            {
                case "Bucket":
                    label.text = _bootstrapSettings.BucketName ?? "No Bucket Created";
                    break;
                case "Region":
                    if (GameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex >= 0)
                    {
                        label.text = GameLiftConfig.UpdateModel.RegionBootstrap.AllRegions[GameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex];
                    }
                    break;
                case "BootstrapStatus":
                    label.text = _bootstrapSettings.BucketName != null ? "Active" : "Inactive";
                    
                    break;
            }
        }
        UpdateModel(index);
    }
    
    
    private void UpdateModel(int index)
    {
        GameLiftConfig.UpdateModel.SelectedProfileIndex = index;
        
        GameLiftConfig.UpdateModel.Update();
        GameLiftConfig.CurrentState.SelectedProfile = GameLiftConfig.UpdateModel.AllProlfileNames[index];
    }

    private void OnTabButtonClicked(ClickEvent evt, Button button)
    {
        switch (button.name)
        {
            case "CreateProfile":
            {
                var targetWizard = GetWizard("AddNewProfile");
                ChangeWizard(targetWizard);
                break;
            }
            case "Cancel":
            {
                CLearCredentials();
                SetupBootMenu();
                break;
            }
            case "CreateAccountProfile":
            {
                if (SaveProfile())
                {
                    var targetWizard = GetWizard("BootstrapMenu");
                    ChangeWizard(targetWizard);
                }
                else
                {
                    Debug.Log("Error");
                }
                break;
            }
            case "BootstrapProfile":
            {
                Debug.Log("Bootstrapping Account");
                //ToggleButtons(button, false);
                var bucketResponse = BucketCreation();
                if (bucketResponse.Success)
                {
                    EnableInfoBox("Tab2Success");
                }
                else
                {
                    var errorBox = Root.Q<VisualElement>("Tab2Error");
                    errorBox.style.display = DisplayStyle.Flex;
                    errorBox.Q<Label>().text = bucketResponse.ErrorMessage;
                    Debug.Log(bucketResponse.ErrorMessage);
                }
                break;
            }
        }
    }
    
    private CancellationTokenSource _refreshBucketsCancellation;
    private BootstrapSettings _bootstrapSettings;

    private void SetupBootstrap()
    {
        _bootstrapSettings = BootstrapSettingsFactory.Create();
        _refreshBucketsCancellation = new CancellationTokenSource();
        _bootstrapSettings.SetUp(_refreshBucketsCancellation.Token)
            .ContinueWith(task =>
            {
                _bootstrapSettings.RefreshCurrentBucket();
            }, TaskContinuationOptions.ExecuteSynchronously);
    }

    private void SetUpForSelectedMode()
    {
        _bootstrapSettings.RefreshCurrentBucket();
    }

    private Response BucketCreation()
    {
        _refreshBucketsCancellation?.Cancel();
        _bootstrapSettings.RefreshBucketName();
        _bootstrapSettings.LifeCyclePolicyIndex = 0;
        return _bootstrapSettings.CreateBucket();
    }

    private void BucketSelection()
    {
        _refreshBucketsCancellation = new CancellationTokenSource();
        _ = _bootstrapSettings.RefreshExistingBuckets(_refreshBucketsCancellation.Token);
        if (_bootstrapSettings.BucketName != null)
        {
            BucketSelection(_bootstrapSettings.BucketName);
            _bootstrapSettings.SaveSelectedBucket();
        }
    }

    private void BucketSelection(string selectedBucket)
    {
        _bootstrapSettings.SelectBucket(selectedBucket);
    }
}
