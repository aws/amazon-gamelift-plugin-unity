// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public class AwsCredentialsFactory : IAwsCredentialsFactory
    {
        public AwsCredentials Create()
        {
            TextProvider textProvider = TextProviderFactory.Create();
            return new AwsCredentials(textProvider, UnityLoggerFactory.Create(textProvider));
        }
    }

    public interface IAwsCredentialsFactory
    {
        public AwsCredentials Create();
    }
}