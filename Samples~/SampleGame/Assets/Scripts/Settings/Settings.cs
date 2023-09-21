// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SERVER
using System;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;

public class Settings
{
    public const string ConfigFilePath = "GameLiftServerRuntimeSettings.yaml";

    private readonly ISettingsStore _settingsStore = new SettingsStore(new FileWrapper(), settingsFilePath: ConfigFilePath);

    private static string GetKeyName(SettingsKeys value) => Enum.GetName(typeof(SettingsKeys), value);

    public GetSettingResponse GetSetting(SettingsKeys key)
    {
        var request = new GetSettingRequest() { Key = GetKeyName(key) };
        return _settingsStore.GetSetting(request);
    }

    public virtual PutSettingResponse PutSetting(SettingsKeys key, string value)
    {
        var request = new PutSettingRequest() { Key = GetKeyName(key), Value = value };
        return _settingsStore.PutSetting(request);
    }

    public virtual ClearSettingResponse ClearSetting(SettingsKeys key)
    {
        var request = new ClearSettingRequest() { Key = GetKeyName(key) };
        return _settingsStore.ClearSetting(request);
    }

    public virtual Response PutSettingOrClear(SettingsKeys key, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return _settingsStore.ClearSetting(new ClearSettingRequest() { Key = GetKeyName(key) });
        }

        return _settingsStore.PutSetting(new PutSettingRequest() { Key = GetKeyName(key), Value = value });
    }
}
#endif
