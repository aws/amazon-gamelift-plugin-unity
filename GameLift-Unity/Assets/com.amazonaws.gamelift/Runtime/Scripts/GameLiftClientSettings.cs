// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace AmazonGameLift.Runtime
{
    [CreateAssetMenu(fileName = "GameLiftClientSettings", menuName = "GameLift/Client Settings")]
    public sealed class GameLiftClientSettings : ScriptableObject
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayUrl;
        public string LocalUrl = "http://localhost";
        public ushort LocalPort = 8080;
        public bool IsLocalTest;

        public GameLiftConfiguration GetConfiguration()
        {
            string endpoint = IsLocalTest
                ? $"{LocalUrl}:{LocalPort}"
                : ApiGatewayUrl;
            string awsRegion = IsLocalTest ? "eu-west-1" : AwsRegion;
            return new GameLiftConfiguration
            {
                ApiGatewayEndpoint = endpoint,
                AwsRegion = awsRegion,
                UserPoolClientId = UserPoolClientId,
            };
        }
    }
}
