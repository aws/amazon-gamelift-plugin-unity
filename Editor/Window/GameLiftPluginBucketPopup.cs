// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
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
            titleContent = new GUIContent(_textProvider.Get(Strings.UserProfilePageBootstrapPopupWindowTitle));
            maxSize = new Vector2(PopupWidth, PopupHeight);
            minSize = maxSize;
        }
        
        public void Init(string bucketName)
        {
            _root.Q<Label>(Strings.UserProfilePageBootstrapPopupTitle).text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupTitle);
            _root.Q<Label>(Strings.UserProfilePageBootstrapPopupDescription).text =
                _textProvider.Get(Strings.UserProfilePageBootstrapPopupDescription);
            _root.Q<TextField>(Strings.UserProfilePageBootstrapPopupBucketText).label =
                _textProvider.Get(Strings.UserProfilePageBootstrapPopupBucketText);

            var labelLink = _root.Q<Label>(Strings.UserProfilePageBootstrapPopupFreeTierLink);
            labelLink.text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupFreeTierLink);
            labelLink.parent.RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsFreeTier));

            var bucketNameTextField = _root.Q<TextField>(Strings.UserProfilePageBootstrapPopupBucketText);
            bucketNameTextField.label = _textProvider.Get(Strings.UserProfilePageBootstrapPopupBucketText);
            bucketNameTextField.value = bucketName;

            var cancelButton = _root.Q<Button>(Strings.UserProfilePageBootstrapPopupCancelButton);
            cancelButton.text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupCancelButton);
            cancelButton.RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            var continueButton = _root.Q<Button>(Strings.UserProfilePageBootstrapPopupContinueButton);
            continueButton.text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupContinueButton);
            continueButton.RegisterCallback<ClickEvent>(_ =>
            {
                OnConfirm?.Invoke(bucketNameTextField.value);
                Close();
            });
        }
    }
}