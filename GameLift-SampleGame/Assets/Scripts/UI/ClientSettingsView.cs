// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using AmazonGameLift.Runtime;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ClientSettingsView : MonoBehaviour
{
    [SerializeField]
    private Text _text;

    [SerializeField]
    private GameLiftClientSettings _gameLiftSettings;

#if !UNITY_SERVER
    private void Start()
    {
        string message = $"Local Testing Mode: {_gameLiftSettings.IsLocalTest}\n";

        if (_gameLiftSettings.IsLocalTest)
        {
            message += $"GameLift Local URL: {_gameLiftSettings.LocalUrl}\n" +
            $"GameLift Local Port: {_gameLiftSettings.LocalPort}\n";
        }
        else
        {
            message += $"AWS Region: {_gameLiftSettings.AwsRegion}\n" +
            $"Cognito User Pool Client ID: {_gameLiftSettings.UserPoolClientId}\n" +
            $"API Gateway URL: {_gameLiftSettings.ApiGatewayUrl}\n";
        }

        _text.text = message;
    }
#endif
}
