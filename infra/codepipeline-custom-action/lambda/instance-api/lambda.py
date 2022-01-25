import json
import boto3
import os

# env variables
BUILDER_INSTANCE_PROFILE_ARN = os.environ['BUILDER_INSTANCE_PROFILE_ARN']

print('Loading function')
ec2 = boto3.client('ec2')
ssm = boto3.client('ssm')

COMMAND_START = 'start'
COMMAND_STOP = 'stop'
COMMAND_STATUS_EC2 = 'status_ec2'
COMMAND_STATUS_SSM = 'status_ssm'

STATUS_STOPPED = 'STOPPED'
STATUS_STARTED = 'STARTED'
STATUS_IN_PROGRESS = 'IN PROGRESS'


def lambda_handler(event, context):
    # Log the received event
    print("Received event: " + json.dumps(event, indent=2))

    # Get parameters from the event
    command = event['command']

    try:
        if command == COMMAND_START:
            return start_instance(event)
        elif command == COMMAND_STOP:
            return stop_instance(event)
        elif command == COMMAND_STATUS_EC2:
            return check_instance_status_ec2(event)
        elif command == COMMAND_STATUS_SSM:
            return check_instance_status_ssm(event)
        else:
            raise Exception('Unknown command')

    except Exception as e:
        print(e)
        message = 'Error submitting Batch Job'
        print(message)
        raise Exception(message)


def get_instance_status(state_name):
    if state_name in ['running']:
        return STATUS_STARTED
    elif state_name in ['terminated', 'stopped']:
        return STATUS_STOPPED
    else:
        return STATUS_IN_PROGRESS


def get_detailed_instance_status(status):
    if status in ['passed']:
        return STATUS_STARTED
    elif status in ['failed']:
        return STATUS_STOPPED
    elif status in ['insufficient-data', 'initializing']:
        return STATUS_IN_PROGRESS
    else:
        raise Exception('Unsupported detailed instance status: ' + status)


def start_instance(event):
    # Get parameters from event 
    image_id = event.get('imageId', '')
    instance_type = event.get('instanceType', '')
    key_name = event.get('keyName', '')

    # TODO: validate parameters

    # Send command to the builder instance
    response = ec2.run_instances(
        ImageId=image_id,
        MinCount=1,
        MaxCount=1,
        InstanceType=instance_type,
        # KeyName = key_name,
        IamInstanceProfile={
            'Arn': BUILDER_INSTANCE_PROFILE_ARN
        },
        TagSpecifications=[{
            'ResourceType': 'instance',
            'Tags': [{
                'Key': 'AmiBuilder',
                'Value': 'True'
            }]
        }]
    )

    # Log response
    # print("Response: " + json.dumps(instances, indent=2))

    instances = response.get('Instances', [])
    if not instances:
        raise Exception('Instance creation error.')

    instance = instances[0]
    instance_id = instance['InstanceId']

    return {
        'instanceId': instance_id,
        'status': STATUS_IN_PROGRESS
    }


def stop_instance(event):
    # Get parameters from event 
    instance_id = event['instanceId']

    # Send command to the builder instance
    response = ec2.terminate_instances(
        InstanceIds=[instance_id]
    )

    # Log response
    # print("Response: " + json.dumps(response, indent=2))

    instances = response.get('TerminatingInstances', [])
    if not instances:
        raise Exception('Instance termination error.')

    state_name = instances[0].get('CurrentState', {}).get('Name', '')
    return {
        'instanceId': instance_id,
        'status': get_instance_status(state_name)
    }


def check_instance_status_ssm(event):
    # Get parameters from event 
    instance_id = event['instanceId']

    # Send command to the builder instance
    response = ssm.describe_instance_information(
        Filters=[{
            'Key': 'InstanceIds',
            'Values': [instance_id]
        }]
    )

    status = STATUS_IN_PROGRESS
    instance_list = response.get('InstanceInformationList', [])
    if instance_list:
        status = STATUS_STARTED

    return {
        'instanceId': instance_id,
        'status': status
    }


def check_instance_status_ec2(event):
    # Get parameters from event 
    instance_id = event['instanceId']

    # Send command to the builder instance
    response = ec2.describe_instances(
        InstanceIds=[instance_id]
    )

    # Log response
    # print("Response: " + json.dumps(response, indent=2))

    reservations = response.get('Reservations', [])
    if not reservations:
        raise Exception('Describe instances error.')

    reservation = reservations[0]
    instances = reservation.get('Instances', [])
    if not instances:
        raise Exception('No instance found with provided id')

    state_name = instances[0].get('State', {}).get('Name', '')
    status = get_instance_status(state_name)

    if status == STATUS_STARTED:

        # we have to wait until all status checks are passed
        response = ec2.describe_instance_status(InstanceIds=[instance_id])

        instance_statuses = response.get('InstanceStatuses', [])
        if not instance_statuses:
            raise Exception('Describe instance status error.')

        details = instance_statuses[0].get('InstanceStatus', {}).get('Details', [])
        if not details:
            raise Exception('Describe instance status error (missing details).')

        detailed_status = details[0].get('Status', '<empty>')
        status = get_detailed_instance_status(detailed_status)

    return {
        'instanceId': instance_id,
        'status': status
    }
