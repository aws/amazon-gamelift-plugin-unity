// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class LandingPage
    {
        private readonly VisualElement _container;

        public LandingPage(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/LandingPage");
            var uxml = mVisualTreeAsset.Instantiate();
            
            container.Add(uxml);
            LocalizeText();
            
            _container.Q<Button>("CreateAccount").RegisterCallback<ClickEvent>(_ => OnCreateAccountClicked());
            _container.Q<Button>("AddProfile").RegisterCallback<ClickEvent>(_ => OnAddProfileClicked());
            _container.Q<Button>("DownloadSampleGame").RegisterCallback<ClickEvent>(_ => OnImportSampleClicked());
        }

        private static void OnCreateAccountClicked()
        {
            Application.OpenURL(""); // TODO: Confirm URL for this button
        }

        private static void OnAddProfileClicked()
        {
            GameLiftPlugin.OpenAccountProfilesTab();
        }

        private static void OnImportSampleClicked()
        {
            GameLiftPlugin.ImportSampleGame();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("LandingPageHeader", Strings.LandingPageHeader);
            l.SetElementText("LandingPageDescription", Strings.LandingPageDescription);
            l.SetElementText("LandingPageNoAccountCardText", Strings.LandingPageNoAccountCardText);
            l.SetElementText("LandingPageNoAccountCardButton", Strings.LandingPageNoAccountCardButton);
            l.SetElementText("LandingPageAccountCardText", Strings.LandingPageAccountCardText);
            l.SetElementText("LandingPageAccountCardButton", Strings.LandingPageAccountCardButton);
            l.SetElementText("LandingPageSampleHeader", Strings.LandingPageSampleHeader);
            l.SetElementText("LandingPageSampleDescription", Strings.LandingPageSampleDescription);
            l.SetElementText("DownloadSampleGame", Strings.LandingPageSampleButton);
        }
    }
}