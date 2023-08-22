// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

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

namespace Editor.GameLiftConfigurationUI
{
    public class GameLiftPlugin : EditorWindow
    {
        public readonly CoreApi CoreApi;
        public readonly AwsCredentialsCreation CreationModel;
        public readonly AwsCredentialsUpdate UpdateModel;
        public List<VisualElement> TabMenus;
        public State CurrentState;
        public AmazonGameLiftWrapper GameLiftWrapper;
        private VisualTreeAsset _mVisualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Tab> _allTabs = new();
        private readonly List<Button> _buttons = new();
        private readonly Color _focusColor = new(0.172549f, 0.3647059f, 0.5294118f, 1);

        private GameLiftPlugin()
        {
            var awsCredentials = AwsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
            awsCredentials.SetUp();
            var settingsStore = new SettingsStore(new FileWrapper(), settingsFilePath: Paths.PluginConfigurationFile);
            CoreApi = new CoreApi(settingsStore);
        }

        [MenuItem("Amazon GameLift/Show Amazon GameLift Window", priority = 0)]
        public static void ShowWindow()
        {
            GetWindow();
        }

        public static GameLiftPlugin GetWindow()
        {
            var inspectorType = Type.GetType("UnityEditor.GameView,UnityEditor.dll");
            var window = GetWindow<GameLiftPlugin>(inspectorType);
            window.titleContent = new GUIContent("Amazon GameLift");
            return window;
        }

        [MenuItem("Amazon GameLift/Bring Panel to Front", priority = 1)]
        public static void Action1()
        {
            ShowWindow();
        }

        [MenuItem("Amazon GameLift/Set AWS Account Profiles", priority = 100)]
        public static void OpenAccountProfilesTab()
        {
            GetWindow().OpenTab("AWSAccountCredentials");
        }

        [MenuItem("Amazon GameLift/Host with Anywhere", priority = 101)]
        public static void OpenAnywhereTab()
        {
            GetWindow().OpenTab("GameLiftAnywhere");
        }

        [MenuItem("Amazon GameLift/Host with Managed EC2", priority = 102)]
        public static void OpenEC2Tab()
        {
            GetWindow().OpenTab("ManagedEC2");
        }
        
        [MenuItem("Amazon GameLift/Import Sample Game", priority = 103)]
        public static void ImportSampleGame()
        {
            string filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";
            AssetDatabase.ImportPackage(filePackagePath, interactive:true);
        }
        
        [MenuItem("Amazon GameLift/Help/Documentation", priority = 200)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(Urls.AwsHelpGameLiftUnity);
        }

        [MenuItem("Amazon GameLift/Help/AWS GameTech Forum", priority = 201)]
        public static void OpenGameTechForums()
        {
            Application.OpenURL(Urls.AwsGameTechForums);
        }

        [MenuItem("Amazon GameLift/Help/Report Issues", priority = 202)]
        public static void OpenReportIssues()
        {
            Application.OpenURL(Urls.GitHubAwsLabs);
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
            ApplyInfoBoxSettings();
            _allTabs = SetupTabs();
        }

        public void OpenS3Popup(string bucketName)
        {
            var popup = new GameLiftPluginBucketPopup();
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

        private List<Tab> SetupTabs()
        {
            return new List<Tab>
            {
                new AmazonGameLiftTab(_root, this, new TextProvider()),
                new AwsCredentialsTab(_root, this),
                new GameLiftAnywhereTab(_root, this),
                new ManagedEC2Tab(_root, this),
            };
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
            TabMenus.ForEach(menu => menu.style.display = DisplayStyle.None);
            _currentTab = TabMenus[0];
            _currentTab.style.display = DisplayStyle.Flex;
        }

        public void OpenTab(string tabName)
        {
            var targetTab = _root.Q<Button>(tabName);
            var targetContent = TabMenus.FirstOrDefault(tab => tab.name == $"{tabName}Tab");
            ChangeTab(targetTab, targetContent);
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