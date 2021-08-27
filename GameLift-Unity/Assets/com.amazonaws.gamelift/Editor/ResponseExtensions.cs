// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal static class ResponseExtensions
    {
        public static void LogError(this Response response, TextProvider textProvider)
        {
            if (response == null)
            {
                return;
            }

            if (textProvider is null)
            {
                throw new ArgumentNullException(nameof(textProvider));
            }

            Debug.LogError($"{textProvider.GetError(response.ErrorCode)} {response.ErrorMessage}");
        }

        public static void Log(this Response response, TextProvider textProvider)
        {
            if (response == null)
            {
                return;
            }

            if (textProvider is null)
            {
                throw new ArgumentNullException(nameof(textProvider));
            }

            Debug.Log($"{textProvider.GetError(response.ErrorCode)} {response.ErrorMessage}");
        }
    }
}
