// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
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

        private readonly StateManager _stateManager;

        private const string MainContentClassName = "main__content";
        private const string TabContentSelectedClassName = "tab__content--selected";
        private const string TabButtonSelectedClassName = "tab__button--selected";
        private const string TabButtonClassName = "tab__button";
        private const string TabContentClassName = "tab__content";

        private GameLiftPlugin()
        {
            _stateManager = new StateManager(new CoreApi());
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
            var anywherePage = new AnywherePage(CreateContentContainer(Pages.Anywhere, tabContentContainer), _stateManager);
            var ec2Page = new ManagedEC2Page(CreateContentContainer(Pages.ManagedEC2, tabContentContainer));
            var helpPage = new HelpAndDocumentationPage(CreateContentContainer(Pages.Help, tabContentContainer));

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