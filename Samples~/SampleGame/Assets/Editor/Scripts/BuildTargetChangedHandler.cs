// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEditor.Build;

namespace AmazonGameLift.Editor
{
    public class BuildTargetChangedHandler : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;
        private const string _unityServerDefine = "UNITY_SERVER";

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone)
            {
                return;
            }

            var defineEditor = new ScriptingDefineSymbolEditor(BuildTargetGroup.Standalone);

            if (EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
            {
                defineEditor.Add(_unityServerDefine);
            }
            else
            {
                defineEditor.Remove(_unityServerDefine);
            }
        }
    }
}
