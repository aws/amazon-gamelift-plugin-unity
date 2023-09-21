// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Keeps the region UI state and saves it to settings.
    /// </summary>
    internal class RegionBootstrap
    {
        private const int DefaultIndex = -1;
        private readonly CoreApi _coreApi;
        private int _currentIndex = -1;

        public string[] AllRegions { get; private set; } = Array.Empty<string>();

        public virtual bool CanSave => IsRegionInRange();

        public int RegionIndex { get; set; }

        public RegionBootstrap(CoreApi coreApi)
        {
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            AllRegions = _coreApi.ListAvailableRegions().ToArray();
        }

        public virtual void Refresh()
        {
            GetSettingResponse getResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!getResponse.Success)
            {
                RegionIndex = DefaultIndex;
                return;
            }

            RegionIndex = Array.IndexOf(AllRegions, getResponse.Value);

            if (RegionIndex < 0)
            {
                RegionIndex = DefaultIndex;
                return;
            }

            _currentIndex = RegionIndex;
        }

        public virtual (bool success, string errorCode) Save()
        {
            if (!IsRegionInRange())
            {
                return (false, ErrorCode.ValueInvalid);
            }

            if (_currentIndex == RegionIndex)
            {
                return (false, null);
            }

            if (!CanSave)
            {
                return (false, null);
            }

            if (!CanSave)
            {
                return (false, ErrorCode.ValueInvalid);
            }

            string region = AllRegions[RegionIndex];

            Response putResponse = _coreApi.PutSetting(SettingsKeys.CurrentRegion, region);

            if (!putResponse.Success)
            {
                return (false, putResponse.ErrorCode);
            }

            _currentIndex = RegionIndex;
            return (true, null);
        }

        private bool IsRegionInRange()
        {
            return RegionIndex >= 0 && RegionIndex < AllRegions.Length;
        }
    }
}
