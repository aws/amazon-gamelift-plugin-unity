using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Window
{
    public class GameLiftRequestAdapter
    {
        private readonly GameLiftPlugin _gameLiftPlugin;
        private string _fleetName;
        private string _fleetId;
        public const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Deployed by the Amazon GameLift Plug-in for Unity.";
        private VisualElement _container;

        public GameLiftRequestAdapter(GameLiftPlugin gameLiftPlugin)
        {
            _gameLiftPlugin = gameLiftPlugin;
        }

        internal async Task<bool> CreateAnywhereFleet(string fleetName)
        {
            if (_gameLiftPlugin.GameLiftWrapper != null)
            {
                var success = await CreateCustomLocationIfNotExists(FleetLocation);
                if (success)
                {
                    var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation,fleetName);
                    var fleetNameResponse = _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.FleetName, fleetName);
                    var fleetIdResponse = _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.FleetId, fleetId);
                    var customLocationNameResponse =
                        _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.CustomLocationName, FleetLocation);
                    return fleetNameResponse.Success && customLocationNameResponse.Success && fleetIdResponse.Success;
                }

                return false;
            }

            Debug.Log("Error: Missing Account Profile");
            return false;
        }
        
        private async Task<bool> CreateCustomLocationIfNotExists(string fleetLocation)
        {
            try
            {
                var listLocationsResponse = await _gameLiftPlugin.GameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" }
                });

                var foundLocation =
                    listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await _gameLiftPlugin.GameLiftWrapper.CreateLocation(
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
                var errorBox = _container.Q<VisualElement>("FleetErrorInfoBox");
                errorBox.style.display = DisplayStyle.Flex;
                errorBox.Q<Label>().text = ex.Message;
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
                    },
                    Tags = new List<Tag>
                    {
                        new()
                        {
                            Key = "CreatedBy",
                            Value = "AmazonGameLiftUnityPlugin"
                        }
                    }
                    };
                var createFleetResponse = await _gameLiftPlugin.GameLiftWrapper.CreateFleet(createFleetRequest);
                
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
                var listFleetResponse = await _gameLiftPlugin.GameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _gameLiftPlugin.GameLiftWrapper.DescribeFleetAttributes(describeFleetRequest);
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
                var computeNameResponse = _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                var ipAddressResponse = _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                var webSocketResponse = _gameLiftPlugin.CoreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);

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
                    await _gameLiftPlugin.GameLiftWrapper.RegisterCompute(registerComputeRequest);

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
                    await _gameLiftPlugin.GameLiftWrapper.DeregisterCompute(deregisterComputeRequest);

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
                    await _gameLiftPlugin.GameLiftWrapper.DescribeCompute(describeComputeRequest);

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