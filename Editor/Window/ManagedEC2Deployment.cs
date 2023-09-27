// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2Deployment
    {
        private readonly DeploymentSettings _deploymentSettings;
        
        internal ManagedEC2Deployment(DeploymentSettings deploymentSettings, ManagedEC2FleetParameters parameters)
        {
            _deploymentSettings = deploymentSettings;
            UpdateModelFromParameters(parameters);
        }
        
        public void UpdateModelFromParameters(ManagedEC2FleetParameters parameters)
        {
            _deploymentSettings.FleetName = parameters.FleetName;
            _deploymentSettings.BuildName = parameters.BuildName;
            _deploymentSettings.LaunchParameters = parameters.LaunchParameters;
            _deploymentSettings.BuildFolderPath = parameters.GameServerFolder;
            _deploymentSettings.BuildFilePath = parameters.GameServerFile;
            _deploymentSettings.BuildOperatingSystem = parameters.OperatingSystem;
        }
        
        public void StartDeployment()
        {
            _deploymentSettings.GameName = Application.productName.Substring(0, 12);
            _deploymentSettings.Save();
            _deploymentSettings.StartDeployment(ConfirmChanges).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
            });
        }

        public async Task DeleteDeployment()
        {
            await _deploymentSettings.DeleteDeployment();
        }

        private Task<bool> ConfirmChanges(ConfirmChangesRequest request)
        {
            var stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());

            StackUpdateDialog dialog = UnityEditor.EditorWindow.GetWindow<StackUpdateDialog>();
            return dialog.SetUp(stackUpdateModelFactory.Create(request));
        }
    }
}