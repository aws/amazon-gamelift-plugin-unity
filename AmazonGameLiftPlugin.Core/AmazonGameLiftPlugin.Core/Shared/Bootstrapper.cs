using Serilog;

namespace AmazonGameLiftPlugin.Core.Shared
{
    public class Bootstrapper
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File("logs/amazon-gamelift-plugin-logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
