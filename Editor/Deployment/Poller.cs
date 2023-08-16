using System;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class Poller : IResponsePoller
    {
        public async Task<T> Poll<T>(int periodMs, Func<T> action, Predicate<T> stopCondition = null) where T : Response
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            T response;

            while (true)
            {
                response = action();

                if (!response.Success)
                {
                    break;
                }

                if (stopCondition != null && stopCondition(response))
                {
                    break;
                }

                await Task.Delay(periodMs);
            }

            return response;
        }
    }
}