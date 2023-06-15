// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Runtime
{
    [System.Serializable]
    public struct GameLiftConfiguration
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayEndpoint;
        public bool IsGameLiftAnywhere;
    }

    public struct GameLiftAnywhereConfiguration
    {
        public string ComputeName;
        public string FleetId;
        public string IpAddress;
        public string FleetLocation;
    }
}
