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
        
        
        Task<DescribeFleetAttributesResponse> DescribeFleets(DescribeFleetAttributesRequest request);

        Task<DeregisterComputeResponse> DeregisterCompute(DeregisterComputeRequest request);

        Task<DescribeComputeResponse> DescribeCompute(DescribeComputeRequest request);

        Task<ListFleetsResponse> ListFleets(ListFleetsRequest request);

        Task<ListLocationsResponse> ListLocations(ListLocationsRequest request);

        Task<CreateLocationResponse> CreateLocation(CreateLocationRequest request);

        Task<CreateFleetResponse> CreateFleet(CreateFleetRequest request);

        Task<RegisterComputeResponse> RegisterCompute(RegisterComputeRequest request);

        Task<GetComputeAuthTokenResponse> GetComputeAuthToken(GetComputeAuthTokenRequest request);
    }
}