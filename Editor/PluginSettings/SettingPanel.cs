// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class SettingPanel
    {
        private readonly Setting _setting;
        private readonly string _labelSettingsConfigured;
        private readonly string _labelSettingsNotConfigured;
        private readonly string _settingTitle;
        private readonly string _settingTooltip;
        private readonly string _primaryActionDisabledTooltip;
        private GUIStyle _hintStyle;

        protected TextProvider TextProvider { get; }

        public SettingPanel(Setting setting, TextProvider textProvider)
        {
            _setting = setting ?? throw new ArgumentNullException(nameof(setting));
            TextProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _labelSettingsConfigured = textProvider.Get(Strings.LabelSettingsConfigured);
            _labelSettingsNotConfigured = textProvider.Get(Strings.LabelSettingsNotConfigured);
            _settingTitle = textProvider.Get(setting.Title);
            _settingTooltip = textProvider.Get(setting.Tooltip);
            _primaryActionDisabledTooltip = _setting.PrimaryActionDisabledTooltip != null
                    ? TextProvider.Get(_setting.PrimaryActionDisabledTooltip)
                    : null;
        }

        public void Draw()
        {
            if (_hintStyle == null)
            {
                _hintStyle = ResourceUtility.GetInfoLabelStyle();
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(_settingTitle, style: EditorStyles.largeLabel);
                    GUILayout.FlexibleSpace();
                    DrawConfiguredStatus();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5f);
                    GUILayout.Label(_settingTooltip, _hintStyle);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(7f);
                DrawAction();
            }
            EditorGUILayout.EndVertical();
        }

        public virtual void DrawAction()
        {
            bool disabled = !_setting.IsPrimaryActionEnabled;

            using (new EditorGUI.DisabledScope(disabled))
            {
                string label = TextProvider.Get(_setting.PrimaryActionMessage);
                string tooltip = disabled ? _primaryActionDisabledTooltip : null;

                if (GUILayout.Button(new GUIContent(label, tooltip)))
                {
                    _setting.RunPrimaryAction();
                }
            }
        }

        private void DrawConfiguredStatus()
        {
            if (_setting.IsConfigured)
            {
                GUILayout.Label(_labelSettingsConfigured, ResourceUtility.GetConfiguredStyle());
            }
            else
            {
                GUILayout.Label(_labelSettingsNotConfigured, ResourceUtility.GetNotConfiguredStyle());
            }

            GUILayout.Space(3f);
        }
    }
}
