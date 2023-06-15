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
    public class GameLiftAnywhereCore
    {
        private string _accessKey { get; set; }
        private string _secretAccessKey { get; set; }
        private string _region { get; set; }
        private string _computeName { get; set; }
        private string _fleetId { get; set; }
        private string _ipAddress { get; set; }
        private string _locationName { get; set; }
        
        public GameLiftClientSettings gameLiftClientSettings;

        private readonly ICredentialsStore _credentialsStore = new CredentialsStore(new FileWrapper());
        public GameLiftAnywhereCore()
        {
            gameLiftClientSettings = Resources.FindObjectsOfTypeAll<GameLiftClientSettings>()[0];
        }
        
        public async Task UpdateAuth()
        {
            try
            {
                SetupCredentials();
                RunPowerShellCredentialsSetup();
                await CreateFleet(); 
                await GenerateAuthToken();
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
            gameLiftClientSettings.AuthToken = authToken;
            return Task.CompletedTask;
        }
        
        private void SetupCredentials()
        {
            var response = _credentialsStore.GetProfiles(new GetProfilesRequest());
            string[] allProfileNames = response.Profiles.ToArray();
            
            if (!response.Success)
            {
                return;
            }

            var credentials = RetrieveAwsCredentials(gameLiftClientSettings.ProfileName);
            
            _accessKey = credentials.AccessKey;
            _secretAccessKey = credentials.SecretKey;
            _region = gameLiftClientSettings.AwsRegion;
            _computeName = gameLiftClientSettings.ComputeName;
            _fleetId = gameLiftClientSettings.FleetID;
            _ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                .ToString() ?? "0.0.0.0";
            _locationName = gameLiftClientSettings.FleetLocation;
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
        
            // _logger.Log($"Generated authentication token: {token}", LogType.Log);
            // _logger.Log($"Register Compute: {compute}", LogType.Log);
            
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
            gameLiftClientSettings.FleetID = _fleetId;
        }

        private string RunPowerShellCommand(string command)
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
        
        private string ExtractFromJson(string jsonString, string fieldToExtract)
        {
            // Parse the JSON string and extract the "AuthToken" field
            Match match = Regex.Match(jsonString, $@"""{fieldToExtract}""\s*:\s*""([^""]+)""");
        
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
        
            return null;
        }

        protected virtual RetriveAwsCredentialsResponse RetrieveAwsCredentials(string profileName)
        {
            var request = new RetriveAwsCredentialsRequest() { ProfileName = profileName };
            return _credentialsStore.RetriveAwsCredentials(request);
        }
    }
}