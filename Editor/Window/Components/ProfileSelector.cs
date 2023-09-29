using System.Linq;
using Editor.CoreAPI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ProfileSelector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProfileSelector> { }

        private readonly StateManager _stateManager;
        private readonly TextProvider _textProvider;

        private DropdownField _dropdown => this.Q<DropdownField>("Dropdown");
        private Label _bucketName => this.Q<Label>("BucketName");
        private Label _region => this.Q<Label>("Region");
        private Label _status => this.Q<Label>("BootstrapStatus");

        public ProfileSelector()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/ProfileSelector");
            asset.CloneTree(this);

            LocalizeText();

            _stateManager = EditorWindow.GetWindow<GameLiftPlugin>().StateManager;
            _stateManager.OnProfileSelected += UpdateGUI;
            _stateManager.OnBucketBootstrapped += UpdateGUI;
            _textProvider = TextProviderFactory.Create();

            _dropdown.RegisterValueChangedCallback(value => { _stateManager.SelectedProfileName = value.newValue; });
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            var profiles = _stateManager.CoreApi.ListCredentialsProfiles().Profiles.ToList();
            _dropdown.choices = profiles;
            _dropdown.SetValueWithoutNotify(_stateManager.SelectedProfileName);
            _region.text = _stateManager.Region;
            if (_stateManager.IsBootstrapped)
            {
                _bucketName.text = _stateManager.BucketName;
                _status.text = "Active";
            }
            else
            {
                _bucketName.text = _textProvider.Get(Strings.BootstrapNoBucketCreated);
                _status.text = "Inactive";
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(this);
            l.SetElementText("DropdownLabel", Strings.ProfileSelectorDropdownLabel);
            l.SetElementText("BucketNameLabel", Strings.ProfileSelectorBucketNameLabel);
            l.SetElementText("RegionLabel", Strings.ProfileSelectorRegionLabel);
            l.SetElementText("BootstrapStatusLabel", Strings.ProfileSelectorStatusLabel);
        }
    }
}