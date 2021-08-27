// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class QuitGame : MonoBehaviour
{
    [UsedImplicitly]
    public void Run()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
