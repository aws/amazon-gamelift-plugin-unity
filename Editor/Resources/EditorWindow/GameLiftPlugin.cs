// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow.Pages;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] private Texture icon;

        private VisualTreeAsset _mVisualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;

        private const string TabContentSelectedClassName = "TabContent--selected";
        private const string TabButtonSelectedClassName = "TabButton--selected";
        private const string TabButtonClassName = "TabButton";
        private const string TabContentClassName = "TabContent";

        private static GameLiftPlugin GetWindow()
        {
            var inspectorType = Type.GetType("UnityEditor.GameView,UnityEditor.dll");
            var window = GetWindow<GameLiftPlugin>(inspectorType);
            window.titleContent = new GUIContent("Amazon GameLift", window.icon);
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
            GetWindow().OpenTab("Credentials");
        }

        [MenuItem("Amazon GameLift/Host with Anywhere", priority = 101)]
        public static void OpenAnywhereTab()
        {
            GetWindow().OpenTab("Anywhere");
        }

        [MenuItem("Amazon GameLift/Host with Managed EC2", priority = 102)]
        public static void OpenEC2Tab()
        {
            GetWindow().OpenTab("EC2");
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
            _mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/GameLiftPlugin");
            if (_mVisualTreeAsset == null)
            {
                return;
            }

            VisualElement uxml = _mVisualTreeAsset.Instantiate();
            _root.Add(uxml);
            
            ApplyText();

            var contentContainer = _root.Q(className: "main__content").Children().First();
            var landingPage = new LandingPage(contentContainer);

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_root);
            l.SetElementText("Landing", Strings.TabLanding);
            l.SetElementText("Credentials", Strings.TabCredentials);
            l.SetElementText("Anywhere", Strings.TabAnywhere);
            l.SetElementText("EC2", Strings.TabEC2);
            l.SetElementText("Help", Strings.TabHelp);
        }

        private void OpenTab(string tabName)
        {
            _tabContent.ForEach(page =>
            {
                if (!page.name.StartsWith(tabName))
                {
                    page.RemoveFromClassList(TabContentSelectedClassName);
                }
                else
                {
                    page.AddToClassList(TabContentSelectedClassName);
                }
            });

            _tabButtons.ForEach(button =>
            {
                if (button.name != tabName)
                {
                    button.RemoveFromClassList(TabButtonSelectedClassName);
                }
                else
                {
                    button.AddToClassList(TabButtonSelectedClassName);
                }
            });
        }
    }
}