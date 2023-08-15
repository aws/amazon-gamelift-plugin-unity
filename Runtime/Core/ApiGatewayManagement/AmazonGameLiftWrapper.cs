// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using UnityEngine;


namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class AmazonGameLiftWrapper : IAmazonGameLiftClientWrapper
    {
        private readonly IAmazonGameLift _amazonGameLiftClient;
        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());
        IAmazonS3Wrapper awsWrapper;

        private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
        
        public AmazonGameLiftWrapper(IAmazonGameLift amazonGameLiftClient)
        {
            _amazonGameLiftClient = amazonGameLiftClient;
            //awsWrapper.
        }

        /// <summary>
        /// Client region is code dedicated to Amazon GameLift SDK calls made by the game client. 
        /// </summary>
        #region Client
        
        public async Task<CreateGameSessionResponse> CreateGameSessionAsync(
                CreateGameSessionRequest request,
                CancellationToken cancellationToken = default
            )
        {
            return await _amazonGameLiftClient.CreateGameSessionAsync(request, cancellationToken);
        }

        public async Task<CreatePlayerSessionResponse> CreatePlayerSession(CreatePlayerSessionRequest request)
        {
            return await _amazonGameLiftClient.CreatePlayerSessionAsync(request);
        }

        public async Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request)
        {
            return await _amazonGameLiftClient.SearchGameSessionsAsync(request);
        }

        public async Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request)
        {
            return await _amazonGameLiftClient.DescribeGameSessionsAsync(request);
        }
        #endregion
        
        /// <summary>
        /// Server region is code dedicated to Amazon GameLift SDK and AWS SDK calls made by the game server. All of these calls will be done via UI Elements or on Startup. 
        /// </summary>
        #region Server

        public async Task<ListLocationsResponse> ListLocations(ListLocationsRequest request)
        {
            return await _amazonGameLiftClient.ListLocationsAsync(request);
        }

        public async Task<CreateLocationResponse> CreateLocation(CreateLocationRequest request)
        {
            return await _amazonGameLiftClient.CreateLocationAsync(request);
        }

        public async Task<RegisterComputeResponse> RegisterCompute(RegisterComputeRequest request)
        {
            return await _amazonGameLiftClient.RegisterComputeAsync(request);
        }

        private async Task<GetComputeAuthTokenResponse> GetComputeAuthToken(GetComputeAuthTokenRequest request)
        {
            return await _amazonGameLiftClient.GetComputeAuthTokenAsync(request);
        }

        public Task<CreateFleetResponse> CreateFleet(CreateFleetRequest request)
        {
            return _amazonGameLiftClient.CreateFleetAsync(request);
        }
        
        public Task<ListFleetsResponse> ListFleets(ListFleetsRequest request)
        {
            return _amazonGameLiftClient.ListFleetsAsync(request);
        }
        
        public Task<DescribeFleetAttributesResponse> DescribeFleets(DescribeFleetAttributesRequest request)
        {
            return _amazonGameLiftClient.DescribeFleetAttributesAsync(request);
        }

        public async Task<string> GenerateAuthToken(string computeName, string fleetId)
        {
            try
            {
                var computeAuthTokenRequest = new GetComputeAuthTokenRequest
                {
                    ComputeName = computeName,
                    FleetId = fleetId
                };
                var computeAuthTokenResponse = await GetComputeAuthToken(computeAuthTokenRequest);
                return computeAuthTokenResponse.AuthToken;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        #endregion
    }
}