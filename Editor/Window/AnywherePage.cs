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
        private readonly GameLiftPlugin _gameLiftConfig;

        private DropdownField _fleetNameDropdown;

        public AnywherePage(VisualElement container, GameLiftPlugin gameLiftPlugin)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            var fleetInput = new ConnectToFleetInput(container, gameLiftPlugin, ConnectToFleetInput.FleetStatus.NotCreated);
            var computeInput =
                new RegisterComputeInput(container, gameLiftPlugin, fleetInput, RegisterComputeInput.ComputeStatus.NotRegistered);
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("LabelAnywhereTitle", Strings.LabelAnywhereTitle);
            l.SetElementText("LabelAnywhereDescription", Strings.LabelAnywhereDescription);
            l.SetElementText("LabelAnywhereIntegrateTitle", Strings.LabelAnywhereIntegrateTitle);
            l.SetElementText("LabelAnywhereIntegrateDescription", Strings.LabelAnywhereIntegrateDescription);
            l.SetElementText("LabelAnywhereConnectTitle", Strings.LabelAnywhereConnectTitle);
            l.SetElementText("LabelAnywhereConnectFleetName", Strings.LabelAnywhereConnectFleetName);
            l.SetElementText("LabelAnywhereConnectFleetNameHint", Strings.LabelAnywhereConnectFleetNameHint);
            l.SetElementText("LabelAnywhereConnectedFleetID", Strings.LabelAnywhereConnectedFleetID);
            l.SetElementText("LabelAnywhereConnectedFleetStatus", Strings.LabelAnywhereConnectedFleetStatus);
            l.SetElementText("LabelAnywhereComputeTitle", Strings.LabelAnywhereComputeTitle);
            l.SetElementText("LabelAnywhereComputeName", Strings.LabelAnywhereComputeName);
            l.SetElementText("LabelAnywhereComputeIP", Strings.LabelAnywhereComputeIP);
            l.SetElementText("LabelAnywhereAuthTokenTitle", Strings.LabelAnywhereAuthTokenTitle);
            l.SetElementText("LabelAnywhereAuthTokenField", Strings.LabelAnywhereAuthTokenField);
            l.SetElementText("LabelAnywhereAuthTokenFieldNote", Strings.LabelAnywhereAuthTokenFieldNote);
            l.SetElementText("LabelAnywhereLaunchClient", Strings.LabelAnywhereLaunchClient);
            l.SetElementText("LabelAnywhereLaunchClientField", Strings.LabelAnywhereLaunchClientField);
            l.SetElementText("LabelAnywhereIntegrateServerLink", Strings.LabelAnywhereIntegrateServerLink);
            l.SetElementText("LabelAnywhereIntegrateClientLink", Strings.LabelAnywhereIntegrateClientLink);
            l.SetElementText("ButtonAnywhereConnectButton", Strings.ButtonAnywhereConnectButton);
            l.SetElementText("ButtonAnywhereCompute", Strings.ButtonAnywhereCompute);
            l.SetElementText("ButtonAnywhereLaunchClient", Strings.ButtonAnywhereLaunchClient);
        }

    }

}