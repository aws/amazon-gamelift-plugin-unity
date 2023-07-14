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
        private string _profileName;
        
        #region Client
        
        internal AmazonGameLiftClientWrapper(IAmazonGameLift amazonGameLiftClient)
        {
            _amazonGameLiftClient = amazonGameLiftClient;
        }

        public AmazonGameLiftClientWrapper(string profileName)
        {
            _profileName = profileName;
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
            SetupCredentials();
            return new AmazonGameLiftClient(AccessKey, SecretAccessKey);
        }
        #endregion
        
        #region Server
        private string AccessKey { get; set; }
        private string SecretAccessKey { get; set; }
        private string ComputeName { get; set; }
        private string FleetId { get; set; }
        private string IPAddress { get; set; }
        private string LocationName { get; set; }
        
        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());

        private readonly GameLiftClientSettings _gameLiftClientSettings;
        
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
                SetupConfiguration();
                await Task.WhenAll(CreateFleet(),RegisterCompute(), GenerateAuthToken());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task CreateFleet()
        {
            //### Move this entire thing to being UX based.
            await CreateCustomLocation();
            //await CreateFleetCommand(); 
        }

        private void SetupCredentials()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _profileName = _gameLiftClientSettings.ProfileName;
            var credentials = RetrieveAwsCredentials(_profileName);
            AccessKey = credentials.AccessKey;
            SecretAccessKey = credentials.SecretKey;
            
        }

        private void SetupConfiguration()
        {
            ComputeName = _gameLiftClientSettings.ComputeName;
            FleetId = _gameLiftClientSettings.FleetID;
            IPAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                .ToString() ?? "0.0.0.0";
            LocationName = _gameLiftClientSettings.FleetLocation;
        }

        private async Task GenerateAuthToken()
        {
            try
            {
                var computeAuthTokenRequest = new GetComputeAuthTokenRequest
                {
                    ComputeName = ComputeName,
                    FleetId = FleetId
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
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = ComputeName,
                    FleetId = FleetId,
                    IpAddress = IPAddress,
                    Location = LocationName
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
                
                var foundLocation = listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == LocationName);
                
                if(foundLocation == null)
                {
                    var createLocationResponse = await CreateLocation(new CreateLocationRequest()
                    {
                        LocationName = LocationName
                    });
                    
                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Created Custom Location {LocationName}");
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
                            Location = LocationName
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
