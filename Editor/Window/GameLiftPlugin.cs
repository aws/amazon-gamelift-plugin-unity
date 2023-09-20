// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using Editor.CoreAPI;
using Editor.Resources.EditorWindow;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class GameLiftPlugin : EditorWindow
    {
        [SerializeField] private Texture _icon;
        internal Texture Icon => _icon;

        private VisualTreeAsset _visualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;
        private VisualElement _tabContentContainer;

        private readonly IAmazonGameLiftClientFactory _amazonGameLiftClientFactory;
        public readonly CoreApi CoreApi;
        public readonly AwsCredentialsCreation CreationModel;
        public readonly AwsCredentialsUpdate UpdateModel;
        
        private readonly StateManager _stateManager;

        private const string MainContentClassName = "main__content";
        private const string TabContentSelectedClassName = "tab__content--selected";
        private const string TabButtonSelectedClassName = "tab__button--selected";
        private const string TabButtonClassName = "tab__button";
        private const string TabContentClassName = "tab__content";

        public GameLiftPlugin(IAwsCredentialsFactory awsCredentialsFactory, IAmazonGameLiftClientFactory amazonGameLiftClientFactory)
        {
            _stateManager = new StateManager(new CoreApi());
            _amazonGameLiftClientFactory = amazonGameLiftClientFactory;

            var awsCredentials = awsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
        }
        
        private GameLiftPlugin()
        {
            _stateManager = new StateManager(new CoreApi());
            
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

            LocalizeText();

            var tabContentContainer = _root.Q(className: MainContentClassName);
            var landingPage = new LandingPage(CreateContentContainer(Pages.Landing, tabContentContainer));

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_root);
            l.SetElementText(GetPageName(Pages.Landing), Strings.TabLanding);
            l.SetElementText(GetPageName(Pages.Credentials), Strings.TabCredentials);
            l.SetElementText(GetPageName(Pages.Anywhere), Strings.TabAnywhere);
            l.SetElementText(GetPageName(Pages.ManagedEC2), Strings.TabManagedEC2);
            l.SetElementText(GetPageName(Pages.Help), Strings.TabHelp);
        }

        internal void OpenTab(Pages tabName) => OpenTab(GetPageName(tabName));

        private VisualElement CreateContentContainer(Pages page, VisualElement contentContainer)
        {
            var container = new VisualElement
            {
                name = $"{GetPageName(page)}Content",
            };
            container.AddToClassList(TabContentClassName);
            contentContainer.Add(container);
            return container;
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

        private static string GetPageName(Pages page) => Enum.GetName(typeof(Pages), page);

        internal enum Pages
        {
            Landing,
            Credentials,
            Anywhere,
            ManagedEC2,
            Help,
        }
    }
}