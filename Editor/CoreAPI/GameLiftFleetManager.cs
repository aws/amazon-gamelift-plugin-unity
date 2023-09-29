using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.Window.Models;
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
        private StateManager _stateManager;

        public GameLiftFleetManager(IAmazonGameLiftWrapper wrapper)
        {
            _amazonGameLiftWrapper = wrapper;
        }

        public async Task<Response> CreateAnywhereFleet(string fleetName)
        {
            if (_amazonGameLiftWrapper != null)
            {
                var success = await CreateCustomLocationIfNotExists(FleetLocation);
                if (success)
                {
                    var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation, fleetName);

                    _stateManager.SelectedProfile.FleetName = fleetName;
                    _stateManager.SelectedProfile.FleetId = fleetId;
                
                    var profileSaveResponse = _stateManager.SaveProfiles();
                    
                    return Response.Ok(profileSaveResponse);
                }

                return Response.Fail(new GenericResponse(ErrorCode.CustomLocationCreationFailed));
            }

            return Response.Fail(new GenericResponse(ErrorCode.AccountProfileMissing));
        }

        private async Task<bool> CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await _amazonGameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" }
                });

                var foundLocation =
                    listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

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
                // var errorBox = _container.Q<VisualElement>("FleetErrorInfoBox");
                // errorBox.style.display = DisplayStyle.Flex;
                // errorBox.Q<Label>().text = ex.Message;
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
                var errorBox = _container.Q<VisualElement>("FleetErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }

        public async Task<List<FleetAttributes>> ListFleetAttributes()
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
                return describeFleetResponse.FleetAttributes;
            }
            catch (Exception ex)
            {
                // var errorBox = _container.Q<VisualElement>("FleetErrorInfoBox");
                // errorBox.style.display = DisplayStyle.Flex;
                // errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }
    }
}