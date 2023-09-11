// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.GameLiftConfigurationUI
{
    public class GameLiftPluginBucketPopup : EditorWindow
    {
        private static VisualTreeAsset m_VisualTreeAsset;
        private VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();
        public Action<string> OnConfirm;
        private const float PopupWidth = 500f;
        private const float PopupHeight = 160f;
        
        public void OnEnable()
        {
            _root = rootVisualElement;
            m_VisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/GameLiftPluginBucketPopup");
            _root.Add(m_VisualTreeAsset.Instantiate());
            titleContent = new GUIContent(_textProvider.Get(Strings.LabelBootstrapPopupWindowTitle));
            maxSize = new Vector2(PopupWidth, PopupHeight);
            minSize = maxSize;
        }
        
        public void Init(string bucketName)
        {
            _root.Q<Label>(Strings.LabelBootstrapPopupTitle).text = _textProvider.Get(Strings.LabelBootstrapPopupTitle);
            _root.Q<Label>(Strings.LabelBootstrapPopupDescription).text =
                _textProvider.Get(Strings.LabelBootstrapPopupDescription);
            _root.Q<TextField>(Strings.LabelBootstrapPopupBucket).label =
                _textProvider.Get(Strings.LabelBootstrapPopupBucket);

            var labelLink = _root.Q<Label>(Strings.LinkBootstrapPopupFreeTier);
            labelLink.text = _textProvider.Get(Strings.LinkBootstrapPopupFreeTier);
            labelLink.RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsFreeTier));

            var bucketNameTextField = _root.Q<TextField>(Strings.LabelBootstrapPopupBucket);
            bucketNameTextField.label = _textProvider.Get(Strings.LabelBootstrapPopupBucket);
            bucketNameTextField.value = bucketName;

            var cancelButton = _root.Q<Button>(Strings.ButtonBootstrapPopupCancel);
            cancelButton.text = _textProvider.Get(Strings.ButtonBootstrapPopupCancel);
            cancelButton.RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            var continueButton = _root.Q<Button>(Strings.ButtonBootstrapPopupContinue);
            continueButton.text = _textProvider.Get(Strings.ButtonBootstrapPopupContinue);
            continueButton.RegisterCallback<ClickEvent>(_ =>
            {
                OnConfirm?.Invoke(bucketNameTextField.value);
                Close();
            });
        }
    }
}