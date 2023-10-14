// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class LandingPage
    {
        private readonly VisualElement _container;
        private StatusBox _statusBox;

        public LandingPage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/LandingPage");
            var uxml = mVisualTreeAsset.Instantiate();
            container.Add(uxml);
            SetupStatusBoxes();
            LocalizeText();
            
            if (stateManager.SelectedProfile == null)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Info, Strings.LandingPageInfoStatusBoxText);
            }
            else if (!stateManager.IsBootstrapped)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.LandingPageWarningStatusBoxText);
            }

            _container.Q<Button>("CreateAccount").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.CreateAwsAccountLearnMore));
            _container.Q<Button>("AddProfile").RegisterCallback<ClickEvent>(_ => OnAddProfileClicked());
            _container.Q<Button>("DownloadSampleGame").RegisterCallback<ClickEvent>(_ => OnImportSampleClicked());
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

        private void SetupStatusBoxes()
        {
            _statusBox = new StatusBox();
            var statusBoxContainer = _container.Q("LandingPageStatusBoxContainer");
            statusBoxContainer.Add(_statusBox);
        }
    }
}