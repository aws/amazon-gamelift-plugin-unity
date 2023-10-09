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
            
            _container.Q<Label>(Strings.HelpPageEstimatingPriceLink).RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            _container.Q<Label>(Strings.HelpPageFleetIQLink).RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            _container.Q<Label>(Strings.HelpPageFlexMatchLink).RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            
            _container.Q<VisualElement>("HelpPageReportIssueLink").RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            _container.Q<VisualElement>("HelpPageDocumentationLink").RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            _container.Q<VisualElement>("HelpPageVideoTutorialLink").RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
            _container.Q<VisualElement>("HelpPageForumLink").RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.MissingLink));
        }

        private void OnLinkClicked(string url)
        {
            Application.OpenURL(url);
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("HelpPageTitle", Strings.HelpPageTitle);
            l.SetElementText("HelpPageDescription", Strings.HelpPageDescription);
            l.SetElementText("HelpPageReportIssueLinkText", Strings.HelpPageReportIssueLink);
            l.SetElementText("HelpPageDocumentationLinkText", Strings.HelpPageDocumentationLink);
            l.SetElementText("HelpPageVideoTutorialLinkText", Strings.HelpPageVideoTutorialLink);
            l.SetElementText("HelpPageForumLinkText", Strings.HelpPageForumLink);
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