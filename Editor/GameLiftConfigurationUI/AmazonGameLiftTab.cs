// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftConfigurationUI
{
    public class AmazonGameLiftTab : Tab
    {
        private readonly GameLiftPlugin _gameLiftConfig;
        private readonly TextProvider _textProvider;

        public AmazonGameLiftTab(VisualElement root, GameLiftPlugin gameLiftConfig, TextProvider textProvider)
        {
            _gameLiftConfig = gameLiftConfig;
            _textProvider = textProvider;
            Root = root;
            SetupTab();

            SetLabelText("LabelLandingTitle");
            SetLabelText("LabelLandingDescription");
            SetLabelText("LabelLandingSampleTitle");
            SetLabelText("LabelLandingSampleDescription");

            SetLabelTextAndLink("LabelLandingLinks1", Urls.AwsHelpGameLiftUnity);
            SetLabelTextAndLink("LabelLandingLinks2", Urls.AwsGameTechForums); 
            SetLabelTextAndLink("LabelLandingLinks3", Urls.GitHubAwsLabs);
            SetLabelTextAndLink("LabelLandingLinks4", Urls.GitHubChangelog);

            SetButtonLabelAndAction("ButtonLandingSampleImport", _ => GameLiftPlugin.ImportSampleGame());
        }

        private void SetLabelText(string labelName)
        {
            Root.Q<Label>(labelName).text = _textProvider.Get(labelName);
        }

        private void SetLabelTextAndLink(string labelName, string linkUrl)
        {
            var label = Root.Q<Label>(labelName);
            Debug.LogWarning($"Got label ${label.name}");
            label.text = _textProvider.Get(labelName);
            label.RegisterCallback<ClickEvent>(evt =>
            {
                Debug.LogWarning($"{labelName} hsa been clicked!");
                Application.OpenURL(linkUrl);
            });
        }

        private void SetButtonLabelAndAction(string buttonName, Action<ClickEvent> action)
        {
            var button = Root.Q<Button>(buttonName);
            button.text = _textProvider.Get(buttonName);
            button.RegisterCallback<ClickEvent>(action.Invoke);
        }

        private void SetupTab()
        {
            var tabName = "Tab1";
            base.SetupTab(tabName, OnTabButtonClicked);
            SetupBootMenu();
        }

        private void SetupBootMenu()
        {
            VisualElement targetWizard;
            switch (_gameLiftConfig.CurrentState.AllProfiles.Length)
            {
                case 0:
                    EnableInfoBox("Tab1Help");
                    targetWizard = GetWizard("Cards");
                    break;
                default:
                {
                    if (_gameLiftConfig.CurrentState.SelectedBootstrapped == false)
                    {
                        EnableInfoBox("Tab1Warning");
                    }

                    targetWizard = GetWizard("AccountDetails");
                    break;
                }
            }

            ChangeWizard(targetWizard);
        }

        private void OnTabButtonClicked(ClickEvent evt, Button button)
        {
            switch (button.name)
            {
                case "AddProfile":
                {
                    var targetTab = _gameLiftConfig.TabMenus[1];
                    _gameLiftConfig.ChangeTab(button, targetTab);
                    break;
                }
                case "DownloadSampleGame":
                {
                    var filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";
                    AssetDatabase.ImportPackage(filePackagePath, interactive: true);
                    break;
                }
            }
        }

        public override void OnAccountSelect()
        {
        }
    }
}