# GameLift Plugin Deployment Scenarios Development Guide

## Pre-requisites

### Install python 3

* Go to https://www.python.org/downloads/
* Download and install

### Install cfn-format

* Go to https://github.com/awslabs/aws-cloudformation-template-formatter/releases
* Download cfn-format
* Unzip it to a bin folder of your PATH

## Adding a new deployment scenario

1. Create a new directories under `CloudFormation` directory named `scenario<##>_<name>`
1. Create the following files:
   * `<scenario_name>/cloudformation.yml` -- This will contain the CFN template of the deployment scenario
   * `<scenario_name>/parameters.json` -- This will contain the parameters to the CFN template
   * `<scenario_name>/lambda/*.py` -- These are the lambda function source code that will be uploaded to s3 and used to create the lambda functions
   * `<scenario_name>/Deployer.cs` -- This will contain the necessary data for the deployment scenario to be displayed in the plugin UI
   * `<scenario_name>/tests/test.py` -- This will run integ tests against a deployed scenario (the scenario is already deployed with `bin/deploy_cfn.py`)
1. Load the project in Unity, and .meta files will be generated for you. These .meta files will need to be committed eventually

## Update & test an existing deployment scenario

1. Use Amazon GameLift Plug-in for Unity to create a CloudFormation stack
    * If the deployment failed immediately, it means the template is mal-formatted. Check the error output to locate the error.
    * If the deployment started but eventually failed, go to CloudFormation console to find out why.
1. Modify `GAME_NAME` and `REGION` in `python3 <scenario_name>/tests/test.py` to match the stack you deployed
1. Run `aws configure` to set your default AWS credentials to match the account in which your stack was deployed
1. Run `python3 <scenario_name>/tests/test.py` to verify that the scenario is still working
1. Make any updates to the template and test, then repeat the last 3 steps, i.e. redeploy, wait, test
    * To speed up iteration, you can remove the `Fleet` resource in the template and replace all of its references to a hard-coded resource ID/ARN

## Before you submit a pull request

1. Run `cfn-format --write <scenario_name>/cloudformation.yml`, example: `cfn-format --write scenario1_auth_only/cloudformation.yml`
   * NOTE: a caveat in cfn-format is that it does not preserve comments, we'll need to manually add copyrights back once the formatter completes.
   ```
   # Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
   # SPDX-License-Identifier: MIT-0
   ```
