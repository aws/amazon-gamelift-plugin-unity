using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.Runtime.Internal;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;
using ErrorCode = AmazonGameLift.Editor.ErrorCode;
using Model = Amazon.GameLift.Model;

namespace Editor.CoreAPI
{
    public class GameLiftFleetManager
    {
        private readonly CoreApi _coreApi;
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Deployed by the Amazon GameLift Plug-in for Unity.";
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftFleetManager(CoreApi coreApi, IAmazonGameLiftWrapper wrapper)
        {
            _coreApi = coreApi;
            _amazonGameLiftWrapper = wrapper;
        }
        
        public async Task<Response> CreateAnywhereFleet(string fleetName)
        {
            if (_amazonGameLiftWrapper != null)
            {
                var createCustomLocationIfNotExists = await CreateCustomLocationIfNotExists(FleetLocation);
                if (createCustomLocationIfNotExists.Success)
                {
                    var fleetCreateResponse = await CreateFleet(ComputeType.ANYWHERE, FleetLocation, fleetName);
                    if (fleetCreateResponse.Success)
                    {
                        var fleetNameResponse = _coreApi.PutSetting(SettingsKeys.FleetName, fleetName);
                        var fleetIdPutResponse = _coreApi.PutSetting(SettingsKeys.FleetId, fleetCreateResponse.FleetId);
                        if (!fleetNameResponse.Success)
                        {
                            return Response.Fail(ErrorCode.InvalidFleetName);
                        }

                        if (!fleetIdPutResponse.Success)
                        {
                            return Response.Fail(ErrorCode.InvalidFleetId);
                        }
                    }
                    else
                    {
                        return fleetCreateResponse;
                    }
                }
                
                return createCustomLocationIfNotExists;
            }

            return Response.Fail(ErrorCode.AccountProfileMissing);
        }

        private async Task<Response> CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await _amazonGameLiftWrapper.ListLocations(new Model.ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" }
                });

                var foundLocation =
                    listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await _amazonGameLiftWrapper.CreateLocation(
                        new Model.CreateLocationRequest()
                        {
                            LocationName = fleetLocation
                        });

                    if (createLocationResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Debug.Log($"Created Custom Location {fleetLocation}");
                    }
                }

                return Response.Ok(new Response());
            }
            catch (Exception ex)
            {
                return Response.Fail(ErrorCode.CustomLocationCreationFailed);
            }
        }

        private async Task<CreateFleetResponse> CreateFleet(ComputeType computeType, string fleetLocation, string fleetName)
        {
            try
            {
                var createFleetRequest = new Model.CreateFleetRequest
                {
                    Name = fleetName,
                    ComputeType = computeType,
                    Description = FleetDescription,
                    Locations = new List<Model.LocationConfiguration>
                    {
                        new()
                        {
                            Location = fleetLocation,
                        }
                    },
                    Tags = new List<Model.Tag>
                    {
                        { new() { Key = "CreatedBy", Value = "AmazonGameLiftUnityPlugin" } }
                    }
                };
                var createFleetResponse = await _amazonGameLiftWrapper.CreateFleet(createFleetRequest);

                if (createFleetResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    Debug.Log($"Created Fleet {fleetName}");
                }

                return Response.Ok(new CreateFleetResponse
                {
                    FleetId = createFleetResponse.FleetAttributes.FleetId
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new CreateFleetResponse
                {
                    ErrorCode = ErrorCode.CustomFleetFailed
                });
            }
        }

        public async Task<DescribeFleetAttributesResponse> ListFleetAttributes()
        {
            try
            {
                var listFleetRequest = new Model.ListFleetsRequest();
                var listFleetResponse = await _amazonGameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new Model.DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _amazonGameLiftWrapper.DescribeFleetAttributes(describeFleetRequest);
                return Response.Ok(new DescribeFleetAttributesResponse
                {
                    FleetAttributes = describeFleetResponse.FleetAttributes
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new DescribeFleetAttributesResponse()
                {
                    ErrorCode = ErrorCode.ListFleetAttributesFailed
                });
            }
        }
    }
}