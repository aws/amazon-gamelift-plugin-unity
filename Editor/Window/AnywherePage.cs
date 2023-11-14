// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;
using AmazonGameLift.Runtime;

namespace AmazonGameLift.Editor
{
    public class AnywherePage
    {
        private const string _primaryButtonClassName = "button--primary";
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private readonly RegisterComputeInput _registerComputeInput;
        private readonly GameLiftClientSettings _gameLiftClientSettings;
        private StatusBox _statusBox;
        private Button _launchServerButton;
        private Button _configureClientButton;

        public AnywherePage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            LocalizeText();

            container.Q<VisualElement>("AnywherePageIntegrateServerLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageIntegrateServerLink));
            container.Q<VisualElement>("AnywherePageIntegrateClientLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageIntegrateClientLink));
            SetupStatusBoxes();

            _stateManager.OnFleetChanged += UpdateGui;
            _stateManager.OnUserProfileUpdated += UpdateGui;
            _stateManager.OnComputeChanged += UpdateGui;
            _stateManager.OnClientSettingsChanged += UpdateGui;

            var fleetInputContainer = uxml.Q("AnywherePageConnectFleetTitle");
            var fleetInput = new ConnectToFleetInput(fleetInputContainer, stateManager);
            var computeInputContainer = uxml.Q("AnywherePageComputeTitle");
            _registerComputeInput = new RegisterComputeInput(computeInputContainer, stateManager);
            
            _launchServerButton = uxml.Q<Button>("AnywherePageLaunchServerButton");
            _launchServerButton.RegisterCallback<ClickEvent>(_ =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server,
                    EditorUserBuildSettings.selectedStandaloneTarget);
                EditorApplication.EnterPlaymode();
            });

            _gameLiftClientSettings = AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>("Assets/Settings/GameLiftClientSettings.asset");
            _configureClientButton = uxml.Q<Button>("AnywherePageConfigureClientButton");
            _configureClientButton.RegisterCallback<ClickEvent>(_ =>
            {
                _gameLiftClientSettings.ConfigureAnywhereClientSettings();
                _stateManager.OnClientSettingsChanged?.Invoke();
            });

            UpdateGui();
        }

        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("AnywherePageStatusBox");
        }

        private void UpdateGui()
        {
            if (!_stateManager.IsBootstrapped)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.AnywherePageStatusBoxNotBootstrappedWarning);
            }
            else
            {
                _statusBox.Close();
            }

            ComputeStatus computeStatus = _registerComputeInput.getComputeStatus();
            bool isComputeRegistered = computeStatus is ComputeStatus.Registered;
            bool isClientConfigured = _gameLiftClientSettings && _gameLiftClientSettings.IsGameLiftAnywhere;
            bool isConfigureClientEnabled = isComputeRegistered && !isClientConfigured;
            
            _configureClientButton.SetEnabled(isConfigureClientEnabled);
            
            if (isConfigureClientEnabled)
            {
                _configureClientButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _configureClientButton.RemoveFromClassList(_primaryButtonClassName);
            }
            
            bool isLaunchServerEnabled = isComputeRegistered && isClientConfigured;
            _launchServerButton.SetEnabled(isLaunchServerEnabled);

            if (isLaunchServerEnabled) 
            {
                _launchServerButton.AddToClassList(_primaryButtonClassName);
            }
            else 
            {
                _launchServerButton.RemoveFromClassList(_primaryButtonClassName);
            }       
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageTitle", Strings.AnywherePageTitle);
            l.SetElementText("AnywherePageDescription", Strings.AnywherePageDescription);
            l.SetElementText("AnywherePageIntegrateTitle", Strings.AnywherePageIntegrateTitle);
            l.SetElementText("AnywherePageIntegrateDescription", Strings.AnywherePageIntegrateDescription);
            l.SetElementText("AnywherePageIntegrateServerLink", Strings.AnywherePageIntegrateServerLink);
            l.SetElementText("AnywherePageIntegrateClientLink", Strings.AnywherePageIntegrateClientLink);
            l.SetElementText("AnywherePageConnectFleetTitle", Strings.AnywherePageConnectFleetTitle);
            l.SetElementText("AnywherePageComputeTitle", Strings.AnywherePageComputeTitle);
            l.SetElementText("AnywherePageLaunchTitle", Strings.AnywherePageLaunchTitle);
            l.SetElementText("AnywherePageConfigureClientLabel", Strings.AnywherePageConfigureClientLabel);
            l.SetElementText("AnywherePageConfigureClientButton", Strings.AnywherePageConfigureClientButton);
            l.SetElementText("AnywherePageLaunchServerLabel", Strings.AnywherePageLaunchServerLabel);
            l.SetElementText("AnywherePageLaunchServerButton", Strings.AnywherePageLaunchServerButton);
            l.SetElementText("AnywherePageLaunchClientLabel", Strings.AnywherePageLaunchClientLabel);
            l.SetElementText("AnywherePageLaunchClientDescription", Strings.AnywherePageLaunchClientDescription);
        }
    }
}