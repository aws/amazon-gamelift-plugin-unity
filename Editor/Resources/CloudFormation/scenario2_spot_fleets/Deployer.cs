// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.SpotFleets
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "SPOT Fleets with Queue and Custom Matchmaker";

        public override string Description =>
            "This CloudFormation template sets up the exact same scenario as 'Multi-Region Fleets', except that 3 fleets are created instead of 1, with 2 of the fleets being SPOT fleets containing nuanced instance types. "
            + "This is to demonstrate the best practices in using GameLift queues to keep availability high and cost low.";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario4_spot_fleets";

        public override bool HasGameServer => true;

        public override int PreferredUiOrder => 4;

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
            string zipPath = GameLiftCoreApi.GetUniqueTempFilePath();
            GameLiftCoreApi.Zip(request.BuildFolderPath, zipPath);

            UploadServerBuildResponse uploadBuildResponse = GameLiftCoreApi.UploadServerBuild(
                request.Profile, request.Region, request.BucketName, request.BuildS3Key, zipPath);

            if (GameLiftCoreApi.FileExists(zipPath))
            {
                GameLiftCoreApi.FileDelete(zipPath);
            }

            if (!uploadBuildResponse.Success)
            {
                return Task.FromResult(Response.Fail(new DeploymentResponse(uploadBuildResponse)));
            }

            ExecuteChangeSetResponse executeResponse = GameLiftCoreApi.ExecuteChangeSet(
                request.Profile, request.Region, request.StackName, request.ChangeSetName);

            if (!executeResponse.Success)
            {
                return Task.FromResult(Response.Fail(new DeploymentResponse(executeResponse)));
            }

            return Task.FromResult(Response.Ok(new DeploymentResponse()));
        }
    }
}
