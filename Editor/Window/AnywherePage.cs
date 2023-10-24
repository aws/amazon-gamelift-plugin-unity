// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Amazon.GameLift.Model;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class AnywherePage
    {
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private StatusBox _statusBox;

        public AnywherePage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            LocalizeText();

            container.Q<VisualElement>("AnywherePageIntegrateServerLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageServerSetupDocumentation));
            container.Q<VisualElement>("AnywherePageIntegrateClientLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageClientSetupDocumentation));
            SetupStatusBoxes();

            _stateManager.OnUserProfileUpdated += UpdateGui;

            var fleetInputContainer = uxml.Q("AnywherePageConnectFleetTitle");
            var fleetInput = new ConnectToFleetInput(fleetInputContainer, stateManager);
            var computeInputContainer = uxml.Q("AnywherePageComputeTitle");
            var computeInput =
                new RegisterComputeInput(computeInputContainer, stateManager);
            var launchButton = uxml.Q<Button>("AnywherePageLaunchClientButton");
            launchButton.RegisterCallback<ClickEvent>(_ =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server,
                    EditorUserBuildSettings.selectedStandaloneTarget);

                stateManager.JustLaunchedServer = true;
                EditorApplication.EnterPlaymode();
            });
            UpdateGui();
        }

        public async Task GenerateAuthToken()
        {
            var result = await _stateManager.GameLiftWrapper.GetComputeAuthToken(new GetComputeAuthTokenRequest()
                { ComputeName = _stateManager.ComputeName, FleetId = _stateManager.AnywhereFleetId });
            _container.Q<Label>("AnywherePageAuthTokenDisplay").text = result.AuthToken;
        }

        private void SetupStatusBoxes()
        {
            _statusBox = new StatusBox();
            var statusBoxContainer = _container.Q("AnywherePageStatusBoxContainer");
            statusBoxContainer.Add(_statusBox);
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
            l.SetElementText("AnywherePageCreateFleetTitle", Strings.AnywherePageCreateFleetTitle);
            l.SetElementText("AnywherePageConnectFleetTitle", Strings.AnywherePageConnectFleetTitle);
            l.SetElementText("AnywherePageComputeTitle", Strings.AnywherePageComputeTitle);
            l.SetElementText("AnywherePageAuthTokenTitle", Strings.AnywherePageAuthTokenTitle);
            l.SetElementText("AnywherePageAuthTokenLabel", Strings.AnywherePageAuthTokenLabel);
            l.SetElementText("AnywherePageAuthTokenNote", Strings.AnywherePageAuthTokenNote);
            l.SetElementText("AnywherePageLaunchClientTitle", Strings.AnywherePageLaunchClientTitle);
            l.SetElementText("AnywherePageLaunchClientLabel", Strings.AnywherePageLaunchClientLabel);
            l.SetElementText("AnywherePageLaunchClientButton", Strings.AnywherePageLaunchClientButton);
        }
    }
}