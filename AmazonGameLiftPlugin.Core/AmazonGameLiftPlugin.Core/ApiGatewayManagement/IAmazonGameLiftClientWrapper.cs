using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift.Model;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public interface IAmazonGameLiftClientWrapper
    {
        Task<CreateGameSessionResponse> CreateGameSessionAsync(
                CreateGameSessionRequest request,
                CancellationToken cancellationToken = default
            );

        Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request);

        Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request);
    }
}
