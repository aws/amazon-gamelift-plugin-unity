# Amazon GameLift Plug-in for Unity

![GitHub](https://img.shields.io/github/license/aws/amazon-gamelift-plugin-unity)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/aws/amazon-gamelift-plugin-unity)
![GitHub all releases](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/total)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/latest/total)

## Overview

Amazon GameLift provides tools for preparing your multiplayer games and custom game servers to run on the GameLift
service. The GameLift SDKs contain libraries needed to enable game clients and servers to communicate with the GameLift
service. The Amazon GameLift Plug-in for Unity makes it easier to access GameLift resources and integrate GameLift into
your Unity game. You can use the Plug-in for Unity to access GameLift APIs and deploy AWS CloudFormation templates for
common gaming scenarios. Pre-built sample scenarios include:

* Auth Only — This scenario creates a game backend service that performs only player authentication and no game server
  capability. It creates a Cognito user pool to store player authentication information, as well as an API gateway REST
  endpoint backed up AWS Lambda handlers to start a game and view game connection information. The Lambda handler always
  returns a 501 Error (Unimplemented).

* Single-Region Fleet — This scenario creates a game backend service with a single GameLift fleet. After player
  authenticates and starts a game (with a `POST` request to `/start_game`), a AWS Lambda handler searches for an
  existing viable game session with an open player slot on the fleet via `gamelift::SearchGameSession`. If an open slot
  is not found, the Lambda creates a new game session via `gamelift::CreateGameSession`. Once game start is request, the
  game client should poll the back end (with `POST` requests to `/get_game_connection`) to receive a viable game
  session.

* Multi-Region Fleets with Queue and Custom Matchmaker — In this scenario, Amazon GameLift queues are used in
  conjunction with a custom matchmaker. The custom matchmaker forms matches by grouping up the oldest players in the
  waiting pool. The customer matchmaker does not consider other factors like skills or latency. When there are enough
  players for a match, the Lambda calls `GameLift:StartGameSessionPlacement` to start a queue placement. Once the
  placement is done, GameLift publishes a message to the SNS topic in the backend service, which triggers a Lambda
  function to store the placement details along with the game conection details to a DynamoDB table. Subquent
  GetGameConnection calls would read from this table and return the connection information to the game client.

* SPOT Fleets with Queue and Custom Matchmaker — This This scenario is the same as Multi-Region Fleets with Queue and
  Custom Matchmaker except it configures three fleets. Two of the fleets are SPOT fleets containing nuanced instance
  types to provide durability for SPOT unavilabilities; the third fleet is an ON_DEMAND fleet to serve as a backup in
  case the other SPOT fleets go unviable. Using a GameLift queue can keep availability high and cost low. For more
  information and best practices about queues,
  see [Setting up GameLift queues for game session placement](https://docs.aws.amazon.com/gamelift/latest/developerguide/queues-intro.html)
  .

* FlexMatch — This scenario uses GameLift FlexMatch, customizable matchmaking service for multiplayer games. On
  StartGame requests, a Lambda creates matchmaking ticket via `gamelift:StartMatchmaking`, and a separate lambda listen
  to FlexMatch events similar to the queue example above. This deployment scenario also uses a low frequency poller to
  describe incomplete tickets via `gamelift::DescribeMatchmaking`. The incomplete tickets are periodically described so
  they are not discarded by GameLift. This is a best practice recommended
  by [Track Matchmaking Events](https://docs.aws.amazon.com/gamelift/latest/flexmatchguide/match-client.html#match-client-track)
  . For more information on FlexMatch,
  see [What is GameLift FlexMatch?](https://docs.aws.amazon.com/gamelift/latest/flexmatchguide/match-intro.html)

Each sample scenario uses an AWS CloudFormation template to create a stack with all of the resources needed for the
sample game. You can remove the resources by deleting the corresponding AWS CloudFormation stack.

The Amazon GameLift Plug-in for Unity also includes a sample game you can use to explore the basics of integrating your
game with Amazon GameLift.

For more information about Amazon GameLift, see  [Amazon GameLift](https://docs.aws.amazon.com/gamelift). For more
information about the Amazon GameLift Plug-in for Unity,
see [Integrating Games with the Amazon GameLift Plug-in for Unity](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html)
.

## Installing the Amazon GameLift Plug-in for Unity

1. Download the Amazon GameLift Plug-in for Unity. You can find the latest version on the **Amazon GameLift Plug-in for
   Unity** GitHub repository. Under the [latest release](https://github.com/aws/amazon-gamelift-plugin-unity/releases),
   select Assets and download the `com.amazonaws.gamelift-<version>.tgz` file.
2. Launch Unity and select a project.
3. On the top menu bar, select **Window**, and then choose **Package Manager**.
4. In the **Package Manager** tab, under the tab, select **+**, and then choose **Add package from tarball...**.
5. In the **Select packages on disk** window, select the file **com.amazonaws.gamelift-1.0.0.tgz**, and then choose
   Open.
6. Once the plug-in is loaded, **GameLift** will be added as a new option on the Unity top menu bar. It may take a few
   minutes to install and recompile scripts. The **GameLift Plug-in Settings** tab will automatically open once the
   installation and compilation complete.
7. Go to **GameLift Plug-in Settings > SDK** tab, choose **Use .NET 4.x**. This will override the current API
   compatibility level to 4.x for the Unity project Player Settings. This is required because GameLift Server SDK only
   supports .NET 4.5 currently.

## Setting up for Local Testing

1. In Unity, on the **GameLift Plug-in Settings**, select **Test** tab, then **Download GameLift Local**. This will
   automatically open your web browser and ask where you would like the testing tools to be
   downloaded (`GameLift_<Date>.zip`, e.g. `GameLift_06_03_2021.zip`). Some browser may be set to not asking for the
   download location, so please check your download directory to locate the zip file that was automatically downloaded.
2. Unzip the file downloaded
3. Go back to **GameLift Plug-in Settings**, select **GameLift Local Path** to configure the filepath of
   the `GameLiftLocal.jar` in the plug-in
4. If you haven't installed JRE, select **Install JRE** to download and install JRE from the official website. If you
   already have JRE installed, but the plug-in still shows "Not Configured" for JRE, then
   follow [this guide](https://www.java.com/en/download/help/path.html) to add the JRE `bin` directory to your
   `PATH` system environment variable.
5. Open **Local Testing UI**
6. Set the path to your GameLift SDK integrated server executable (If you don't have one, see the below section to build
   a sample game server)
   * For game servers built against the Windows platform, this executable should be a `.exe` file
   * For game servers built against the Mac OS platform, this executable should be a `.app` file 
7. Select **Deploy and Run**
    * This will automatically execute GameLift Local (via `java -jar <path_to_gamelift_local_jar> -p <port>`), and then
      after 10 seconds, execute the game server executable
8. If the GameLift Server SDK is configured correctly in your server executable, you should
   see `Healthcheck status: true` in GameLift Local terminal

## Importing and running the sample game locally

1. In Unity, on the top menu bar, select **GameLift**, and then choose **Import Sample Game**.
2. In the **Import Sample Game** window, choose **Import** to import the game and all of its assets and dependencies.
3. In Unity, on the menu, select **GameLift**, and then choose **Apply Windows Sample Server Build Settings**
   or **Apply MacOS Sample Server Build Settings**. After the game server settings are configured, Unity will recompile
   assets.
4. In Unity, on the menu, select **File**, and then choose **Build Settings...**, confirm Server Build is checked,
   choose **Build**, and then select a build folder.
5. Unity will build the sample game server, placing the executable and required assets in the specified build folder.
6. Close the build window.
7. In Unity, on the menu, select **GameLift**, and then choose **Apply Windows Sample Client Build Settings**
   or **Apply MacOS Sample Server Build Settings**. After the game client settings are configured, Unity will recompile
   assets.
8. In Unity, on the menu, select **Go To Client Settings**. This will display an Inspector tab on the right side of the
   Unity screen. In the **GameLift Client Settings** tab, choose **Local Testing Mode**.
9. Build the game client. In Unity, on the menu, select **File**, and then choose **Build Settings...**, confirm **
   Server Build** is not checked, choose **Build**, and then select a build folder (This build folder should be
   different from the Server build folder).
10. Unity will build the sample game client, placing the executable and required assets in the specified build folder.
11. Close the build window.
12. The game server and client are built. In the next few steps, you run the game and see how it interacts with
    GameLift.
13. In Unity, in the Plug-in for Unity tab, select the **Test** tab.
14. In the Test pane, select **Open Local Test UI**.
15. In the Local Testing window, specify a **Game Server .exe/.app File Path**. The path must include the executable name.
    For example, `C:/MyGame/GameServer/MyGameServer.exe`.
16. Select **Deploy and Run**. The Plug-in for Unity will launch the game server and open a GameLift Local log window.
    The windows will contain log messages including messages sent between the game server and GameLift Local.
17. Launch the game client. You can find it in the build location you specified when building the sample game client. To
    test multiple player interactivity, you should launch at least 2 game clients.
18. In the GameLift Sample Game, provide an email and password and then select Log In. The email and password are not
    validated and are not used in Local Testing mode.
19. In the GameLift Sample Game, choose **Start**. The game client will look for a game session. If one cannot be found,
    it will create a game session. The game client then starts the game session. You can see game activity in the logs.
20. Press `Enter` on all game clients to notify the server that the player is ready.
21. Once the game starts, press NUMPAD keys to match and clear dots off the grid.
22. In the game client, choose Quit or close the window to stop the game client.
23. In Unity, in the Local Testing window, choose **Stop** or close the game server windows to stop the server.

## Deploying a Scenario

### First, update your credentials and account bootstrap location:

1. Update your credentials. In Unity, in the Plug-in for Unity tab, select the **Deploy** tab, and then create new
   credentials or select existing credentials.
2. Update the account bootstrap location. In the **Deploy** pane, select **Update Account Bootstrap**. In the Account
   Bootstrapping window, you can choose an existing Amazon S3 bucket or create a new Amazon S3 bucket. The bootstrap
   location is an Amazon S3 bucket used during deployment. It is used to store game server assets and other
   dependencies. The AWS Region you select for the bucket must be the same Region you will use for the sample scenario
   deployment.

### Next, deploy a sample scenario:

NOTE: Only Windows server executables are supported in the plugin at the moment. On Mac, you'll need to build the game
server using Windows platform and use it for deployment, even though your local testing was done using game server built 
against Mac OS platform.

1. In Unity, in the Plug-in for Unity tab, select the **Deploy** tab.
2. In the Deploy pane, select **Open Deployment UI**.
3. In the Deployment window, select a scenario. The **Auth Only** scenario does not require a server executable and can
   deploy quickly. All other scenarios require a server path and server executable and can take about 30 minutes to
   deploy.
4. Specify a **Game Name**. It must be unique. It will be used as part of the AWS CloudFormation stack name when the
   scenario is deployed. For example, if you specify **MySampleGame**, the corresponding AWS CloudFormation stack will
   be named **GameLiftPluginForUnity-MySampleGame**.
5. Select the **Game Server Build Folder Path**. The build folder path points to the folder containing the server
   executable and dependencies. For example, "c:/SampleGame/GameServer". You will not be able to select a build folder
   path if it is not required by the chosen scenario.
6. Select the **Game Server Build .exe File Path**. The build executable file path points to the game server executable.
   For example, "c:/SampleGame/GameServer/SampleGame.exe". You will not be able to select a build executable file path
   if it is not required by the chosen scenario.
7. Select **Start Deployment** to initiate deployment of the scenario. You can follow the status of the update in the
   Deployment window under Current State.
8. When the scenario completes deployment, **Current State** will be updated to include the **Cognito Client ID** and **
   API Gateway Endpoint**.
9. To update sample game settings, on the Unity menu, choose **Go To Client Connection Settings**. This will display an
   Inspector tab on the right side of the Unity screen. Make sure **Local Testing Mode** is not selected.
10. Use the API Gateway endpoint value to specify API Gateway Endpoint and the Amazon Cognito client ID to specify the
    Coginito Client ID. Select the same AWS Region you used for the scenario deployment. You can then rebuild and run
    the sample game client using the deployed scenario resources.

## FAQ

### What Unity versions are supported?

The Amazon GameLift Plug-in for Unity is compatible only with officially supported versions of Unity 2019.4 LTS,
2020.3 LTS, 2021.3 LTS for Windows and Mac OS.

For Unity version 2021.3 and above, "Windows Dedicated Server Build Support" module is required for building the sample
game server for local testing (on Windows development environment) and deploying to GameLift.
If you are developing using MacOS, "Mac Dedicated Server Build Support" module is additionally required for local testing.
Please refer to [Unity documentation](https://docs-multiplayer.unity3d.com/netcode/current/reference/dedicated-server/index.html) 
on how to install the module using Unity Hub.

### Where are the logs?

An additional error log file related to the Unity game project can be found in the following location: **
logs/amazon-gamelift-plugin-logs[YYYYMMDD].txt**. Note, that the log file is created once a day.

### Why did my deployment fail?

If your deployment failed, go to **AWS CloudFormation console > <failed stack>**, and check out event tab to see which
resource failed and why.

If you are using the following deployment scenarios:

- "SPOT Fleets with Queue and Custom Matchmaker"
- "FlexMatch"

You might encounter "GeneralServiceException", this exception means that your fleet creation failed, and this can happen
if your AWS account is limited by GameLift (e.g. new account, invalid payment details, etc.). Please go to **GameLift
AWS console > Service Limits**, and request for limit increase on your account, then work with AWS customer support to
get your account limit lifted.

### My CloudFormation stack failed to create Build resource due to error "The WINDOWS_2012 option for the OperatingSystem parameter is deprecated."

Starting from 04/20/2022, WINDOWS_2012 operating system is no longer available on GameLift. Please either update to GameLift Plugin For Unity 
version 1.3.0 or above, or manually update the `BuildOperatingSystemParameter` in **Editor/Resources/CloudFormation/<YOUR SCENARIO>/parameters.json**
to use value `WINDOWS_2016` instead of `WINDOWS_2012`.

### I need help!

If you are blocked, please don't hesitate to reach out for support via:

* [AWS Support Center](https://console.aws.amazon.com/support/home)
* [GitHub issues](https://github.com/aws/amazon-gamelift-plugin-unity/issues)
* [AWS game tech forums](https://forums.awsgametech.com/)

## For GitHub Contributors

### How to set up the project development environment?

MS Windows OS is required. To build the Plug-in, you need to install some dependencies. This will require administrator
rights on your machine:

* A supported Unity version
    * You also need to add target Unity editor folder (e.g. **C:\Program Files\Unity\Hub\Editor\<version>\Editor\\**) to
      the Windows PATH environment variable.
* Visual Studio 2019 (can be installed with Unity)
* .NET 4.5 Developer Pack to build the Server SDK (NOTE: 4.5.1 or 4.5.2 does not work.) Due
  to [.NET 4.5 reaching end of life](https://dotnet.microsoft.com/download/dotnet-framework/net45). You can only install
  4.5 by following these steps:
    1. Open the Visual Studio Installer application. You should find your Visual Studio installation.
    2. Press Modify on your installation.
    3. Go to the Individual components tab.
    4. Check ".NET 4.5 Framework targeting pack", and press "Modify".
* .NET 4.7.1 Developer Pack to build AmazonGameLiftPlugin.Core. This can be downloaded
  at https://dotnet.microsoft.com/download/visual-studio-sdks, or as a part of MS Visual Studio.
* NodeJS/npm: https://nodejs.org/en/download/

### How to make changes to the plugin?

1. Clone the **amazon-gamelift-plugin-unity** repository from GitHub
1. Run `Scripts~\windows\release.ps1` to build the plugin and dependent libraries (only needed once)
1. In Unity Hub, create a new project
1. Open Unity Package Manager, import project from disk, and select the `package.json` located in the project root
1. Setup code debugging in Unity: https://docs.unity3d.com/Manual/ManagedCodeDebugging.html, and change Unity project to 
   Debug Mode
1. A .sln file should be created in the Unity project root, you can open that with Visual Studio
1. Make changes to the plugin code, and Unity should recompile after each change
1. Once changes are made, run the unit tests via **Window > General > Test Runner**

### How to build and package the plugin?

Run `Scripts~\windows\release.ps1` to clean, build and export the plugin into a tarball with a single command.

Alternatively:
1. Run `Scripts~\windows\clean.ps1` to delete all dlls and temp files (If you want to build faster, you can comment out 
   `.clean-download-files` execution)
1. Run `Scripts~\windows\build.ps1` to build dlls and sample game
1. Run `Scripts~\windows\export.ps1` to export the plugin into a tarball (.tgz) file stored in the project root folder


The redistributable Plug-in files are located at **GameLift-Unity\Assets\com.amazonaws.gamelift**. If you want to update
the sample game, open the GameLift-SampleGame project in Unity and run the main menu command at **Assets > Export
Sample**. If you want to update the custom scenario sample, open the GameLift-Unity project in Unity and run the main
menu command at **Assets > Export Custom Scenario**.

### How to Run Tests

In order to run the plugin tests (after importing the plugin into your project), the package must be enabled for testing. To do this, follow instructions in [Unity Docs](https://docs.unity3d.com/Manual/cus-tests.html#tests) or:

1. Open the Project manifest (located at <project>/Packages/manifest.json)
1. Verify `com.amazonaws.gamelift` is present as a dependency.
1. Add to the bottom of the file:

````
    "testables": [ "com.amazonaws.gamelift" ]
````

After enabling testing, the project tests can be ran via [Unity Test Runner](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html)


## License

This project is licensed under the Apache-2.0 License.
