// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Editor.CoreAPI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class AnywherePage
    {
        private readonly VisualElement _container;
        private readonly ScriptingDefineSymbolEditor _defineEditor;
        private const string _unityServerDefine = "UNITY_SERVER";

        public AnywherePage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            LocalizeText();
            
            container.Q<VisualElement>("AnywherePageIntegrateServerLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageServerSetupDocumentation));
            container.Q<VisualElement>("AnywherePageIntegrateClientLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AnywherePageClientSetupDocumentation));

            var fleetInputContainer = uxml.Q("AnywherePageConnectFleetTitle");
            var fleetInput = new ConnectToFleetInput(fleetInputContainer, stateManager);
            var computeInputContainer = uxml.Q("AnywherePageComputeTitle");
            var computeInput =
                new RegisterComputeInput(computeInputContainer, stateManager);
            var launchButton = uxml.Q<Button>("AnywherePageLaunchClientButton");
            _defineEditor = new ScriptingDefineSymbolEditor(BuildTargetGroup.Standalone);
            launchButton.RegisterCallback<ClickEvent>(_ =>
            {
                _defineEditor.Add(_unityServerDefine);
                EditorApplication.EnterPlaymode();
            });
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