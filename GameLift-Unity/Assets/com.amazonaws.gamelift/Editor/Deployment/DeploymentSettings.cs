// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using UnityEngine;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// The main backend for deployment to AWS.
    /// </summary>
    internal class DeploymentSettings
    {
        private const int StackInfoRefreshDelayMs = 2000;
        private readonly List<DeployerBase> _deployers = new List<DeployerBase>();
        private readonly ScenarioLocator _scenarioLocator;
        private readonly PathConverter _pathConverter;
        private readonly CoreApi _coreApi;
        private readonly ScenarioParametersUpdater _parametersUpdater;
        private readonly TextProvider _textProvider;
        private readonly DeploymentWaiter _deploymentWaiter;
        private readonly DelayedOperation _delayedStackInfoRefresh;
        private int _scenarioIndex;
        private string _gameName;
        private DeploymentStackInfo _currentStackInfo;
        private readonly IDeploymentIdContainer _currentDeploymentId;
        private readonly ILogger _logger;
        private readonly Status _status = new Status();

        public IReadStatus Status => _status;

        #region Bootstrap parameters

        public string CurrentProfile { get; private set; }

        public string CurrentRegion { get; private set; }

        public string CurrentBucketName { get; private set; }

        public bool HasCurrentBucket { get; private set; }

        public bool IsBootstrapped => CurrentProfile != null && HasCurrentBucket;

        #endregion

        public string[] AllScenarios { get; private set; } = new string[0];

        public string ScenarioName { get; private set; }

        public string ScenarioPath { get; private set; }

        public string ScenarioDescription { get; private set; }

        public string ScenarioHelpUrl { get; private set; }

        #region Scenario parameters

        public string GameName
        {
            get => _gameName;
            set => _ = SetGameNameAsync(value);
        }

        public string BuildFolderPath { get; set; }

        public string BuildFilePath { get; set; }

        #endregion Scenario parameters

        public int ScenarioIndex
        {
            get => _scenarioIndex;
            set
            {
                if (_scenarioIndex == value)
                {
                    return;
                }

                _scenarioIndex = value;
                RefreshScenario();
            }
        }

        public bool IsFormFilled => !string.IsNullOrWhiteSpace(GameName)
            && IsValidScenarioIndex
            && IsBuildFolderPathFilled
            && IsBuildFilePathFilled;

        public bool IsBuildFilePathFilled => IsValidScenarioIndex
            && (!_deployers[ScenarioIndex].HasGameServer || _coreApi.FileExists(BuildFilePath));

        public bool IsBuildFolderPathFilled => IsValidScenarioIndex
           && (!_deployers[ScenarioIndex].HasGameServer || _coreApi.FolderExists(BuildFolderPath));

        public bool IsBuildRequired =>
            IsValidScenarioIndex && _deployers[ScenarioIndex].HasGameServer;

        public bool IsValidScenarioIndex => ScenarioIndex >= 0 && ScenarioIndex < _deployers.Count;

        public bool DoesDeploymentExist { get; private set; }

        public bool HasCurrentStack => CurrentStackInfo.Details != null;

        public DeploymentStackInfo CurrentStackInfo
        {
            get => _currentStackInfo;
            private set
            {
                _currentStackInfo = value;
                CurrentStackInfoChanged?.Invoke();
            }
        }

        public bool IsDeploymentRunning { get; private set; }

        public bool CanCancel => _deploymentWaiter.CanCancel == true;

        public bool CanDeploy => !IsDeploymentRunning && IsBootstrapped && IsFormFilled && IsCurrentStackModifiable;

        public bool IsCurrentStackModifiable =>
            CurrentStackInfo.StackStatus == null || CurrentStackInfo.StackStatus.IsStackStatusModifiable();

        public event Action CurrentStackInfoChanged;

        internal DeploymentSettings(ScenarioLocator scenarioLocator, PathConverter pathConverter,
            CoreApi coreApi, ScenarioParametersUpdater parametersUpdater, TextProvider textProvider, DeploymentWaiter deploymentWaiter, IDeploymentIdContainer currentDeploymentId, Delay delay, ILogger logger)
        {
            _scenarioLocator = scenarioLocator ?? throw new ArgumentNullException(nameof(scenarioLocator));
            _pathConverter = pathConverter ?? throw new ArgumentNullException(nameof(pathConverter));
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _parametersUpdater = parametersUpdater ?? throw new ArgumentNullException(nameof(parametersUpdater));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _deploymentWaiter = deploymentWaiter ?? throw new ArgumentNullException(nameof(deploymentWaiter));
            _currentDeploymentId = currentDeploymentId ?? throw new ArgumentNullException(nameof(currentDeploymentId));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ClearCurrentStackInfo();
            _delayedStackInfoRefresh = new DelayedOperation(RefreshCurrentStackInfo, delay, StackInfoRefreshDelayMs);
        }

        public async Task SetGameNameAsync(string value)
        {
            if (_gameName == value)
            {
                return;
            }

            _gameName = value;
            ClearCurrentStackInfo();
            await _delayedStackInfoRefresh.Request();
        }

        public void Refresh()
        {
            IEnumerable<DeployerBase> deployers = _scenarioLocator.GetScenarios();
            _deployers.Clear();
            _deployers.AddRange(deployers);

            AllScenarios = _deployers.Select(deployer => deployer.DisplayName).ToArray();

            GetSettingResponse bucketNameResponse = _coreApi.GetSetting(SettingsKeys.CurrentBucketName);
            CurrentBucketName = bucketNameResponse.Success ? bucketNameResponse.Value : null;
            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);
            CurrentRegion = currentRegionResponse.Success ? currentRegionResponse.Value : null;

            HasCurrentBucket = !string.IsNullOrEmpty(CurrentBucketName) && _coreApi.IsValidRegion(CurrentRegion);

            GetSettingResponse profileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);
            CurrentProfile = profileResponse.Success ? profileResponse.Value : null;
            RefreshScenario();
        }

        public void RefreshCurrentStackInfo()
        {
            if (IsDeploymentRunning)
            {
                return;
            }

            if (string.IsNullOrEmpty(GameName))
            {
                ClearCurrentStackInfo();
                return;
            }

            if (string.IsNullOrEmpty(CurrentProfile))
            {
                _logger.Log(string.Format(DevStrings.FailedToDescribeStackTemplate, DevStrings.ProfileInvalid), LogType.Warning);
                ClearCurrentStackInfo();
                return;
            }

            if (!_coreApi.IsValidRegion(CurrentRegion))
            {
                _logger.Log(string.Format(DevStrings.FailedToDescribeStackTemplate, DevStrings.RegionInvalid), LogType.Warning);
                ClearCurrentStackInfo();
                return;
            }

            string stackName = _coreApi.GetStackName(GameName);
            DescribeStackResponse describeResponse = _coreApi.DescribeStack(CurrentProfile, CurrentRegion, stackName);

            if (!describeResponse.Success)
            {
                ClearCurrentStackInfo();
                return;
            }

            CurrentStackInfo = DeploymentStackInfoFactory.Create(_textProvider, describeResponse, CurrentRegion, ScenarioName);
        }

        public void Restore()
        {
            ScenarioIndex = 1; // Selects "Single-Region Fleet" Deployment Scenario by default
            BuildFilePath = null;
            BuildFolderPath = null;
            GetSettingResponse response = _coreApi.GetSetting(SettingsKeys.DeploymentGameName);
            GameName = response.Success ? response.Value : null;
            GetSettingResponse scenarioIndexResponse = _coreApi.GetSetting(SettingsKeys.DeploymentScenarioIndex);

            if (scenarioIndexResponse.Success)
            {
                int? index = SettingsFormatter.ParseInt(scenarioIndexResponse.Value);
                ScenarioIndex = index ?? 0;
            }

            GetSettingResponse serverPathResponse = _coreApi.GetSetting(SettingsKeys.DeploymentBuildFolderPath);

            if (serverPathResponse.Success)
            {
                BuildFolderPath = serverPathResponse.Value;
            }

            GetSettingResponse serverExePathResponse = _coreApi.GetSetting(SettingsKeys.DeploymentBuildFilePath);

            if (serverExePathResponse.Success)
            {
                BuildFilePath = serverExePathResponse.Value;
            }
        }

        public void Save()
        {
            _coreApi.PutSetting(SettingsKeys.DeploymentScenarioIndex, SettingsFormatter.FormatInt(ScenarioIndex));
            _coreApi.PutSettingOrClear(SettingsKeys.DeploymentBuildFolderPath, BuildFolderPath);
            _coreApi.PutSettingOrClear(SettingsKeys.DeploymentBuildFilePath, BuildFilePath);
            _coreApi.PutSettingOrClear(SettingsKeys.DeploymentGameName, GameName);
        }

        public async Task WaitForCurrentDeployment()
        {
            if (IsDeploymentRunning || !_currentDeploymentId.HasValue)
            {
                return;
            }

            try
            {
                IsDeploymentRunning = true;
                _delayedStackInfoRefresh.Cancel();
                _deploymentWaiter.InfoUpdated += OnDeploymentWaiterInfoUpdated;
                DeploymentResponse response = await _deploymentWaiter.WaitUntilDone(_currentDeploymentId.Get());
                LogWaitResponse(response);

                if (response.ErrorCode != ErrorCode.OperationCancelled)
                {
                    _currentDeploymentId.Clear();
                }
            }
            catch (Exception)
            {
                _currentDeploymentId.Clear();
                CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusExceptionThrown));
                throw;
            }
            finally
            {
                IsDeploymentRunning = false;
                _deploymentWaiter.InfoUpdated -= OnDeploymentWaiterInfoUpdated;
            }
        }

        private void LogWaitResponse(DeploymentResponse response)
        {
            if (response.Success || response.ErrorCode == ErrorCode.OperationCancelled)
            {
                return;
            }

            if (response.ErrorCode == ErrorCode.StackStatusInvalid
                || response.ErrorCode == CoreErrorCode.StackDoesNotExist)
            {
                _logger.LogResponseError(response, LogType.Log);
            }
            else
            {
                _logger.LogResponseError(response);
            }
        }

        public void CancelWaitingForDeployment()
        {
            _deploymentWaiter.CancelWaiting();
        }

        public void CancelDeployment()
        {
            if (!CanCancel)
            {
                _logger.Log(DevStrings.OperationInvalid, LogType.Warning);
                return;
            }

            Response response = _deploymentWaiter.CancelDeployment();

            if (response.Success)
            {
                RefreshCurrentStackInfo();
            }
            else
            {
                _logger.LogResponseError(response);
            }
        }

        public async Task StartDeployment(ConfirmChangesDelegate confirmChanges)
        {
            if (confirmChanges is null)
            {
                throw new ArgumentNullException(nameof(confirmChanges));
            }

            if (!IsFormFilled)
            {
                return;
            }

            string exeFilePath = null;
            DeployerBase currentDeployer = _deployers[ScenarioIndex];

            if (currentDeployer.HasGameServer)
            {
                exeFilePath = GetExeFilePathInBuildOrNull();

                if (exeFilePath == null)
                {
                    _status.IsDisplayed = true;
                    _status.SetMessage(_textProvider.Get(Strings.StatusDeploymentExePathInvalid), MessageType.Error);
                    return;
                }
            }

            if (IsDeploymentRunning)
            {
                return;
            }

            IsDeploymentRunning = true;
            _delayedStackInfoRefresh.Cancel();
            string parametersPath = _pathConverter.GetParametersFilePath(ScenarioPath);
            IReadOnlyDictionary<string, string> parameters = currentDeployer.HasGameServer
                ? PrepareParameters(exeFilePath)
                : PrepareGameParameter();
            _parametersUpdater.Update(parametersPath, parameters);
            CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusDeploymentStarting));
            string stackName = _coreApi.GetStackName(GameName);
            var deploymentId = new DeploymentId(CurrentProfile, CurrentRegion, stackName, currentDeployer.DisplayName);
            _currentDeploymentId.Set(deploymentId);

            try
            {
                DeploymentResponse response = await currentDeployer.StartDeployment(ScenarioPath, BuildFolderPath, GameName,
                    isDevelopmentBuild: EditorUserBuildSettings.development, confirmChanges);

                if (!response.Success)
                {
                    if (response.ErrorCode != ErrorCode.OperationCancelled)
                    {
                        _logger.LogResponseError(response);
                        string messageTemplate = _textProvider.Get(Strings.StatusDeploymentFailure);
                        string message = string.Format(messageTemplate, _textProvider.GetError(response.ErrorCode));
                        _status.SetMessage(message, MessageType.Error);
                        _status.IsDisplayed = true;
                    }

                    return;
                }

                _deploymentWaiter.InfoUpdated += OnDeploymentWaiterInfoUpdated;
                response = await _deploymentWaiter.WaitUntilDone(deploymentId);
                LogWaitResponse(response);

                if (response.ErrorCode != ErrorCode.OperationCancelled)
                {
                    _currentDeploymentId.Clear();
                }
            }
            catch (Exception ex)
            {
                _currentDeploymentId.Clear();
                _logger.LogException(ex);
                string messageTemplate = _textProvider.Get(Strings.StatusDeploymentFailure);
                string message = string.Format(messageTemplate, ex.Message);
                _status.SetMessage(message, MessageType.Error);
                _status.IsDisplayed = true;
                throw;
            }
            finally
            {
                IsDeploymentRunning = false;
                _deploymentWaiter.InfoUpdated -= OnDeploymentWaiterInfoUpdated;
                RefreshCurrentStackInfo();
            }
        }

        private void OnDeploymentWaiterInfoUpdated(DeploymentInfo info)
        {
            CurrentStackInfo = DeploymentStackInfoFactory.Create(_textProvider, info);
        }

        private IReadOnlyDictionary<string, string> PrepareGameParameter()
        {
            return new Dictionary<string, string>
            {
                { ScenarioParameterKeys.GameName, GameName }
            };
        }

        private IReadOnlyDictionary<string, string> PrepareParameters(string exeFilePathInBuild)
        {
            string launchPath = _coreApi.GetServerGamePath(exeFilePathInBuild);
            return new Dictionary<string, string>
            {
                { ScenarioParameterKeys.GameName, GameName },
                { ScenarioParameterKeys.LaunchPath, launchPath },
            };
        }

        private string GetExeFilePathInBuildOrNull()
        {
            int index = BuildFilePath.IndexOf(BuildFolderPath);

            if (index < 0)
            {
                return null;
            }

            return BuildFilePath.Remove(0, BuildFolderPath.Length).TrimStart('\\', '/');
        }

        private void RefreshScenario()
        {
            if (!IsValidScenarioIndex)
            {
                return;
            }

            DeployerBase deployer = _deployers[ScenarioIndex];
            ScenarioName = deployer.DisplayName;
            ScenarioDescription = deployer.Description;
            ScenarioHelpUrl = deployer.HelpUrl;
            ScenarioPath = _pathConverter.GetScenarioAbsolutePath(deployer.ScenarioFolder);

            if (!deployer.HasGameServer)
            {
                BuildFolderPath = null;
                BuildFilePath = null;
            }

            RefreshCurrentStackInfo();
        }

        private void ClearCurrentStackInfo()
        {
            CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusNothingDeployed));
        }
    }
}
