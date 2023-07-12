// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class AmazonGameLiftClientWrapper : IAmazonGameLiftClientWrapper
    {
        private IAmazonGameLift _amazonGameLiftClient;
        private readonly string _profileName;

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
        
        public AmazonGameLiftClientWrapper()
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
            SetupCredentials();
            return new AmazonGameLiftClient(_accessKey, _secretAccessKey);
        }

        private string _accessKey { get; set; }
        private string _secretAccessKey { get; set; }
        private string _computeName { get; set; }
        private string _fleetId { get; set; }
        private string _ipAddress { get; set; }
        private string _locationName { get; set; }

        private readonly GameLiftClientSettings _gameLiftClientSettings;

        public async Task<ListLocationsResponse> ListLocations(ListLocationsRequest request)
        {
            return await _amazonGameLiftClient.ListLocationsAsync(request);
        }
        
        public async Task UpdateAuthToken()
        {
            try
            {
                SetupConfiguration();
                await Task.WhenAll(CreateFleet(), RegisterCompute(), GenerateAuthToken());
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(e +
                               " Please Create a GameLift Client Settings Object (Assets>Create>GameLift>Client Settings");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError("Error Occurred: "+ e);
                throw;
            }
        }

        private Task CreateFleet()
        {
            CreateCustomLocation();
            //CreateFleetCommand(); //### Add this automation to a unity button when we have UX
            return Task.CompletedTask;
        }
        
        private Task GenerateAuthToken()
        {
            //var authToken = GenerateAuthTokenCommand(); 
            
            //Debug.Log("AuthToken Generated: "+ authToken);
            //_gameLiftClientSettings.AuthToken = authToken;
            return Task.CompletedTask;
        }
        
        private void SetupCredentials()
        {
            _accessKey = RunCliCommandProcess("configure get aws_access_key_id");
            _secretAccessKey = RunCliCommandProcess("configure get aws_secret_access_key");
        }

        private void SetupConfiguration()
        {
            _computeName = _gameLiftClientSettings.ComputeName;
            _fleetId = _gameLiftClientSettings.FleetID;
            _ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                .ToString() ?? "0.0.0.0";
            _locationName = _gameLiftClientSettings.FleetLocation;
        }

        private static string RunCliCommandProcess(string command)
        {
            const string awsCliPath = "aws";

            var startInfo = new ProcessStartInfo
            {
                FileName = awsCliPath,
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            using var reader = process?.StandardOutput;
            var output = reader?.ReadToEnd();
            
            process?.WaitForExit();
        
            return output;
        }

        private string GenerateAuthTokenCommand()
        {
            //Compute the Auth token that we use for the server.
            var token = RunCliCommand($"gamelift get-compute-auth-token --fleet-id {_fleetId} --compute-name {_computeName}");
            
            token = ExtractFromJson(token,"AuthToken");
            return token;
        }

        private async Task RegisterCompute()
        {
            DescribeComputeRequest describeComputeRequest = new DescribeComputeRequest()
            {
                ComputeName = _computeName,
                FleetId = _fleetId
            };
            
            var describeComputeResponse = await _amazonGameLiftClient.DescribeComputeAsync(describeComputeRequest);
            Debug.Log(describeComputeResponse.Compute);
            
            // var computeRequest = new RegisterComputeRequest()
            // {
            //     ComputeName = _computeName,
            //     FleetId = _fleetId,
            //     IpAddress = _ipAddress,
            //     Location = _locationName
            // };
            // var registerComputeResponse = await _amazonGameLiftClient.RegisterComputeAsync(computeRequest);
            // Debug.Log(registerComputeResponse.ToString());
            //_gameLiftClientSettings.WebSocketUrl = registerComputeResponse
            
            // //Register Compute Name. Makes sure that the compute is registered, will return the same response if the name already exists
            // var computeInfo = RunCliCommand($"gamelift register-compute --compute-name {_computeName} --fleet-id {_fleetId} --ip-address {_ipAddress} --location custom-{_locationName}");
            // computeInfo = ExtractFromJson(computeInfo, "GameLiftServiceSdkEndpoint");
            
            //_gameLiftClientSettings.WebSocketUrl = computeInfo;
        }

        private async void CreateCustomLocation()
        {
            try
            {
                ListLocationsResponse listLocationsResponse = await ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string>() { "CUSTOM" }
                });
                
                Debug.Log(listLocationsResponse.Locations);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);

                // return Response.Fail(new StartGameResponse
                // {
                //     ErrorCode = ErrorCode.UnknownError,
                //     ErrorMessage = ex.Message
                // });
            }
            
            
            //RunCliCommand($"gamelift create-location --location-name custom-{_locationName}");
        }
        
        private void CreateFleetCommand()
        {
            var fleet = RunCliCommand($"gamelift create-fleet --name {_computeName} --compute-type ANYWHERE --locations \"Location=custom-{_locationName}\"");
            _fleetId = ExtractFromJson(fleet, "FleetId");
            _gameLiftClientSettings.FleetID = _fleetId;
        }

        private static string RunCliCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "aws.exe",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        
            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
        
            return output;
        }
        
        private static string ExtractFromJson(string jsonString, string fieldToExtract)
        {
            // Parse the JSON string and extract the "AuthToken" field
            var match = Regex.Match(jsonString, $@"""{fieldToExtract}""\s*:\s*""([^""]+)""");
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
        
            return null;
        }
    }
}
