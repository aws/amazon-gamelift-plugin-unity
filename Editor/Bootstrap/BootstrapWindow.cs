// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class BootstrapWindow : EditorWindow
    {
        private const float WindowWidthPixels = 560f;
        private const float WindowSelectionHeightPixels = 340f;
        private const float WindowCreationHeightPixels = 300f;
        private const float WarningHeightPixels = 45f;
        private const float TopMarginPixels = 15f;
        private const float LeftMarginPixels = 15f;
        private const float RightMarginPixels = 13f;
        private const float SelectionTextFieldHeight = 20f;
        private const float SelectionHeightPixels = 110f;
        private const float LabelWidthPixels = 150f;
        private const float VerticalSpacingPixels = 5f;
        private const float SpinnerSizePixels = 23f;
        private HyperLinkButton _consoleHyperLinkButton;
        private TextFilter _bucketsTextFilter;
        private StatusLabel _statusLabel;
        private BootstrapSettings _bootstrapSettings;
        private int _confirmedMode = -1;
        private ImageSequenceDrawer _spinnerDrawer;
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
        private string _labelLoading;
        private string _policyWarning;
        private int _cachedPolicy = -1;
        private bool _needResize;
        private CancellationTokenSource _refreshBucketsCancellation;

        private void SetUp()
        {
            _textProvider = TextProviderFactory.Create();
            titleContent = new GUIContent(_textProvider.Get(Strings.TitleBootstrap));
            _bootstrapSettings = BootstrapSettingsFactory.Create();
            _bucketsTextFilter = new TextFilter(string.Empty, SelectionHeightPixels);
            _statusLabel = new StatusLabel();
            _controlDrawer = ControlDrawerFactory.Create();
            _spinnerDrawer = SpinnerDrawerFactory.Create(size: SpinnerSizePixels);
            _consoleHyperLinkButton = new HyperLinkButton(_textProvider.Get(Strings.LabelBootstrapS3Console),
              Urls.AwsS3Console, ResourceUtility.GetHyperLinkStyle());
            _bootstrapSettings.OnBucketsLoaded += OnBucketsLoaded;
            _bootstrapSettings.Status.Changed += OnStatusChanged;
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
            _policyWarning = _textProvider.Get(Strings.LabelBootstrapLifecycleWarning);
            _labelLoading = _textProvider.Get(Strings.LabelBootstrapBucketSelectionLoading);

            _refreshBucketsCancellation = new CancellationTokenSource();
            _bootstrapSettings.SetUp(_refreshBucketsCancellation.Token)
                .ContinueWith(task =>
                {
                    _confirmedMode = _bootstrapSettings.SelectedMode;
                    _bootstrapSettings.RefreshCurrentBucket();
                    UpdateWindowSize();
                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void OnEnable()
        {
            SetUp();
        }

        private void OnDisable()
        {
            _refreshBucketsCancellation?.Cancel();
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
        private void OnInspectorUpdate()
        {
            if (_bootstrapSettings.IsBucketListLoaded)
            {
                _spinnerDrawer.Stop();
            }
            else
            {
                _spinnerDrawer.Start();
            }

            if (_spinnerDrawer.IsRunning)
            {
                Repaint();
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
            _bootstrapSettings.RefreshCurrentBucket();

            if (_bootstrapSettings.SelectedMode == BootstrapSettings.CreationMode)
            {
                _refreshBucketsCancellation?.Cancel();
                _bootstrapSettings.RefreshBucketName();
            }
            else
            {
                _refreshBucketsCancellation = new CancellationTokenSource();
                _ = _bootstrapSettings.RefreshExistingBuckets(_refreshBucketsCancellation.Token);
            }

            GUI.FocusControl(null);
            UpdateWindowSize();
        }

        #region Form controls

        private void DrawCurrentBucket()
        {
            _controlDrawer.DrawReadOnlyText(_labelBootstrapCurrentBucket, _bootstrapSettings.CurrentBucketName, _tooltipCurrentBucket);

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

        private void DrawModeSelection()
        {
            _bootstrapSettings.SelectedMode = GUILayout.SelectionGrid(_bootstrapSettings.SelectedMode, _uiModes, 1, EditorStyles.radioButton);
        }

        private void DrawBucketCreation()
        {
            EditorGUILayout.HelpBox(_labelBucketCosts, MessageType.Warning);

            _bootstrapSettings.BucketName = _controlDrawer.DrawTextField(_labelBootstrapBucketName, _bootstrapSettings.BucketName);
            _controlDrawer.DrawReadOnlyText(_labelBootstrapRegion, _bootstrapSettings.CurrentRegion, _tooltipRegion);
            GUILayout.Space(VerticalSpacingPixels);

            _bootstrapSettings.LifeCyclePolicyIndex = _controlDrawer.DrawPopup(
                _labelBootstrapBucketLifecycle,
                _bootstrapSettings.LifeCyclePolicyIndex, _bootstrapSettings.AllLifecyclePolicyNames,
                _tooltipBootstrapBucketLifecycle);

            if (_cachedPolicy != _bootstrapSettings.LifeCyclePolicyIndex)
            {
                _cachedPolicy = _bootstrapSettings.LifeCyclePolicyIndex;
                _needResize = true;
            }

            if (_bootstrapSettings.LifeCyclePolicyIndex != BootstrapSettings.NoneLifeCyclePolicyIndex)
            {
                EditorGUILayout.HelpBox(_policyWarning, MessageType.Warning);
            }

            GUILayout.Space(VerticalSpacingPixels);

            if (_needResize)
            {
                UpdateWindowSize();
            }
        }

        private void DrawBucketSelection()
        {
            EditorGUILayout.PrefixLabel(_labelBootstrapBucketName);

            if (!_bootstrapSettings.IsBucketListLoaded)
            {

                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(SelectionHeightPixels + SelectionTextFieldHeight)))
                {
                    GUILayout.Label(_labelLoading);
                    _spinnerDrawer.Draw();
                }
            }
            else
            {
                _bucketsTextFilter.Draw();
            }

            _bootstrapSettings.SelectBucket(_bucketsTextFilter.ConfirmedOption);
        }

        private void DrawStatus()
        {
            _statusLabel.Draw(_bootstrapSettings.Status.Message, _bootstrapSettings.Status.Type);
        }

        #endregion

        private void DrawLink(HyperLinkButton link)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);
                link.Draw();
            }
        }

        private void UpdateWindowSize()
        {
            switch (_bootstrapSettings.SelectedMode)
            {
                case BootstrapSettings.CreationMode:
                    float heightPixels = _cachedPolicy == BootstrapSettings.NoneLifeCyclePolicyIndex
                        ? WindowCreationHeightPixels
                        : WindowCreationHeightPixels + WarningHeightPixels;
                    SetWindowSize(heightPixels);
                    break;
                default:
                    SetWindowSize(WindowSelectionHeightPixels);
                    break;
            }
        }

        private void SetWindowSize(float height)
        {
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: height));
        }

        private void OnBucketsLoaded(IReadOnlyList<string> buckets)
        {
            _bucketsTextFilter.SetOptions(buckets);
            Repaint();
        }

        private void OnStatusChanged()
        {
            Repaint();
        }
    }
}
