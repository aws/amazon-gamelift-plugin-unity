// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    internal class BootstrapSettingsFactory
    {
        public static BootstrapSettings Create()
        {
            BucketLifecyclePolicyTextProvider bucketLifecyclePolicyTextProvider = BucketLifecyclePolicyTextProviderFactory.Create();
            IEnumerable<string> policyLifecycles = bucketLifecyclePolicyTextProvider.GetAllLifecyclePolicies();
            return new BootstrapSettings(policyLifecycles, TextProviderFactory.Create(), new BootstrapBucketFormatter());
        }
    }
}
