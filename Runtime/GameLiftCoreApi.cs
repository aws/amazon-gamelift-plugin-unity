// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if !UNITY_SERVER

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Latency;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;
using UnityEngine;

namespace AmazonGameLift.Runtime
{
    public class GameLiftCoreApi
    {
        private readonly GameLiftConfiguration _configuration;
        private readonly bool _isAnywhereMode;

        protected GameLiftCoreApi(GameLiftConfiguration configuration)
        {
            _configuration = configuration;
            _userIdentity = new UserIdentity(new AmazonCognitoIdentityWrapper(configuration.AwsRegion));
            _apiGateway = new ApiGateway(_userIdentity, new JwtTokenExpirationCheck(), new HttpClientWrapper());
            
            var clientSettings = Resources.FindObjectsOfTypeAll<GameLiftClientSettings>()[0];
            if (_configuration.IsGameLiftAnywhere)
            {
                var gameLiftClientWrapper = new AmazonGameLiftClientWrapper(clientSettings.ProfileName);
                _anywhereGame = new GLAGameAdapter(gameLiftClientWrapper,clientSettings.FleetID, clientSettings.FleetLocation);
                _isAnywhereMode = true;
            }
        }
        
        #region User Accounts

        private readonly IUserIdentity _userIdentity;

        public virtual SignUpResponse SignUp(string email, string password)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignUpResponse());
            }

            var request = new SignUpRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                Password = password,
            };
            return _userIdentity.SignUp(request);
        }

        public virtual ConfirmSignUpResponse ConfirmSignUp(string email, string confirmationCode)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new ConfirmSignUpResponse());
            }

            var request = new ConfirmSignUpRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
            };
            return _userIdentity.ConfirmSignUp(request);
        }

        public virtual SignInResponse SignIn(string email, string password)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignInResponse()
                {
                    AccessToken = string.Empty,
                    IdToken = string.Empty,
                    RefreshToken = string.Empty,
                });
            }

            var request = new SignInRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                Password = password,
            };
            return _userIdentity.SignIn(request);
        }

        public virtual SignOutResponse SignOut(string accessToken)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignOutResponse());
            }

            var request = new SignOutRequest()
            {
                AccessToken = accessToken,
            };
            return _userIdentity.SignOut(request);
        }

        #endregion

        #region Matchmaking

        private readonly ApiGateway _apiGateway;
        private readonly LatencyService _latencyService = new LatencyService(new PingWrapper());
        private readonly GLAGameAdapter _anywhereGame;

        public virtual string[] ListAvailableRegions()
        {
            return AwsRegionMapper.AvailableRegions().ToArray();
        }

        public virtual Task<GetLatenciesResponse> GetLatencies(string[] regions)
        {
            var request = new GetLatenciesRequest()
            {
                Regions = regions
            };
            return _latencyService.GetLatencies(request);
        }

        public virtual Task<GetGameConnectionResponse> GetGameConnection(string idToken, string refreshToken)
        {
            var request = new GetGameConnectionRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                ApiGatewayEndpoint = _configuration.ApiGatewayEndpoint,
                IdToken = idToken,
                RefreshToken = refreshToken,
            };

            if (_isAnywhereMode)
            {
                return _anywhereGame.GetGameConnection(request);
            }
            return _apiGateway.GetGameConnection(request);
        }

        public virtual Task<StartGameResponse> StartGame(string idToken, string refreshToken, Dictionary<string, long> latencies)
        {
            var request = new StartGameRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                ApiGatewayEndpoint = _configuration.ApiGatewayEndpoint,
                IdToken = idToken,
                RefreshToken = refreshToken,
                RegionLatencies = latencies
            };

            if (_isAnywhereMode)
            {
                return _anywhereGame.StartGame(request);
            }
            return _apiGateway.StartGame(request);
        }
        #endregion
    }
}

#endif
