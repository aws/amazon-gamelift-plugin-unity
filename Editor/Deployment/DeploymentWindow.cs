// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class DeploymentWindow : EditorWindow
    {
        private const float WindowWidthPixels = 600f;
#if UNITY_2019_1_OR_NEWER
        private const float WindowIdleHeightNoFormPixels = 375f;
#else
        private const float WindowIdleHeightNoFormPixels = 350f;
#endif
        private const float TopMarginPixels = 18f;
        private const float LeftMarginPixels = 15f;
        private const float RightMarginPixels = 13f;
        private const float LabelWidthPixels = 185f;
        private const float VerticalSpacingPixels = 5f;
        private const float SpinnerSizePixels = 23f;

        private HyperLinkButton _cfnConsoleLinkButton;
        private HyperLinkButton _deploymentHelpLinkButton;
        private HyperLinkButton _scenarioHelpLinkButton;
        private StatusLabel _statusLabel;
        private DeploymentSettings _model;
        private int _previousScenarioIndex = -1;
        private bool? _previousBootstrapped;
        private float _bootstrapWarningHeight;
        private float _scenarioHeight;
        private bool _isSizeDirty;
        private TextProvider _textProvider;

        private string _labelSelectionMode;
        private string _labelCurrentStack;
        private string _labelGameName;
        private string _labelBuildFolderPath;
        private string _labelBuildFilePath;
        private string _labelRegion;
        private string _tooltipRegion;
        private string _labelBucket;
        private string _tooltipBucket;
        private string _labelScenarioHelp;
        private string _labelStartButton;
        private string _labelCancelButton;
        private string _labelDefaultFolderName;
        private string _labelApiGateway;
        private string _labelCognitoClientId;
        private string _labelDeploymentCosts;
        private string _labelBootstrapWarning;
        private string _titleServerFileDialog;
        private string _titleServerFolderDialog;
        private StackUpdateModelFactory _stackUpdateModelFactory;
        private ImageSequenceDrawer _spinnerDrawer;
        private ControlDrawer _controlDrawer;
        private string _labelScenarioPath;

        private bool IsFormDisabled => _model == null || _model.IsDeploymentRunning;

        private void SetUp()
        {
            _stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());
            _textProvider = TextProviderFactory.Create();
            _labelSelectionMode = _textProvider.Get(Strings.LabelDeploymentSelectionMode);
            _labelCurrentStack = _textProvider.Get(Strings.LabelDeploymentCurrentStack);
            _labelGameName = _textProvider.Get(Strings.LabelDeploymentGameName);
            _labelBuildFilePath = _textProvider.Get(Strings.LabelDeploymentBuildFilePath);
            _labelBuildFolderPath = _textProvider.Get(Strings.LabelDeploymentBuildFolderPath);
            _labelRegion = _textProvider.Get(Strings.LabelDeploymentRegion);
            _tooltipRegion = _textProvider.Get(Strings.TooltipDeploymentRegion);
            _labelBucket = _textProvider.Get(Strings.LabelDeploymentBucket);
            _tooltipBucket = _textProvider.Get(Strings.TooltipDeploymentBucket);
            _labelScenarioPath = _textProvider.Get(Strings.LabelDeploymentScenarioPath);
            _labelScenarioHelp = _textProvider.Get(Strings.LabelDeploymentScenarioHelp);
            _labelStartButton = _textProvider.Get(Strings.LabelDeploymentStartButton);
            _labelCancelButton = _textProvider.Get(Strings.LabelDeploymentCancelButton);
            _labelDefaultFolderName = _textProvider.Get(Strings.LabelDefaultFolderName);
            _labelApiGateway = _textProvider.Get(Strings.LabelDeploymentApiGateway);
            _labelCognitoClientId = _textProvider.Get(Strings.LabelDeploymentCognitoClientId);
            _labelDeploymentCosts = _textProvider.Get(Strings.LabelDeploymentCosts);
            _labelBootstrapWarning = _textProvider.Get(Strings.LabelDeploymentBootstrapWarning);
            _titleServerFileDialog = _textProvider.Get(Strings.TitleDeploymentServerFileDialog);
            _titleServerFolderDialog = _textProvider.Get(Strings.TitleDeploymentServerFolderDialog);

            titleContent = new GUIContent(_textProvider.Get(Strings.TitleDeployment));
            _model = DeploymentSettingsFactory.Create();
            string deploymentHelpLabelText = _textProvider.Get(Strings.LabelDeploymentHelp);
            _deploymentHelpLinkButton = new HyperLinkButton(deploymentHelpLabelText, Urls.AwsHelpDeployment, ResourceUtility.GetHyperLinkStyle());
            _scenarioHelpLinkButton = new HyperLinkButton(_labelScenarioHelp, string.Empty, ResourceUtility.GetHyperLinkStyle());
            _statusLabel = new StatusLabel();
            string goToCloudFormationConsoleLabelText = _textProvider.Get(Strings.LabelDeploymentCloudFormationConsole);
            _cfnConsoleLinkButton = new HyperLinkButton(goToCloudFormationConsoleLabelText, Urls.AwsCloudFormationConsole, ResourceUtility.GetHyperLinkStyle());
            SetWindowSize(WindowIdleHeightNoFormPixels);
            _model.Refresh();
            _model.Restore();
            _spinnerDrawer = SpinnerDrawerFactory.Create(size: SpinnerSizePixels);
            _controlDrawer = ControlDrawerFactory.Create();
            _ = _model.WaitForCurrentDeployment();
        }

        private void OnEnable()
        {
            SetUp();
            _model.CurrentStackInfoChanged += OnCurrentStackInfoChanged;
            _model.Status.Changed += OnStatusChanged;
            Settings.SharedInstance.AnySettingChanged += OnAnySettingChanged;
        }

        private void OnDisable()
        {
            Settings.SharedInstance.AnySettingChanged -= OnAnySettingChanged;
            _model.Save();
            _model.CancelWaitingForDeployment();
            _model.Status.Changed -= OnStatusChanged;
            _model.CurrentStackInfoChanged -= OnCurrentStackInfoChanged;
        }

        private void OnInspectorUpdate()
        {
            if (_spinnerDrawer.IsRunning)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = LabelWidthPixels;
            float scenarioHeight;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(TopMarginPixels);
                    EditorGUILayout.HelpBox(_labelDeploymentCosts, MessageType.Warning);

                    if (!_model.IsBootstrapped)
                    {
                        EditorGUILayout.HelpBox(_labelBootstrapWarning, MessageType.Warning);
                    }

                    GUILayout.Space(VerticalSpacingPixels);

                    using (new EditorGUI.DisabledScope(IsFormDisabled))
                    {
                        scenarioHeight = DrawScenarioSelection();
                        DrawSeparator();
                        scenarioHeight += DrawScenarioForm();
                    }

                    using (new EditorGUI.DisabledScope(false))
                    {
                        DrawSeparator();
                        GUILayout.Label(_labelCurrentStack);
                        GUILayout.Space(VerticalSpacingPixels);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            DrawInfo(_model.CurrentStackInfo.Status);

                            if (_model.IsDeploymentRunning)
                            {
                                _spinnerDrawer.Draw();
                            }
                        }

                        if (_model.IsDeploymentRunning)
                        {
                            GUILayout.Space(EditorGUIUtility.singleLineHeight - SpinnerSizePixels);
                        }

                        GUILayout.Space(VerticalSpacingPixels);
                        DrawInfo(_model.CurrentStackInfo.Details, GUILayout.Height(55));
                        DrawStackOutput(_labelCognitoClientId, _model.CurrentStackInfo.UserPoolClientId);
                        DrawStackOutput(_labelApiGateway, _model.CurrentStackInfo.ApiGatewayEndpoint);
                        GUILayout.Space(VerticalSpacingPixels);
                        DrawLink(_model.HasCurrentStack ? _cfnConsoleLinkButton : _deploymentHelpLinkButton);
                        GUILayout.Space(VerticalSpacingPixels);
                        DrawButtons();

                        if (_model.Status.IsDisplayed)
                        {
                            _statusLabel.Draw(_model.Status.Message, _model.Status.Type);
                        }
                    }
                }

                GUILayout.Space(RightMarginPixels);
            }

            UpdateWindowSize(scenarioHeight);
            UpdateSpinnerDrawer();
        }

        private void UpdateSpinnerDrawer()
        {
            if (_model.IsDeploymentRunning)
            {
                _spinnerDrawer.Start();
            }
            else
            {
                _spinnerDrawer.Stop();
            }
        }

        private void DrawStackOutput(string label, string output)
        {
            if (output != null)
            {
                _controlDrawer.DrawReadOnlyText(label, output);
            }
            else
            {
                GUILayout.Space(.5f * VerticalSpacingPixels);
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
            }
        }

        private float DrawScenarioForm()
        {
            _model.GameName = DrawTextField(_labelGameName, _model.GameName);

            using (new EditorGUI.DisabledScope(!_model.IsBuildRequired))
            {
                _model.BuildFolderPath = _controlDrawer.DrawFolderPathField(_labelBuildFolderPath,
                    _model.BuildFolderPath, _labelDefaultFolderName, _titleServerFolderDialog);
                _model.BuildFilePath = _controlDrawer.DrawFilePathField(_labelBuildFilePath, _model.BuildFilePath, "exe", _titleServerFileDialog);
            }

            GUILayout.Space(VerticalSpacingPixels);
            _controlDrawer.DrawReadOnlyText(_labelRegion, _model.CurrentRegion, _tooltipRegion);
            _controlDrawer.DrawReadOnlyText(_labelBucket, _model.CurrentBucketName, _tooltipBucket);
            const int fieldCount = 4;
            return fieldCount * (VerticalSpacingPixels + EditorGUIUtility.singleLineHeight);
        }

        private void UpdateWindowSize(float scenarioHeight)
        {
            if (Event.current.type == EventType.Repaint)
            {
                UpdateIfScenarioChanged(scenarioHeight);
                UpdateIfBootstrapStateChanged();
            }

            if (_isSizeDirty)
            {
                _isSizeDirty = false;
                SetWindowSize(WindowIdleHeightNoFormPixels + _scenarioHeight + _bootstrapWarningHeight);
            }
        }

        private void UpdateIfScenarioChanged(float scenarioHeight)
        {
            if (_model.ScenarioIndex == _previousScenarioIndex)
            {
                return;
            }

            _previousScenarioIndex = _model.ScenarioIndex;
            _scenarioHeight = scenarioHeight;
            _isSizeDirty = true;
            _scenarioHelpLinkButton = new HyperLinkButton(_labelScenarioHelp, _model.ScenarioHelpUrl,
                ResourceUtility.GetHyperLinkStyle());
        }

        private void UpdateIfBootstrapStateChanged()
        {
            if (_model.IsBootstrapped == _previousBootstrapped)
            {
                return;
            }

            _previousBootstrapped = _model.IsBootstrapped;
            _bootstrapWarningHeight = _model.IsBootstrapped ? 0f : 40f;
            _isSizeDirty = true;
        }

        #region Form controls

        private float DrawScenarioSelection()
        {
            _model.ScenarioIndex = EditorGUILayout.Popup(_labelSelectionMode, _model.ScenarioIndex, _model.AllScenarios);
            GUILayout.Space(VerticalSpacingPixels);
            float pathHeight = _controlDrawer.DrawReadOnlyTextWrapped(_labelScenarioPath, _model.ScenarioPath);
            GUILayout.Space(2 * VerticalSpacingPixels);
            Rect descriptionRect = DrawInfo(_model.ScenarioDescription);
            float height = 2 * VerticalSpacingPixels + descriptionRect.height + pathHeight - EditorGUIUtility.singleLineHeight;

            if (!string.IsNullOrEmpty(_model.ScenarioHelpUrl))
            {
                GUILayout.Space(VerticalSpacingPixels);
                DrawLink(_scenarioHelpLinkButton);
                height += VerticalSpacingPixels + EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        private void DrawButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool disabled = !_model.CanCancel;

                using (new EditorGUI.DisabledScope(disabled))
                {
                    if (GUILayout.Button(_labelCancelButton) && ConfirmCancel())
                    {
                        _model.CancelDeployment();
                    }
                }

                using (new EditorGUI.DisabledScope(!_model.CanDeploy))
                {
                    if (GUILayout.Button(_labelStartButton))
                    {
                        _model.StartDeployment(ConfirmChangeSet)
                            .ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    Debug.LogException(task.Exception);
                                }
                            });
                    }
                }
            }
        }

        #endregion

        private bool ConfirmCancel()
        {
            return EditorUtility.DisplayDialog(_textProvider.Get(Strings.TitleDeploymentCancelDialog),
                _textProvider.Get(Strings.LabelDeploymentCancelDialogBody),
                _textProvider.Get(Strings.LabelDeploymentCancelDialogOkButton),
                _textProvider.Get(Strings.LabelDeploymentCancelDialogCancelButton));
        }

        private Rect DrawInfo(string text, params GUILayoutOption[] options)
        {
            var scope = new EditorGUILayout.HorizontalScope(options);

            using (scope)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField(text, ResourceUtility.GetInfoLabelStyle());
            }

            return scope.rect;
        }

        private void DrawLink(HyperLinkButton linkButton)
        {
            using (new EditorGUI.DisabledScope(false))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5f);
                    linkButton.Draw();
                }
            }
        }

        private string DrawTextField(string label, string value)
        {
            return EditorGUILayout.TextField(label, value);
        }

        private void DrawSeparator()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }

        private void SetWindowSize(float height)
        {
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: height));
        }

        private Task<bool> ConfirmChangeSet(ConfirmChangesRequest request)
        {
            StackUpdateDialog dialog = GetWindow<StackUpdateDialog>();
            return dialog.SetUp(_stackUpdateModelFactory.Create(request));
        }

        private void OnCurrentStackInfoChanged()
        {
            Repaint();
        }

        private void OnStatusChanged()
        {
            Repaint();
        }

        private void OnAnySettingChanged()
        {
            _model.Refresh();
            Repaint();
        }
    }
}
