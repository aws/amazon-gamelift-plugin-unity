// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SERVER
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;

public class Settings
{
    private readonly ISettingsStore _settingsStore = new SettingsStore(new FileWrapper(), settingsFilePath: "todo");

    public GetSettingResponse GetSetting(string key)
    {
        var request = new GetSettingRequest() { Key = key };
        return _settingsStore.GetSetting(request);
    }

    public virtual PutSettingResponse PutSetting(string key, string value)
    {
        var request = new PutSettingRequest()
        {
            Key = key,
            Value = value
        };
        return _settingsStore.PutSetting(request);
    }

    public virtual ClearSettingResponse ClearSetting(string key)
    {
        var request = new ClearSettingRequest() { Key = key };
        return _settingsStore.ClearSetting(request);
    }

    public virtual Response PutSettingOrClear(string key, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return _settingsStore.ClearSetting(new ClearSettingRequest() { Key = key });
        }

        return _settingsStore.PutSetting(new PutSettingRequest()
        {
            Key = key,
            Value = value
        });
    }
}
#endif
