// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift.Model;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public interface IAmazonGameLiftClientWrapper
    {
        Task<CreateGameSessionResponse> CreateGameSessionAsync(
                CreateGameSessionRequest request,
                CancellationToken cancellationToken = default
            );

        Task<CreatePlayerSessionResponse> CreatePlayerSession(CreatePlayerSessionRequest request);

        Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request);

        Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request);
    }
}
