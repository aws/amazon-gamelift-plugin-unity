# GameLift Plugin For Unity Sample Client/Server

## Summary

This sample code shows how to set up a basic GameLift server and client for games using the Unity Game Engine. It
consists of a Unity project that has been configured to be built in two ways, as a SERVER or a CLIENT. In CLIENT
configuration, no preprocessor symbols are needed. In SERVER configuration, the UNITY_SERVER symbol needs to be defined
in Unity Project Settings.

The code can be viewed in the C# scripts in the `Assets\Scripts` folder. Client-only code is under Client; server-only
code is under server, and everything else needs to be shared between them.

The server and can be deployed to GameLift and other infrastructire can be created with the plugin UI. One or more
clients can be run on your local machine and they can connect to the server to join a game. When some players have
arrived the game can be started.

The idea of the game is to use the numeric keypad keys to match up pairs, triplets, quadruplets etc. matching colored
dots on a three by three board. Accordingly the middle dot is 5, the left dot on the middle row is 4, both of the blue
dots. Pressing the numeric keypad keys 4 and 5 simultaneously makes a move and matches the two blue dots for a score of
two. Alternatively match the purple dots by pressing 1, 3 and 8 for a score of three. Matching four and upward dots in
one move is possible, however note that due to the way that keyboards are wired, certain combinations may not be
possible on some hardware.

## How to Play

1. When first start the game client, sign up and log in. If you are using local mode, i.e. the endpoint
   is `http://localhost`, you can enter any values in username and password to login
1. Press "ENTER" to get ready. There needs to be at least 2 players to be ready for the game to start.
1. Use NUMPAD keys to match colored dots

## Setting up the game client

The client will need the AWS Region, Cognito Client ID and API Gateway Endpoint. They can be set in Unity, the
scriptable object is at `Assets\Settings\GameLiftClientSettings`. When the scenario is deployed, its outputs are also
displayed in the Deployment window. Copy them out of the window and paste in the GameLiftClientSettings. Anyone playing
against you on a different machine should use the same settings to join your server, so you could distribute the client,
if you wanted that.

Make sure that the scenes are in the "Scenes In Build" list in the File/Build Settings window. For running the smaple in
Unity editor, start the BootstrapScene scene.

## Building the project

1. To build client, just open the File/Build Settings window, make sure you have "Server Build" unchecked, and press "
   Build". Select a client build folder.
1. To build server, in the File/Build Settings window, press "Player Settings", and add a UNITY_SERVER to Scripting
   Define Symbols. Restart Unity. Open the File/Build Settings window, make sure you have "Server Build" checked, and
   press "Build". Select a server build folder.

You can also use "GameLift / Configure Sample Server" and "GameLift / Configure Sample Client" in Unity main menu.

## How to deploy a server to GameLift

Deploy a scenario to AWS from Unity with the plugin's GameLift/Deployment window. The fleet has to be in the active
state before you can connect to it. You can check it at https://console.aws.amazon.com/gamelift/home (make sure you
select your region on the page).

## Understandng and using the Client

The client operates in three modes depending on what you are doing for a server.

1. The game client will try to locate a local server, and if not found, attempt to find a GameLift server (with the
   default alias or one supplied on the command line. If a server cannot be reached, the client will go into a
   serverless standalone mode.

1. It will never again try to look for a server. This is by design. Just kill the client and start another if you want
   another mode.

"Hello World" appears in each client, and the clients enter a synchronized 'attract' mode (a color change animation),
driven by the server.

1. Press RETURN inside each client window to start. When all clients have pressed return, the game begins. GO! appears
   in each client.

1. You should be able to press numeric keypad keys in one client or another to make matches and increase your score. All
   clients should see the matches being made, and everyone’s scores. If you have a laptop, some function key combination
   may be required to activate numeric keypad keys. In rare cases, no numeric keypad maybe available. Keys Y U I, H J K,
   N M comma may be used instead. The game will score matches until the ESC key is pressed.

### Log Output

You should find your logs in the folder:

`%UserProfile%\AppData\LocalLow\DefaultCompany\amazon-gamelift-unity\output_log.txt`

where `%UserProfile%` is usually `C:\Users\`\<your username\>

If you have multiple clients and servers running locally by the way, they are all writing to the same file making the
logs messy and illegible. If you want to change the target log file, run the client or the server from command line
with "-logFile <file path>" option. It will be helpful in the GameLift fleet to have your server log in `C:\game\`. The
server is set up to upload its logs over 1 hour old to a more permanent storage on S3. To access them there, call the
GetGameSessionLog API: https://docs.aws.amazon.com/cli/latest/reference/gamelift/get-game-session-log.html

### Client LOCAL and GAMELIFT modes

Local mode uses a server running on the local machine. Clients started on the local machine will detect the local server
and try to connect to it. Four player multiplayer is possible, and the server and all four clients must be run locally.
GameLift Local is required for local mode. You can use any email and password, there is no check.

GameLift mode uses an internet connection for authentication and server connection, a GameLift fleet running in an AWS
account to provide server functionality, and no server must be run locally. To use GameLift mode, ensure that the
connection is available, and then run only the client on the local machine. Four player multiplayer is possible, across
the Internet (depending on the scenario used).

#### How to use in LOCAL mode with GameLift Local

Local mode uses a server running on the local machine, and the client will use GameLift Local to find the server.

1. In Unity project, you should see the Plugin Settings window.

1. Press "Download JRE" and install the Java 8 Runtime.

1. Press "Download GameLift Local" and unpack the GameLift Local JAR from inside the GameLift Managed Servers SDK
   package.

1. Set the path of GameLiftLocal.jar in the plugin UI.

1. Open the GameLift / Local Testing window in Unity.

1. Start the GameLift Local and the server by pressing `Deploy & Run`.

1. In Unity editor, in the scriptable object `Assets\Settings\GameLiftClientSettings`, set API Gateway Endpoint
   to `http://localhost:<GameLift Local port>`.

1. Build the local client. Run one to four clients. The clients should work as usual but connect to the local game
   server.

Clients will report `CLIENT | LOCAL | CONNECTED 1UP`,  `CLIENT | LOCAL | CONNECTED 2UP`, etc.

#### How to use in GAMELIFT mode

1. To use the GameLift server in GameLift mode, simply run the clients without a local server running, and the game will
   try to connect. Clients can be on different machines and play together. NOTE, the corporate network (or VPN
   connection to the corporate network) can disrupt the client’s ability to reach the GameLift fleet, due to blocked
   ports. Port permissions must be set on the fleet to allow access on port 33430. (NB other ports can be specified to
   the server with -port 33430 or other values in [33430-33440] range if needed; See the section for setting up your own
   GameLift server below.)

Clients will report `CLIENT | GAMELIFT | CONNECTED 1UP`,  `CLIENT | GAMELIFT | CONNECTED 2UP`, etc.

## Running Tests

1. Unit tests can be found at **GameLift-SampleGame\Assets\Tests\Editor\Unit**, or in the **SampleTests.Unit project**
   in your IDE. They can be run from the Unity Editor: **Window > General > Test Runner, EditMode**.
1. Play mode integration UI tests can be found at **GameLift-SampleGame\Assets\Tests\UI**, or in the **SampleTests.UI
   project** in your IDE. They can be run from the Unity Editor: **Window > General > Test Runner, PlayMode**. These
   tests need **GameLift-SampleGame\UiTestSettings.json** filled with your test parameters.
