// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class ManagedEc2Deployment
    {
        private readonly DeploymentSettings _model;
        private readonly FleetParameters _parameters;
        
        internal ManagedEc2Deployment(DeploymentSettings model, FleetParameters parameters)
        {
            _model = model;
            _parameters = parameters;
        }
        
        public void UpdateModelFromParameters()
        {
            _model.FleetName = _parameters.FleetName;
            _model.BuildName = _parameters.BuildName;
            _model.LaunchParameters = _parameters.LaunchParameters;
            _model.BuildFolderPath = _parameters.GameServerFolder;
            _model.BuildFilePath = _parameters.GameServerFile;
            _model.BuildOperatingSystem = _parameters.OperatingSystem;
        }
        
        public void StartDeployment()
        {
            UpdateModelFromParameters();
            _model.GameName = Application.productName.Substring(0, 12);
            if (!_model.CanDeploy) return;
            _model.Save();
            _model.StartDeployment(ConfirmChanges).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
            });
        }

        public async Task DeleteDeployment()
        {
            var waiter = new Waiter(CoreApi.SharedInstance);
            waiter.InfoUpdated += () => { _model.RefreshCurrentStackInfo(); };
            await _model.DeleteDeployment();
            await waiter.WaitUntilDone(_model);
        }

        private Task<bool> ConfirmChanges(ConfirmChangesRequest request)
        {
            var stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());

            StackUpdateDialog dialog = UnityEditor.EditorWindow.GetWindow<StackUpdateDialog>();
            return dialog.SetUp(stackUpdateModelFactory.Create(request));
        }
    }
}