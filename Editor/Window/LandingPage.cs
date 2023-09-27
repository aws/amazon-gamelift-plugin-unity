// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLift.Editor;
using Editor.CoreAPI;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class LandingPage
    {
        private readonly VisualElement _container;
        private StatusBox _infoBox;
        private StatusBox _warningBox;

        public LandingPage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/LandingPage");
            var uxml = mVisualTreeAsset.Instantiate();
            
            container.Add(uxml);
            LocalizeText();
            SetupStatusBoxes();

            //if (stateManager.SelectedProfile == null)
            if (true)
            {
                _infoBox.Show("You will need to configure an AWS account profile to use Amazon GameLift.");
            }
            //else if (!stateManager.IsBootstrapped)
            if (true)
            {
                _warningBox.Show("Profile configuration is incomplete, navigate to AWS Account Credentials for next steps");
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
            _infoBox = new StatusBox(StatusBox.StatusBoxType.Info);
            _warningBox =  new StatusBox(StatusBox.StatusBoxType.Warning);
            
            var errorContainer = _container.Q("LandingPageErrorContainer");
            
            errorContainer.Add(_infoBox);
            errorContainer.Add(_warningBox);
        }
    }
}