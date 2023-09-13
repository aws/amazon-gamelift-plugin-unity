using System;
using UnityEngine;

using AmazonGameLiftPlugin.Core.Shared;

namespace Tests.Utils
{
    public class MockLogger : ILogger
    {
        
        public void Log(string message, LogType logType)
        {    
        }

        public void LogResponseError(Response response, LogType logType = LogType.Error)
        {
            
        }

        public void LogException(Exception ex)
        {
        }
    }
}