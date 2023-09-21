// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using Editor.Resources.EditorWindow;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class AnywherePage
    {
        private readonly VisualElement _container;

        public AnywherePage(VisualElement container, GameLiftPlugin gameLiftPlugin)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            var fleetInput =
                new ConnectToFleetInput(container, gameLiftPlugin, ConnectToFleetInput.FleetStatus.NotCreated);
            var computeInput =
                new RegisterComputeInput(container, gameLiftPlugin, fleetInput,
                    RegisterComputeInput.ComputeStatus.NotRegistered);
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageTitle", Strings.AnywherePageTitle);
            l.SetElementText("AnywherePageDescription", Strings.AnywherePageDescription);
            l.SetElementText("AnywherePageIntegrateTitle", Strings.AnywherePageIntegrateTitle);
            l.SetElementText("AnywherePageIntegrateDescription", Strings.AnywherePageIntegrateDescription);
            l.SetElementText("AnywherePageIntegrateServerLink", Strings.AnywherePageIntegrateServerLink);
            l.SetElementText("AnywherePageIntegrateClientLink", Strings.AnywherePageIntegrateClientLink);
            l.SetElementText("AnywherePageCreateFleetTitle", Strings.AnywherePageCreateFleetTitle);
            l.SetElementText("AnywherePageCreateFleetHint", Strings.AnywherePageCreateFleetHint);
            l.SetElementText("AnywherePageCreateFleetButton", Strings.AnywherePageCreateFleetButton);
            l.SetElementText("AnywherePageCreateFleetCancelButton", Strings.AnywherePageCreateFleetCancelButton);
            l.SetElementText("AnywherePageConnectFleetTitle", Strings.AnywherePageConnectFleetTitle);
            l.SetElementText("AnywherePageConnectFleetLabel", Strings.AnywherePageConnectFleetLabel);
            l.SetElementText("AnywherePageConnectFleetIDLabel", Strings.AnywherePageConnectFleetIDLabel);
            l.SetElementText("AnywherePageConnectFleetStatusLabel", Strings.AnywherePageConnectFleetStatusLabel);
            l.SetElementText("AnywherePageConnectFleetNewButton", Strings.AnywherePageConnectFleetNewButton);
            l.SetElementText("AnywherePageComputeTitle", Strings.AnywherePageComputeTitle);
            l.SetElementText("AnywherePageComputeNameLabel", Strings.AnywherePageComputeNameLabel);
            l.SetElementText("AnywherePageComputeIPLabel", Strings.AnywherePageComputeIPLabel);
            l.SetElementText("AnywherePageComputeStatusLabel", Strings.AnywherePageComputeStatusLabel);
            l.SetElementText("AnywherePageComputeRegisterButton", Strings.AnywherePageComputeRegisterButton);
            l.SetElementText("AnywherePageComputeRegisterNewButton", Strings.AnywherePageComputeRegisterNewButton);
            l.SetElementText("AnywherePageComputeCancelButton", Strings.AnywherePageComputeCancelButton);
            l.SetElementText("AnywherePageAuthTokenTitle", Strings.AnywherePageAuthTokenTitle);
            l.SetElementText("AnywherePageAuthTokenLabel", Strings.AnywherePageAuthTokenLabel);
            l.SetElementText("AnywherePageAuthTokenNote", Strings.AnywherePageAuthTokenNote);
            l.SetElementText("AnywherePageLaunchClientTitle", Strings.AnywherePageLaunchClientTitle);
            l.SetElementText("AnywherePageLaunchClientLabel", Strings.AnywherePageLaunchClientLabel);
            l.SetElementText("AnywherePageLaunchClientButton", Strings.AnywherePageLaunchClientButton);
        }
    }
}