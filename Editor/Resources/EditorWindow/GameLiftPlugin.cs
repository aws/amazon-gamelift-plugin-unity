// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLift.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] internal Texture icon;

        private VisualTreeAsset _mVisualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        private const string TabContentSelectedClassName = "TabContent--selected";
        private const string TabButtonSelectedClassName = "TabButton--selected";
        private const string TabButtonClassName = "TabButton";
        private const string TabContentClassName = "TabContent";

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

            var contentContainer = _root.Q(className: "main__content");

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void ApplyText()
        {
            SetElementText("Landing", Strings.TabLanding);
            SetElementText("Credentials", Strings.TabCredentials);
            SetElementText("Anywhere", Strings.TabAnywhere);
            SetElementText("EC2", Strings.TabEC2);
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

        internal void OpenTab(string tabName)
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