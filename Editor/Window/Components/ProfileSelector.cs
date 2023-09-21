using System.Collections.Generic;
using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLiftPlugin.Editor
{
    public class ProfileSelector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProfileSelector> { }

        private DropdownField _dropdown => this.Q < DropdownField>("Dropdown");
        
        public ProfileSelector()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/ProfileSelector");
            asset.CloneTree(this);
            
            LocalizeText();

            _dropdown.choices = new List<string>(); // TODO: Load choices from plugin state
            _dropdown.RegisterValueChangedCallback(value => { }); // TODO: Register callback to change profile with main plugin file
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