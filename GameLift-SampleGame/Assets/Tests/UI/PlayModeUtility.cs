// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace SampleTests.UI
{
    public static class PlayModeUtility
    {
        public static void DestroyAll<T>() where T : Component
        {
            T[] oldObjects = Object.FindObjectsOfType<T>();

            foreach (T item in oldObjects)
            {
                Object.Destroy(item.gameObject);
            }
        }
    }
}
