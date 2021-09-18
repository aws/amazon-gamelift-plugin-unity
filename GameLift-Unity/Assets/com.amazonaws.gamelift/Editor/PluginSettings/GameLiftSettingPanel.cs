// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class GameLiftSettingPanel : SettingPanel
    {
        private readonly GameLiftLocalSetting _setting;
        private readonly string _startPath;
        private readonly string _labelSetPath;
        private readonly string _titleSetPathDialog;

        public GameLiftSettingPanel(GameLiftLocalSetting setting, TextProvider textProvider)
          : base(setting, textProvider)
        {
            _setting = setting ?? throw new ArgumentNullException(nameof(setting));
            _startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _labelSetPath = textProvider.Get(Strings.LabelSettingsLocalTestingSetPathButton);
            _titleSetPathDialog = textProvider.Get(Strings.TitleLocalTestingGameLiftPathDialog);
        }

        public override void DrawAction()
        {
            EditorGUILayout.BeginHorizontal();
            {
                base.DrawAction();

                if (GUILayout.Button(_labelSetPath))
                {
                    string path = EditorUtility.OpenFilePanel(_titleSetPathDialog, _startPath, "jar");
                    _setting.SetPath(path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
