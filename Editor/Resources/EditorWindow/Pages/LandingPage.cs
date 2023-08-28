// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
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
            ApplyText();
            
            _container.Q<Button>("CreateAccount").RegisterCallback<ClickEvent>(_ => onCreateAccountClicked());
            _container.Q<Button>("AddProfile").RegisterCallback<ClickEvent>(_ => onAddProfileClicked());
            _container.Q<Button>("DownloadSampleGame").RegisterCallback<ClickEvent>(_ => onImportSampleClicked());
        }

        private void onCreateAccountClicked()
        {
            Application.OpenURL(""); // TODO: Confirm URL for this button
        }

        private void onAddProfileClicked()
        {
            GameLiftPlugin.OpenAccountProfilesTab();
        }

        private void onImportSampleClicked()
        {
            GameLiftPlugin.ImportSampleGame();
        }

        private void ApplyText()
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