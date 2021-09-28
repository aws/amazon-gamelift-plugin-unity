// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class SettingsWindow : EditorWindow
    {
        private const float SettingsWindowMinWidth = 320;
        private const float SettingsWindowMinHeight = 500;
        private const float GameLiftLogoHeight = 35;
        private const float SettingsPanelTopMargin = 15f;
        private const float TopMarginPixels = 7f;
        private const float LeftMarginPixels = 17f;
        private const float RightMarginPixels = 13f;
        private const float SpaceBetweenSettingsPixels = 15f;
        private Settings _settings;
        private StatusLabel _statusLabel;
        private SettingPanel _dotNetPanel;
        private GameLiftSettingPanel _glLocalPanel;
        private SettingPanel _javaPanel;
        private SettingPanel _credentialsPanel;
        private SettingPanel _bootstrapPanel;
        private ControlDrawer _controlDrawer;
        private ImageLoader _imageLoader;
        private TextProvider _textProvider;

        [SerializeField]
        private Texture2D _gameLiftLogo;
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
        private string _labelTestTab;
        private string _labelDeployTab;
        private string _labelOpenSdkIntegrationDoc;
        private string _labelOpenSdkApiDoc;

        public static bool IsOpen { get; private set; }

        private void SetUp()
        {
            _imageLoader = new ImageLoader();
            _textProvider = TextProviderFactory.Create();
            _labelSdkTab = _textProvider.Get(Strings.LabelSettingsSdkTab);
            _labelDeployTab = _textProvider.Get(Strings.LabelSettingsDeployTab);
            _labelTestTab = _textProvider.Get(Strings.LabelSettingsTestTab);
            _labelHelpTab = _textProvider.Get(Strings.LabelSettingsHelpTab);
            _labelOpenForums = _textProvider.Get(Strings.LabelSettingsOpenForums);
            _labelOpenAwsHelp = _textProvider.Get(Strings.LabelSettingsOpenAwsHelp);
            _labelReportSecurity = _textProvider.Get(Strings.LabelSettingsReportSecurity);
            _labelReportBugs = _textProvider.Get(Strings.LabelSettingsReportBugs);
            _labelOpenDeployment = _textProvider.Get(Strings.LabelSettingsOpenDeployment);
            _labelOpenLocalTest = _textProvider.Get(Strings.LabelSettingsOpenLocalTest);
            _labelPingSdk = _textProvider.Get(Strings.LabelSettingsPingSdk);
            _labelOpenSdkIntegrationDoc = _textProvider.Get(Strings.LabelOpenSdkIntegrationDoc);
            _labelOpenSdkApiDoc = _textProvider.Get(Strings.LabelOpenSdkApiDoc);

            titleContent = new GUIContent(_textProvider.Get(Strings.TitleSettings));
            minSize = new Vector2(SettingsWindowMinWidth, SettingsWindowMinHeight);
            _statusLabel = new StatusLabel();
            _controlDrawer = ControlDrawerFactory.Create();
            _settings = Settings.SharedInstance;
            _dotNetPanel = new SettingPanel(_settings.DotNetSetting, _textProvider);
            _glLocalPanel = new GameLiftSettingPanel(_settings.GameLiftLocalSetting, _textProvider);
            _javaPanel = new SettingPanel(_settings.JavaSetting, _textProvider);
            _credentialsPanel = new SettingPanel(_settings.CredentialsSetting, _textProvider);
            _bootstrapPanel = new SettingPanel(_settings.BootstrapSetting, _textProvider);

            if (_settingsState == null)
            {
                _settingsState = new SettingsState(_settings, _textProvider);
            }
            else
            {
                _settingsState.Restore(_settings, _textProvider);
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
            // HACK: During InitializeOnLoadMethod in FirstTimeSettingsLauncher.cs, Resource.Load() cannot find
            // the image at "Images/Dark/GameLiftLogo", though subsequent attempts load the image successfully.
            // This is suspected to be due to a race condition between InitializeOnLoadMethod and readiness
            // of the Editor Resources.
            if (_gameLiftLogo == null)
            {
                _gameLiftLogo = _imageLoader.LoadImage(AssetNames.GameLiftLogo);
            }

            // Displays the GameLift Logo
            GUILayout.Space(TopMarginPixels);
            if (_gameLiftLogo)
            {
                GUILayout.Space(TopMarginPixels);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(LeftMarginPixels);
                    float logoHeight = GameLiftLogoHeight;
                    float logoWidth = GameLiftLogoHeight / _gameLiftLogo.height * _gameLiftLogo.width;
                    Rect logoRect = GUILayoutUtility.GetRect(logoWidth, logoHeight, style: GUI.skin.box);
                    GUI.DrawTexture(logoRect, _gameLiftLogo, ScaleMode.ScaleToFit);
                    GUILayout.Space(RightMarginPixels);
                }
            } 

            // Displays the Settings UI top nav
            GUILayout.Space(TopMarginPixels);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(LeftMarginPixels);

                DrawTabHeader(_labelSdkTab, SettingsState.TabSdk);
                DrawTabHeader(_labelTestTab, SettingsState.TabTest);
                DrawTabHeader(_labelDeployTab, SettingsState.TabDeploy);
                GUILayout.FlexibleSpace();
                DrawTabHeader(_labelHelpTab, SettingsState.TabHelp);

                GUILayout.Space(RightMarginPixels);
            }

            // Displays the current active panel, e.g. SDK / Testing / Deployment / Help
            GUILayout.Space(SettingsPanelTopMargin);
            DrawActiveTab();

            _settingsState.Refresh();
        }

        private void DrawTabHeader(string label, int tabId, float width = 70f)
        {
            GUIStyle style = _settingsState.ActiveTab == tabId ? ResourceUtility.GetTabActiveStyle() : ResourceUtility.GetTabNormalStyle();

            if (GUILayout.Button(label, style, GUILayout.MaxWidth(width)))
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
                if (GUILayout.Button(_labelOpenDeployment))
                {
                    EditorMenu.ShowDeployment();
                }
                
            }

            if (_settingsState.CanDeploy)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5F);
                    _statusLabel.Draw(_textProvider.Get(Strings.SettingsUIDeployNextStepLabel), MessageType.Info);
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
                if (GUILayout.Button(_labelOpenLocalTest))
                {
                    EditorMenu.ShowLocalTesting();
                }
            }

            if (_settingsState.CanRunLocalTest)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5F);
                    _statusLabel.Draw(_textProvider.Get(Strings.SettingsUITestNextStepLabel), MessageType.Info);
                }
            }
        }

        private void DrawSdkTab()
        {
            _dotNetPanel.Draw();
            _controlDrawer.DrawSeparator();

            if (GUILayout.Button(_labelPingSdk))
            {
                EditorMenu.PingSdk();
            }

            GUILayout.Space(TopMarginPixels);

            if (GUILayout.Button(_labelOpenSdkIntegrationDoc))
            {
                EditorMenu.OpenGameLiftServerCSharpSdkIntegrationDoc();
            }

            GUILayout.Space(TopMarginPixels);

            if (GUILayout.Button(_labelOpenSdkApiDoc))
            {
                EditorMenu.OpenGameLiftServerCSharpSdkApiDoc();
            }

            GUILayout.Space(TopMarginPixels);

            if (DotNetSetting.IsApiCompatibilityLevel4X())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5F);
                    _statusLabel.Draw(_textProvider.Get(Strings.SettingsUISdkNextStepLabel), MessageType.Info);
                }
            }
        }
    }
}
