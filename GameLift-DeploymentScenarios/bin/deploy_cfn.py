import sys
import boto3
import botocore
import json
from datetime import datetime
from upload_lambda import upload_lambda

LAMBDA_ZIP_S3_KEY_PARAMETER = 'LambdaZipS3KeyParameter'
LAMBDA_ZIP_S3_BUCKET_PARAMETER = 'LambdaZipS3BucketParameter'
BUILD_S3_BUCKET_PARAMETER = 'BuildS3BucketParameter'
BUILD_S3_KEY_PARAMETER = 'BuildS3KeyParameter'


def main(region, game_name, template_path, parameters_path, stack_name, lambda_src_path=None, lambda_zip_dir=None,
         build_s3_bucket=None, build_s3_key=None):
    cf = boto3.client('cloudformation', region_name=region)

    template_data = _parse_template(cf, template_path)
    parameters_data = _parse_parameters(cf, parameters_path)

    if lambda_src_path and lambda_zip_dir:
        lambda_s3_info = upload_lambda(region, game_name, lambda_src_path, lambda_zip_dir)
        _add_parameter(parameters_data, LAMBDA_ZIP_S3_BUCKET_PARAMETER, lambda_s3_info.get_s3_bucket())
        _add_parameter(parameters_data, LAMBDA_ZIP_S3_KEY_PARAMETER, lambda_s3_info.get_s3_key())

    if build_s3_bucket and build_s3_key:
        _add_parameter(parameters_data, BUILD_S3_BUCKET_PARAMETER, build_s3_bucket)
        _add_parameter(parameters_data, BUILD_S3_KEY_PARAMETER, build_s3_key)

    params = {
        'TemplateBody': template_data,
        'Parameters': parameters_data,
        'StackName': stack_name,
        'Capabilities': ['CAPABILITY_NAMED_IAM'],
    }

    print ("Parameters:", parameters_data)

    try:
        if _stack_exists(cf, stack_name):
            print('Updating {}'.format(stack_name))
            stack_result = cf.update_stack(**params)
            waiter = cf.get_waiter('stack_update_complete')
        else:
            print('Creating {}'.format(stack_name))
            stack_result = cf.create_stack(**params)
            waiter = cf.get_waiter('stack_create_complete')
        print("...waiting for stack to be ready...")
        waiter.wait(StackName=stack_name)
        print(json.dumps(
            cf.describe_stacks(StackName=stack_result['StackId']),
            indent=2,
            default=json_serial
        ))
    except botocore.exceptions.ClientError as ex:
        error_message = ex.response['Error']['Message']
        if error_message == 'No updates are to be performed.':
            print("No changes")
        else:
            raise


def _parse_template(cf, template_path):
    with open(template_path) as template_file:
        template_data = template_file.read()
    cf.validate_template(TemplateBody=template_data)
    return template_data


def _parse_parameters(cf, parameters):
    with open(parameters) as parameter_file:
        parameter_data = json.load(parameter_file)
    return parameter_data


def _stack_exists(cf, stack_name):
    stacks = cf.list_stacks()['StackSummaries']
    for stack in stacks:
        if stack['StackStatus'] == 'DELETE_COMPLETE':
            continue
        if stack_name == stack['StackName']:
            return True
    return False


def json_serial(obj):
    """JSON serializer for objects not serializable by default json code"""
    if isinstance(obj, datetime):
        serial = obj.isoformat()
        return serial
    raise TypeError("Type not serializable")


def _add_parameter(parameter_data, key, value):
    parameter_data.append(
        {
            'ParameterKey': key,
            'ParameterValue': value,
        }
    )


if __name__ == '__main__':
    main(*sys.argv[1:])
