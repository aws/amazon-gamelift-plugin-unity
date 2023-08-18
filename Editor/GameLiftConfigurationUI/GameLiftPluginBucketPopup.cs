// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftConfigurationUI
{
    public class GameLiftPluginBucketPopup : EditorWindow
    {
        private static VisualTreeAsset m_VisualTreeAsset;
        private VisualElement _root;
        private GameLiftPluginBucketPopup window;

        public void OpenPopup(GameLiftPlugin mainWindow, string bucketName)
        {
            window = GetWindow<GameLiftPluginBucketPopup>();
            window.titleContent = new GUIContent("Possible Cost For Bootstrapping");
            window.maxSize = new Vector2(500f, 160f);
            window.minSize = window.maxSize;
            _root = rootVisualElement;
            m_VisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/GameLiftPluginBucketPopup");
            var uxml = m_VisualTreeAsset.Instantiate();
            _root.Add(uxml);
            SetupPopup(mainWindow, bucketName);
        }

        private void SetupPopup(GameLiftPlugin mainWindow, string bucketName)
        {
            var cancelButton = _root.Q<Button>("CancelButton");
            var continueButton = _root.Q<Button>("ContinueButton");
            var bucketNameTextField = _root.Q<TextField>("BucketNameTextField");
            bucketNameTextField.value = bucketName;
            cancelButton.RegisterCallback<ClickEvent>(_ => window.Close());
            continueButton.RegisterCallback<ClickEvent>(_ => mainWindow.BootStrapPassthrough(bucketNameTextField.value));
        }
    }
}
