# Gamelift CDK Infrastructure

Before deploying this pipepeline you should complete all installation steps from the `codepipeline-custom-action` folder. 

## Initialize Infrastructure on New Account

### Create GitHub Access Token

1. Follow [GitHub docs](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) to create an access token
1. Create a plaintext secret in AWS Secrets Manager with ID `UnityGitHubAccessToken` and the github token ("ghp_...") as plaintext

### Create Unity License Activation Secret

1. Log in to AWS console
1. Go to **Secret Manager**
1. Create a new secret named **UnityLicenseActivationSecrets**
1. Choose "Other type of secret" as Secret Type
1. Add `UnityUsername`, `UnityPassword` and `UnitySerial` with corresponding values
1. Save secret

### Create builder AMI

1. Windows 2019 Server Base AMI, to create:
   1. Go to EC2, launch a Windows 2019 Server instance with 200GB of disk space
   1. Run the following script in powershell
      ```
      Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
      choco install -y visualstudio2019buildtools --package-parameters "--quiet --wait --norestart --nocache --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.Net.Component.4.5.TargetingPack"
      mkdir -Path "C:\Unity"
      mkdir -Path "C:\Temp"
      choco install -y unity --version=2021.1.18 --params="'/InstallationPath:C:\Unity\'"
      $env:Path += ";C:\Unity\Editor"
      choco install -y dotnetcore dotnetcore-sdk netfx-4.7.1-devpack
      choco install -y awscli
      $env:Path += ";C:\Program Files\Amazon\AWSCLIV2"
      ```
   1. Go to **C:\ProgramData\Amazon\EC2-Windows\Launch\Config\LaunchConfig.json**, modify **adminPasswordType** to `Specify`, and set a **adminPassword**
   1. Go to AWS Console > Secrets Manager, and store a new secret `UnityBuilderAmiAdministratorPassword`, with key `Password` and your password as the value
   1. Run `C:\ProgramData\Amazon\EC2-Windows\Launch\Scripts\InitializeInstance.ps1 -SchedulePerBoot` in powershell
   1. Create an AMI from the instance on EC2
1. Go to **infra/lib/gamelift-stack.ts**
   1. Update `imageId` with the AMI id

### Create Infrastructure Stacks

Follow the Development Workflow sections below to create the **Custom Action** and **CICD** stacks.

## Development workflow

### Create/Update Custom Action stack

1. Change directory to **infra/codepipeline-custom-action**
1. Load Unity Plugin CICD AWS credentials into your terminal session
1. Run `aws cloudformation package --template-file template.yml --output-template-file deployment.yml --s3-bucket <S3_BUCKET>`
1. Run `aws cloudformation deploy --template-file deployment.yml --capabilities CAPABILITY_NAMED_IAM --stack-name codepipeline-custom-action-unity --parameter-overrides CustomActionProviderVersion=2`
   1. NOTE: if you change `CustomActionProviderVersion`, make sure to update the `providerVersion` in **infra/lib/gamelift-stack.ts** and also this README

### Create/Update CICD stack

1. (One-off) Install npm if needed: https://docs.npmjs.com/downloading-and-installing-node-js-and-npm
1. Change directory to **infra**
1. Run `npm i`
1. Load Unity Plugin CICD AWS credentials into your terminal session
1. Run `npm run cdk diff` to view diff
1. Run `npm run cdk deploy` to deploy the CICD stack

### Test building on AMI without CodeBuild

1. Launch a new instance from the AMI, see **gamelift-stack.ts** for AMI id
   1. Make sure to use at least c5.2xlarge as the instance type
   1. Specify `ec2-builder-instance-profile` as your instance profile
   1. Make sure port 3389 is allowlisted in security group (it should be by default)
1. Wait for the instance state to be RUNNING
1. Connect to the instance by downloading the RDP file and running it via Microsoft Remote Desktop
1. Fetch the Administrator password from Secrets Manager with secret id `UnityBuilderAmiAdministratorPassword`
1. `git clone https://github.com/aws/amazon-gamelift-plugin-unity.git`
1. Run `Scripts~/ci/ci.ps1`

### Troubleshoot CodeBuild failures

1. Go to CodePipeline, find the pipeline, find CodeBuild, click on **Details**, you should see the Step Function workflow,
   investigate if you see any errors there
1. If it failed on the SSM RunCommand state, go to Systems Manager, find Run Command, go to Command History. You should
   see the terminal output of the build
1. If the above still doesn't work, trigger a new build, wait for an EC2 instance to be launched, add RDP (3309) in the
   security group, then follow the "Test building on AMI without CodeBuild" section above to ssh into the host and start a build
   1. The plugin code is checked out in **C:\Windows\Temp\<uuid>** (NOT "C921..."), you can find the logs in **build/**
   1. NOTE: once the step function is completed, the instance will be terminated. If you need more time, stop the step
      functions first

### Active License Activations Exceeded Capacity

Unity Pro license only allow 2 activations per Seat. I.e. GameLift team owns the license, seats are assigned to individual
devs, each seat can be activated on up to 2 of the dev's machines.

Activation is done as a first step when building the plugin (see **Scripts~/ci/ci.ps1**), and the script performs a best-effort
attempt to return the active license after the script execution succeeds or fails. However, there could be issues communicating
with the license server, or the script could time out, both of which result in the seat continuing to be activated on the instance.

When this happens, check the activation logs in S3 (*build/ActivationLog.txt*), which would show you that the activation
failed due to active activations exceeded limit. Find the Unity serial number in the log (SC-...), and return the activations 
by logging on unity.com and following steps in
[this doc](https://support.unity.com/hc/en-us/articles/205056069-How-do-I-return-the-activations-on-my-Pro-Plus-Enterprise-license-). 

