// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public sealed class DeploymentFormatter
    {
        public string GetServerGamePath(string gameFilePathInBuild)
        {
            if (gameFilePathInBuild is null)
            {
                throw new ArgumentNullException(nameof(gameFilePathInBuild));
            }

            return $"C:\\game\\{gameFilePathInBuild}";
        }

        public string GetBuildS3Key()
        {
            string timeStamp = DateTime.Now.Ticks.ToString();
            return $"GameLift_Build_{timeStamp}.zip";
        }

        public string GetStackName(string gameName)
        {
            if (gameName is null)
            {
                throw new ArgumentNullException(nameof(gameName));
            }

            return $"GameLiftPluginForUnity-{gameName}";
        }

        public string GetChangeSetName() => $"changeset-{Guid.NewGuid()}";

        public string GetCloudFormationFileKey(string fileName)
        {
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string timeStamp = DateTime.Now.Ticks.ToString();
            string[] parts = fileName.Split('.');
            string name = parts.First();
            string extension = parts.Last();
            return $"CloudFormation/{name}_{timeStamp}.{extension}";
        }

        public string GetS3Url(string bucketName, string region, string fileName)
        {
            if (bucketName is null)
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (region is null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return $"https://{bucketName}.s3.{region}.amazonaws.com/{fileName}";
        }

        public string GetLambdaS3Key(string gameName)
        {
            if (gameName is null)
            {
                throw new ArgumentNullException(nameof(gameName));
            }

            return $"functions/gamelift/GameLift_{gameName}_{DateTime.Now.Ticks}.zip";
        }
    }
}
