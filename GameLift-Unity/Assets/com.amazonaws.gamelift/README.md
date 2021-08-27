# Amazon GameLift Plugin

## Overview

Amazon GameLift Plugin is a plugin that can be used by Unity developers and integrated into their game code without recompiling from the source. The plugin is available from the Unity marketplace and provides easy use of Amazon GameLift functions in hosting, running, and scaling session-based multiplayer games. The plugin supports 5 key deployment scenarios.

Below, there is a brief description of the path a user takes when using the Amazon GameLift Plugin:

1. **Installation**. Download the Amazon GameLift Plugin installation package and import it in the Unity project.
2. **Configuration**. Configure the plugin as follows:
	- Select the recommended version of .NET (.NET 4.x profile for compatibility with external plugin libraries in current Unity project).
	- Download GameLiftLocal.jar and specify the path to it.
	- Install the Java runtime.
	- Provide the credentials for your AWS account and specify the target GameLift region.
	- Bootstrap the AWS account by choosing an existing S3 bucket or creating a new one for deploying the game server.
3. **Stack deployment**. Deploy a CloudFormation stack using one of 5 deployment scenario templates (Auth Only, Single Region Fleet, Multi-Region Fleets with Queue and Custom Matchmaker, SPOT Fleets with Queue and Custom Matchmaker, and FlexMatch) or the custom one.

## How to install the Amazon GameLift Plugin

### Step 1. Download the installation package
**To download Amazon GameLift Plugin, do the following:**
1.	Open the Amazon GameLift Getting Started page: https://aws.amazon.com/gamelift/getting-started/.
2.	Find the Amazon GameLift Plugin card and click the **Download** button.
3.	The **com.amazonaws.gamelift.zip** file will be downloaded and saved on your computer.

### Step 2. Unzip the installation package
When the **com.amazonaws.gamelift.zip** file is downloaded, unzip it. The **com.amazonaws.gamelift** folder contains the following files:
- **Editor/**. Editor platform-specific assets folder.
- **Examples~/**. Contains samples for the plugin.
- **Runtime/**. Runtime platform-specific assets folder.
- **Tests/**. Package tests folder.
- **CHANGELOG.md**. Description of package changes in reverse chronological order.
- **LICENSE.md**. Contains license information.
- **package.json**. Defines package dependencies and other metadata.
- **README.md**. Developer package documentation.

### Step 3. Import the installation package in your Unity project
**To import the plugin installation package, do the following:**
1.	Run Unity and open the project needed.
2.	Navigate to **Window > Package Manager**. 
3.	In the opened **Package Manager** window, click the **“+”** button and select the **Add package from disk** option. 
4.	In the window opened, specify the path to the **package.json** file and click **Open**.
5.	Once the package is imported, the **GameLift** menu item will be added in the Unity menu bar, and the **GameLift Plugin Settings** window will be opened automatically, and you can start configuring the Amazon GameLift Plugin.

## How to install the sample game
You need to add the plugin to your project first.
In Unity 2018.4, you need to open the **com.amazonaws.gamelift\Examples~\SampleGame** folder of the plugin and import the .unitypackage file into your project.

In Unity 2019.1 and newer:
1.	Open your project in Unity, go to **Window > Package Manager > Amazon GameLift Plugin**.
2.	Find **Sample Game** in the **Samples** section and click **Import**. The sample scenario will be imported to your project.

You can find more information about the sample in the README file int the sample's Assets folder.

## FAQ

### What Unity versions are supported?
The Amazon GameLift Plugin is compatible only with officially supported versions of Unity for Windows. It is recommended to use it with the following ones:
- 2018.4 LTS
- 2019.4.LTS
- 2020.2.X
- 2020.3 LTS

### How to deploy a CloudFormation stack using predefined scenario?
1.	Open the **Deployment** window by clicking **GameLift > Deployment** in the menu bar.
2.	In the **Deployment** window, find the scenario needed in the **Select scenario** drop-down list. A brief description of each scenario is displayed below the drop-down. 
3.	Specify the name of your game in the **Game Name** field.
4.	Specify your server build root folder path in the **Build Folder Path** field and the game server executable path in the **Build Exe File Path** field (for Single-Region Fleet, Multi-Region, Spot Fleets, and Flex Match scenarios). The region and S3 bucket name which you created during configuring the plugin are displayed below in the appropriate fields.
5.	Click the **Deploy** button to start deployment. The stack statuses and details will be displayed in the **Current State** section. For more details about CloudFormation stacks and stack status codes, read the AWS CloudfFormation guide: https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/using-cfn-describing-stacks.html
6.	When the deployment process is completed, the status will be displayed in the **Current State** section.
While deployment is in progress, you can close the **Deployment** window if needed and then open it again. The process will not be interrupted.
You can interrupt the deployment process by clicking the **Cancel** button.
7.	If needed, you can redeploy your stack using another scenario or changing deployment parameters.

Note: you can change more scenario parameters in the parameters.json files in the scenario folders. For example, you can pass additional command line parameters with LaunchParametersParameter.

### How to deploy a custom scenario?
1.	In Unity 2018.4, you need to open the **com.amazonaws.gamelift\Examples~\CustomScenario** folder of the plugin and import the .unitypackage file into your project.
or
2. In Unity 2019.1 and newer, open your project in Unity, go to **Window > Package Manager > Amazon GameLift Plugin**; find **Custom Scenario** in the **Samples** section and click **Import**.
3.	Find the **Custom Scenario** folder in **Assets/Editor/** and customize it as needed.
4.	Go to the **Deployment** window by clicking the **GameLift > Deployment** menu item. 
 
7.	In the **Deployment** window, find the custom scenario in the **Select scenario** drop-down list.
 
8.	Specify the name of your game in the **Game Name** field.
9.	Specify your server build root folder path in the **Build Folder Path** field and the game server executable path in the **Build Exe File Path** field (for Single-Region Fleet, Multi-Region, Spot Fleets, and Flex Match scenarios). The region and S3 bucket name which you created during configuring the plugin are displayed below in the appropriate fields.
10.	Click the **Deploy** button to start deployment. The stack statuses and details will be displayed in the **Current State** section. For more details about CloudFormation stacks and stack status codes, read the AWS CloudfFormation guide: https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/using-cfn-describing-stacks.html
11.	When the deployment process is completed, the status will be displayed in the **Current State** section.
While deployment is in progress, you can close the **Deployment** window if needed and then open it again. The process will not be interrupted.
You can interrupt the deployment process by clicking the **Cancel** button.
12.	If needed, you can redeploy your stack using another scenario or changing deployment parameters.

Also, you can create the custom scenario manually based on an existing scenario, customize it, and deploy:
1.	Go to the **com.amazonaws.gamelift\Editor\Resources\CloudFormation** folder in your local plugin.
2.	Copy a scenario folder and paste it somewhere in your project's **Assets** folder.
3.	Open the scenario folder.
4.	Change the scenario assembly definition name.
5.	Open the **Deployer.cs** and change:
•	the namespace to a new unique value,
•	the DisplayName property to a new unique value,
•	the ScenarioFolder property value to your relative folder path (starting with Assets),
•	other properties you need.
6. Deploy the scenario via the **Deployment** window according to the instruction above.

### Where to find logs?
An additional error log file related to the Unity game project can be found in the following location: **logs/amazon-gamelift-plugin-logs[YYYYMMDD].txt**. Note, that the log file is created once a day.
