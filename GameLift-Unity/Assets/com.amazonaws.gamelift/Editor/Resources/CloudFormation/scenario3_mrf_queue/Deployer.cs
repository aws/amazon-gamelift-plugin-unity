// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.MrfQueue
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "Multi-Region Fleets with Queue and Custom Matchmaker";

        public override string Description =>
            "This CloudFormation template sets up a scenario where Amazon GameLift queues are used in conjunction with a custom matchmaker. "
            + "For simplicity sake, this custom matchmaker form matches by taking the oldest players in the waiting pool and not considering any other factors such as skills or latency. "
            + "Once the group of players to form matches is identified, a lambda function calls GameLift:StartGameSessionPlacement to start a queue placement.";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario3_mrf_queue";

        public override bool HasGameServer => true;

        public override int PreferredUiOrder => 3;

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
