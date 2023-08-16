using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftPlugin.Scripts
{
    public class AwsCredentialsTab : Tab
    {
        private readonly GameLiftPlugin _gameLiftConfig;
        private List<TextField> _accountDetailTextFields = new();
    

        public AwsCredentialsTab(VisualElement root, GameLiftPlugin gameLiftConfig)
        {
            _gameLiftConfig = gameLiftConfig;
            Root = root;
            TabNumber = 2;
            SetupConfigSettings();
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
        
        private void SetupConfigSettings()
        {
            var selectedProfile = _gameLiftConfig.CoreApi.GetSetting(SettingsKeys.SelectedProfile);
            if (selectedProfile.Success)
            {
                _gameLiftConfig.CurrentState.SelectedProfile = selectedProfile.Value;
            }
        }

        private void SetupBootMenu()
        {
            VisualElement targetWizard;
            switch (_gameLiftConfig.CurrentState.AllProfiles.Length)
            {
                case 0:
                    targetWizard = GetWizard("Cards");
                    break;
                case 1:
                    if (_gameLiftConfig.CurrentState.SelectedProfile == null)
                    {
                        EnableInfoBox("Tab2Warning");
                        //TODO Set SelectedProfile and change dropdown to "Choose Profile" and make all the below labels ---
                    }
                    targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false ? GetWizard("BootstrapMenu") : GetWizard("CompletedProfile");
                    break;
                default:
                {
                    if (_gameLiftConfig.CurrentState.AllProfiles.Any(profile => profile == "default"))
                    {
                        targetWizard = _gameLiftConfig.CurrentState.SelectedBootstrapped == false ? GetWizard("BootstrapMenu") : GetWizard("CompletedProfile");
                    }
                    else
                    {
                        targetWizard = GetWizard("BootstrapMenu");
                        if (_gameLiftConfig.CurrentState.SelectedProfile == null)
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

            _gameLiftConfig.CreationModel.ProfileName = credentials[0];
            _gameLiftConfig.CreationModel.AccessKeyId = credentials[1];
            _gameLiftConfig.CreationModel.SecretKey = credentials[2];
            _gameLiftConfig.CreationModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _gameLiftConfig.CreationModel.Create();
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
                accountSelect.RegisterValueChangedCallback(_ =>
                {
                    OnAccountSelect(accountSelect.index);
                });

                accountSelect.choices = _gameLiftConfig.CurrentState.AllProfiles.ToList();
                if (accountSelect.choices.Contains("default"))
                {
                    if (_gameLiftConfig.CurrentState.SelectedProfile is "default" or null)
                    {
                        accountSelect.index = accountSelect.choices.IndexOf("default");
                    }
                    else
                    {
                        accountSelect.index =
                            accountSelect.choices.IndexOf(_gameLiftConfig.CurrentState.SelectedProfile);
                    }
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
                        if (_gameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex >= 0)
                        {
                            label.text = _gameLiftConfig.UpdateModel.RegionBootstrap.AllRegions[_gameLiftConfig.UpdateModel.RegionBootstrap.RegionIndex];
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
            _gameLiftConfig.UpdateModel.SelectedProfileIndex = index;
        
            _gameLiftConfig.UpdateModel.Update();
            _gameLiftConfig.CurrentState.SelectedProfile = _gameLiftConfig.UpdateModel.AllProlfileNames[index];
            _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.SelectedProfile, _gameLiftConfig.CurrentState.SelectedProfile);
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
                .ContinueWith(_ =>
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
}
