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

        public AmazonGameLiftTab(VisualElement root, GameLiftPlugin gameLiftConfig)
        {
            _gameLiftConfig = gameLiftConfig;
            Root = root;
            SetupTab();
        }

        private void SetupTab()
        {
            base.SetupTab("Tab1", (evt, args) => {} );
            SetupBootMenu();
            
            SetLabelText(Strings.LabelLandingTitle);
            SetLabelText(Strings.LabelLandingDescription);
            SetLabelText(Strings.LabelLandingSampleTitle);
            SetLabelText(Strings.LabelLandingSampleDescription);

            SetLabelTextAndLink(Strings.LabelLandingLinks1, Urls.AwsHelpGameLiftUnity);
            SetLabelTextAndLink(Strings.LabelLandingLinks2, Urls.AwsGameTechForums); 
            SetLabelTextAndLink(Strings.LabelLandingLinks3, Urls.GitHubAwsLabs);
            SetLabelTextAndLink(Strings.LabelLandingLinks4, Urls.GitHubChangelog);

            SetButtonLabelAndAction(Strings.ButtonLandingSampleImport, _ => GameLiftPlugin.ImportSampleGame());
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

        public override void OnAccountSelect()
        {
            
        }
    }
}