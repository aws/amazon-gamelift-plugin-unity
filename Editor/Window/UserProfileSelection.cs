using System.Linq;
using System.Threading;
using AmazonGameLift.Editor;
using Editor.CoreAPI;
using Editor.Resources.EditorWindow.Pages;
using UnityEngine.UIElements;

namespace Editor.Window
{
    internal class UserProfileSelection
    {
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;
        private readonly VisualElement _container;
        private readonly AwsUserProfilesPage _awsUserProfilesPage;
        private readonly StateManager _stateManager;
        
        public UserProfileSelection(VisualElement container, StateManager stateManager, AwsUserProfilesPage profilesPage) //TODO Once the state manager is merged in, change from gameliftplugin to statemanager
        {
            _container = container;
            _stateManager = stateManager;
            _awsUserProfilesPage = profilesPage;
            _bootstrapSettings = profilesPage.BootstrapSettings;
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

        public void BucketSelection(string selectedBucket)
        {
            _bootstrapSettings.SelectBucket(selectedBucket);
            _bootstrapSettings.SaveSelectedBucket();
            _container.Q<Label>("S3BucketNameLabel").text = selectedBucket;
        }

        public void AccountSelection(bool isSetup)
        {
            _awsUserProfilesPage.RefreshProfiles();
            var accountSelectFields = _container.Query<DropdownField>(null,"AccountSelection").ToList();
            foreach (var accountSelect in accountSelectFields)
            {
                if (isSetup)
                {
                    accountSelect.RegisterValueChangedCallback(_ => { OnAccountSelect(accountSelect.index); });
                }
                
                accountSelect.choices = _awsUserProfilesPage.UpdateModel.AllProlfileNames.ToList();
                if (accountSelect.choices.Contains("default"))
                {
                    accountSelect.index = accountSelect.choices.IndexOf(_stateManager.SelectedProfile is "default" or null ? "default" : _stateManager.SelectedProfile);
                }
            }
            
        }

        private void OnAccountSelect(int index)
        {
            var accountSelectLabels = _container.Query<Label>(null, "AccountSelectLabel").ToList();
            BucketSelection();
            foreach (var label in accountSelectLabels)
            {
                switch (label.name)
                {
                    case "S3BucketNameLabel":
                        label.text = _bootstrapSettings.BucketName ?? "No Bucket Created";
                        break;
                    case "Region":
                        if (_awsUserProfilesPage.UpdateModel.RegionBootstrap.RegionIndex >= 0)
                        {
                            label.text =
                                _awsUserProfilesPage.UpdateModel.RegionBootstrap.AllRegions[
                                    _awsUserProfilesPage.UpdateModel.RegionBootstrap.RegionIndex];
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
            _awsUserProfilesPage.UpdateModel.SelectedProfileIndex = index;
            _awsUserProfilesPage.UpdateModel.Update();
            _stateManager.SelectedProfile = _awsUserProfilesPage.UpdateModel.AllProlfileNames[index];
            _stateManager.CoreApi.PutSetting(SettingsKeys.CurrentProfileName,
                _stateManager.SelectedProfile);
        }
    }
}