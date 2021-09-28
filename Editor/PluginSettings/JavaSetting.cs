// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Ensures that Java runtime is available. Primary action is to open a web browser with the download page.
    /// </summary>
    internal class JavaSetting : Setting
    {
        private readonly CoreApi _coreApi;

        public JavaSetting(CoreApi coreApi)
            : base(Strings.LabelSettingsJavaTitle, Strings.LabelSettingsJavaButton, Strings.TooltipSettingsJava) =>
            _coreApi = coreApi ?? throw new System.ArgumentNullException(nameof(coreApi));

        internal override void RunPrimaryAction()
        {
            Application.OpenURL(Urls.JavaDownload);
        }

        protected override bool RefreshIsConfigured()
        {
            return _coreApi.CheckInstalledJavaVersion();
        }
    }
}
