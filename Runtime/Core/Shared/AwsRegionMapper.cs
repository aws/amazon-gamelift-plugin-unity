// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon;

namespace AmazonGameLiftPlugin.Core.Shared
{
    public static class AwsRegionMapper
    {
        private static readonly Dictionary<string, RegionEndpoint> s_regions = new Dictionary<string, RegionEndpoint>
        {
            {
                "us-east-2",
                RegionEndpoint.USEast2
            },
            {
                "us-east-1",
                RegionEndpoint.USEast1
            },
            {
                "us-west-1",
                RegionEndpoint.USWest1
            },
            {
                "us-west-2",
                RegionEndpoint.USWest2
            },
            {
                "ap-south-1",
                RegionEndpoint.APSouth1
            },
            {
                "ap-northeast-2",
                RegionEndpoint.APNortheast2
            },
            {
                "ap-southeast-1",
                RegionEndpoint.APSoutheast1
            },
            {
                "ap-southeast-2",
                RegionEndpoint.APSoutheast2
            },
            {
                "ap-northeast-1",
                RegionEndpoint.APNortheast1
            },
            {
                "ca-central-1",
                RegionEndpoint.CACentral1
            },
            {
                "cn-north-1",
                RegionEndpoint.CNNorth1
            },
            {
                "eu-central-1",
                RegionEndpoint.EUCentral1
            },
            {
                "eu-west-1",
                RegionEndpoint.EUWest1
            },
            {
                "eu-west-2",
                RegionEndpoint.EUWest2
            },
            {
                "sa-east-1",
                RegionEndpoint.SAEast1
            },
            {
                "cn-northwest-1",
                RegionEndpoint.CNNorthWest1
            }
        };

        internal static RegionEndpoint GetRegionEndpoint(string region)
        {
            if (IsValidRegion(region))
            {
                return s_regions[region];
            }

            throw new Exception(ErrorCode.InvalidRegion);
        }

        public static bool IsValidRegion(string region)
        {
            return !string.IsNullOrEmpty(region) && s_regions.ContainsKey(region);
        }

        public static IEnumerable<string> AvailableRegions()
        {
            return s_regions.Keys;
        }
    }
}
