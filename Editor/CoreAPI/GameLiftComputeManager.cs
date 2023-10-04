using System;
using System.Net;
using System.Threading.Tasks;
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
    public class GameLiftComputeManager
    {
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;

        public GameLiftComputeManager(IAmazonGameLiftWrapper wrapper)
        {
            _amazonGameLiftWrapper = wrapper;
        }

        public async Task<RegisterFleetComputeResponse> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress, string existingComputeName = null)
        {
            if (!string.IsNullOrWhiteSpace(existingComputeName))
            {
                await DeregisterCompute(existingComputeName, fleetId);
            }

            var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            if (webSocketUrl != null)
            {
                return Response.Ok(new RegisterFleetComputeResponse()
                {
                    ComputeName = computeName,
                    IpAddress = ipAddress,
                    WebSocketUrl = webSocketUrl
                });
            }

            return Response.Fail(new RegisterFleetComputeResponse { ErrorCode = ErrorCode.RegisterComputeFailed });
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