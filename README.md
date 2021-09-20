# Amazon GameLift Plug-in for Unity

![GitHub release (latest by date)](https://img.shields.io/github/v/release/aws/amazon-gamelift-plugin-unity)
![GitHub](https://img.shields.io/github/license/aws/amazon-gamelift-plugin-unity)
![GitHub all releases](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/total)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/latest/total)

## Overview

Amazon GameLift provides tools for preparing your multiplayer games and custom game servers to run on the GameLift service. The GameLift SDKs contain libraries needed to enable game clients and servers to communicate with the GameLift service. The Amazon GameLift Plug-in for Unity makes it easier to access GameLift resources and integrate GameLift into your Unity game. You can use the Plug-in for Unity to access GameLift APIs and deploy AWS CloudFormation templates for common gaming scenarios. Pre-built sample scenarios include: 

* Auth Only — This scenario creates a game backend service that performs player authentication. It creates a AWS Lambda handler to start a game and view game connection information. The handler always returns a 501 Error (Unimplemented).

* Single-Region Fleet — This scenario creates a game backend service with a single GameLift fleet. After player authenticates and starts a game (with a POST to /start_game), a AWS Lambda handler searches for an existing viable game session with an open player slot on the fleet. If an open slot is not found, the Lambda creates a new game session. The game client should POST to /get_game_connection to receive a viable game session.

* Multi-Region Fleets with Queue and Custom Matchmaker — In this scenario, Amazon GameLift queues are used in conjunction with a custom matchmaker. The custom matchmaker forms matches by taking the oldest players in the waiting pool. It does not consider other factors like skills or latency. When there are enough players for a match, the Lambda calls GameLift:StartGameSessionPlacement to start a queue placement.

* SPOT Fleets with Queue and Custom Matchmaker — This This scenario is the same as Multi-Region Fleets with Queue and Custom Matchmaker except it configures three fleets. Two of the fleets are SPOT fleets containing nuanced instance types. Using a GameLift queue can keep availability high and cost low. This is a best practice. For more information about queues including more best practices, see Setting up GameLift queues for game session placement.

* FlexMatch — This scenario uses GameLift FlexMatch, customizable matchmaking service for multiplayer games. It acquires matchmaking ticket status by listening to FlexMatch events. It also uses a low frequency poller to find incomplete tickets. The incomplete tickets are periodically pinged so they are not discarded by GameLift. This is a best practice. For more information about FlexMatch, see What is GameLift FlexMatch.

Each sample scenario uses an AWS CloudFormation template to create a stack with all of the resources needed for the sample game. You can remove the resources by deleting the corresponding AWS CloudFormation stack.

The Amazon GameLift Plug-in for Unity also includes a sample game you can use to explore the basics of integrating your game with Amazon GameLift. 

For more information about Amazon GameLift, see  [Amazon GameLift](https://docs.aws.amazon.com/gamelift). For more information about the Amazon GameLift Plug-in for Unity, see [Integrating Games with the Amazon GameLift Plug-in for Unity](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html).

## Installing  the Amazon GameLift Plug-in for Unity

1. Download the Amazon GameLift Plug-in for Unity. You can find the latest version on the Amazon GameLift Plug-in for Unity Repository page. Under the latest release, select Assets and select the tar file.
2. Launch Unity and select a project.
3. On the menu, select **Window**, and then choose **Package Manager**.
4. In the **Package Manager** tab, under the tab, select **+**, and then choose **Add package from tarball...**.
5. In the **Select packages on disk** window, navigate to the com.amazonaws.gamelift folder, select the file **com.amazonaws.gamelift-1.0.0.tgz**, and then choose Open.
6. Once the Plug-in for Unity is loaded, GameLift will be added as a new item on the Unity menu. It may take a few minutes to install and recompile scripts. The GameLift Plug-in Settings tab will automatically open.
7. Choose **Use .NET 4.x**. This will override the current .NET settings for the project. GameLift Local requires .NET 4.x. You must select .NET 4.x to use the GameLift Local SDK and to test locally unless have already built your game executable.


## Importing and running the sample game

1. In Unity, on the menu, select **GameLift**, and then choose **Import Sample Game**.
2. In the **Import Sample Game** window, choose **Import** to import the game and all of its assets and dependencies.
3. Build the game server. In Unity, on the menu, select **GameLift**, and then choose **Apply Sample Server Build Settings**. After the game server settings are configured, Unity will recompile assets.
4. In Unity, on the menu, select **File**, and then choose **Build Settings...**, confirm Server Build is checked, choose **Build**, and then select a build folder.
5. Unity will build the sample game server, placing the executable and required assets in the specified build folder.
6. Close the build window.
7. In Unity, on the menu, select **GameLift**, and then choose **Apply Sample Client Build Settings**. After the game client settings are configured, Unity will recompile assets.
8. In Unity, on the menu, select **Go To Client Settings**. This will display an Inspector tab on the right side of the Unity screen. In the **GameLift Client Settings** tab, choose **Local Testing Mode**.
9. Build the game client. In Unity, on the menu, select **File**, and then choose **Build Settings...**, confirm **Server Build** is not checked, choose **Build**, and then select a build folder.
10. Unity will build the sample game client, placing the executable and required assets in the specified build folder.
11. Close the build window.
12. The game server and client are built. In the next few steps, you run the game and see how it interacts with GameLift.
13. In Unity, in the Plug-in for Unity tab, select the **Deploy** tab.
14. In the Test pane, select **Open Local Test UI**.
15. In the Local Testing window, specify a **Game Server .exe File Path**. The path must include the executable name. For example, C:/MyGame/GameServer/MyGameServer.exe.
16. Select **Deploy and Run**. The Plug-in for Unity will launch the game server and open a GameLift Local log window. The windows will contain log messages including messages sent between the game server and GameLift Local.
17. Launch the game client. You can find it in the build location you specified when building the sample game client.
18. In the GameLift Sample Game, provide an email and password and then select Log In. The email and password are not validated and are not used.
19. In the GameLift Sample Game, choose **Start**. The game client will look for a game session. If one cannot be found, it will create a game session. The game client then starts the game session. You can see game activity in the logs.
20. In the game client, choose Quit or close the window to stop the game client.
21. In Unity, in the Local Testing window, choose **Stop** or close the game server windows to stop the server.

## Deploying a Scenario

** First, update your credentials and account bootstrap location: **

1. Update your credentials. In Unity, in the Plug-in for Unity tab, select the **Deploy** tab,
and then create new credentials or select existing credentials. 
2. Update the account bootstrap location. In the **Deploy** pane, select **Update Account Bootstrap**. In the Account Bootstrapping window, you can choose an existing Amazon S3 bucket or create a new Amazon S3 bucket. The bootstrap location is an Amazon S3 bucket used during deployment. It is used to store game server assets and other dependencies. The AWS Region you select for the bucket must be the same Region you will use for the sample scenario deployment.

** Next, deploy a sample scenario: **

1. In Unity, in the Plug-in for Unity tab, select the **Deploy** tab.
2. In the Deploy pane, select **Open Deployment UI**.
3. In the Deployment window, select a scenario. The **Auth Only** scenario does not require a server executable and can deploy quickly. All other scenarios require a server path and server executable and can take more than 30 minutes to deploy.
4. Specify a **Game Name**. It must be unique. It will be used as part of the AWS CloudFormation stack name when the scenario is deployed. For example, if you specify **MySampleGame**, the corresponding AWS CloudFormation stack will be named "GameLiftUnityPlugin-MySampleGame".
5. Select the **Game Server Build Folder Path**. The build folder path points to the folder containing the server executable and dependencies. For example, "c:/SampleGame/GameServer". You will not be able to select a build folder path if it is not required by the chosen scenario.
6. Select the **Game Server Build .exe File Path**. The build executable file path points to the game server executable. For example, "c:/SampleGame/GameServer/SampleGame.exe". You will not be able to select a build executable file path if it is not required by the chosen scenario.
7. Select **Start Deployment** to initiate deployment of the scenario. You can follow the status of the update in the Deployment window under Current State.
8. When the scenario completes deployment, **Current State** will be updated to include the **Cognito Client ID** and **API Gateway Endpoint** you can copy and paste into the sample game.
9. To update sample game settings, on the Unity menu, choose **Go To Client Connection Settings**. This will display an Inspector tab on the right side of the Unity screen. Make sure **Local Testing Mode** is not selected.  
10. Use the API Gateway endpoint value to specify API Gateway Endpoint and the Amazon Cognito client ID to specify the Coginito Client ID. Select the same AWS Region you used for the scenario deployment. You can then rebuild and run the sample game client using the deployed scenario resources.


## FAQ

### What Unity versions are supported?
The Amazon GameLift Plug-in for Unity is compatible only with officially supported versions of Unity 2019.4 LTS and 2020.3 LTS for Windows.


### Where are the logs?
An additional error log file related to the Unity game project can be found in the following location: **logs/amazon-gamelift-plugin-logs[YYYYMMDD].txt**. Note, that the log file is created once a day.

## For contributors

### How to set up the project development environment?

MS Windows OS is required. To build the Plug-in, you need to install some dependencies. This will require administrator rights on your machine:
* the .NET SDK https://docs.microsoft.com/en-us/dotnet/core/sdk
* the .NET 4.5 Developer Pack to build the Server SDK
* the .NET 4.7.1 Developer Pack to build AmazonGameLiftPlugin.Core
* and a supported Unity version.
  
This will require administrator rights on your machine.

You also need to add target Unity editor folder (e.g. **C:\Program Files\Unity\Hub\Editor\<version>\Editor\\**) to the Windows PATH environment variable.

Currently, the .NET 4.5 Developer Pack can only be installed as a part of MS Visual Studio. You can obtain Visual Studio at https://visualstudio.microsoft.com/vs/compare/.
When you have Visual Studio 2019 installed:
1. Open the Visual Studio Installer application. You should find your Visual Studio installation.
2. Press Modify on your installation.
3. Go to the Individual components tab.
4. Check ".NET 4.5 Framework targeting pack", and press "Modify".

.NET 4.7.1 Developer Pack can be downloaded at https://dotnet.microsoft.com/download/visual-studio-sdks, or as a part of MS Visual Studio.

The redistributable Plug-in files are located at **GameLift-Unity\Assets\com.amazonaws.gamelift**.
If you want to update the sample game, open the GameLift-SampleGame project in Unity and run the main menu command at **Assets > Export Sample**.
If you want to update the custom scenario sample, open the GameLift-Unity project in Unity and run the main menu command at **Assets > Export Custom Scenario**.

### How to run unit and integration tests?

There is a list of the test types by their location. 

* **AmazonGameLiftPlugin.Core**:
	* Unit tests can be found in the **AmazonGameLiftPlugin.Core.Tests project**. They can be run from your IDE. 
* **GameLift-Unity (plugin)**: 
	* Unit tests can be found at **GameLift-Unity\Assets\com.amazonaws.gamelift\Tests\Editor\Unit**, or in the **AmazonGameLiftPlugin.Editor.UnitTests project** in your IDE. They can be run from the Unity Editor:  **Window > General > Test Runner**. 
* **GameLift-SampleGame**:
	* Unit tests can be found at **GameLift-SampleGame\Assets\Tests\Editor\Unit**, or in the **SampleTests.Unit project** in your IDE. They can be run from the Unity Editor: **Window > General > Test Runner, EditMode**. 
	* Play mode integration UI tests can be found at **GameLift-SampleGame\Assets\Tests\UI**, or in the **SampleTests.UI project** in your IDE. They can be run from the Unity Editor: **Window > General > Test Runner, PlayMode**. These tests need **GameLift-SampleGame\UiTestSettings.json** filled with your test parameters.
	
## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This project is licensed under the Apache-2.0 License.
