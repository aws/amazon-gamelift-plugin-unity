// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class EC2Page
    {
        private readonly VisualElement _container;
        private string _fleetName;
        private FleetType _fleetType;

        public EC2Page(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/EC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            _fleetType = FleetType.SingleRegion; // TODO: Read from storage
            var fleetTypeInput = new FleetTypeInput(container, FleetTypeInput.InputState.Initial, _fleetType);
            fleetTypeInput.OnValueChanged += value => Debug.Log($"Fleet type changed to {value}");

            container.Q<Foldout>("EC2ParametersSection").text = $"{Application.productName} parameters";
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            // l.SetElementText("LabelAnywhereIntegrateTitle", "");
        }
    }

    public enum FleetType
    {
        SingleRegion = 0,
        SpotFleet = 1,
        FlexMatch = 2
    }
}