// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AmazonGameLiftPlugin.Core.JavaCheck.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;

namespace AmazonGameLiftPlugin.Core.JavaCheck
{
    public class InstalledJavaVersionProvider : IInstalledJavaVersionProvider
    {
        private readonly IProcessWrapper _process;

        public InstalledJavaVersionProvider(IProcessWrapper process)
        {
            _process = process;
        }

        public CheckInstalledJavaVersionResponse CheckInstalledJavaVersion(CheckInstalledJavaVersionRequest request)
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "java.exe";
            processStartInfo.Arguments = " -version";
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;

            string processOutput = null;
            try
            {
                processOutput = _process.GetProcessOutput(processStartInfo);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = false
                });
            }

            var outputPattern = new Regex("\\d+\\.\\d+\\.\\d+(?:_\\d+)?\"");

            Match outputMatch = outputPattern.Match(processOutput);

            if (!outputMatch.Success)
            {
                return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = false
                });
            }

            // example output: java version "1.8.0"
            string[] outputWords = processOutput
                .Split(' ');

            if (outputWords.Length < 3)
            {
                return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = false
                });
            }

            string installedVersion = outputWords[2]
                .Replace("\"", "");

            var regex = new Regex(@"([0-9]+)");
            MatchCollection versionNumbers = regex.Matches(installedVersion);

            if (versionNumbers.Count < 2)
            {
                return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = false
                });
            }

            //we need to match major version in string like "1.8.0" here major version is 8
            string majorVersion = versionNumbers[1].Value;

            bool majorVersionParsed = int.TryParse(majorVersion, out int majorVersionAsNumber);

            if (!majorVersionParsed)
            {
                return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = false
                });
            }

            bool isInstalled = majorVersionAsNumber >= request.ExpectedMinimumJavaMajorVersion;

            return Response.Ok(new CheckInstalledJavaVersionResponse
            {
                IsInstalled = isInstalled
            });
        }
    }
}
