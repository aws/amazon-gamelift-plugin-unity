// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using UnityEditor;

public static class GameLiftClientSettingsMenu
{
    [MenuItem("GameLift/Go To Client Connection Settings", priority = 9201)]
    public static void Run()
    {
        GameLiftClientSettings settings = AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>("Assets/Settings/GameLiftClientSettings.asset");

        if (settings)
        {
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
