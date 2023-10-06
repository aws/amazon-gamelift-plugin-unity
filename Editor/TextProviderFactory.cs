// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public static class TextProviderFactory
    {
        private static TextProvider s_textProvider;

        internal static TextProvider Create()
        {
            if (s_textProvider == null)
            {
                s_textProvider = new TextProvider();
            }

            return s_textProvider;
        }
    }
}
