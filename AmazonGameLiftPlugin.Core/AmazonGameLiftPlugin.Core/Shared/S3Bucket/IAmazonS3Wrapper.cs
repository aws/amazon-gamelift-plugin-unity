// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.S3.Model;

namespace AmazonGameLiftPlugin.Core.Shared.S3Bucket
{
    public interface IAmazonS3Wrapper
    {
        PutBucketResponse PutBucket(string bucketName);

        ListBucketsResponse ListBuckets();

        GetBucketLocationResponse GetBucketLocation(GetBucketLocationRequest request);

        PutBucketResponse PutBucket(PutBucketRequest request);

        bool DoesBucketExist(string bucketName);

        GetLifecycleConfigurationResponse GetLifecycleConfiguration(string bucketName);

        PutLifecycleConfigurationResponse PutLifecycleConfiguration(PutLifecycleConfigurationRequest request);

        PutObjectResponse PutObject(PutObjectRequest request);

        string GetRegionEndpoint();
    }
}
