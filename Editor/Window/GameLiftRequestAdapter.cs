﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.Shared;
using Editor.Window.Models;
using UnityEngine;
using UnityEngine.UIElements;
using ErrorCode = AmazonGameLift.Editor.ErrorCode;

namespace Editor.Window
{
    public class GameLiftRequestAdapter
    {
        private readonly CoreApi _coreApi;
        private readonly IAmazonGameLiftClientWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _fleetId;
        public const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftRequestAdapter(CoreApi coreApi, IAmazonGameLiftClientWrapper wrapper)
        {
            _coreApi = coreApi;
            _amazonGameLiftWrapper = wrapper;
        }

        internal async Task<Response> CreateAnywhereFleet(string fleetName)
        {
            if (_amazonGameLiftWrapper != null)
            {
                var success = await CreateCustomLocationIfNotExists(FleetLocation);
                if (success)
                {
                    var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation,fleetName);
                    var fleetNameResponse = _coreApi.PutSetting(SettingsKeys.FleetName, fleetName);
                    var fleetIdResponse = _coreApi.PutSetting(SettingsKeys.FleetId, fleetId);
                    if (!fleetNameResponse.Success)
                    {
                        return Response.Fail(new GenericResponse(ErrorCode.InvalidFleetName));
                    }
                    if (!fleetIdResponse.Success)
                    {
                        return Response.Fail(new GenericResponse(ErrorCode.InvalidFleetId));
                        
                    }
                    return Response.Ok(new GenericResponse());
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
                            Location = fleetLocation
                        }
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

        internal async Task<List<FleetAttributes>> ListFleets()
        {
            try
            {
                var listFleetRequest = new ListFleetsRequest();
                var listFleetResponse = await _amazonGameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _amazonGameLiftWrapper.DescribeFleets(describeFleetRequest);
                return describeFleetResponse.FleetAttributes;
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

        internal async Task<bool> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            if (webSocketUrl != null)
            {
                var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                var webSocketResponse = _coreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);

                return computeNameResponse.Success && ipAddressResponse.Success && webSocketResponse.Success;
            }

            return false;
        }
        

        private async Task<string> RegisterCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            try
            {
                var registerComputeRequest = new RegisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                    IpAddress = ipAddress,
                    Location = fleetLocation
                };
                var registerComputeResponse =
                    await _amazonGameLiftWrapper.RegisterCompute(registerComputeRequest);

                return registerComputeResponse.Compute.GameLiftServiceSdkEndpoint;
            }
            catch (Exception ex)
            {
                var errorBox = _container.Q<VisualElement>("ComputeErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }

        private async Task<bool> DeregisterCompute(string computeName, string fleetId)
        {
            try
            {
                var deregisterComputeRequest = new DeregisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId
                };
                var deregisterComputeResponse =
                    await _amazonGameLiftWrapper.DeregisterCompute(deregisterComputeRequest);

                return deregisterComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }

        internal async Task<bool> DescribeCompute(string computeName, string fleetId)
        {
            try
            {
                var describeComputeRequest = new DescribeComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                };
                var describeComputeResponse =
                    await _amazonGameLiftWrapper.DescribeCompute(describeComputeRequest);

                return describeComputeResponse.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }
    }
}