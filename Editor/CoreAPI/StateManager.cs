using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Editor.CoreAPI
{
    public class StateManager
    {
        public CoreApi CoreApi { get; }

        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftComputeManager ComputeManager { get; set; }

        public IAmazonGameLiftWrapper GameLiftWrapper { get; private set; }

        public IAmazonGameLiftWrapperFactory AmazonGameLiftWrapperFactory { get; }

        private UserProfile _selectedProfile;
        private List<UserProfile> _allProfiles;
        
        public UserProfile SelectedProfile => _selectedProfile;

        public string SelectedProfileName
        {
            get
            {
                if (_selectedProfile != null)
                {
                    return !string.IsNullOrWhiteSpace(_selectedProfile.Name) ? _selectedProfile.Name : "";
                }

                return "";
            }
            set => SetProfile(value);
        }

        public string Region
        {
        get => _selectedProfile.Region;
        set => _selectedProfile.Region = value;

        }

        public IReadOnlyList<string> AllProfiles => CoreApi.ListCredentialsProfiles().Profiles.ToList();

        public bool IsBootstrapped => !string.IsNullOrWhiteSpace(_selectedProfile?.Name) &&
                                      string.IsNullOrWhiteSpace(_selectedProfile?.Region) &&
                                      string.IsNullOrWhiteSpace(_selectedProfile.BootStrappedBucket);
        
        public Action OnProfileSelected { get; set; }
        
        public string BucketName { get; set; }
        public string SelectedFleetName { get; set; }
        public string FleetLocation { get; set; }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftWrapperFactory = new AmazonGameLiftWrapperFactory(coreApi);
            RefreshProfiles();
            SetProfile(coreApi.GetSetting(SettingsKeys.CurrentProfileName).Value);
        }

        private void SetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName) || profileName == SelectedProfileName) return;

            _selectedProfile = _allProfiles.FirstOrDefault(profile => profile.Name == profileName);
            if (_selectedProfile != null)
            {
                var credentials = CoreApi.RetrieveAwsCredentials(profileName);
                Region = credentials.Region;
                GameLiftWrapper = AmazonGameLiftWrapperFactory.Get(SelectedProfileName);
                FleetManager = new GameLiftFleetManager(GameLiftWrapper);
                ComputeManager = new GameLiftComputeManager(GameLiftWrapper);
                OnProfileSelected?.Invoke();
            }
            else
            {
                Debug.Log("test user");
                _selectedProfile = new UserProfile();
            }
        }

        public void RefreshProfiles()
        {
            var profiles = CoreApi.GetSetting(SettingsKeys.UserProfiles).Value;
            var deserializer = new DeserializerBuilder().Build();
            _allProfiles = deserializer.Deserialize<List<UserProfile>>(profiles);
        }

        public PutSettingResponse SaveProfiles()
        {
            var serializer = new SerializerBuilder().Build();
            var profiles = serializer.Serialize(_allProfiles);
            return CoreApi.PutSetting(SettingsKeys.UserProfiles, profiles);
        }
    }
}