// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using Aws.GameLift.Server;
using UnityEngine;

namespace AmazonGameLiftPlugin.Core
{
    [CreateAssetMenu(fileName = "GameLiftClientSettings", menuName = "GameLift/Client Settings")]
    public class GameLiftClientSettings : ScriptableObject
    {
        public string WebSocketUrl;
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
                //AwsRegion = AwsRegion,
                UserPoolClientId = UserPoolClientId,
                IsGameLiftAnywhere = IsAnywhereTest,
            };
        }

        public ServerParameters GetStartupParameters()
        {
            var webSocketUrl = WebSocketUrl;
            var processId = $"process-{Guid.NewGuid()}";
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
