// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using Editor.GameLiftConfigurationUI;
using Editor.Resources.EditorWindow;
using Editor.Resources.EditorWindow.Pages;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class GameLiftPlugin : UnityEditor.EditorWindow
    {
        [SerializeField] private Texture _icon;
        internal Texture Icon => _icon;

        private VisualTreeAsset _visualTreeAsset;
        private VisualElement _root;
        private VisualElement _currentTab;
        private List<Button> _tabButtons;
        private List<VisualElement> _tabContent;
        private AwsUserProfilesPage _userProfilesPage;
        private VisualElement _tabContentContainer;
        
        public AmazonGameLiftWrapper GameLiftWrapper;
        public State CurrentState;
        public readonly CoreApi CoreApi;
        public readonly AwsCredentialsCreation CreationModel;
        public readonly AwsCredentialsUpdate UpdateModel;

        private const string MainContentClassName = "main__content";
        private const string TabContentSelectedClassName = "tab__content--selected";
        private const string TabButtonSelectedClassName = "tab__button--selected";
        private const string TabButtonClassName = "tab__button";
        private const string TabContentClassName = "tab__content";
        
        private GameLiftPlugin()
        {
            CoreApi = CoreApi.SharedInstance;
            var awsCredentials = AwsCredentialsFactory.Create();
            CreationModel = awsCredentials.Creation;
            UpdateModel = awsCredentials.Update;
            
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;
            _visualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/GameLiftPlugin");
            if (_visualTreeAsset == null)
            {
                return;
            }

            VisualElement uxml = _visualTreeAsset.Instantiate();
            _root.Add(uxml);

            ApplyText();

            _tabContentContainer = _root.Q(className: MainContentClassName);
            var yes = new AwsUserProfilesPage(SetupTab(GetPageName(Pages.Credentials)), this);

            _tabButtons = _root.Query<Button>(className: TabButtonClassName).ToList();
            _tabContent = _root.Query(className: TabContentClassName).ToList();

            _tabButtons.ForEach(button => button.RegisterCallback<ClickEvent>(_ => { OpenTab(button.name); }));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_root);
            l.SetElementText(GetPageName(Pages.Landing), Strings.TabLanding);
            l.SetElementText(GetPageName(Pages.Credentials), Strings.TabCredentials);
            l.SetElementText(GetPageName(Pages.Anywhere), Strings.TabAnywhere);
            l.SetElementText(GetPageName(Pages.ManagedEC2), Strings.TabManagedEC2);
            l.SetElementText(GetPageName(Pages.Help), Strings.TabHelp);
        }
        
        private VisualElement SetupTab(string tabName)
        {
            var container = new VisualElement
            {
                name = $"{tabName}Content",
            };
            container.AddToClassList(TabContentClassName);
            _tabContentContainer.Add(container);
            return container;
        }

        internal void OpenTab(Pages tabName) => OpenTab(GetPageName(tabName));

        private void OpenTab(string tabName)
        {
            _tabContent.ForEach(page =>
            {
                if (page.name == $"{tabName}Content")
                {
                    page.AddToClassList(TabContentSelectedClassName);
                }
                else
                {
                    page.RemoveFromClassList(TabContentSelectedClassName);
                }
            });

            _tabButtons.ForEach(button =>
            {
                if (button.name == tabName)
                {
                    button.AddToClassList(TabButtonSelectedClassName);
                }
                else
                {
                    button.RemoveFromClassList(TabButtonSelectedClassName);
                }
            });
        }
        
        public void RefreshProfiles()
        {
            UpdateModel.Refresh();
            CurrentState.AllProfiles = UpdateModel.AllProlfileNames;
        }
        
        public void SetupWrapper()
        {
            var credentials = CoreApi.RetrieveAwsCredentials(CurrentState.SelectedProfile);
            var client = new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
            GameLiftWrapper = new AmazonGameLiftWrapper(client);
        }
        
        public void OpenS3Popup(string bucketName)
        {
            var popup = CreateInstance<GameLiftPluginBucketPopup>();
            popup.Init(bucketName);
            popup.OnConfirm += BootstrapAccount;
            popup.ShowModalUtility();
        }
        
        private void BootstrapAccount (string bucketName)
        {
            _userProfilesPage.BootstrapAccount(bucketName);
        }
        
        private static string GetPageName(Pages page) => Enum.GetName(typeof(Pages), page);

        internal enum Pages
        {
            Landing,
            Credentials,
            Anywhere,
            ManagedEC2,
            Help,
        }
        
        public struct State
        {
            public bool SelectedBootstrapped;
            public string[] AllProfiles;
            public string SelectedProfile;
        }
    }
}