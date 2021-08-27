// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    internal interface IReadStatus
    {
        bool IsDisplayed { get; }

        string Message { get; }

        MessageType Type { get; }

        event Action Changed;
    }
}
