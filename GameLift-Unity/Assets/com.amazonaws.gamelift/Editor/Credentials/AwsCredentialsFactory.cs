// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsFactory
    {
        public static AwsCredentials Create()
            => new AwsCredentials(TextProviderFactory.Create());
    }
}

