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
        private GameLiftPlugin _gameLiftConfig;
        private string _fleetName;
        private string _computeName = "ComputerName-ProfileName";
        private string _ipAddress = "120.120.120.120";
        private string _fleetId;
        public const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Created By Amazon GameLift Unity Plugin";
        private VisualElement _container;

        public GameLiftRequestAdapter(GameLiftPlugin gameLiftConfig)
        {
            _gameLiftConfig = gameLiftConfig;
        }

        internal async Task<bool> CreateAnywhereFleet(string fleetName)
        {
            if (_gameLiftConfig.GameLiftWrapper != null)
            {
                var success = await CreateCustomLocationIfNotExists(FleetLocation);
                if (success)
                {
                    var fleetId = await CreateFleet(ComputeType.ANYWHERE, FleetLocation,fleetName);
                    var fleetNameResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.FleetName, fleetName);
                    var fleetIdResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.FleetId, fleetId);
                    var customLocationNameResponse =
                        _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.CustomLocationName, FleetLocation);
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
                var listLocationsResponse = await _gameLiftConfig.GameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" }
                });

                var foundLocation =
                    listLocationsResponse.Locations.FirstOrDefault(l => l.LocationName.ToString() == fleetLocation);

                if (foundLocation == null)
                {
                    var createLocationResponse = await _gameLiftConfig.GameLiftWrapper.CreateLocation(
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
                    }
                };
                var createFleetResponse = await _gameLiftConfig.GameLiftWrapper.CreateFleet(createFleetRequest);
                
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
                var listFleetResponse = await _gameLiftConfig.GameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _gameLiftConfig.GameLiftWrapper.DescribeFleets(describeFleetRequest);
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
                var computeNameResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                var ipAddressResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                var webSocketResponse = _gameLiftConfig.CoreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);

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
                    await _gameLiftConfig.GameLiftWrapper.RegisterCompute(registerComputeRequest);

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
                    await _gameLiftConfig.GameLiftWrapper.DeregisterCompute(deregisterComputeRequest);

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
                    await _gameLiftConfig.GameLiftWrapper.DescribeCompute(describeComputeRequest);

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