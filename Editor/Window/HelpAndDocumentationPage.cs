// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
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
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/HelpAndDocumentationPage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();
            
            _container.Q<Label>(Strings.HelpPageEstimatingPriceLink).RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AboutGameLiftPricing));
            _container.Q<Label>(Strings.HelpPageFleetIQLink).RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsFleetIqDocumentation));
            _container.Q<Label>(Strings.HelpPageFlexMatchLink).RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsFlexMatchDocumentation));
            
            _container.Q<VisualElement>("HelpPageReportIssueLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.GitHubAwsIssues));
            _container.Q<VisualElement>("HelpPageDocumentationLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsGameLiftDocs));
            _container.Q<VisualElement>("HelpPageVideoTutorialLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.MissingLink)); //TODO Still waiting on confirmation of this final link
            _container.Q<VisualElement>("HelpPageForumLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsGameTechForums));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("HelpPageTitle", Strings.HelpPageTitle);
            l.SetElementText("HelpPageDescription", Strings.HelpPageDescription);
            l.SetElementText("HelpPageReportIssueLink", Strings.HelpPageReportIssueLink);
            l.SetElementText("HelpPageDocumentationLink", Strings.HelpPageDocumentationLink);
            l.SetElementText("HelpPageVideoTutorialLink", Strings.HelpPageVideoTutorialLink);
            l.SetElementText("HelpPageForumLink", Strings.HelpPageForumLink);
            l.SetElementText("HelpPageEstimatingPriceTitle", Strings.HelpPageEstimatingPriceTitle);
            l.SetElementText("HelpPageEstimatingPriceDescription", Strings.HelpPageEstimatingPriceDescription);
            l.SetElementText("HelpPageEstimatingPriceLink", Strings.HelpPageEstimatingPriceLink);
            l.SetElementText("HelpPageFleetIQTitle", Strings.HelpPageFleetIQTitle);
            l.SetElementText("HelpPageFleetIQDescription", Strings.HelpPageFleetIQDescription);
            l.SetElementText("HelpPageFleetIQLink", Strings.HelpPageFleetIQLink);
            l.SetElementText("HelpPageFlexMatchTitle", Strings.HelpPageFlexMatchTitle);
            l.SetElementText("HelpPageFlexMatchDescription", Strings.HelpPageFlexMatchDescription);
            l.SetElementText("HelpPageFlexMatchLink", Strings.HelpPageFlexMatchLink);
        }
    }
}