// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class BootstrapWindow : EditorWindow
    {
        private const float WindowWidthPixels = 560f;
#if UNITY_2019_1_OR_NEWER
        private const float WindowSelectionHeightPixels = 320f;
        private const float WindowCreationHeightPixels = 275f;
#else
        private const float WindowSelectionHeightPixels = 300f;
        private const float WindowCreationHeightPixels = 260f;
#endif
        private const float TopMarginPixels = 15f;
        private const float LeftMarginPixels = 15f;
        private const float RightMarginPixels = 13f;
        private const float SelectionHeightPixels = 110f;
        private const float LabelWidthPixels = 150f;
        private const float VerticalSpacingPixels = 5f;

        private HyperLinkButton _consoleHyperLinkButton;
        private TextFilter _bucketsTextFilter;
        private StatusLabel _statusLabel;
        private BootstrapSettings _bootstrapSettings;
        private int _confirmedMode = -1;
        private ControlDrawer _controlDrawer;
        private TextProvider _textProvider;

        private string[] _uiModes = new string[0];
        private string _labelBootstrapCreateButton;
        private string _labelBootstrapCurrentBucket;
        private string _labelBootstrapSelectButton;
        private string _labelBootstrapBucketName;
        private string _labelBootstrapRegion;
        private string _labelBootstrapBucketLifecycle;
        private string _tooltipBootstrapBucketLifecycle;
        private string _tooltipRegion;
        private string _tooltipCurrentBucket;
        private string _labelBucketCosts;

        private void SetUp()
        {
            _textProvider = TextProviderFactory.Create();
            titleContent = new GUIContent(_textProvider.Get(Strings.TitleBootstrap));
            _bootstrapSettings = BootstrapSettingsFactory.Create();
            _bucketsTextFilter = new TextFilter(string.Empty, SelectionHeightPixels);
            _statusLabel = new StatusLabel();
            _controlDrawer = ControlDrawerFactory.Create();
            _consoleHyperLinkButton = new HyperLinkButton(_textProvider.Get(Strings.LabelBootstrapS3Console),
              Urls.AwsS3Console, ResourceUtility.GetHyperLinkStyle());
            _bootstrapSettings.OnBucketsLoaded += OnBucketsLoaded;
            _bootstrapSettings.Status.Changed += OnStatusChanged;
            _bootstrapSettings.SetUp();
            _uiModes = new[]
            {
                _textProvider.Get(Strings.LabelBootstrapCreateMode),
                _textProvider.Get(Strings.LabelBootstrapSelectMode),
            };
            _labelBootstrapCurrentBucket = _textProvider.Get(Strings.LabelBootstrapCurrentBucket);
            _labelBootstrapCreateButton = _textProvider.Get(Strings.LabelBootstrapCreateButton);
            _labelBootstrapSelectButton = _textProvider.Get(Strings.LabelBootstrapSelectButton);
            _labelBootstrapBucketName = _textProvider.Get(Strings.LabelBootstrapBucketName);
            _labelBootstrapRegion = _textProvider.Get(Strings.LabelBootstrapRegion);
            _labelBootstrapBucketLifecycle = _textProvider.Get(Strings.LabelBootstrapBucketLifecycle);
            _tooltipBootstrapBucketLifecycle = _textProvider.Get(Strings.TooltipBootstrapBucketLifecycle);
            _tooltipRegion = _textProvider.Get(Strings.TooltipDeploymentRegion);
            _tooltipCurrentBucket = _textProvider.Get(Strings.TooltipBootstrapCurrentBucket);
            _labelBucketCosts = _textProvider.Get(Strings.LabelBootstrapBucketCosts);
        }

        private void OnEnable() =>
            SetUp();

        private void OnDisable()
        {
            _bootstrapSettings.Status.Changed -= OnStatusChanged;
            _bootstrapSettings.OnBucketsLoaded -= OnBucketsLoaded;
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
            SetUpForSelectedMode();
            DrawModeSelection();

            if (_bootstrapSettings.SelectedMode == BootstrapSettings.CreationMode)
            {
                DrawBucketCreation();
            }
            else
            {
                DrawBucketSelection();
            }

            _controlDrawer.DrawSeparator();
            DrawCurrentBucket();
            GUILayout.Space(10f);

            if (_bootstrapSettings.SelectedMode == BootstrapSettings.CreationMode)
            {
                DrawCreateButton();
            }
            else
            {
                DrawSelectButton();
            }

            if (_bootstrapSettings.Status.IsDisplayed)
            {
                DrawStatus();
            }
        }

        private void SetUpForSelectedMode()
        {
            if (_bootstrapSettings.SelectedMode == _confirmedMode)
            {
                return;
            }

            _confirmedMode = _bootstrapSettings.SelectedMode;
            _bucketsTextFilter.CurrentText = string.Empty;
            _bootstrapSettings.SelectBucket(string.Empty);
            _bootstrapSettings.RefreshCurrentBucket();

            if (_bootstrapSettings.SelectedMode == BootstrapSettings.CreationMode)
            {
                _bootstrapSettings.RefreshBucketName();
            }
            else
            {
                _bootstrapSettings.RefreshExistingBuckets();
            }

            GUI.FocusControl(null);
            UpdateWindowSize();
        }

        #region Form controls

        private void DrawCurrentBucket()
        {
            DrawBucketName(_labelBootstrapCurrentBucket, _bootstrapSettings.CurrentBucketName, _tooltipCurrentBucket);

            if (!_bootstrapSettings.HasCurrentBucket)
            {
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                return;
            }

            _consoleHyperLinkButton.Url = _bootstrapSettings.CurrentBucketUrl;
            DrawLink(_consoleHyperLinkButton);
        }

        private void DrawCreateButton()
        {
            using (new EditorGUI.DisabledScope(!_bootstrapSettings.CanCreate))
            {
                if (GUILayout.Button(_labelBootstrapCreateButton))
                {
                    _bootstrapSettings.CreateBucket();
                }
            }
        }

        private void DrawSelectButton()
        {
            using (new EditorGUI.DisabledScope(!_bootstrapSettings.CanSaveSelectedBucket))
            {
                if (GUILayout.Button(_labelBootstrapSelectButton))
                {
                    _bootstrapSettings.SaveSelectedBucket();
                }
            }
        }

        private void DrawModeSelection() =>
            _bootstrapSettings.SelectedMode = GUILayout.SelectionGrid(
              _bootstrapSettings.SelectedMode, _uiModes, 1, EditorStyles.radioButton);

        private void DrawBucketCreation()
        {
            EditorGUILayout.HelpBox(_labelBucketCosts, MessageType.Warning);

            DrawBucketName(_labelBootstrapBucketName, _bootstrapSettings.BucketName);
            _controlDrawer.DrawReadOnlyText(_labelBootstrapRegion, _bootstrapSettings.CurrentRegion, _tooltipRegion);
            GUILayout.Space(VerticalSpacingPixels);

            _bootstrapSettings.LifeCyclePolicyIndex = _controlDrawer.DrawPopup(
                _labelBootstrapBucketLifecycle,
                _bootstrapSettings.LifeCyclePolicyIndex, _bootstrapSettings.AllLifecyclePolicies,
                _tooltipBootstrapBucketLifecycle);

            GUILayout.Space(VerticalSpacingPixels);
        }

        private void DrawBucketSelection()
        {
            EditorGUILayout.PrefixLabel(_labelBootstrapBucketName);

            using (new EditorGUILayout.HorizontalScope())
            {
                _bucketsTextFilter.Draw();
                _bootstrapSettings.SelectBucket(_bucketsTextFilter.ConfirmedOption);
            }
        }

        private void DrawStatus() =>
            _statusLabel.Draw(_bootstrapSettings.Status.Message, _bootstrapSettings.Status.Type);

        #endregion

        private void DrawLink(HyperLinkButton link)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);
                link.Draw();
            }
        }

        private void DrawBucketName(string label, string value, string tooltip = null)
        {
            _controlDrawer.DrawReadOnlyText(label, value, tooltip);
        }

        private void UpdateWindowSize()
        {
            if (_bootstrapSettings.SelectedMode == BootstrapSettings.CreationMode)
            {
                SetWindowSize(WindowCreationHeightPixels);
            }
            else
            {
                SetWindowSize(WindowSelectionHeightPixels);
            }
        }

        private void SetWindowSize(float height) =>
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: height));

        private void OnBucketsLoaded(IReadOnlyList<string> buckets) =>
            _bucketsTextFilter.SetOptions(buckets);

        private void OnStatusChanged() => Repaint();
    }
}
