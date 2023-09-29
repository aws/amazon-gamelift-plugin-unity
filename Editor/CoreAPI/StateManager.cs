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
        private readonly ISerializer _serializer = new SerializerBuilder().Build();
        private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

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

        public string FleetName
        {
            get => _selectedProfile.FleetName;
            set
            {
                _selectedProfile.FleetName = value;
                SaveProfiles();
            }
        }

        public string FleetId
        {
            get => _selectedProfile.FleetId;
            set
            {
                _selectedProfile.FleetId = value;
                SaveProfiles();
            }
        }

        public string ComputeName
        {
            get => _selectedProfile.ComputeName;
            set
            {
                _selectedProfile.ComputeName = value;
                SaveProfiles();
            }
        }


        public string IpAddress
        {
            get => _selectedProfile.IpAddress;
            set
            {
                _selectedProfile.IpAddress = value;
                SaveProfiles();
            }
        }


        public string WebSocketUrl
        {
            get => _selectedProfile.WebSocketUrl;
            set
            {
                _selectedProfile.WebSocketUrl = value;
                SaveProfiles();
            }
        }


        public IReadOnlyList<string> AllProfiles => CoreApi.ListCredentialsProfiles().Profiles.ToList();

        public bool IsBootstrapped => !string.IsNullOrWhiteSpace(_selectedProfile?.Name) &&
                                      !string.IsNullOrWhiteSpace(_selectedProfile?.Region) &&
                                      !string.IsNullOrWhiteSpace(_selectedProfile?.BootStrappedBucket);

        public Action OnProfileSelected { get; set; }
        public Action OnBucketBootstrapped { get; set; }

        public string BucketName
        {
            get => _selectedProfile.BootStrappedBucket;
            set
            {
                _selectedProfile.BootStrappedBucket = value;
                SaveProfiles();
            }
        }

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
            if (_selectedProfile == null)
            {
                Debug.Log("test user");
                _selectedProfile = new UserProfile()
                {
                    Name = profileName,
                };
                _allProfiles.Add(_selectedProfile);
                SaveProfiles();
            }

            var credentials = CoreApi.RetrieveAwsCredentials(profileName);
            Region = credentials.Region; 
            GameLiftWrapper = AmazonGameLiftWrapperFactory.Get(SelectedProfileName);
            FleetManager = new GameLiftFleetManager(GameLiftWrapper, this);
            ComputeManager = new GameLiftComputeManager(GameLiftWrapper, this);
            OnProfileSelected?.Invoke(); 
        }

        public void RefreshProfiles()
        {
            var profiles = CoreApi.GetSetting(SettingsKeys.UserProfiles).Value;
            if (string.IsNullOrWhiteSpace(profiles))
            {
                _allProfiles = new List<UserProfile>();
            }
            else
            {
                _allProfiles = _deserializer.Deserialize<List<UserProfile>>(profiles);
            }
        }

        public PutSettingResponse SaveProfiles()
        {
            var profiles = _serializer.Serialize(_allProfiles);
            return CoreApi.PutSetting(SettingsKeys.UserProfiles, profiles);
        }

        public void SetBucketBootstrap(string bucketName)
        {
            BucketName = bucketName;
            OnBucketBootstrapped?.Invoke();
        }
    }
}