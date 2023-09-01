﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] private Texture icon;

        private VisualTreeAsset _visualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

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

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void ApplyText()
        {
            SetElementText("Landing", Strings.TabLanding);
            SetElementText("Credentials", Strings.TabCredentials);
            SetElementText("Anywhere", Strings.TabAnywhere);
            SetElementText("EC2", Strings.TabManagedEC2);
            SetElementText("Help", Strings.TabHelp);
        }

        private void SetElementText(string elementName, string text)
        {
            var button = _root.Q<TextElement>(elementName);
            if (button != default)
            {
                button.text = _textProvider.Get(text);
            }
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

        private static class Pages
        {
            public const string Landing = "Landing";
            public const string Credentials = "Credentials";
            public const string Anywhere = "Anywhere";
            public const string ManagedEC2 = "EC2";
            public const string Help = "Help";
        }
    }
}