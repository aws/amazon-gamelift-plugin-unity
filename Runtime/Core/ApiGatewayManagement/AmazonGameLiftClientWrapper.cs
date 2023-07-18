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

        private readonly GameLiftClientSettings _gameLiftClientSettings;

        
        /// <summary>
        /// Client region is code dedicated to Amazon GameLift SDK calls made by the game client. 
        /// </summary>
        #region Client
        
        internal AmazonGameLiftClientWrapper(IAmazonGameLift amazonGameLiftClient)
        {
            _amazonGameLiftClient = amazonGameLiftClient;
        }

        public AmazonGameLiftClientWrapper(string profileName)
        {
            _gameLiftClientSettings = Resources.FindObjectsOfTypeAll<GameLiftClientSettings>()[0];
            _amazonGameLiftClient = Create();
        }
        
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

        private IAmazonGameLift Create()
        {
            var credentials = SetupCredentials();
            return new AmazonGameLiftClient(credentials.AccessKey, credentials.SecretKey);
        }
        #endregion
        
        
        /// <summary>
        /// Server region is code dedicated to Amazon GameLift SDK and AWS SDK calls made by the game server. All of these calls will be done via UI Elements or on Startup. 
        /// </summary>
        #region Server
        
        public AmazonGameLiftClientWrapper()
        {
            _gameLiftClientSettings = Resources.FindObjectsOfTypeAll<GameLiftClientSettings>()[0];
            _amazonGameLiftClient = Create();
        }

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
        
        private RetriveAwsCredentialsResponse RetrieveAwsCredentials(string profileName)
        {
            var request = new RetriveAwsCredentialsRequest { ProfileName = profileName };
            return _credentialsStore.RetriveAwsCredentials(request);
        }
        
        public async Task UpdateAuthToken()
        {
            try
            {
                await Task.WhenAll(CreateFleet(), RegisterCompute(), GenerateAuthToken());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private string IPAddress { get; set; }
        
        private async Task CreateFleet()
        {
            //TODO Move this entire thing to being UX based. ASG2-49
            await CreateCustomLocation();
            //await CreateFleetCommand(); 
        }

        private RetriveAwsCredentialsResponse SetupCredentials()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var credentials = RetrieveAwsCredentials(_gameLiftClientSettings.ProfileName);
            return credentials;
        }

        private async Task GenerateAuthToken()
        {
            try
            {
                var computeAuthTokenRequest = new GetComputeAuthTokenRequest
                {
                    ComputeName = _gameLiftClientSettings.ComputeName,
                    FleetId = _gameLiftClientSettings.FleetID
                };
                var computeAuthTokenResponse = await GetComputeAuthToken(computeAuthTokenRequest);
                _gameLiftClientSettings.AuthToken = computeAuthTokenResponse.AuthToken;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task RegisterCompute()
        {
            //Reregistering compute will just return the same compute back to the user.
            try
            {
                
                var ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                    .ToString() ?? "0.0.0.0";
                
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = _gameLiftClientSettings.ComputeName,
                    FleetId = _gameLiftClientSettings.FleetID,
                    IpAddress = ipAddress,
                    Location = _gameLiftClientSettings.FleetLocation
                };
                var registerComputeResponse = await RegisterCompute(registerComputeRequest);
                    
                _gameLiftClientSettings.WebSocketUrl = registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task CreateCustomLocation()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var listLocationsResponse = await ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string>{ "CUSTOM" }
                });
                
                var foundLocation = listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == _gameLiftClientSettings.FleetLocation);
                
                if(foundLocation == null)
                {
                    var createLocationResponse = await CreateLocation(new CreateLocationRequest()
                    {
                        LocationName = _gameLiftClientSettings.FleetLocation
                    });
                    
                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Created Custom Location {_gameLiftClientSettings.FleetLocation}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private async Task CreateFleetCommand()
        {
            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    ComputeType = ComputeType.ANYWHERE,
                    Description = "Created By Amazon GameLift Unity Plugin",
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = _gameLiftClientSettings.FleetLocation
                        }
                    }
                };
                var createFleetResponse = await CreateFleet(createFleetRequest);
                
                _gameLiftClientSettings.FleetID = createFleetResponse.FleetAttributes.FleetId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        #endregion
    }
}

