// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public class AnywherePage
    {
        private readonly VisualElement _container;
        private readonly VisualElement _fleetNameInput;
        private readonly VisualElement _fleetNameDropdownContainer;
        private readonly VisualElement _fleetID;
        private readonly VisualElement _fleetStatus;

        private FleetStatus _fleetState;
        private readonly DropdownField _fleetNameDropdown;

        public AnywherePage(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            _fleetState = FleetStatus.Creating;

            _fleetNameInput = container.Q("LabelAnywhereConnectNoFleet");
            _fleetNameDropdownContainer = container.Q("LabelAnywhereConnectFleets");
            _fleetNameDropdown = container.Q<DropdownField>("LabelAnywhereConnectFleets");
            _fleetID = container.Q("AnywhereFleetID");
            _fleetStatus = container.Q("AnywhereFleetStatus");
            
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            switch (_fleetState)
            {
                case FleetStatus.Creating:
                    _fleetNameInput.RemoveFromClassList("foldout--hidden");
                    _fleetNameDropdownContainer.AddToClassList("foldout--hidden");
                    break;
                case FleetStatus.Selecting:
                    _fleetNameInput.AddToClassList("foldout--hidden");
                    _fleetNameDropdownContainer.RemoveFromClassList("foldout--hidden");
                    _fleetID.AddToClassList("foldout--hidden");
                    _fleetStatus.AddToClassList("foldout--hidden");
                    break;
                case FleetStatus.Selected:
                    _fleetNameInput.AddToClassList("foldout--hidden");
                    _fleetNameDropdownContainer.RemoveFromClassList("foldout--hidden");
                    _fleetID.RemoveFromClassList("foldout--hidden");
                    _fleetStatus.RemoveFromClassList("foldout--hidden");
                    break;
            }
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            // l.SetElementText("LabelAnywhereIntegrateTitle", "");
        }
    }

    public enum FleetStatus
    {
        Creating,
        Selecting,
        Selected
    }
}