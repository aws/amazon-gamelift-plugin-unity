// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class SettingsWindow : EditorWindow
    {
        private const float TopMarginPixels = 7f;
        private const float LeftMarginPixels = 17f;
        private const float RightMarginPixels = 13f;
        private const float SpaceBetweenSettingsPixels = 12f;
        private Settings _settings;
        private StatusLabel _statusLabel;
        private SettingPanel _dotNetPanel;
        private GameLiftSettingPanel _glLocalPanel;
        private SettingPanel _javaPanel;
        private SettingPanel _credentialsPanel;
        private SettingPanel _bootstrapPanel;
        private ControlDrawer _controlDrawer;

        [SerializeField]
        private SettingsState _settingsState;
        private string _labelSdkTab;
        private string _labelOpenForums;
        private string _labelOpenAwsHelp;
        private string _labelReportSecurity;
        private string _labelPingSdk;
        private string _labelReportBugs;
        private string _labelOpenDeployment;
        private string _labelOpenLocalTest;
        private string _labelHelpTab;
        private string _labelOpenPdf;
        private string _labelTestTab;
        private string _labelDeployTab;

        public static bool IsOpen { get; private set; }

        private void SetUp()
        {
            var imageLoader = new ImageLoader();
            TextProvider textProvider = TextProviderFactory.Create();
            _labelSdkTab = textProvider.Get(Strings.LabelSettingsSdkTab);
            _labelDeployTab = textProvider.Get(Strings.LabelSettingsDeployTab);
            _labelTestTab = textProvider.Get(Strings.LabelSettingsTestTab);
            _labelHelpTab = textProvider.Get(Strings.LabelSettingsHelpTab);
            _labelOpenPdf = textProvider.Get(Strings.LabelSettingsOpenPdf);
            _labelOpenForums = textProvider.Get(Strings.LabelSettingsOpenForums);
            _labelOpenAwsHelp = textProvider.Get(Strings.LabelSettingsOpenAwsHelp);
            _labelReportSecurity = textProvider.Get(Strings.LabelSettingsReportSecurity);
            _labelReportBugs = textProvider.Get(Strings.LabelSettingsReportBugs);
            _labelOpenDeployment = textProvider.Get(Strings.LabelSettingsOpenDeployment);
            _labelOpenLocalTest = textProvider.Get(Strings.LabelSettingsOpenLocalTest);
            _labelPingSdk = textProvider.Get(Strings.LabelSettingsPingSdk);

            titleContent = new GUIContent(textProvider.Get(Strings.TitleSettings));
            minSize = new Vector2(360, 315);
            _statusLabel = new StatusLabel();
            _controlDrawer = ControlDrawerFactory.Create();
            _settings = Settings.SharedInstance;
            _dotNetPanel = new SettingPanel(_settings.DotNetSetting, textProvider);
            _glLocalPanel = new GameLiftSettingPanel(_settings.GameLiftLocalSetting, textProvider);
            _javaPanel = new SettingPanel(_settings.JavaSetting, textProvider);
            _credentialsPanel = new SettingPanel(_settings.CredentialsSetting, textProvider);
            _bootstrapPanel = new SettingPanel(_settings.BootstrapSetting, textProvider);

            if (_settingsState == null)
            {
                _settingsState = new SettingsState(_settings, textProvider);
            }
            else
            {
                _settingsState.Restore(_settings, textProvider);
            }

            _settings.AnySettingChanged += Repaint;
            _settings.Refresh();
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

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);
                EditorGUILayout.LabelField("Amazon GameLift");
                GUILayout.Space(RightMarginPixels);
            }

            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                GUILayout.Space(LeftMarginPixels);

                DrawTabHeader(_labelSdkTab, SettingsState.TabSdk);
                DrawTabHeader(_labelTestTab, SettingsState.TabTest);
                DrawTabHeader(_labelDeployTab, SettingsState.TabDeploy);
                GUILayout.FlexibleSpace();
                DrawTabHeader(_labelHelpTab, SettingsState.TabHelp);

                GUILayout.Space(RightMarginPixels);
            }

            _controlDrawer.DrawSeparator();
            DrawActiveTab();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);
                string message = _settingsState.Status.IsDisplayed
                    ? _settingsState.Status.Message
                    : string.Empty;
                _statusLabel.Draw(message, _settingsState.Status.Type);
                GUILayout.Space(RightMarginPixels);
            }

            _settingsState.Refresh();
        }

        private void DrawTabHeader(string label, int tabId, float width = 70f)
        {
            GUIStyle style = _settingsState.ActiveTab == tabId ? ResourceUtility.GetTabActiveStyle() : ResourceUtility.GetTabNormalStyle();
            bool pressed = GUILayout.Button(label, style, GUILayout.MaxWidth(width));

            if (pressed)
            {
                _settingsState.ActiveTab = tabId;
            }
        }

        private void DrawActiveTab()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels - 5f);

                using (new EditorGUILayout.VerticalScope())
                {
                    switch (_settingsState.ActiveTab)
                    {
                        case SettingsState.TabSdk:
                            DrawSdkTab();
                            break;
                        case SettingsState.TabTest:
                            DrawTestTab();
                            break;
                        case SettingsState.TabDeploy:
                            DrawDeployTab();
                            break;
                        case SettingsState.TabHelp:
                            DrawHelpTab();
                            break;
                        default:
                            break;
                    }
                }

                GUILayout.Space(RightMarginPixels);
            }
        }

        private void DrawHelpTab()
        {
            if (GUILayout.Button(_labelOpenAwsHelp))
            {
                EditorMenu.OpenAwsDocumentation();
            }

            GUILayout.Space(TopMarginPixels);

            if (GUILayout.Button(_labelOpenForums))
            {
                EditorMenu.OpenForums();
            }

            GUILayout.Space(TopMarginPixels);

            if (GUILayout.Button(_labelReportBugs))
            {
                EditorMenu.ReportBugs();
            }

            GUILayout.Space(TopMarginPixels);

            if (GUILayout.Button(_labelReportSecurity))
            {
                EditorMenu.ReportSecurity();
            }
        }

        private void DrawDeployTab()
        {
            _credentialsPanel.Draw();
            GUILayout.Space(SpaceBetweenSettingsPixels);
            _bootstrapPanel.Draw();
            _controlDrawer.DrawSeparator();

            using (new EditorGUI.DisabledGroupScope(!_settingsState.CanDeploy))
            {
                bool pressed = GUILayout.Button(_labelOpenDeployment);

                if (pressed)
                {
                    EditorMenu.ShowDeployment();
                }
            }
        }

        private void DrawTestTab()
        {
            _glLocalPanel.Draw();
            GUILayout.Space(SpaceBetweenSettingsPixels);
            _javaPanel.Draw();
            _controlDrawer.DrawSeparator();

            using (new EditorGUI.DisabledGroupScope(!_settingsState.CanRunLocalTest))
            {
                bool pressed = GUILayout.Button(_labelOpenLocalTest);

                if (pressed)
                {
                    EditorMenu.ShowLocalTesting();
                }
            }
        }

        private void DrawSdkTab()
        {
            _dotNetPanel.Draw();
            _controlDrawer.DrawSeparator();

            bool pressed = GUILayout.Button(_labelPingSdk);

            if (pressed)
            {
                EditorMenu.PingSdk();
            }
        }
    }
}
