// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] internal Texture _icon;

        private VisualTreeAsset _visualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;

        private const string TabContentSelectedClassName = "tab__content--selected";
        private const string TabButtonSelectedClassName = "tab__button--selected";
        private const string TabButtonClassName = "tab__button";
        private const string TabContentClassName = "tab__content";

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
            var l = new ElementLocalizer(_root);
            l.SetElementText(Pages.Landing, Strings.TabLanding);
            l.SetElementText(Pages.Credentials, Strings.TabCredentials);
            l.SetElementText(Pages.Anywhere, Strings.TabAnywhere);
            l.SetElementText(Pages.ManagedEC2, Strings.TabManagedEC2);
            l.SetElementText(Pages.Help, Strings.TabHelp);
        }

        internal void OpenTab(string tabName)
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