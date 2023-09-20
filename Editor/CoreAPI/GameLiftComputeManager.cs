﻿using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
 using AmazonGameLiftPlugin.Core.Shared;
 using Editor.CoreAPI.Models;
 using UnityEngine;
using UnityEngine.UIElements;
 using ErrorCode = AmazonGameLift.Editor.ErrorCode;

 namespace Editor.CoreAPI
{
    public class GameLiftComputeManager
    {
        private readonly CoreApi _coreApi;
        private readonly IAmazonGameLiftClientWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftComputeManager(CoreApi coreApi, IAmazonGameLiftClientWrapper wrapper)
        {
            _coreApi = coreApi;
            _amazonGameLiftWrapper = wrapper;
        }

        internal async Task<Response> RegisterFleetCompute(string computeName, string fleetId, string fleetLocation,
            string ipAddress)
        {
            if (_amazonGameLiftWrapper != null)
            {
                var webSocketUrl = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
                if (webSocketUrl != null)
                {
                    var computeNameResponse = _coreApi.PutSetting(SettingsKeys.ComputeName, computeName);
                    var ipAddressResponse = _coreApi.PutSetting(SettingsKeys.IpAddress, ipAddress);
                    
                    _coreApi.PutSetting(SettingsKeys.WebSocketUrl, webSocketUrl);

                    if (!computeNameResponse.Success)
                    {
                        return Response.Fail(new GenericResponse(ErrorCode.InvalidComputeName));
                    }

                    if (!ipAddressResponse.Success)
                    {
                        return Response.Fail(new GenericResponse(ErrorCode.InvalidIpAddress));
                    }

                    return Response.Ok(new GenericResponse());
                }

                return Response.Fail(new GenericResponse(ErrorCode.RegisterComputeFailed));
            }
            
            return Response.Fail(new GenericResponse(ErrorCode.AccountProfileMissing));
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
                // var errorBox = _container.Q<VisualElement>("ComputeErrorInfoBox");
                // errorBox.style.display = DisplayStyle.Flex;
                // errorBox.Q<Label>().text = ex.Message;
                Debug.Log(ex.Message);
                return null;
            }
        }
    }
}