using System;
using System.Net;
using System.Threading.Tasks;
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
    public class GameLiftComputeManager
    {
        private readonly CoreApi _coreApi;
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftComputeManager(CoreApi coreApi, IAmazonGameLiftWrapper wrapper)
        {
            _coreApi = coreApi;
            _amazonGameLiftWrapper = wrapper;
        }

        public async Task<Response> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            var registerComputeResponse = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
            var webSocketUrl = registerComputeResponse.WebSocketUrl;
            if (webSocketUrl != null)
            {
                var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                var webSocketResponse = _coreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);
                
                if (!computeNameResponse.Success)
                {
                    return Response.Fail(ErrorCode.InvalidComputeName, "Invalid Compute Name");
                }
                if (!ipAddressResponse.Success)
                {
                    return Response.Fail(ErrorCode.InvalidIpAddress, "Invalid Ip Address");
                }
                if (!webSocketResponse.Success)
                {
                    return Response.Fail(ErrorCode.InvalidWebsocketUrl, "Invalid Websocket Response");
                }
            }

            return registerComputeResponse;
        }

        private async Task<RegisterComputeResponse> RegisterCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            try
            {
                var registerComputeRequest = new Model.RegisterComputeRequest()
                {
                    ComputeName = computeName,
                    FleetId = fleetId,
                    IpAddress = ipAddress,
                    Location = fleetLocation
                };
                var registerComputeResponse =
                    await _amazonGameLiftWrapper.RegisterCompute(registerComputeRequest);

                return Response.Ok(new RegisterComputeResponse()
                {
                    WebSocketUrl = registerComputeResponse.Compute.GameLiftServiceSdkEndpoint
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new RegisterComputeResponse
                {
                    ErrorCode = ErrorCode.RegisterComputeFailed,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<bool> DeregisterCompute(string computeName, string fleetId)
        {
            try
            {
                var deregisterComputeRequest = new Model.DeregisterComputeRequest()
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
                var describeComputeRequest = new Model.DescribeComputeRequest()
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