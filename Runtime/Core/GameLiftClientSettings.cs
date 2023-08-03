// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Aws.GameLift.Server;
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
        public string ComputeName { get; }
        public string FleetID { get;  set; }
        public string FleetLocation { get; }
        public string AuthToken { get; set; }
        public string ProfileName { get; }
        public bool IsAnywhereTest { get; }

        public GameLiftServerProcessConfiguration GetConfiguration()
        {
            return new GameLiftServerProcessConfiguration
            {
                ApiGatewayEndpoint = ApiGatewayUrl,
                AwsRegion = AwsRegion,
                UserPoolClientId = UserPoolClientId,
                IsGameLiftAnywhere = IsAnywhereTest,
            };
        }

        public ServerParameters GetStartupParameters()
        {
            var processId = $"process-{Guid.NewGuid()}";
            return new ServerParameters
            {
                FleetId = FleetID,
                HostId = ComputeName,
                ProcessId = processId,
                WebSocketUrl = WebSocketUrl,
                AuthToken = AuthToken
            };
        }

        public GameLiftComputeConfiguration GetGameLiftAnywhereConfiguration()
        {
            return new GameLiftComputeConfiguration()
            {
                AwsRegion = AwsRegion,
                AuthToken = AuthToken,
                ComputeName = ComputeName,
                FleetID = FleetID,
                FleetLocation = FleetLocation,
                ProfileName = ProfileName
            };
        }
    }
}
