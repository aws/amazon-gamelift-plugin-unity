// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.AuthOnly
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "Auth Only";

        public override string Description =>
            "This CloudFormation template sets up a minimal game backend service with only 1 functionality -- player authentication. "
            + "Lambda handler to start a game and view game connection information are stubbed to always return 501 error (Unimplemented).";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario1_auth_only";

        public override bool HasGameServer => false;

        public override int PreferredUiOrder => 1;

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
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
