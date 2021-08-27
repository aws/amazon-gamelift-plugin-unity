// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting
{
    public class GameLiftProcess : IGameLiftProcess
    {
        private readonly IProcessWrapper _processWrapper;

        public GameLiftProcess(IProcessWrapper processWrapper)
        {
            _processWrapper = processWrapper;
        }

        public StartResponse Start(StartRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.GameLiftLocalFilePath))
                {
                    return Response.Fail(new StartResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                string arg = FormatCommand(request);

                int processId = _processWrapper.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = "java",
                    Arguments = arg,
                    WorkingDirectory = Path.GetDirectoryName(request.GameLiftLocalFilePath)
                });

                return Response.Ok(new StartResponse
                {
                    ProcessId = processId
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StartResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        private string FormatCommand(StartRequest request)
        {
            var arguments = new List<string>();

            arguments.Add("-jar");
            arguments.Add(EscapeArgument(request.GameLiftLocalFilePath));
            arguments.Add("-p");
            arguments.Add(EscapeArgument(request.Port.ToString()));

            return string.Join(" ", arguments);
        }

        private string EscapeArgument(string argument)
        {
            return "\"" + Regex.Replace(argument, @"(\\+)$", @"$1$1") + "\"";
        }

        public StopResponse Stop(StopRequest request)
        {
            try
            {
                _processWrapper.Kill(request.ProcessId);

                return Response.Ok(new StopResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StopResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public RunLocalServerResponse RunLocalServer(RunLocalServerRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.FilePath))
                {
                    return Response.Fail(new RunLocalServerResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                int processId = _processWrapper.Start(new ProcessStartInfo
                {
                    UseShellExecute = request.ShowWindow,
                    FileName = request.FilePath
                });

                return Response.Ok(new RunLocalServerResponse
                {
                    ProcessId = processId
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new RunLocalServerResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
