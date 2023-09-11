// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
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
            
            _container.Q<Label>(Strings.LabelHelpEstimatingLearnMore).RegisterCallback<ClickEvent>(_ => OnEstimatingLearnMoreClicked());
            _container.Q<Label>(Strings.LabelHelpFleetIqLearnMore).RegisterCallback<ClickEvent>(_ => OnFleetIqLearnMoreClicked());
            _container.Q<Label>(Strings.LabelHelpFlexMatchLearnMore).RegisterCallback<ClickEvent>(_ => OnFlexMatchLearnMoreClicked());
        }

        private void OnLinkClicked(string url)
        {
            Application.OpenURL(url);
        }

        private void OnEstimatingLearnMoreClicked()
        {
            OnLinkClicked(""); //TODO <ASG6> still need links for this. Has been requested.
        }

        private void OnFleetIqLearnMoreClicked()
        {
            OnLinkClicked(""); //TODO <ASG6> still need links for this. Has been requested
        }
        
        private void OnFlexMatchLearnMoreClicked()
        {
            OnLinkClicked(""); //TODO <ASG6> still need links for this. Has been requested.
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
            l.SetElementText("LabelEstimatingHelpLearnMore", Strings.LabelHelpEstimatingLearnMore);
            l.SetElementText("LabelFleetIqHelpLearnMore", Strings.LabelHelpFleetIqLearnMore);
            l.SetElementText("LabelFlexMatchHelpLearnMore", Strings.LabelHelpFlexMatchLearnMore);
        }
    }
}