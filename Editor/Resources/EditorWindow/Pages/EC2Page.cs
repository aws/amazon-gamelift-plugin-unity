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

        public EC2Page(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/EC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            var test = container.Q<TextField>("EC2FleetNameInput");
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            // l.SetElementText("LabelAnywhereIntegrateTitle", "");
        }
    }
}