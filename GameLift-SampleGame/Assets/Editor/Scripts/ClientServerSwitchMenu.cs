// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ClientServerSwitchMenu
{
    private const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string UnityServerDefine = "UNITY_SERVER";

    [MenuItem("GameLift/Apply Sample Client Build Settings", priority = 9202)]
    public static void RunClient()
    {
        EditorUserBuildSettings.enableHeadlessMode = false;
        Switch(RemoveServer);
        Debug.Log("Sample Client has been successfully configured:\n-Sample scenes are added to build settings\n-Scripting define symbols are set\n-Server build has been disabled");
    }

    [MenuItem("GameLift/Apply Sample Server Build Settings", priority = 9101)]
    public static void RunServer()
    {
        EditorUserBuildSettings.enableHeadlessMode = true;
        Switch(AddServer);
        Debug.Log("Sample Server has been successfully configured:\n-Sample scenes are added to build settings\n-Scripting define symbols are set\n-Server build has been enabled");
    }

    private static void Switch(Func<string, string> updateDefines)
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootstrapScenePath, enabled: true),
            new EditorBuildSettingsScene(GameScenePath, enabled: true),
        };

        EditorSceneManager.OpenScene(BootstrapScenePath);
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        defines = updateDefines(defines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);

#if !UNITY_2019_4_OR_NEWER
        bool ok = EditorUtility.DisplayDialog("Unity restart required",
                "Restart Unity to finish the configuration?",
                "Yes",
                "No");

        if (ok)
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }
#endif
    }

    private static string AddServer(string defines)
    {
        if (defines.Contains(UnityServerDefine + ";") || defines.EndsWith(UnityServerDefine))
        {
            return defines;
        }

        return defines + ";" + UnityServerDefine;
    }

    private static string RemoveServer(string defines)
    {
        int index = defines.IndexOf(UnityServerDefine);

        if (index < 0)
        {
            return defines;
        }

        return defines.Remove(index, UnityServerDefine.Length);
    }
}
