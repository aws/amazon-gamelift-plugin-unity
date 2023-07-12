// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core
{
    [System.Serializable]
    public struct GameLiftConfiguration
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayEndpoint;
        public bool IsGameLiftAnywhere;
    }

    [System.Serializable]
    public struct GameLiftAnywhereConfiguration
    {
        public string AwsRegion;
        public string ComputeName;
        public string FleetID;
        public string FleetLocation;
        public string AuthToken;
        public string ProfileName;
    }
}
