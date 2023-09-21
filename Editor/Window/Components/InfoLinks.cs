// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLiftPlugin.Editor
{
    public class InfoLinks : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InfoLinks> { }
        
        public InfoLinks()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/InfoLinks");
            asset.CloneTree(this);

            LocalizeText();

            this.Q<Label>("InfoLinkDocumentationLink")
                .RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.AwsHelpGameLiftUnity));
            this.Q<Label>("InfoLinkForumLink").RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.AwsGameTechForums));
            this.Q<Label>("InfoLinkTroubleshootingLink")
                .RegisterCallback<ClickEvent>(_ => OnLinkClicked("")); // TODO: Get correct action
            this.Q<Label>("InfoLinkReportIssuesLink")
                .RegisterCallback<ClickEvent>(_ => OnLinkClicked(Urls.GitHubAwsLabs));
        }

        private void OnLinkClicked(string url)
        {
            Application.OpenURL(url);
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(this);
            l.SetElementText("InfoLinkDocumentationLink", Strings.InfoLinkDocumentationLink);
            l.SetElementText("InfoLinkForumLink", Strings.InfoLinkForumLink);
            l.SetElementText("InfoLinkTroubleshootingLink", Strings.InfoLinkTroubleshootingLink);
            l.SetElementText("InfoLinkReportIssuesLink", Strings.InfoLinkReportIssuesLink);
        }
    }
}