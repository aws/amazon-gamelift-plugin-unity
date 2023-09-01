// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class HelpAndDocumentationPage
    {
        private readonly VisualElement _container;

        public HelpAndDocumentationPage(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/HelpAndDocumentationPage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();
            
            // _container.Q<Button>("CreateAccount").RegisterCallback<ClickEvent>(_ => onCreateAccountClicked());
            // _container.Q<Button>("AddProfile").RegisterCallback<ClickEvent>(_ => onAddProfileClicked());
            // _container.Q<Button>("DownloadSampleGame").RegisterCallback<ClickEvent>(_ => onImportSampleClicked());
        }

        private void onLinkClicked(string url)
        {
            Application.OpenURL(url);
        }

        private void onEstimatingLearnMoreClicked()
        {
            //onLinkClicked(Urls.); TODO still need links for this
        }

        private void onFleetIqLearnMoreClicked()
        {
            onLinkClicked("");
        }
        
        private void onFlexMatchLearnMoreClicked()
        {
            onLinkClicked("");
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("LabelHelpTitle", Strings.LabelHelpTitle);
            l.SetElementText("LabelHelpTitleDescription", Strings.LabelHelpTitleDescription);
            l.SetElementText("LabelHelpReportIssues", Strings.LabelHelpReportIssues);
            l.SetElementText("LabelHelpDocumentation", Strings.LabelHelpDocumentation);
            l.SetElementText("LabelHelpVideoTutorials", Strings.LabelHelpVideoTutorials);
            l.SetElementText("LabelHelpAwsForum", Strings.LabelHelpAwsForum);
            l.SetElementText("LabelHelpEstimatingTitle", Strings.LabelHelpEstimatingTitle);
            l.SetElementText("LabelHelpEstimatingDescription", Strings.LabelHelpEstimatingDescription);
            l.SetElementText("LabelHelpFleetIqTitle", Strings.LabelHelpFleetIqTitle);
            l.SetElementText("LabelHelpFleetIqDescription", Strings.LabelHelpFleetIqDescription);
            l.SetElementText("LabelHelpFlexMatchTitle", Strings.LabelHelpFlexMatchTitle);
            l.SetElementText("LabelHelpFlexMatchDescription", Strings.LabelHelpFlexMatchDescription);
            l.SetElementText("LabelEstimatingHelpLearnMore", Strings.LabelEstimatingHelpLearnMore);
            l.SetElementText("LabelFleetIqHelpLearnMore", Strings.LabelFleetIqHelpLearnMore);
            l.SetElementText("LabelFlexMatchHelpLearnMore", Strings.LabelFlexMatchHelpLearnMore);
        }
    }
}