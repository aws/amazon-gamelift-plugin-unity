# GameScalePluginResources

## Adding a new deployment scenario

1. Create a new directories under `src` and `tst` named `scenario<##>_<name>`
1. Create the following files:
   * `src/<scenario_name>/cloudformation.yml` -- This will contain the CFN template of the deployment scenario
   * `src/<scenario_name>/parameters.json` -- This will contain the parameters to the CFN template
   * `src/<scenario_name>/lambda/*.py` -- These are the lambda function source code that will be uploaded to s3 and used to create the lambda functions
   * `tst/<scenario_name>/test.py` -- This will run integ tests against a deployed scenario (the scenario is already deployed with `bin/deploy_cfn.py`) 

## Update & Test an existing deployment scenario

1. Create an IAM user with programmatic access and Administrator permissions in your personal account
1. Run `aws configure` to add the IAM user credentials
1. Run `python bin/bootstrap_account.py` to bootstrap account with an S3 bucket to deploy Lambda function
1. Run `python bin/deploy_cfn.py ...` to deploy the CFN template
   * Example:
     ```
     cd /path/to/workspace/GameScalePluginResources;
     python3 bin/deploy_cfn.py \
         ../src/scenario1_auth_only/cloudformation.yml \
         ../src/scenario1_auth_only/parameters.json \
         TestDeploymentScenario1Stack
     ```
1. Wait for the code to succeed with exit code 0
    * If the deployment failed immediately, it means the template is malformatted. Check the error output to locate the error.
    * If the deployment started but eventually failed, go to CloudFormation console to find out why.
1. Run `python tst/<scenario_name>/test.py` to verify that the scenario is still working
1. Make any updates to the template, then repeat the last 3 steps, i.e. redeploy, wait, test