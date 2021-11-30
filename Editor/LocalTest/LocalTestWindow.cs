// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class LocalTestWindow : EditorWindow
    {
        private const float WindowWidthPixels = 570f;
#if UNITY_2019_1_OR_NEWER
        private const float WindowHeightPixels = 140f;
#else
        private const float WindowHeightPixels = 130f;
#endif
        private const float TopMarginPixels = 15f;
        private const float LeftMarginPixels = 15f;
        private const float RightMarginPixels = 13f;
        private const float LabelWidthPixels = 180f;
        private const float VerticalSpacingPixels = 5f;

        private StatusLabel _statusLabel;
        private HyperLinkButton _helpLinkButton;
        private ControlDrawer _controlDrawer;
        private LocalTest _model;

        private string _labelServerPath;
        private string _titleServerPathDialog;
        private string _tooltipLocalTestingServerPath;
        private string _labelLocalTestingPort;
        private string _tooltipPort;
        private string _labelStartButton;
        private string _labelStopButton;
        private string _labelGameLiftLocalPath;

        [NonSerialized]
        private bool _countedHeight;
        private CancellationTokenSource _cancellation;

        private void SetUp()
        {
            TextProvider textProvider = TextProviderFactory.Create();
            titleContent = new GUIContent(textProvider.Get(Strings.TitleLocalTesting));

            if (_model != null)
            {
                LocalTestFactory.Restore(_model, textProvider);
            }
            else
            {
                _model = LocalTestFactory.Create(textProvider);
            }

            _statusLabel = new StatusLabel();
            _helpLinkButton = new HyperLinkButton(textProvider.Get(Strings.LabelLocalTestingHelp),
              Urls.AwsHelpGameLiftLocal, ResourceUtility.GetHyperLinkStyle());
            _controlDrawer = ControlDrawerFactory.Create();
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: WindowHeightPixels));

            _labelServerPath = OperatingSystemUtility.isMacOs() ?
                    textProvider.Get(Strings.LabelLocalTestingMacOsServerPath) :
                    textProvider.Get(Strings.LabelLocalTestingWindowsServerPath);
            _titleServerPathDialog = textProvider.Get(Strings.TitleLocalTestingServerPathDialog);
            _tooltipLocalTestingServerPath = textProvider.Get(Strings.TooltipLocalTestingServerPath);
            _labelLocalTestingPort = textProvider.Get(Strings.LabelLocalTestingPort);
            _tooltipPort = textProvider.Get(Strings.TooltipLocalTestingPort);
            _labelStartButton = textProvider.Get(Strings.LabelLocalTestingStartButton);
            _labelStopButton = textProvider.Get(Strings.LabelLocalTestingStopButton);
            _labelGameLiftLocalPath = textProvider.Get(Strings.LabelLocalTestingJarPath);
            _model.Refresh();
        }

        private void OnEnable()
        {
            SetUp();
            _cancellation = new CancellationTokenSource();
            _model.Status.Changed += OnStatusChanged;
        }

        private void OnDisable()
        {
            _cancellation.Cancel();
            _model.Save();
            _model.Status.Changed -= OnStatusChanged;
        }

        private void OnGUI()
        {
            float uncountedHeight = 0f;
            EditorGUIUtility.labelWidth = LabelWidthPixels;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(TopMarginPixels);
                    uncountedHeight += DrawControls();
                }

                GUILayout.Space(RightMarginPixels);
            }

            if (!_countedHeight && Event.current.type == EventType.Repaint)
            {
                _countedHeight = true;
                this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: WindowHeightPixels + uncountedHeight));
            }
        }

        private float DrawControls()
        {
            float uncountedHeight = 0f;

            using (new EditorGUI.DisabledScope(_model.IsDeploymentRunning))
            {
                _model.BuildExecutablePath = _controlDrawer.DrawFilePathField(
                  _labelServerPath, _model.BuildExecutablePath, "exe,app", _titleServerPathDialog,
                  _tooltipLocalTestingServerPath);

                GUILayout.Space(VerticalSpacingPixels);
                float height = _controlDrawer.DrawReadOnlyTextWrapped(_labelGameLiftLocalPath, _model.GameLiftLocalPath);
                GUILayout.Space(VerticalSpacingPixels + height);
                _model.GameLiftLocalPort = _controlDrawer.DrawIntField(_labelLocalTestingPort, _model.GameLiftLocalPort, _tooltipPort);
                uncountedHeight += height;
            }

            GUILayout.Space(2 * VerticalSpacingPixels);
            DrawLink(_helpLinkButton);
            GUILayout.Space(VerticalSpacingPixels);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!_model.CanStop))
                {
                    if (GUILayout.Button(_labelStopButton))
                    {
                        _model.Stop();
                    }
                }

                using (new EditorGUI.DisabledScope(!_model.CanStart))
                {
                    if (GUILayout.Button(_labelStartButton))
                    {
                        _ = _model.Start(_cancellation.Token);
                    }
                }
            }

            if (_model.Status.IsDisplayed)
            {
                GUILayout.Space(VerticalSpacingPixels / 2f);
                _statusLabel.Draw(_model.Status.Message, _model.Status.Type);
            }

            return uncountedHeight;
        }

        private void DrawLink(HyperLinkButton linkButton)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);
                linkButton.Draw();
            }
        }

        private void OnStatusChanged()
        {
            Repaint();
        }
    }
}
