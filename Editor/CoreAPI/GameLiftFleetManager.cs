// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;
using ErrorCode = AmazonGameLift.Editor.ErrorCode;

namespace Editor.CoreAPI
{
    public class GameLiftFleetManager
    {
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Deployed by the Amazon GameLift Plug-in for Unity.";
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftFleetManager(IAmazonGameLiftWrapper amazonGameLiftWrapper)
        {
            _amazonGameLiftWrapper = amazonGameLiftWrapper;
        }

        public async Task<CreateAnywhereFleetResponse> CreateFleet(string fleetName)
        {
            if (_amazonGameLiftWrapper == null)
            {
                return Response.Fail(new CreateAnywhereFleetResponse { ErrorCode = ErrorCode.AccountProfileMissing });
            }

            if (string.IsNullOrWhiteSpace(fleetName))
            {
                return Response.Fail(new CreateAnywhereFleetResponse { ErrorCode = ErrorCode.InvalidFleetName });
            }

            try
            {
                var listLocationsResponse = await _amazonGameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" },
                });

                var foundLocation = listLocationsResponse.Locations
                    .FirstOrDefault(l => l.LocationName.ToString() == FleetLocation);

                if (foundLocation == null)
                {
                    await _amazonGameLiftWrapper.CreateLocation(new CreateLocationRequest()
                        {
                            LocationName = FleetLocation
                        });
                }
            }
            catch (Exception ex)
            {
                return Response.Fail(new CreateAnywhereFleetResponse()
                    { 
                        ErrorCode = ErrorCode.CustomLocationCreationFailed, 
                        ErrorMessage = ex.Message 
                    });
            }

            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    Name = fleetName,
                    ComputeType = ComputeType.ANYWHERE,
                    Description = FleetDescription,
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = FleetLocation,
                        }
                    },
                    Tags = new List<Tag>
                    {
                        { new() { Key = "CreatedBy", Value = "AmazonGameLiftUnityPlugin" } }
                    }
                };
                var createFleetResponse = await _amazonGameLiftWrapper.CreateFleet(createFleetRequest);

                return Response.Ok(new CreateAnywhereFleetResponse()
                {
                    FleetId = createFleetResponse.FleetAttributes.FleetId,
                    FleetName = fleetName
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new CreateAnywhereFleetResponse()
                {
                    ErrorCode = ErrorCode.CreateFleetFailed,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<List<FleetAttributes>> DescribeFleetAttributes(ComputeType computeType)
        {
            try
            {
                var listFleetRequest = new ListFleetsRequest();
                var listFleetResponse = await _amazonGameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _amazonGameLiftWrapper.DescribeFleetAttributes(describeFleetRequest);
                return describeFleetResponse.FleetAttributes.Where(fleet => fleet.ComputeType == computeType)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }
    }
}