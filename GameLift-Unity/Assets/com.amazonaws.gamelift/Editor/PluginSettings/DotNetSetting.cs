// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Ensures that Unity project .NET settings are set up. Primary action is to set the required project settings.
    /// </summary>
    internal class DotNetSetting : Setting
    {
        public DotNetSetting()
            : base(Strings.LabelSettingsDotNetTitle, Strings.LabelSettingsDotNetButton, Strings.TooltipSettingsDotNet)
        {
        }

        internal override void RunPrimaryAction()
        {
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone,
                                                        ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android,
                                                        ApiCompatibilityLevel.NET_4_6);
        }

        protected override bool RefreshIsConfigured()
        {
            return IsApiCompatibilityLevel4X();
        }

        public static bool IsApiCompatibilityLevel4X()
        {
            ApiCompatibilityLevel apiLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            return apiLevel == ApiCompatibilityLevel.NET_4_6;
        }
    }
}
