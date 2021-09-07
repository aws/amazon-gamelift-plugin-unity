// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal class DeploymentSettingsFactory
    {
        public static DeploymentSettings Create()
        {
            var parametersUpdater = new ScenarioParametersUpdater(CoreApi.SharedInstance, () => new ScenarioParametersEditor());
            TextProvider textProvider = TextProviderFactory.Create();
            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            return new DeploymentSettings(ScenarioLocator.SharedInstance, PathConverter.SharedInstance,
                            CoreApi.SharedInstance, parametersUpdater, textProvider,
                            new DeploymentWaiter(), DeploymentIdContainerFactory.Create(), new Delay(), logger);
        }
    }
}
