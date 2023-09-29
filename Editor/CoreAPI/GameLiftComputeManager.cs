using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLiftPlugin.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.CoreAPI
{
    public class GameLiftComputeManager
    {
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private readonly StateManager _stateManager;

        public GameLiftComputeManager(IAmazonGameLiftWrapper wrapper, StateManager stateManager)
        {
            _amazonGameLiftWrapper = wrapper;
            _stateManager = stateManager;
        }

        public async Task<bool> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            if (webSocketUrl != null)
            {
                _stateManager.ComputeName = computeName;
                _stateManager.IpAddress = ipAddress;
                _stateManager.WebSocketUrl = webSocketUrl;

                return true;
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
                Debug.Log(ex.Message);
                return null;
            }
        }

        public async Task<bool> DeregisterCompute(string computeName, string fleetId)
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

        public async Task<bool> DescribeCompute(string computeName, string fleetId)
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