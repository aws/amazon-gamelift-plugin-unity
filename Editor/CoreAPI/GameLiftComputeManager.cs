﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
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

        public async Task<RegisterFleetComputeResponse> RegisterFleetCompute(string computeName, string fleetId,
            string fleetLocation,
            string ipAddress)
        {
            if (_amazonGameLiftWrapper != null)
            {
                if (string.IsNullOrWhiteSpace(computeName))
                {
                    return Response.Fail(new RegisterFleetComputeResponse
                    {
                        ErrorCode = ErrorCode.InvalidComputeName
                    });
                }
                
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    return Response.Fail(new RegisterFleetComputeResponse
                    {
                        ErrorCode = ErrorCode.InvalidIpAddress
                    });
                }
                
                var registerFleetComputeResponse = await RegisterCompute(computeName, fleetId, fleetLocation, ipAddress);
                if (registerFleetComputeResponse.Success)
                {
                    return Response.Ok(new RegisterFleetComputeResponse()
                    {
                        ComputeName = computeName,
                        IpAddress = ipAddress,
                        WebSocketUrl = registerFleetComputeResponse.WebSocketUrl
                    });
                }

                return registerFleetComputeResponse;
            }

            return Response.Fail(new RegisterFleetComputeResponse { ErrorCode = ErrorCode.AccountProfileMissing });
        }

        private async Task<RegisterFleetComputeResponse> RegisterCompute(string computeName, string fleetId, string fleetLocation,
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

                return Response.Ok(new RegisterFleetComputeResponse()
                {
                    WebSocketUrl = registerComputeResponse.Compute.GameLiftServiceSdkEndpoint
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new RegisterFleetComputeResponse
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