// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class AnywherePage
    {
        private const string _primaryButtonClassName = "button--primary";
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private StatusBox _statusBox;
        private Button _launchButton;

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

            _stateManager.OnUserProfileUpdated += UpdateGui;
            _stateManager.OnComputeChanged += UpdateGui;

            var fleetInputContainer = uxml.Q("AnywherePageConnectFleetTitle");
            var fleetInput = new ConnectToFleetInput(fleetInputContainer, stateManager);
            var computeInputContainer = uxml.Q("AnywherePageComputeTitle");
            var computeInput =
                new RegisterComputeInput(computeInputContainer, stateManager);
            _launchButton = uxml.Q<Button>("AnywherePageLaunchServerButton");
            _launchButton.RegisterCallback<ClickEvent>(_ =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server,
                    EditorUserBuildSettings.selectedStandaloneTarget);
                EditorApplication.EnterPlaymode();
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

            if (string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId) ||
                string.IsNullOrWhiteSpace(_stateManager.ComputeName))
            {
                _launchButton.RemoveFromClassList(_primaryButtonClassName);
                _launchButton.SetEnabled(false);
            }
            else
            {
                _launchButton.AddToClassList(_primaryButtonClassName);
                _launchButton.SetEnabled(true);
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
            l.SetElementText("AnywherePageConfigureClientDescription", Strings.AnywherePageConfigureClientDescription);
            l.SetElementText("AnywherePageLaunchServerLabel", Strings.AnywherePageLaunchServerLabel);
            l.SetElementText("AnywherePageLaunchServerButton", Strings.AnywherePageLaunchServerButton);
        }
    }
}