using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class CreateAnywhereFleetResponse : Response
    {
        public string FleetId { get; set; }
        public string FleetName { get; set; }
    }
}