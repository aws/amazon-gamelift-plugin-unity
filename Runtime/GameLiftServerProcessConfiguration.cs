// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core
{
    [System.Serializable]
    public struct GameLiftServerProcessConfiguration
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayEndpoint;
        public bool IsGameLiftAnywhere;
    }
}
