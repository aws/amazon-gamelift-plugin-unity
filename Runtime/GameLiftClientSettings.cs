// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Aws.GameLift.Server;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AmazonGameLift.Runtime
{
    [CreateAssetMenu(fileName = "GameLiftClientSettings", menuName = "GameLift/Client Settings")]
    public sealed class GameLiftClientSettings : ScriptableObject
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayUrl;
        
        public string ComputeName;
        public string FleetID;
        public string FleetLocation;
        public string AuthToken;
        public string ProfileName;
        public bool IsAnywhereTest;

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

        public ServerParameters GetStartupParameters()
        {
            var webSocketUrl = $"wss://{AwsRegion}.api.amazongamelift.com";
            var processId = $"fleet-{Math.Floor(Random.value * 100000)}";
            Debug.Log($"GAMELIFT PROCESS ID = {processId}");
            return new ServerParameters
            {
                FleetId = FleetID,
                HostId = ComputeName,
                ProcessId = processId,
                WebSocketUrl = webSocketUrl,
                AuthToken = AuthToken
            };
        }
    }
}
