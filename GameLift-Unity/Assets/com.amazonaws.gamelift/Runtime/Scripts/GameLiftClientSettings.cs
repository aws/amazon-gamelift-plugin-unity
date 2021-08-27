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
        public string ApiGatewayEndpoint;

        public GameLiftConfiguration GetConfiguration() =>
            new GameLiftConfiguration
            {
                ApiGatewayEndpoint = ApiGatewayEndpoint,
                AwsRegion = AwsRegion,
                UserPoolClientId = UserPoolClientId,
            };
    }
}
