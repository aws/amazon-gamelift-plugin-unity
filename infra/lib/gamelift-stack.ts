import * as cdk from '@aws-cdk/core';
import * as codecommit from '@aws-cdk/aws-codecommit';
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as pipeactions from '@aws-cdk/aws-codepipeline-actions';
import * as s3 from '@aws-cdk/aws-s3';
import * as ec2 from '@aws-cdk/aws-ec2';
import { CustomBuildImageAction } from './custom-action';

export class GameliftStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const bucket = new s3.Bucket(this, 'amazon-gamelift-plugin-unity-artifacts')

    const pipeline = new codepipeline.Pipeline(this, 'Pipeline', {})

    const sourceart = new codepipeline.Artifact()

    const apppackage = new codepipeline.Artifact()

    pipeline.addStage({
      stageName: 'Source'
    }).addAction(new pipeactions.GitHubSourceAction({
      actionName: 'GitHubSource',
      owner: 'aws',
      repo: 'amazon-gamelift-plugin-unity',
      oauthToken: cdk.SecretValue.secretsManager('UnityGitHubAccessToken'),
      output: sourceart,
      branch: 'develop',
    }))

    pipeline.addStage({
      stageName: 'Build'
    }).addAction(
      new CustomBuildImageAction({
        actionName: 'Package',
        inputs: [sourceart],
        outputs: [apppackage],
        providerName: 'EC2-CodePipeline-Builder',
        providerVersion: '2',
        imageId: new ec2.GenericWindowsImage({ 'us-west-2': 'ami-071f7dd51a1918fee' }),
        instanceType: ec2.InstanceType.of(ec2.InstanceClass.COMPUTE5, ec2.InstanceSize.XLARGE2),
        command: `Scripts~/ci/ci.ps1`,
        outputArtifactPath: "build"
      })
    )
    pipeline.addStage({
      stageName: 'Deploy'
    }).addAction(new pipeactions.S3DeployAction({
      input: apppackage,
      actionName: 'Deploy',
      bucket: bucket,
    }))
  }
}
