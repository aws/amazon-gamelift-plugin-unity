// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsWindow : EditorWindow
    {
        private const float WindowWidthPixels = 415f;
        private const float WindowCreationHeightPixels = 280f;
        private const float TopMarginPixels = 17f;
        private const float LeftMarginPixels = 15f;
        private const float RightMarginPixels = 13f;
        private const float LabelWidthPixels = 165f;

        private AwsCredentials _awsCredentials;
        private StatusLabel _statusLabel;
        private AwsCredentialsCreationPanel _creationPanel;
        private AwsCredentialsUpdatePanel _updatePanel;

        private string[] _uiModes = new string[0];

        private void SetUp()
        {
            ControlDrawer controlDrawer = ControlDrawerFactory.Create();
            TextProvider textProvider = TextProviderFactory.Create();
            titleContent = new GUIContent(textProvider.Get(Strings.TitleAwsCredentials));
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: WindowCreationHeightPixels));
            _awsCredentials = AwsCredentialsFactory.Create();
            _statusLabel = new StatusLabel();
            _creationPanel = new AwsCredentialsCreationPanel(_awsCredentials.Creation, _statusLabel, textProvider, controlDrawer);
            _updatePanel = new AwsCredentialsUpdatePanel(_awsCredentials.Update, _statusLabel, textProvider, controlDrawer);
            _uiModes = new[]
            {
                textProvider.Get(Strings.LabelCredentialsCreateMode),
                textProvider.Get(Strings.LabelCredentialsUpdateMode),
            };
            _awsCredentials.SetUp();
        }

        private void OnEnable()
        {
            SetUp();
            _awsCredentials.Creation.Status.Changed += OnStatusChanged;
        }

        private void OnDisable()
        {
            _awsCredentials.Creation.Status.Changed -= OnStatusChanged;
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = LabelWidthPixels;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(TopMarginPixels);
                    DrawControls();
                }

                GUILayout.Space(RightMarginPixels);
            }
        }

        private void DrawControls()
        {
            using (new EditorGUI.DisabledScope(!_awsCredentials.CanSelect))
            {
                int previousMode = _awsCredentials.SelectedMode;
                _awsCredentials.SelectedMode = GUILayout.SelectionGrid(_awsCredentials.SelectedMode,
                    _uiModes, 1, EditorStyles.radioButton);

                if (previousMode != _awsCredentials.SelectedMode)
                {
                    GUI.FocusControl(null);
                    _creationPanel.CleanUp();
                    _updatePanel.CleanUp();
                }
            }

            GUILayout.Space(7f);

            if (_awsCredentials.IsNewProfileMode)
            {
                _creationPanel.Draw();
            }
            else
            {
                _updatePanel.Draw();
            }
        }

        private void OnStatusChanged()
        {
            Repaint();
        }
    }
}
