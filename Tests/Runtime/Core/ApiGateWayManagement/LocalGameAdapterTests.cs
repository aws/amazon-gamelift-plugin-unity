// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.ApiGateWayManagement
{
    [TestFixture]
    public class LocalGameAdapterTests
    {
        [Test]
        public void StartGame_WhenSearchGameSessionReturnsEmptySessions_IsSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.CreateGameSessionAsync(It.IsAny<CreateGameSessionRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateGameSessionResponse { }).Verifiable();
            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).ReturnsAsync(new DescribeGameSessionsResponse { }).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.StartGameResponse response = adapter.StartGame(new ApiGatewayManagement.Models.StartGameRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void StartGame_WhenSearchGameSessionReturnsExistingSession_IsSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).ReturnsAsync(new DescribeGameSessionsResponse
            {
                GameSessions = new List<GameSession>()
                {
                    new GameSession()
                }
            }).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.StartGameResponse response = adapter.StartGame(new ApiGatewayManagement.Models.StartGameRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void StartGame_WhenExceptionThrows_IsNotSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).Throws(new Exception("Unknown Exception")).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.StartGameResponse response = adapter.StartGame(new ApiGatewayManagement.Models.StartGameRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.ErrorCode, ErrorCode.UnknownError);
        }

        [Test]
        public void GetGameConnection_WhenSearchGameSessionReturnsEmptySessions_IsNotSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).ReturnsAsync(new DescribeGameSessionsResponse { }).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.GetGameConnectionResponse response = adapter.GetGameConnection(new ApiGatewayManagement.Models.GetGameConnectionRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.ErrorCode, ErrorCode.NoGameSessionWasFound);
        }

        [Test]
        public void GetGameConnection_WhenSearchGameSessionReturnsSession_IsSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).ReturnsAsync(new DescribeGameSessionsResponse
            {
                GameSessions = new List<GameSession>
                {
                    new GameSession
                    {
                        IpAddress = "NonEmptyIp",
                        DnsName = "NonEmptyDns",
                        Port = 1
                    }
                }
            }).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.GetGameConnectionResponse response = adapter.GetGameConnection(new ApiGatewayManagement.Models.GetGameConnectionRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.IpAddress, "NonEmptyIp");
            Assert.AreEqual(response.DnsName, "NonEmptyDns");
        }

        [Test]
        public void GetGameConnection_WhenExceptionThrows_IsNotSuccessful()
        {
            var amazonGameLiftClientWrapperMock = new Mock<IAmazonGameLiftClientWrapper>();

            amazonGameLiftClientWrapperMock.Setup(x => x.DescribeGameSessions(It.IsAny<DescribeGameSessionsRequest>())).Throws(new Exception("Unknown Exception")).Verifiable();

            var adapter = new LocalGameAdapter(amazonGameLiftClientWrapperMock.Object);

            ApiGatewayManagement.Models.GetGameConnectionResponse response = adapter.GetGameConnection(new ApiGatewayManagement.Models.GetGameConnectionRequest()).Result;

            amazonGameLiftClientWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.ErrorCode, ErrorCode.UnknownError);
        }
    }
}
