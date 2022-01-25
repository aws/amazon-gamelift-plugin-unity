## Amazon EC2 build environments with AWS CodePipeline and custom actions

This repository provides serverless implementation of 
[AWS CodePipeline custom actions](https://docs.aws.amazon.com/codepipeline/latest/userguide/actions-create-custom-action.html).

The rationale behind this implementation is described in [Building Windows containers with AWS CodePipeline and custom actions](https://aws.amazon.com/blogs/devops/building-windows-containers-with-aws-codepipeline-and-custom-actions/).

## Arhitecture

![Architecture](/doc/ec2-custom-action-architecture.png)

1. An Amazon CloudWatch Event triggers an AWS Lambda function when a custom CodePipeline action is to be executed.
1. The Lambda function retrieves the action's build properties (AMI, instance type, etc.) from CodePipeline, along with location of the input artifacts in the Amazon S3 bucket.
1. The Lambda function starts a Step Functions state machine that carries out the build job execution, passing all the gathered information as input payload.
1. The Step Functions flow acquires an Amazon EC2 instance according to the provided properties, waits until the instance is up and running, and starts an AWS Systems Manager command. The Step Functions flow is also responsible for handling all the errors during build job execution and releasing the Amazon EC2 instance once the Systems Manager command execution is complete.
1. The Systems Manager command runs on an Amazon EC2 instance, downloads CodePipeline input artifacts from the Amazon S3 bucket, unzips them, executes the build script, and uploads any output artifacts to the CodePipeline-provided Amazon S3 bucket.
1. Polling Lambda updates the state of the custom action in CodePipeline once it detects that the Step Function flow is completed.

## Deployment
First, package and upload necessary resources (deployment package for lambda functions). Specify S3 bucket name.
> aws cloudformation package --template-file template.yml --output-template-file deployment.yml --s3-bucket **{deployment-bucket}**

Deploy the stack using the following command:

> aws cloudformation deploy --template-file deployment.yml --capabilities CAPABILITY_NAMED_IAM --stack-name **{stack-name}** --parameter-overrides CustomActionProviderVersion=**{custom-action-version}**

Note that you need to provide the following parameters:
 -  **{stack-name}** - CloudFormation stack name
 -  **{custom-action-version}** - version of the custom action (1, 2, 3, etc.). 
 Each deployment of the custom action has to have a distinct version number.

## Configuration
Once CloudFormation stack is deployed, you can add the custom action to your CI/CD pipelines
either via AWS Console or CloudFormation. See [AWS CodePipeline for Windows Server containers](https://github.com/aws-samples/aws-codepipeline-custom-action/tree/master/examples/windows-container-pipeline) for an example
of using custom actions for building Windows Server containers. 

## Environment variables available to the build scripts

| Variable Name | Example | Description |
|---------------|---------|-------------|
| `PipelineArn` | arn:aws:codepipeline:eu-west-1:111111111111:windows-container-cicd-pipeline | The Amazon Resource Name (ARN) of the pipeline. |
| `PipelineName` | windows-container-cicd-pipeline | The name of the pipeline. |
| `PipelineExecutionId` | ced4b2c3-d4ee-4767-885d-30d45b94bfdf | The execution ID of the pipeline. |

## License
This library is licensed under the MIT-0 License. See the LICENSE file.
