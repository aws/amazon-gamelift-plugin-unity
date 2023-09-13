// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using Editor.Resources.EditorWindow;
using Editor.Window.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] private Texture _icon;

        private VisualTreeAsset _visualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;
        private VisualElement _tabContentContainer;
        
        private readonly StateManager _stateManager;

        public readonly AwsCredentialsCreation CreationModel;
        public readonly AwsCredentialsUpdate UpdateModel;
        
        private const string MainContentClassName = "main__content";
        private const string TabContentSelectedClassName = "tab__content--selected";
        private const string TabButtonSelectedClassName = "tab__button--selected";
        private const string TabButtonClassName = "tab__button";
        private const string TabContentClassName = "tab__content";

        public GameLiftPlugin(IAwsCredentialsFactory awsCredentialsFactory, IAmazonGameLiftClientFactory amazonGameLiftClientFactory)
        {
            _stateManager = new StateManager
            {
                CoreApi = CoreApi.SharedInstance,
                AmazonGameLiftClientFactory = amazonGameLiftClientFactory
            };

            var awsCredentials = awsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
        }
        
        private GameLiftPlugin()
        {
            _stateManager = new StateManager
            {
                CoreApi = CoreApi.SharedInstance
            };
            
            var awsCredentials = AwsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
        }
        
        private static GameLiftPlugin GetWindow()
        {
            var inspectorType = Type.GetType("UnityEditor.GameView,UnityEditor.dll");
            var window = GetWindow<GameLiftPlugin>(inspectorType);
            window.titleContent = new GUIContent("Amazon GameLift", window._icon);
            return window;
        }

        [MenuItem("Amazon GameLift/Show Amazon GameLift Window", priority = 0)]
        public static void ShowWindow()
        {
            GetWindow();
        }

        [MenuItem("Amazon GameLift/Bring Panel to Front", priority = 1)]
        public static void FocusPanel()
        {
            ShowWindow();
        }

        [MenuItem("Amazon GameLift/Set AWS Account Profiles", priority = 100)]
        public static void OpenAccountProfilesTab()
        {
            GetWindow().OpenTab(Pages.Credentials);
        }

        [MenuItem("Amazon GameLift/Host with Anywhere", priority = 101)]
        public static void OpenAnywhereTab()
        {
            GetWindow().OpenTab(Pages.Anywhere);
        }

        [MenuItem("Amazon GameLift/Host with Managed EC2", priority = 102)]
        public static void OpenEC2Tab()
        {
            GetWindow().OpenTab(Pages.ManagedEC2);
        }

        [MenuItem("Amazon GameLift/Import Sample Game", priority = 103)]
        public static void ImportSampleGame()
        {
            string filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";
            AssetDatabase.ImportPackage(filePackagePath, interactive: true);
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
            _visualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/GameLiftPlugin");
            if (_visualTreeAsset == null)
            {
                return;
            }

            VisualElement uxml = _visualTreeAsset.Instantiate();
            _root.Add(uxml);

            ApplyText();
            
            _stateManager.SetupClientFactory();
            _stateManager.SetupWrapper();
            _stateManager.SetupRequestAdapter();

            _tabContentContainer = _root.Q(className: MainContentClassName);
            var anywherePage = new AnywherePage(SetupTab(Pages.Anywhere), _stateManager);

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_root);
            l.SetElementText(Pages.Landing, Strings.TabLanding);
            l.SetElementText(Pages.Credentials, Strings.TabCredentials);
            l.SetElementText(Pages.Anywhere, Strings.TabAnywhere);
            l.SetElementText(Pages.ManagedEC2, Strings.TabManagedEC2);
            l.SetElementText(Pages.Help, Strings.TabHelp);
        }
        
        private VisualElement SetupTab(string tabName)
        {
            var container = new VisualElement
            {
                name = $"{tabName}Content",
            };
            container.AddToClassList(TabContentClassName);
            _tabContentContainer.Add(container);
            return container;
        }

        private void OpenTab(string tabName)
        {
            _tabContent.ForEach(page =>
            {
                if (page.name == $"{tabName}Content")
                {
                    page.AddToClassList(TabContentSelectedClassName);
                }
                else
                {
                    page.RemoveFromClassList(TabContentSelectedClassName);
                }
            });

            _tabButtons.ForEach(button =>
            {
                if (button.name == tabName)
                {
                    button.AddToClassList(TabButtonSelectedClassName);
                }
                else
                {
                    button.RemoveFromClassList(TabButtonSelectedClassName);
                }
            });
        }

        private static class Pages
        {
            public const string Landing = "Landing";
            public const string Credentials = "Credentials";
            public const string Anywhere = "Anywhere";
            public const string ManagedEC2 = "ManagedEC2";
            public const string Help = "Help";
        }
    }
}