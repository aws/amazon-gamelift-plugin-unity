using System;
using Serilog;

namespace AmazonGameLiftPlugin.Core.Shared.Logging
{
    public class Logger
    {
        public static void LogError(Exception ex, string message)
        {
            Log.Error(ex, message);
        }
    }
}
