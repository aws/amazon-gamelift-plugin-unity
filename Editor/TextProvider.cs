// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    internal sealed class TextProvider
    {
        private readonly Dictionary<string, string> _errorMessagesByCode = new Dictionary<string, string>
        {
            { CoreErrorCode.AwsError, "AWS error."},
            { CoreErrorCode.BucketDoesNotExist, "The bucket does not exist."},
            { CoreErrorCode.BucketNameAlreadyExists, "The bucket already exists."},
            { CoreErrorCode.BucketNameCanNotBeEmpty, "The bucket name cannot be empty."},
            { CoreErrorCode.BucketNameIsWrong, "There was a problem with the bucket name."},
            { CoreErrorCode.ChangeSetAlreadyExists, "The changeset already exists."},
            { CoreErrorCode.ChangeSetNotFound, "The changeset was not found."},
            { CoreErrorCode.ConflictError, "There was a conflict."},
            { CoreErrorCode.FileNotFound, "The file was not found."},
            { CoreErrorCode.InsufficientCapabilities, "Insufficient capabilities."},
            { CoreErrorCode.InvalidBucketPolicy, "There was a problem with the bucket policy."},
            { CoreErrorCode.InvalidCfnTemplate, "There was a problem with the template."},
            { CoreErrorCode.InvalidChangeSetStatus, "There was a problem with the changeset status."},
            { CoreErrorCode.InvalidIdToken, "There was a problem with the ID token."},
            { CoreErrorCode.InvalidParameters, "There was a problem with the parameters."},
            { CoreErrorCode.InvalidParametersFile, "There was a problem with the parameters file."},
            { CoreErrorCode.InvalidRegion, "There was a problem with the Region."},
            { CoreErrorCode.InvalidSettingsFile, "There was a problem with the settings file."},
            { CoreErrorCode.LimitExceeded, "Limit exceeded."},
            { CoreErrorCode.NoGameSessionWasFound, "No game session was found."},
            { CoreErrorCode.NoProfileFound, "The profile was not found."},
            { CoreErrorCode.NoSettingsFileFound, "The settings file was not found."},
            { CoreErrorCode.NoSettingsKeyFound, "The settings key was not found."},
            { CoreErrorCode.ParametersFileNotFound, "The parameters file was not found."},
            { CoreErrorCode.ProfileAlreadyExists, "The profile already exists."},
            { CoreErrorCode.ResourceWithTheNameRequestetAlreadyExists, "A resource with the requested name already exists."},
            { CoreErrorCode.StackDoesNotExist, "The stack does not exist."},
            { CoreErrorCode.StackDoesNotHaveChanges, "The stack does not have changes."},
            { CoreErrorCode.TemplateFileNotFound, "Template file was not found."},
            { CoreErrorCode.TokenAlreadyExists, "The token already exists."},
            { CoreErrorCode.UnknownError, "Unknown error."},
            { CoreErrorCode.UserNotConfirmed, "The user did not confirm registration."},
            { ErrorCode.ChangeSetStatusInvalid, "There was a problem with the changeset status."},
            { ErrorCode.OperationCancelled, "The operation was cancelled."},
            { ErrorCode.OperationInvalid, "There was a problem with the operation."},
            { ErrorCode.ReadingFileFailed, "There was a problem reading the file."},
            { ErrorCode.StackStatusInvalid, "There was a problem with the stack status."},
            { ErrorCode.ValueInvalid, "There was a problem with the value."},
            { ErrorCode.WritingFileFailed, "There was a problem writing to the file."},
        };

        private readonly Dictionary<string, string> _textsByKey = new Dictionary<string, string>
        {
            { Strings.LabelBootstrapBucketCosts, "Using S3 will incur costs to your account via Storage, Transfer, etc. See S3 Pricing Plan (https://aws.amazon.com/s3/pricing/) for details."},
            { Strings.LabelBootstrapBucketName, "Bucket Name"},
            { Strings.LabelBootstrapCreateButton, "Create"},
            { Strings.LabelBootstrapCreateMode, "Create a new S3 bucket"},
            { Strings.LabelBootstrapBucketLifecycle, "Policy"},
            { Strings.LabelBootstrapLifecycleWarning, "With lifecycle policy configured on the S3 bucket, stale build artifacts in S3 will be deleted automatically, and will cause fleet creation to fail when created with a build referencing the deleted artifacts."},
            { Strings.LabelBootstrapBucketSelectionLoading, "Loading S3 Buckets..."},
            { Strings.LabelBootstrapRegion, "AWS Region"},
            { Strings.LabelBootstrapSelectButton, "Update"},
            { Strings.LabelBootstrapSelectMode, "Choose existing S3 bucket"},
            { Strings.LabelBootstrapCurrentBucket, "Current S3 bucket"},
            { Strings.LabelBootstrapS3Console, "Go to S3 console"},
            { Strings.LabelCredentialsCreateButton, "Create Credentials Profile"},
            { Strings.LabelCredentialsCreateMode, "Create new credentials profile"},
            { Strings.LabelCredentialsUpdateButton, "Update Credentials Profile"},
            { Strings.LabelCredentialsUpdateMode, "Choose existing credentials profile"},
            { Strings.LabelCredentialsHelp, "How do I create AWS credentials?"},
            { Strings.LabelCredentialsCreateProfileName, "New Profile Name"},
            { Strings.LabelCredentialsCurrentProfileName, "Current Profile"},
            { Strings.LabelCredentialsSelectProfileName, "Profile Name"},
            { Strings.LabelCredentialsAccessKey, "AWS Access Key ID"},
            { Strings.LabelCredentialsRegion, "AWS Region"},
            { Strings.LabelCredentialsSecretKey, "AWS Secret Key"},
            { Strings.LabelDefaultFolderName, "New"},
            { Strings.LabelDeploymentApiGateway, "API Gateway Endpoint"},
            { Strings.LabelDeploymentBootstrapWarning, "You must configure your AWS credentials and a bootstrapping location before deploying a scenario.."},
            { Strings.LabelDeploymentBucket, "S3 Bucket"},
            { Strings.LabelDeploymentBuildFilePath, "Game Server Executable Path"},
            { Strings.LabelDeploymentBuildFolderPath, "Game Server Build Folder Path"},
            { Strings.LabelDeploymentCancelDialogBody, "Are you sure you want to cancel?"},
            { Strings.LabelDeploymentCancelDialogCancelButton, "No"},
            { Strings.LabelDeploymentCancelDialogOkButton, "Yes"},
            { Strings.LabelDeploymentCloudFormationConsole, "View AWS CloudFormation Console"},
            { Strings.LabelDeploymentCosts, "Deploying and running various AWS resources in the CloudFormation template will incur costs to your account. See AWS Pricing Plan (https://aws.amazon.com/pricing/) for details on the cost of each resource in the CloudFormation template."},
            { Strings.LabelDeploymentCognitoClientId, "Cognito Client ID"},
            { Strings.LabelDeploymentCustomMode, "Custom scenario"},
            { Strings.LabelDeploymentHelp, "How to deploy your first game?"},
            { Strings.LabelDeploymentGameName, "Game Name"},
            { Strings.LabelDeploymentCurrentStack, "Deployment Status"},
            { Strings.LabelDeploymentRegion, "AWS Region"},
            { Strings.LabelDeploymentScenarioHelp, "Learn more"},
            { Strings.LabelDeploymentScenarioPath, "Scenario Path"},
            { Strings.LabelDeploymentSelectionMode, "Scenario"},
            { Strings.LabelDeploymentStartButton, "Start Deployment"},
            { Strings.LabelDeploymentCancelButton, "Cancel Deployment"},
            { Strings.LabelSettingsAwsCredentialsSetUpButton, "AWS Credentials"},
            { Strings.LabelAwsCredentialsUpdateButton, "Update AWS Credentials"},
            { Strings.LabelPasswordHide, "Hide"},
            { Strings.LabelPasswordShow, "Show"},
            { Strings.LabelSettingsAllConfigured, "The GameLift Plugin is configured. You can build the sample game client and server and deploy a sample deployment scenario."},
            { Strings.LabelSettingsAwsCredentialsTitle, "AWS Credentials"},
            { Strings.LabelSettingsBootstrapTitle, "AWS Account Bootstrapping"},
            { Strings.LabelSettingsBootstrapButton, "Update Account Bootstrap"},
            { Strings.LabelSettingsBootstrapWarning, "To bootstrap your AWS account, configure your  AWS credentials"},
            { Strings.LabelSettingsConfigured, "Configured"},
            { Strings.LabelSettingsNotConfigured, "Not Configured"},
            { Strings.LabelSettingsDotNetButton, "Use .NET 4.x"},
            { Strings.LabelSettingsDotNetTitle, ".NET Settings"},
            { Strings.LabelSettingsJavaButton, "Download JRE"},
            { Strings.LabelSettingsJavaTitle, "JRE"},
            { Strings.LabelSettingsSdkTab, "SDK"},
            { Strings.LabelSettingsDeployTab, "Deploy"},
            { Strings.LabelSettingsTestTab, "Test"},
            { Strings.LabelSettingsHelpTab, "Help"},
            { Strings.LabelSettingsOpenAwsHelp, "Open AWS documentation"},
            { Strings.LabelSettingsOpenForums, "Open AWS GameTech Forums"},
            { Strings.LabelSettingsOpenDeployment, "Open Deployment UI"},
            { Strings.LabelSettingsPingSdk, "Show SDK DLL Files"},
            { Strings.LabelSettingsReportSecurity, "Report security vulnerabilities"},
            { Strings.LabelSettingsReportBugs, "Report bugs/issues"},
            { Strings.LabelStackUpdateCancelButton, "Cancel"},
            { Strings.LabelStackUpdateCountTemplate, "Resources with action {0}: {1}"},
            { Strings.LabelStackUpdateConfirmButton, "Deploy"},
            { Strings.LabelStackUpdateConsole, "View the changeset in CloudFormation console"},
            { Strings.LabelStackUpdateConsoleWarning, "Do not \"Execute\" the changeset when previewing it in the AWS CloudFormation console."},
            { Strings.LabelStackUpdateQuestion, "Do you want to redeploy your current stack?"},
            { Strings.LabelStackUpdateRemovalHeader, "The following resources will be removed during deployment:"},
            { Strings.LifecycleNone, "No Lifecycle Policy"},
            { Strings.LifecycleSevenDays, "7 days"},
            { Strings.LifecycleThirtyDays, "30 days"},
            { Strings.StatusDeploymentExePathInvalid, "There was a problem with deployment. The build exe is not in the build folder."},
            { Strings.StatusDeploymentFailure, "There was a problem with deployment. Error: {0}."},
            { Strings.StatusDeploymentStarting, "Preparing to deploy the scenario..."},
            { Strings.StatusDeploymentSuccess, "Deployment success."},
            { Strings.StatusExceptionThrown, "An exception was thrown. See the exception details in the Console."},
            { Strings.StatusNothingDeployed, "Nothing is deployed. Click on the «Deploy» button below."},
            { Strings.StatusProfileCreated, "Profile Created. Please bootstrap the account"},
            { Strings.StatusProfileUpdated, "Credentials Updated. Please bootstrap account if necessary"},
            { Strings.StatusBootstrapComplete, "Account bootstrap S3 bucket created!"},
            { Strings.StatusBootstrapUpdateComplete, "Account bootstrapped."},
            { Strings.StatusBootstrapFailedTemplate, "{0} {1}"},
            { Strings.StatusGetProfileFailed, "There was a problem getting the current AWS profile."},
            { Strings.StatusGetRegionFailed, "There was a problem getting the current AWS Region."},
            { Strings.StatusRegionUpdateFailedWithError, "There was a problem updating the AWS Region: {0}."},
            { Strings.TitleAwsCredentials, "AWS Credentials"},
            { Strings.TitleBootstrap, "Account Bootstrapping"},
            { Strings.TitleDeployment, "Deployment"},
            { Strings.TitleDeploymentCancelDialog, "Cancel Current Deployment?"},
            { Strings.TitleDeploymentServerFileDialog, "Select a server build executable file"},
            { Strings.TitleDeploymentServerFolderDialog, "Select a server build folder"},
            { Strings.TitleSettings, "GameLift Plugin Settings"},
            { Strings.TitleStackUpdateDialog, "Review Deployment"},
            { Strings.TooltipCredentialsAccessKey, "Enter the key ID."},
            { Strings.TooltipCredentialsCreateProfile, "Enter your AWS account name."},
            { Strings.TooltipCredentialsRegion, "Select the AWS Region to create the S3 bucket."},
            { Strings.TooltipCredentialsSecretKey, "Enter the secret key."},
            { Strings.TooltipCredentialsSelectProfile, "Select the AWS account name."},
            { Strings.TooltipDeploymentBucket, "You can change this parameter in the Plugin Settings > Update Account Bootstrap menu if needed"},
            { Strings.TooltipDeploymentRegion, "You can change this parameter in the Plugin Settings > Update AWS Credentials menu if needed"},
            { Strings.TooltipSettingsAwsCredentials, "Setup credentials used for account bootstrapping and CloudFormation deployment."},
            { Strings.TooltipSettingsBootstrap, "Create an S3 bucket to store GameLift build artifacts and lambda function source code."},
            { Strings.TooltipSettingsDotNet, "Opt in to change \"API Compatibility Level\" of the project to 4.x, which is the latest version of .NET Framework API that the current GameLift Server SDK supports."},
            { Strings.TooltipSettingsJava, "Download and install the latest Java Runtime Environment (JRE) in order to run GameLift Local.\nNOTE: If you have JDK installed, this may show as \"Not Configured\", but local testing will work as long as you have \"java\" in your PATH environment variable"},
            { Strings.TooltipBootstrapBucketLifecycle, "Specify an expiration date of your S3 bucket."},
            { Strings.TooltipBootstrapCurrentBucket, "This is the name of the S3 bucket which is used currently."},
            { Strings.StackDetailsTemplate, "Scenario: {1}\r\nGame Name: {2}\r\nAWS Region: {0}\r\nLast Updated: {3}"},
            { Strings.StackStatusTemplate, "Deployment Status: {0}"},
            { Strings.SettingsUIDeployNextStepLabel, "The Deployment settings are configured. You can continue by building the sample game server then deploying a sample deployment scenario."},
            { Strings.SettingsUITestNextStepLabel, "The Test settings are configured. You can continue by building the sample game server, and the sample client with local testing configurations, then connect the client to the local server."},
            { Strings.SettingsUISdkNextStepLabel, "The SDK settings are configured. You can find an usage example by going to \"(Top Menu Bar) > GameLift > Import Sample Game\", and look at \"Assets\\Scripts\\Server\\GameLiftServer.cs\"."},
            { Strings.LabelOpenSdkIntegrationDoc, "Open SDK Integration Guide"},
            { Strings.LabelOpenSdkApiDoc, "Open SDK API Reference"},
            
            { Strings.TabLanding, "Amazon GameLift" },
            { Strings.TabCredentials, "AWS User Profiles" },
            { Strings.TabAnywhere, "Host with Anywhere" },
            { Strings.TabManagedEC2, "Host with Managed EC2" },
            { Strings.TabHelp, "Learning Resources" },
            
            { Strings.LandingPageHeader, "Amazon GameLift" },
            { Strings.LandingPageDescription, "Amazon GameLift provides solutions for hosting session-based multiplayer game servers in the cloud. This plugin contains libraries and native UI elements that make it easier to integrate Amazon GameLift into your game and to manage your hosting resources. Use the plugin to access the Amazon GameLift APIs and deploy AWS CloudFormation templates for common deployment scenarios. \nBuilt on AWS global computing infrastructure, Amazon GameLift helps you deliver high-performance, high-reliability, low-cost game servers that scale to meet player demand." },
            { Strings.LandingPageNoAccountCardText, "I need a new AWS account for this project" },
            { Strings.LandingPageNoAccountCardButton, "Create an AWS Account" },
            { Strings.LandingPageAccountCardText, "I have an AWS account for this project" },
            { Strings.LandingPageAccountCardButton, "Create a User Profile" },
            { Strings.LandingPageSampleHeader, "Try our Sample Game" },
            { Strings.LandingPageSampleDescription, "Explore Amazon GameLift with our sample multiplayer game. View integration code, set up hosting with Amazon GameLift Anywhere or Managed EC2 fleets, and experiment with hosting features. Import the sample game into your project, and look for it in the project Assets." },
            { Strings.LandingPageSampleButton, "Import Sample Game" },
            
            { Strings.AnywherePageTitle, "Host with Amazon GameLift Anywhere"},
            { Strings.AnywherePageDescription, "Set up an Amazon GameLift Anywhere fleet to host game servers using your own hardware. With an Anywhere fleet, Amazon GameLift manages game sessions and placement (including matchmaking), while you control your own server hosting infrastructure under a single managed solution.\nCreate an Anywhere fleet for your on-premises or other compute resources. During game development, turn your local workstation into an Anywhere fleet to continuously deploy, test, and iterate your game builds."},
            { Strings.AnywherePageIntegrateTitle, "Integrate Amazon GameLift With Your Game Project"},            
            { Strings.AnywherePageIntegrateDescription, "Use the provided C# libraries to add Amazon GameLift functionality to your game to enable hosting. Working with the sample game? Integration is already done!"},
            { Strings.AnywherePageIntegrateServerLink, "Integrate game server functionality with the Amazon GameLift server SDK"},
            { Strings.AnywherePageIntegrateClientLink, "Integrate game client functionality with the AWS SDK"},
            { Strings.AnywherePageCreateFleetTitle, "Connect to an Anywhere Fleet"},
            { Strings.AnywherePageCreateFleetHint, "Fleet Name must have 1–1024 characters. Valid characters are A-Z, a-z, 0-9, _ and - (hyphen)"},
            { Strings.AnywherePageCreateFleetButton, "Create New Anywhere Fleet"},
            { Strings.AnywherePageCreateFleetCancelButton, "Cancel"},
            { Strings.AnywherePageConnectFleetTitle, "Integrate Amazon GameLift With Your Game Project"},
            { Strings.AnywherePageCreateFleetName, "Fleet Name"},
            { Strings.AnywherePageConnectFleetLabel, "Fleet Name"},
            { Strings.AnywherePageConnectFleetIDLabel, "Fleet ID"},
            { Strings.AnywherePageConnectFleetStatusLabel, "Fleet status"},
            { Strings.AnywherePageConnectFleetNewButton, "Create New Anywhere Fleet"},
            { Strings.AnywherePageComputeTitle, "Add Your Compute to the Fleet"},
            { Strings.AnywherePageComputeNameLabel, "Compute name"},
            { Strings.AnywherePageComputeIPLabel, "IP address"},
            { Strings.AnywherePageComputeStatusLabel, "Status"},
            { Strings.AnywherePageComputeRegisterButton, "Register Compute"},
            { Strings.AnywherePageComputeRegisterNewButton, "Register new Compute"},
            { Strings.AnywherePageComputeCancelButton, "Cancel"},
            { Strings.AnywherePageAuthTokenTitle, "Generate Auth Token - optional"},
            { Strings.AnywherePageAuthTokenLabel, "Status"},
            { Strings.AnywherePageAuthTokenNote, "The auth token is generated when you launch your game."},
            { Strings.AnywherePageLaunchClientTitle, "Launch Client"},
            { Strings.AnywherePageLaunchClientLabel, "Run Game"},
            { Strings.AnywherePageLaunchClientButton, "Launch Client"},
            
            { Strings.InfoLinkDocumentationLink, "Documentation" },
            { Strings.InfoLinkForumLink, "AWS GameTech Forum" },
            { Strings.InfoLinkTroubleshootingLink, "Troubleshooting" },
            { Strings.InfoLinkReportIssuesLink, "Report Issues" },
            
            { Strings.HelpPageTitle, "Help & Documentation" },
            { Strings.HelpPageDescription, "Learn how to integrate and deploy games for hosting on GameLift. The GameLift service manages game server deployment, operation, and scaling. You can deploy custom-built game servers or use GameLift Realtime Servers to configure lightweight game servers for your game." },
            { Strings.HelpPageReportIssueLink, "Report Issues" },
            { Strings.HelpPageDocumentationLink, "Documentation" },
            { Strings.HelpPageVideoTutorialLink, "Video Tutorials" },
            { Strings.HelpPageForumLink, "AWS Forum" },
            { Strings.HelpPageEstimatingPriceTitle, "Estimating Price" },
            { Strings.HelpPageEstimatingPriceDescription, "With AWS Pricing Calculator, you can create a pricing estimate for Amazon GameLift. You don't need an AWS account or in-depth knowledge of AWS to use the calculator." },
            { Strings.HelpPageEstimatingPriceLink, "Learn More" },
            { Strings.HelpPageFleetIQTitle, "Amazon GameLift FleetIQ" },
            { Strings.HelpPageFleetIQDescription, "Amazon GameLift FleetIQ optimizes the use of low-cost Amazon EC2 Spot Instances for cloud-based game hosting. With Amazon GameLift FleetIQ, you can work directly with your hosting resources in Amazon EC2 and Amazon EC2 Auto Scaling while taking advantage of Amazon GameLift optimizations to deliver inexpensive, resilient game hosting for your players." },
            { Strings.HelpPageFleetIQLink, "Learn More" },
            { Strings.HelpPageFlexMatchTitle, "Amazon GameLift FlexMatch" },
            { Strings.HelpPageFlexMatchDescription, "Amazon GameLift FlexMatch is a customizable matchmaking service for multiplayer games. With FlexMatch, you can build a custom set of rules that defines what a multiplayer match looks like for your game, and determines how to evaluate and select compatible players for each match." },
            { Strings.HelpPageFlexMatchLink, "Learn More" },
        };

        public string GetError(string errorCode = null)
        {
            if (errorCode is null)
            {
                return "Unknown error";
            }

            if (_errorMessagesByCode.TryGetValue(errorCode, out string message))
            {
                return message;
            }

            return errorCode;
        }

        /// <exception cref="ArgumentNullException"></exception>
        public string Get(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_textsByKey.TryGetValue(key, out string text))
            {
                return text;
            }

            return key;
        }
    }
}
