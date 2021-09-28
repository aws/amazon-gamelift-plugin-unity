// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal sealed class MockLogger : ILogger
    {
        public void Log(string message, LogType logType)
        {    
        }

        public void LogException(Exception ex)
        {
        }

        public void LogResponseError(Response response, LogType logType)
        {
        }
    }
}
