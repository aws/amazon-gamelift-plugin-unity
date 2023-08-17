// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    internal class Waiter
    {
        public event Action InfoUpdated;

        private CoreApi _gameLiftCoreApi;
        private bool _isWaiting;

        public Waiter()
        {
            _gameLiftCoreApi = CoreApi.SharedInstance;
        }

        public async Task<Response> WaitUntilDone(DeploymentSettings settings)
        {
            if (_isWaiting)
            {
                return Response.Fail(new Response() {ErrorCode = ErrorCode.OperationInvalid});
            }

            _isWaiting = true;

            try
            {
                return await PollStatusUntilDone(settings);
            }
            finally
            {
                _isWaiting = false;
            }
        }

        private async Task<Response> PollStatusUntilDone(DeploymentSettings settings)
        {
            var poller = new Poller();
            DescribeStackResponse describeStackResponse = await poller.Poll(1000,
                () =>
                {
                    DescribeStackResponse response = _gameLiftCoreApi.DescribeStack(settings.CurrentProfile, settings.CurrentRegion, _gameLiftCoreApi.GetStackName(settings.GameName));

                    if (!response.Success)
                    {
                        return response;
                    }
                    
                    InfoUpdated?.Invoke();
                    return response;
                },
                stopCondition: target => !_isWaiting || target.StackStatus.IsStackStatusOperationDone());

            if (!describeStackResponse.Success)
            {
                return Response.Fail(new DeploymentResponse(describeStackResponse));
            }

            if (!_isWaiting)
            {
                return Response.Fail(new DeploymentResponse(ErrorCode.OperationCancelled));
            }

            return Response.Ok(new Response());
        }
    }
}