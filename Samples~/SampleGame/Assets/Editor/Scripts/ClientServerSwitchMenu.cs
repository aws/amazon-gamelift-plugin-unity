// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using Editor.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ClientServerSwitchMenu
{
    private const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string UnityServerDefine = "UNITY_SERVER";
    private const string MissingWindowsModuleError = "Please install Windows build module via UnityHub first. See: https://docs.unity3d.com/Manual/GettingStartedAddingEditorComponents.html";

    private static readonly ScriptingDefineSymbolEditor _defineEditor =
        new ScriptingDefineSymbolEditor(BuildTargetGroup.Standalone);

#if UNITY_EDITOR_OSX
    [MenuItem("Amazon GameLift/Sample Game/Apply MacOS Sample Client Build Settings", priority = 9203)]
    public static void ConfigureMacOsClient()
    {
#if UNITY_2021_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
#else
        EditorUserBuildSettings.enableHeadlessMode = false;
#endif
        EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX );
        Switch(RemoveServer);
        AnywhereFleetSettingsWriter.WriteClientSettings();
        LogSuccessMessage("Sample Client", "MacOS");
    }

    [MenuItem("Amazon GameLift/Sample Game/Apply MacOS Sample Server Build Settings", priority = 9102)]
    public static void ConfigureMacOsServer()
    {
#if UNITY_2021_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
#else
        EditorUserBuildSettings.enableHeadlessMode = true;
#endif
        EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX );
        Switch(AddServer);
        AnywhereFleetSettingsWriter.WriteServerSettings();
        LogSuccessMessage("Sample Server", "MacOS");
    }
#endif

    [MenuItem("Amazon GameLift/Sample Game/Apply Windows Sample Client Build Settings", priority = 9202)]
    public static void RunClient()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
        {
            Debug.LogError(MissingWindowsModuleError);
            return;
        }
        EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64 );
#if UNITY_2021_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
#else
        EditorUserBuildSettings.enableHeadlessMode = false;
#endif
        Switch(RemoveServer);
        AnywhereFleetSettingsWriter.WriteClientSettings();
        LogSuccessMessage("Sample Client", "Windows");
    }

    [MenuItem("Amazon GameLift/Sample Game/Apply Windows Sample Server Build Settings", priority = 9101)]
    public static void RunServer()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
        {
            Debug.LogError(MissingWindowsModuleError);
            return;
        }
        EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64 );
#if UNITY_2021_3_OR_NEWER
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
#else
        EditorUserBuildSettings.enableHeadlessMode = true;
#endif
        Switch(AddServer);
        AnywhereFleetSettingsWriter.WriteServerSettings();
        LogSuccessMessage("Sample Server", "Windows");
    }

    private static void LogSuccessMessage(string name, string platform) {
        Debug.Log($"{name} has been successfully configured for {platform}. Please go to BuildSettings to build the project.");
    }

    private static void Switch(Action updateDefines)
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootstrapScenePath, enabled: true),
            new EditorBuildSettingsScene(GameScenePath, enabled: true),
        };

        EditorSceneManager.OpenScene(BootstrapScenePath);
        updateDefines();

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

    private static void AddServer()
    {
        _defineEditor.Add(UnityServerDefine);
    }

    private static void RemoveServer()
    {
        _defineEditor.Remove(UnityServerDefine);
    }
}
