// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class LandingPage
    {
        private readonly VisualElement _container;
        private StatusBox _statusBox;
        private ElementLocalizer _elementLocalizer;

        public LandingPage(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/LandingPage");
            var uxml = mVisualTreeAsset.Instantiate();
            
            container.Add(uxml);
            SetupStatusBoxes();
            LocalizeText();
            
            //if (stateManager.SelectedProfile == null) //TODO Once Merged, uncomment
            if (true)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Info, Strings.LandingPageInfoStatusBoxText);
            }
            //else if (!stateManager.IsBootstrapped) //TODO Once Merged, uncomment
            if (true)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.LandingPageWarningStatusBoxText);
            }

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
            EditorMenu.OpenAccountProfilesTab();
        }

        private static void OnImportSampleClicked()
        {
            EditorMenu.ImportSampleGame();
        }

        private void LocalizeText()
        {
            _elementLocalizer = new ElementLocalizer(_container);
            _elementLocalizer.SetElementText("LandingPageHeader", Strings.LandingPageHeader);
            _elementLocalizer.SetElementText("LandingPageDescription", Strings.LandingPageDescription);
            _elementLocalizer.SetElementText("LandingPageNoAccountCardText", Strings.LandingPageNoAccountCardText);
            _elementLocalizer.SetElementText("LandingPageNoAccountCardButton", Strings.LandingPageNoAccountCardButton);
            _elementLocalizer.SetElementText("LandingPageAccountCardText", Strings.LandingPageAccountCardText);
            _elementLocalizer.SetElementText("LandingPageAccountCardButton", Strings.LandingPageAccountCardButton);
            _elementLocalizer.SetElementText("LandingPageSampleHeader", Strings.LandingPageSampleHeader);
            _elementLocalizer.SetElementText("LandingPageSampleDescription", Strings.LandingPageSampleDescription);
            _elementLocalizer.SetElementText("DownloadSampleGame", Strings.LandingPageSampleButton);
        }

        private void SetupStatusBoxes()
        {
            _statusBox = new StatusBox();
            var statusBoxContainer = _container.Q("LandingPageStatusBoxContainer");
            statusBoxContainer.Add(_statusBox);
        }
    }
}