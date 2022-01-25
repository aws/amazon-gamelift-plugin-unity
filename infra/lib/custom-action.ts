import * as cdk from '@aws-cdk/core';
import * as cpl from '@aws-cdk/aws-codepipeline';
import * as events from '@aws-cdk/aws-events';
import * as ec2 from '@aws-cdk/aws-ec2';


export interface CustomBuildImageActionProps extends cpl.CommonActionProps {
    /**
     * This is the provider name given to the action when created with the template in
     * https://github.com/aws-samples/aws-codepipeline-custom-action
     */
    readonly providerName: string;
    readonly providerVersion: string;
    readonly imageId: ec2.IMachineImage;
    readonly instanceType: ec2.InstanceType;
    readonly command: string;
    readonly inputs?: cpl.Artifact[];
    readonly outputs?: cpl.Artifact[];
    readonly outputArtifactPath?: string;
}

export class CustomBuildImageAction implements cpl.IAction {
    private readonly _props: CustomBuildImageActionProps;
    readonly actionProperties: cpl.ActionProperties;
    private _pipeline: cpl.IPipeline;
    private _stage: cpl.IStage;
    private _scope: cdk.Construct;

    constructor(props: CustomBuildImageActionProps) {
        this.actionProperties = {
            artifactBounds: { minInputs: 0, maxInputs: 1, minOutputs: 0, maxOutputs: 1 },
            actionName: props.actionName,
            category: cpl.ActionCategory.BUILD,
            inputs: props.inputs,
            outputs: props.outputs,
            owner: 'Custom',
            provider: props.providerName,
            version: props.providerVersion,
        }
        this._props = props;
    }

    bind(scope: cdk.Construct, stage: cpl.IStage, options: cpl.ActionBindOptions): cpl.ActionConfig {
        this._pipeline = stage.pipeline;
        this._stage = stage;
        this._scope = scope;
        return {
            configuration: {
                ImageId: this._props.imageId.getImage(scope).imageId,
                InstanceType: this._props.instanceType.toString(),
                Command: this._props.command,
                OutputArtifactPath: this._props.outputArtifactPath,
            }
        };
    }

    onStateChange(name: string, target?: events.IRuleTarget, options?: events.RuleProps): events.Rule {
        const rule = new events.Rule(this.scope, name, options);
        rule.addTarget(target);
        rule.addEventPattern({
            detailType: ['CodePipeline Action Execution State Change'],
            source: ['aws.codepipeline'],
            resources: [this.pipeline.pipelineArn],
            detail: {
                stage: [this.stage.stageName],
                action: [this.actionProperties.actionName],
            },
        });
        return rule;
    }

    get pipeline() {
        if (this._pipeline) {
            return this._pipeline;
        }
        else {
            throw new Error('Action must be added to a stage that is part of a pipeline before using onStateChange');
        }
    }
    get stage() {
        if (this._stage) {
            return this._stage;
        }
        else {
            throw new Error('Action must be added to a stage that is part of a pipeline before using onStateChange');
        }
    }
    /**
     * Retrieves the Construct scope of this Action.
     * Only available after the Action has been added to a Stage,
     * and that Stage to a Pipeline.
     */
    get scope() {
        if (this._scope) {
            return this._scope;
        }
        else {
            throw new Error('Action must be added to a stage that is part of a pipeline first');
        }
    }
}