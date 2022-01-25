import os
import json
import boto3

# env variables
SSM_DOCUMENT_NAME = os.environ['SSM_DOCUMENT_NAME']

print('Loading function')
ssm = boto3.client('ssm')

COMMAND_RUN = 'run'
COMMAND_STATUS = 'status'

STATUS_FAILED = 'FAILED'
STATUS_SUCCESS = 'SUCCESS'
STATUS_IN_PROGRESS = 'IN PROGRESS'


def lambda_handler(event, context):
    # Log the received event
    print("Received event: " + json.dumps(event, indent=2))

    try:
        # Get parameters from the event
        command = event['command']

        if command == COMMAND_RUN:
            return run_command(event)
        elif command == COMMAND_STATUS:
            return check_command_status(event)
        else:
            raise Exception('Unknown command')

    except Exception as e:
        print(e)
        raise Exception('Error processing a job')


def run_command(event):
    # Get parameters from the event
    instance_id = event['instanceId']
    command_text = event['commandText']
    command_timeout = event['timeout']
    command_working_directory = event['workingDirectory']
    input_bucket_name = event['inputBucketName']
    input_object_key = event['inputObjectKey']

    output_artifact_path = event['outputArtifactPath']
    output_bucket_name = event['outputBucketName']
    output_object_key = event['outputObjectKey']
    
    pipeline_execution_id = event['executionId']
    pipeline_arn = event['pipelineArn']
    pipeline_name = event['pipelineName']

    # Send command to the builder instance
    response = ssm.send_command(
        InstanceIds=[instance_id],
        DocumentName=SSM_DOCUMENT_NAME,
        Parameters={
            'inputBucketName': [input_bucket_name],
            'inputObjectKey': [input_object_key],
            'commands': [command_text],
            'executionTimeout': [str(command_timeout)],
            'workingDirectory': [command_working_directory],
            'outputArtifactPath': [output_artifact_path],
            'outputBucketName': [output_bucket_name],
            'outputObjectKey': [output_object_key],
            'executionId': [pipeline_execution_id],
            'pipelineArn': [pipeline_arn],
            'pipelineName': [pipeline_name]
        },
        CloudWatchOutputConfig={
            'CloudWatchOutputEnabled': True
        }
    )

    # extract command ID
    command_id = response.get('Command', {}).get('CommandId', '')

    return {
        'commandId': command_id,
        'status': STATUS_IN_PROGRESS
    }


def check_command_status(event):
    # Get parameters from the event
    command_id = event['commandId']
    instance_id = event['instanceId']

    response = ssm.list_commands(
        CommandId=command_id,
        InstanceId=instance_id
    )

    commands = response.get('Commands', {})
    if commands:
        command = commands[0]
        aws_status = command['Status']

        status = STATUS_FAILED
        if aws_status in ['Pending', 'InProgress']:
            status = STATUS_IN_PROGRESS
        elif aws_status in ['Success']:
            status = STATUS_SUCCESS

        return {
            'commandId': command_id,
            'status': status
        }

    raise Exception('Command is not found.')
