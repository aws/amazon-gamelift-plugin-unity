using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftPlugin.Scripts
{
    public class GameLiftPlugin : EditorWindow
    {
        private VisualTreeAsset _mVisualTreeAsset = default;
        private VisualTreeAsset _popupVisualTreeAsset = default;

        private VisualElement _root;
        public VisualElement S3PopUp;
        private readonly List<Tab> _allTabs = new();
        public List<VisualElement> TabMenus;
        private VisualElement _currentTab;
        private readonly List<Button> _buttons = new();
        public AmazonGameLiftWrapper GameLiftWrapper;
        private readonly Color _focusColor = new(0.172549f, 0.3647059f, 0.5294118f, 1);

        public readonly AwsCredentialsCreation CreationModel;
        public readonly AwsCredentialsUpdate UpdateModel;
        public string[] allProfileNames;

        public readonly CoreApi CoreApi;

        public State CurrentState = new();

        private GameLiftPlugin()
        {
            var awsCredentials = AwsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
            awsCredentials.SetUp();
            var settingsStore = new SettingsStore(new FileWrapper(), settingsFilePath: Paths.PluginConfigurationFile);
            CoreApi = new CoreApi(settingsStore);
        }

        [MenuItem("GameLift/GameLift Configuration")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameLiftPlugin>();
            window.titleContent = new GUIContent("GameLift Plugin");
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;
            _mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/GameLiftPlugin2");
            if (_mVisualTreeAsset == null)
            {
                return;
            }

            VisualElement uxml = _mVisualTreeAsset.Instantiate();
            _root.Add(uxml);
       
            SetupLinks();
            DisableDefaultButtons();
            SetupTextFieldEvents();
            SetupTabMenu();
            RefreshProfiles();
            SetupTabs();
            ApplyInfoBoxSettings();
        }

        public void OpenS3Popup(string bucketName)
        {
            GameLiftPluginBucketPopup popup = new GameLiftPluginBucketPopup();
            popup.OpenPopup(this, bucketName);
        }

        public void SetupWrapper()
        {
            var credentials = CoreApi.RetrieveAwsCredentials(CurrentState.SelectedProfile);
            var client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
            GameLiftWrapper = new AmazonGameLiftWrapper(client);
            _allTabs.ForEach(tab => tab.OnAccountSelect());
        }
        
        public void ChangeTab(Button targetButton, VisualElement targetTab)
        {
            if (_currentTab != null)
            {
                _currentTab.style.display = DisplayStyle.None;
                foreach (var button in _buttons)
                {
                    button.style.backgroundColor = new StyleColor(Color.clear);
                }
            }
            _currentTab = targetTab;
            if (_currentTab != null)
            {
                _currentTab.style.display = DisplayStyle.Flex;
                targetButton.style.backgroundColor = new StyleColor(_focusColor);
            }
        }

        public void RefreshProfiles()
        {
            UpdateModel.Refresh();
            CurrentState.AllProfiles = UpdateModel.AllProlfileNames;
        }
        
        public void BootStrapPassthrough(string bucketName)
        {
            foreach (var tab in _allTabs)
            {
                if (tab is AwsCredentialsTab credentialsTab)
                {
                    credentialsTab.BootstrapAccount(bucketName);
                }
            }
        }

        private void SetupLinks()
        {
            var labels = _root.Query<VisualElement>(null,"link").ToList();
            foreach (var label in labels)
            {
                label.RegisterCallback<MouseUpEvent,VisualElement>(OnLinkClicked,label);
            }
        }

        private void DisableDefaultButtons()
        {
            var allButtons = _root.Query<Button>(null, "DefaultDisabled").ToList();
            foreach (var button in allButtons)
            {
                button.SetEnabled(false);
            }
        }

        private void SetupTextFieldEvents()
        {
            var allTextFields  = _root.Query<TextField>().ToList();
            foreach (var textField in allTextFields)
            {
                textField.RegisterValueChangedCallback(OnTextChangeButton);
            }
        }

        private void SetupTabs()
        {
            foreach (var tab in TabMenus)
            {
                switch (tab.name)
                {
                    case "AmazonGameLiftTab":
                    {
                        var awsGameLiftTab = new AmazonGameLiftTab(_root, this);
                        _allTabs.Add(awsGameLiftTab);
                        break;
                    }
                    case "AWSAccountCredentialsTab":
                    {
                        var awsCredentialsTab = new AwsCredentialsTab(_root, this);
                        _allTabs.Add(awsCredentialsTab);
                        break;
                    }
                    case "GameLiftAnywhereTab":
                    {
                        var gameLiftAnywhereTab = new GameLiftAnywhereTab(_root, this);
                        _allTabs.Add(gameLiftAnywhereTab);
                        break;
                    }
                    case "ManagedEC2Tab":
                    {
                        var managedEc2Tab = new ManagedEC2Tab(_root, this);
                        _allTabs.Add(managedEc2Tab);
                        break;
                    }
                }
            }
        }
  
        private void SetupTabMenu()
        {
            var tabButtons = _root.Query<Button>(null, "TabButton").ToList();
            foreach (var button in tabButtons)
            {
                button.RegisterCallback<ClickEvent, Button>(OnTabButtonPress, button);
                button.RegisterCallback<MouseOverEvent, Button>(OnTabButtonHover, button);
                button.RegisterCallback<MouseLeaveEvent, Button>(OnTabButtonHover, button);
                _buttons.Add(button);
            }

            _buttons[0].style.backgroundColor = _focusColor;
            TabMenus = _root.Query<VisualElement>(null, "TabMenu").ToList();
            _currentTab = TabMenus[0];
        }
    
        private void OnTabButtonPress(ClickEvent evt, Button button)
        {
            var targetTab = TabMenus.FirstOrDefault(tabMenu => tabMenu.name == button.name + "Tab");
            ChangeTab(button, targetTab);
        }

        private void OnTabButtonHover(MouseOverEvent evt, Button button)
        {
            if (button.style.backgroundColor != _focusColor)
            {
                button.style.backgroundColor = new StyleColor(Color.grey);
            }
        }
    
        private void OnTabButtonHover(MouseLeaveEvent evt, Button button)
        {
            if (button.style.backgroundColor != _focusColor)
            {
                button.style.backgroundColor = new StyleColor(Color.clear);
            }
        }

        private void OnLinkClicked(MouseUpEvent evt, VisualElement linkLabel)
        {
            if (linkLabel.tooltip != "")
            {
                Application.OpenURL(linkLabel.tooltip);
            }
            else
            {
                Debug.Log("Link clicked But still needs link in tooltip");
            }
        }

        private void OnTextChangeButton(ChangeEvent<string> evt)
        {
            var fieldName = "";
            var words = evt.currentTarget.ToString().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length >= 2)
            {
                fieldName =  words[1];
            }
            fieldName = fieldName.Substring(0, fieldName.Length-5);
        
            var button = _root.Q<Button>(fieldName);
            button?.SetEnabled(true);
        }

        private void ApplyInfoBoxSettings()
        {
            var infoBoxes = _root.Query<VisualElement>(null, "InfoBox").ToList();
            foreach (var infoBox in infoBoxes)
            {
                infoBox.Q<Button>("CloseInfoBox").RegisterCallback<ClickEvent>(_ => infoBox.style.display = DisplayStyle.None);
            }
        }
    }

    public struct State
    {
        public bool SelectedBootstrapped;
        public string[] AllProfiles;
        public string SelectedProfile;
        public int SelectedFleetIndex;
    }
}