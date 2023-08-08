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
using UnityEngine;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class AmazonGameLiftClientWrapper : IAmazonGameLiftClientWrapper
    {
        private readonly IAmazonGameLift _amazonGameLiftClient;
        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());

        private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
        
        internal AmazonGameLiftClientWrapper(IAmazonGameLift amazonGameLiftClient)
        {
            _amazonGameLiftClient = amazonGameLiftClient;
        }

        public AmazonGameLiftClientWrapper(RetriveAwsCredentialsResponse credentialsResponse)
        {
            _amazonGameLiftClient = CreateGameLiftClient(credentialsResponse);
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

        private IAmazonGameLift CreateGameLiftClient(RetriveAwsCredentialsResponse credentialsResponse)
        {
            return new AmazonGameLiftClient(credentialsResponse.AccessKey, credentialsResponse.SecretKey);
        }
        #endregion
        
        /// <summary>
        /// Server region is code dedicated to Amazon GameLift SDK and AWS SDK calls made by the game server. All of these calls will be done via UI Elements or on Startup. 
        /// </summary>
        #region Server

        private async Task<ListLocationsResponse> ListLocations(ListLocationsRequest request)
        {
            return await _amazonGameLiftClient.ListLocationsAsync(request);
        }

        private async Task<CreateLocationResponse> CreateLocation(CreateLocationRequest request)
        {
            return await _amazonGameLiftClient.CreateLocationAsync(request);
        }

        private async Task<RegisterComputeResponse> RegisterCompute(RegisterComputeRequest request)
        {
            return await _amazonGameLiftClient.RegisterComputeAsync(request);
        }

        private async Task<GetComputeAuthTokenResponse> GetComputeAuthToken(GetComputeAuthTokenRequest request)
        {
            return await _amazonGameLiftClient.GetComputeAuthTokenAsync(request);
        }

        private Task<CreateFleetResponse> CreateFleet(CreateFleetRequest request)
        {
            return _amazonGameLiftClient.CreateFleetAsync(request);
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

        public async Task<string> RegisterCompute(string computeName, string fleetId, string fleetLocation, string ipAddress)
        {
            try
            {
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                    IpAddress = ipAddress,
                    Location = fleetLocation
                };
                var registerComputeResponse = await RegisterCompute(registerComputeRequest);
                
                return registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string>{ "CUSTOM" }
                });
                
                var foundLocation = listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await CreateLocation(new CreateLocationRequest()
                    {
                        LocationName = fleetLocation
                    });
                    
                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Created Custom Location {fleetLocation}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public async Task<string> CreateFleet(ComputeType computeType, string fleetLocation)
        {
            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    ComputeType = computeType,
                    Description = FleetDescription,
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = fleetLocation
                        }
                    }
                };
                var createFleetResponse = await CreateFleet(createFleetRequest);
                return createFleetResponse.FleetAttributes.FleetId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion
    }
}