// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace AWS.GameLift.Editor
{
    public static class StackUpdateDialogTest
    {
        [MenuItem("GameLift/Testing/Unity Dialog")]
        public static void RunUnity()
        {
            EditorUtility.DisplayDialog("Place Selection On Surface?",
               "Are you sure you want to place on the surface?", "Place", "Do Not Place");
        }

        [MenuItem("GameLift/Testing/StackUpdateDialog (Removal changes)")]
        public static async void RunRemoval()
        {
            IEnumerable<Change> changes = new[]
            {
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "RestApi",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
                new Change()
                {
                    Action = "Remove",
                    LogicalId = "UserPool",
                    ResourceType = "AWS::Cognito::UserPool"
                }
            };
            await RunStackUpdateTest(changes);
        }

        private static async Task RunStackUpdateTest(IEnumerable<Change> changes)
        {
            var changeSet = new ConfirmChangesRequest
            {
                Region = "eu-west-1",
                StackId = "StackId",
                ChangeSetId = "ChangeSetId",
                Changes = changes,
            };
            StackUpdateDialog dialog = EditorWindow.GetWindow<StackUpdateDialog>();
            bool result = await dialog.SetUp(new StackUpdateModel(changeSet, string.Empty));
            Debug.Log($"Result: {result}");
        }

        [MenuItem("GameLift/Testing/StackUpdateDialog (No removal changes)")]
        public static async void Run()
        {
            IEnumerable<Change> changes = new[]
            {
                new Change()
                {
                    Action = "Add",
                    LogicalId = "RestApi2",
                    ResourceType = "AWS::ApiGateway::RestApi"
                },
            };
            await RunStackUpdateTest(changes);
        }
    }
}
