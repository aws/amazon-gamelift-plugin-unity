using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.CoreAPI;
using Editor.Resources.EditorWindow.Pages;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    internal class UserProfileCreation
    {
        private readonly VisualElement _container;
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;
        private StateManager _stateManager;
        private AwsUserProfilesPage _profilesPage;
        
        
        public UserProfileCreation(VisualElement container, StateManager stateManager, AwsUserProfilesPage profilesPage) //TODO Once the state manager is merged in, change from gameliftplugin to statemanager
        {
            _container = container;
            _stateManager = stateManager;
            _profilesPage = profilesPage;
        }
        
        public void BootstrapAccount(string bucketName)
        {
            var bucketResponse = CreateBucket(bucketName);
            if (bucketResponse.Success)
            {
                _container.Q<VisualElement>(null, "Tab2Success").style.display = DisplayStyle.Flex;
                _stateManager.IsBootstrapped = true;
                _profilesPage.UserProfileSelection.BucketSelection(bucketName);
            }
            else
            {
                _stateManager.IsBootstrapped = false;
                var errorBox = _container.Q<VisualElement>("Tab2Error");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = bucketResponse.ErrorMessage;
                Debug.Log(bucketResponse.ErrorMessage);
            }
        }

        public BootstrapSettings SetupBootstrap()
        {
            _bootstrapSettings = BootstrapSettingsFactory.Create();
            _refreshBucketsCancellation = new CancellationTokenSource();
            _bootstrapSettings.SetUp(_refreshBucketsCancellation.Token)
                .ContinueWith(_ => { _bootstrapSettings.RefreshCurrentBucket(); },
                    TaskContinuationOptions.ExecuteSynchronously);
            return _bootstrapSettings;
        }

        private void RefreshBucket()
        {
            _bootstrapSettings.RefreshCurrentBucket();
            
        }

        private Response CreateBucket(string bucketName)
        {
            _refreshBucketsCancellation?.Cancel();
            _bootstrapSettings.BucketName = bucketName;
            _bootstrapSettings.LifeCyclePolicyIndex = 0;
            return _bootstrapSettings.CreateBucket();
        }

        public bool CreateModel()
        {
            _profilesPage.AccountDetailTextFields = _container.Query<TextField>(null, "AccountDetailsInput").ToList();
           var dropdownField = _container.Q<DropdownField>("AccountProfileDropdown");
          
           var credentials = _profilesPage.AccountDetailTextFields.Select(textField => textField.value).ToList();
          
           if (credentials.Any(credential => credential == ""))
           {
               return false;
           }
                          
            _profilesPage.CreationModel.ProfileName = credentials[0];
            _profilesPage.CreationModel.AccessKeyId = credentials[1];
            _profilesPage.CreationModel.SecretKey = credentials[2];
            _profilesPage.CreationModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _profilesPage.CreationModel.Create();
            return true;
        }
        
    }
}