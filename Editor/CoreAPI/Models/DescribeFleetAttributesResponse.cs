using System.Collections.Generic;
using Amazon.GameLift.Model;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class DescribeFleetAttributesResponse : Response
    {
        public List<FleetAttributes> FleetAttributes { get; set; }
    }
}