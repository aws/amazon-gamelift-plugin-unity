// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace AmazonGameLiftPlugin.Core
{
    [CreateAssetMenu(fileName = "GameLiftClientSettings", menuName = "GameLift/Client Settings")]
    public class GameLiftClientSettings : ScriptableObject
    {
        public string WebSocketUrl { get; set; }
        public string UserPoolClientId { get; }
        public string ApiGatewayUrl { get; }
        public string AwsRegion { get; }
        public string computeName { get; }
        public string fleetID { get;  set; }
        public string fleetLocation { get; }
        public string authToken { get; set; }
        public string profileName { get; }
        public bool IsAnywhereTest { get; }

        public GameLiftConfiguration GetConfiguration()
        {
            return new GameLiftConfiguration
            {
                ApiGatewayEndpoint = ApiGatewayUrl,
                AwsRegion = AwsRegion,
                UserPoolClientId = UserPoolClientId,
                IsGameLiftAnywhere = IsAnywhereTest,
            };
        }

        // public ServerParameters GetStartupParameters()
        // {
        //     var processId = $"process-{Guid.NewGuid()}";
        //     return new ServerParameters
        //     {
        //         FleetId = fleetID,
        //         HostId = computeName,
        //         ProcessId = processId,
        //         WebSocketUrl = WebSocketUrl,
        //         AuthToken = authToken
        //     };
        // }

        public GameLiftAnywhereConfiguration GetGameLiftAnywhereConfiguration()
        {
            return new GameLiftAnywhereConfiguration()
            {
                AwsRegion = AwsRegion,
                AuthToken = authToken,
                ComputeName = computeName,
                FleetID = fleetID,
                FleetLocation = fleetLocation,
                ProfileName = profileName
            };
        }
    }
}
