// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    internal class LocalTestFactory
    {
        public static LocalTest Create(TextProvider textProvider)
        {
            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            return new LocalTest(CoreApi.SharedInstance, textProvider, new Delay(), logger);
        }

        public static void Restore(LocalTest target, TextProvider textProvider)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            target.Restore(CoreApi.SharedInstance, textProvider, new Delay(), logger);
        }
    }
}
