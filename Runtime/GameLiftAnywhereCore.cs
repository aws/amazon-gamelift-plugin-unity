// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AmazonGameLift.Runtime
{
    public sealed class GameLiftAnywhereCore
    {
        private string _accessKey { get; set; }
        private string _secretAccessKey { get; set; }
        private string _region { get; set; }
        private string _computeName { get; set; }
        private string _fleetId { get; set; }
        private string _ipAddress { get; set; }
        private string _locationName { get; set; }
        
        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());

        private readonly GameLiftClientSettings _gameLiftClientSettings;

        public GameLiftAnywhereCore()
        {
            _gameLiftClientSettings = Resources.FindObjectsOfTypeAll<GameLiftClientSettings>()[0];
        }
        
        public async Task UpdateAuth()
        {
            try
            {
                SetupCredentials();
                RunPowerShellCredentialsSetup();
                await Task.WhenAll(CreateFleet(), GenerateAuthToken());
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
            RunPowerShellCustomLocationCreator();
            //RunPowerShellFleetCreator(); //### Add this automation to a unity button when we have UX
            return Task.CompletedTask;
        }
        
        private Task GenerateAuthToken()
        {
            var authToken = RunPowerShellAuthTokenGenerator(); 
            
            Debug.Log("AuthToken Generated: "+ authToken);
            _gameLiftClientSettings.AuthToken = authToken;
            return Task.CompletedTask;
        }
        
        private void SetupCredentials()
        {
            var credentials = RetrieveAwsCredentials(_gameLiftClientSettings.ProfileName);
            
            _accessKey = credentials.AccessKey;
            _secretAccessKey = credentials.SecretKey;
            _region = _gameLiftClientSettings.AwsRegion;
            _computeName = _gameLiftClientSettings.ComputeName;
            _fleetId = _gameLiftClientSettings.FleetID;
            _ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                .ToString() ?? "0.0.0.0";
            _locationName = _gameLiftClientSettings.FleetLocation;
        }

        private void RunPowerShellCredentialsSetup()
        {
            RunPowerShellCommand($"aws configure set aws_access_key_id {_accessKey} --profile default");
            RunPowerShellCommand($"aws configure set aws_secret_access_key {_secretAccessKey} --profile default");
            RunPowerShellCommand($"aws configure set region {_region} --profile default");
        }
        
        private string RunPowerShellAuthTokenGenerator()
        {
            //Register Compute Name. Makes sure that the compute is registered, will return the same response if the name already exists
            RunPowerShellCommand($"aws gamelift register-compute --compute-name {_computeName} --fleet-id {_fleetId} --ip-address {_ipAddress} --location custom-{_locationName}");
        
            //Compute the Auth token that we use for the server.
            var token = RunPowerShellCommand($"aws gamelift get-compute-auth-token --fleet-id {_fleetId} --compute-name {_computeName}");
            
            token = ExtractFromJson(token,"AuthToken");
            return token;
        }

        private void RunPowerShellCustomLocationCreator()
        {
            RunPowerShellCommand($"aws gamelift create-location --location-name custom-{_locationName}");
        }
        
        private void RunPowerShellFleetCreator()
        {
            var fleet = RunPowerShellCommand($"aws gamelift create-fleet --name {_computeName} --compute-type ANYWHERE --locations \"Location=custom-{_locationName}\"");
            _fleetId = ExtractFromJson(fleet, "FleetId");
            _gameLiftClientSettings.FleetID = _fleetId;
        }

        private static string RunPowerShellCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
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

        private RetriveAwsCredentialsResponse RetrieveAwsCredentials(string profileName)
        {
            var request = new RetriveAwsCredentialsRequest { ProfileName = profileName };
            return _credentialsStore.RetriveAwsCredentials(request);
        }
    }
}