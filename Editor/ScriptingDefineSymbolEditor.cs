// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    public class ScriptingDefineSymbolEditor
    {
        private readonly BuildTargetGroup _buildTargetGroup;

        public ScriptingDefineSymbolEditor(BuildTargetGroup buildTargetGroup)
        {
            _buildTargetGroup = buildTargetGroup;
        }

        public void Add(string value)
        {
            IEnumerable<string> defines =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup).Split(';');
            if (defines.Contains(value))
            {
                return;
            }

            defines = defines.Append(value);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, string.Join(';', defines));
        }

        public void Remove(string value)
        {
            IEnumerable<string> defines =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup).Split(';');
            if (!defines.Contains(value))
            {
                return;
            }

            defines = defines.Where(item => item != value);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, string.Join(';', defines));
        }
    }
}