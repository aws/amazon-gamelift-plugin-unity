using System;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class LocalGameAdapter : IGameServerAdapter
    {
        private readonly IAmazonGameLiftClientWrapper _amazonGameLiftClientWrapper;
        private readonly string _fleetId = "fleet-123";

        public LocalGameAdapter(IAmazonGameLiftClientWrapper amazonGameLiftClientWrapper)
        {
            _amazonGameLiftClientWrapper = amazonGameLiftClientWrapper;
        }

        public async Task<GetGameConnectionResponse> GetGameConnection(GetGameConnectionRequest request)
        {
            try
            {
                Amazon.GameLift.Model.DescribeGameSessionsResponse describeGameSessionsResponse = await _amazonGameLiftClientWrapper.DescribeGameSessions(new Amazon.GameLift.Model.DescribeGameSessionsRequest
                {
                    FleetId = _fleetId,
                });

                if (describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.GameSession oldestGameSession = describeGameSessionsResponse.GameSessions.First();

                    var response = new GetGameConnectionResponse
                    {
                        IpAddress = oldestGameSession.IpAddress,
                        DnsName = oldestGameSession.DnsName,
                        Port = oldestGameSession.Port.ToString(),
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
                });

                if (!describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.CreateGameSessionResponse createGameSessionResponse = await _amazonGameLiftClientWrapper.CreateGameSessionAsync(new Amazon.GameLift.Model.CreateGameSessionRequest
                    {
                        MaximumPlayerSessionCount = 4,
                        FleetId = _fleetId
                    });
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
