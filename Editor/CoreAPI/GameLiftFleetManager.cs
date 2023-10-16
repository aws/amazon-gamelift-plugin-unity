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
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class GameLiftFleetManager
    {
        public readonly string FleetLocation = "custom-location-1";
        
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
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

            var createLocationSuccess = await CreateCustomLocationIfNotExists(FleetLocation);
            if (!createLocationSuccess)
            {
                return Response.Fail(new CreateAnywhereFleetResponse { ErrorCode = ErrorCode.CustomLocationCreationFailed });
            }

            var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation, fleetName);
            if (string.IsNullOrWhiteSpace(fleetId))
            {
                return Response.Fail(new CreateAnywhereFleetResponse { ErrorCode = ErrorCode.CreateFleetFailed });
            }

            return Response.Ok(new CreateAnywhereFleetResponse()
            {
                FleetId = fleetId,
                FleetName = fleetName
            });
        }

        private async Task<bool> CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await _amazonGameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" },
                });

                var foundLocation =
                    listLocationsResponse.Locations.FirstOrDefault(l =>
                        l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await _amazonGameLiftWrapper.CreateLocation(
                        new CreateLocationRequest()
                        {
                            LocationName = fleetLocation
                        });

                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Debug.Log($"Created Custom Location {fleetLocation}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<string> CreateFleet(ComputeType computeType, string fleetLocation, string fleetName)
        {
            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    Name = fleetName,
                    ComputeType = computeType,
                    Description = FleetDescription,
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = fleetLocation,
                        }
                    },
                    Tags = new List<Tag>
                    {
                        { new() { Key = "CreatedBy", Value = "AmazonGameLiftUnityPlugin" } }
                    }
                };
                var createFleetResponse = await _amazonGameLiftWrapper.CreateFleet(createFleetRequest);

                if (createFleetResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    Debug.Log($"Created Fleet {fleetName}");
                }

                return createFleetResponse.FleetAttributes.FleetId;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
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
        
        public async Task<DescribeFleetLocationResponse> DescribeFleetLocationAttributes(string fleetId)
        {
            try
            {
                var describeFleetLocationAttributesRequest = new DescribeFleetLocationAttributesRequest()
                {
                    FleetId = fleetId
                };

                var describeFleetUtilizationResponse =
                    await _amazonGameLiftWrapper.DescribeFleetLocationAttributes(describeFleetLocationAttributesRequest);

                return Response.Ok(new DescribeFleetLocationResponse
                {
                    Location = describeFleetUtilizationResponse.LocationAttributes[0].LocationState.Location
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new DescribeFleetLocationResponse()
                { 
                    ErrorCode = ErrorCode.CustomLocationCreationFailed, 
                    ErrorMessage = ex.Message 
                });
            }
        }
    }
}
