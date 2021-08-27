// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class SettingsWindow : EditorWindow
    {
#if UNITY_2019_1_OR_NEWER
        private const float WindowHeightPixels = 335f;
#else
        private const float WindowHeightPixels = 325f;
#endif

        private const float WindowWidthPixels = 415f;
        private const float TopMarginPixels = 7f;
        private const float LeftMarginPixels = 17f;
        private const float RightMarginPixels = 13f;
        private const float SpaceBetweenSettingsPixels = 12f;
        private readonly List<SettingPanel> _settingPanels = new List<SettingPanel>();
        private Settings _settings;
        private StatusLabel _statusLabel;
        private SettingsStatus _settingsStatus;

        public static bool IsOpen { get; private set; }

        private void SetUp()
        {
            var imageLoader = new ImageLoader();
            TextProvider textProvider = TextProviderFactory.Create();
            List<SettingPanel> settingPanels = SettingsFactory.CreateSettingPanels(textProvider);

            titleContent = new GUIContent(textProvider.Get(Strings.TitleSettings));
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: WindowHeightPixels));
            _settingPanels.Clear();
            _settingPanels.AddRange(settingPanels);
            _statusLabel = new StatusLabel();
            _settings = Settings.SharedInstance;
            _settings.AnySettingChanged += Repaint;
            _settingsStatus = new SettingsStatus(_settings, textProvider);
            _settings.Refresh();
            ShowUtility();
        }

        private void OnEnable()
        {
            SetUp();
            IsOpen = true;
        }

        private void OnDisable()
        {
            IsOpen = false;

            if (_settings != null)
            {
                _settings.AnySettingChanged -= Repaint;
            }

            FirstTimeSettingsLauncher.SetSetting();
        }

        private void OnGUI()
        {
            GUILayout.Space(TopMarginPixels);

            for (int i = 0; i < _settingPanels.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(LeftMarginPixels);
                    _settingPanels[i].Draw();
                    GUILayout.Space(RightMarginPixels);
                }

                if (i < _settingPanels.Count - 1)
                {
                    GUILayout.Space(SpaceBetweenSettingsPixels);
                }
            }

            _settingsStatus.Refresh();

            if (_settingsStatus.Status.IsDisplayed)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(LeftMarginPixels);
                    _statusLabel.Draw(_settingsStatus.Status.Message, _settingsStatus.Status.Type);
                    GUILayout.Space(RightMarginPixels);
                }
            }
        }
    }
}
