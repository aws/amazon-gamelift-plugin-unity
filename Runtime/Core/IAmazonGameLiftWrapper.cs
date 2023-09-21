// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift.Model;

namespace AmazonGameLiftPlugin.Core
{
    public interface IAmazonGameLiftWrapper
    {
        Task<CreateGameSessionResponse> CreateGameSessionAsync(
                CreateGameSessionRequest request,
                CancellationToken cancellationToken = default
            );

        
        /// <summary>
        /// Editor region is code dedicated to Amazon GameLift SDK calls made by the Unity Editor Plugin. 
        /// </summary>
        #region Editor
        Task<CreatePlayerSessionResponse> CreatePlayerSession(CreatePlayerSessionRequest request);

        Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request);

        Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request);
        #endregion
    }
}
