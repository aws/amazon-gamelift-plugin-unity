// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.GameLift;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using Logger = AmazonGameLiftPlugin.Core.Shared.Logging.Logger;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement 
{
    public class AnywhereGameServerAdapter : IGameServerAdapter
    {
        private readonly IAmazonGameLiftClientWrapper _amazonGameLiftClientWrapper;
        private readonly string _fleetId;
        private readonly string _location;
        private readonly string _playerIdPrefix = "playerId-";
        
        public AnywhereGameServerAdapter(IAmazonGameLiftClientWrapper amazonGameLiftClientWrapper,string fleetId, string fleetLocation )
        {
            _amazonGameLiftClientWrapper = amazonGameLiftClientWrapper;
            _location = fleetLocation;
            _fleetId = fleetId;
        }

        public async Task<GetGameConnectionResponse> GetGameConnection(GetGameConnectionRequest request)
        {
            try
            {
                Amazon.GameLift.Model.DescribeGameSessionsResponse describeGameSessionsResponse = await _amazonGameLiftClientWrapper.DescribeGameSessions(new Amazon.GameLift.Model.DescribeGameSessionsRequest
                {
                    FleetId = _fleetId,
                    StatusFilter = GameSessionStatus.ACTIVE
                });

                if (describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.GameSession oldestGameSession = describeGameSessionsResponse.GameSessions.First();

                    Amazon.GameLift.Model.CreatePlayerSessionResponse createPlayerSessionResponse = await _amazonGameLiftClientWrapper.CreatePlayerSession(new Amazon.GameLift.Model.CreatePlayerSessionRequest
                    {
                        GameSessionId = oldestGameSession.GameSessionId,
                        PlayerId = _playerIdPrefix + Guid.NewGuid().ToString()
                    });
                    Amazon.GameLift.Model.PlayerSession playerSession = createPlayerSessionResponse.PlayerSession;

                    var response = new GetGameConnectionResponse
                    {
                        IpAddress = playerSession.IpAddress,
                        DnsName = playerSession.DnsName,
                        Port = playerSession.Port.ToString(),
                        PlayerSessionId = playerSession.PlayerSessionId,
                        Ready = true
                    };

                    return Response.Ok(response);
                }

                return Response.Fail(new GetGameConnectionResponse
                {
                    ErrorCode = ErrorCode.NoGameSessionWasFound
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new GetGameConnectionResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<StartGameResponse> StartGame(StartGameRequest request)
        {
            try
            {
                Amazon.GameLift.Model.DescribeGameSessionsResponse describeGameSessionsResponse = await _amazonGameLiftClientWrapper.DescribeGameSessions(new Amazon.GameLift.Model.DescribeGameSessionsRequest
                {
                    FleetId = _fleetId,
                    StatusFilter = GameSessionStatus.ACTIVE
                });

                if (!describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.CreateGameSessionResponse createGameSessionResponse = await _amazonGameLiftClientWrapper.CreateGameSessionAsync(new Amazon.GameLift.Model.CreateGameSessionRequest
                    {
                        MaximumPlayerSessionCount = 4,
                        FleetId = _fleetId,
                        Location = _location,
                    });

                    if (createGameSessionResponse.HttpStatusCode != HttpStatusCode.OK)
                    {
                        Debug.Log($"Error createGameSessionResponse not working Error Code: {createGameSessionResponse.HttpStatusCode}");
                    }
                }

                return Response.Ok(new StartGameResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StartGameResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
