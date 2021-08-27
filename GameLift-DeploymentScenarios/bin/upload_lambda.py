# Usage: python3 upload_lambda.py us-west-2 MyGame

import boto3
import time
import sys
import os
import zipfile
from bootstrap_account import get_bootstrap_bucket_name, get_account_id

LAMBDA_ZIP_FILENAME_FORMAT = 'GameLift_{game_name}.zip'
LAMBDA_S3_KEY_FORMAT = 'functions/gamelift/GameLift_{game_name}_{timestamp}.zip'


class UploadLambdaResponse:
    def __init__(self, s3_bucket, s3_key):
        self.s3_bucket = s3_bucket
        self.s3_key = s3_key

    def get_s3_bucket(self):
        return self.s3_bucket

    def get_s3_key(self):
        return self.s3_key


def main(region, game_name, lambda_src_path, lambda_zip_dir):
    upload_lambda(region, game_name, lambda_src_path, lambda_zip_dir)


def upload_lambda(region, game_name, lambda_src_path, lambda_zip_dir):
    sts = boto3.client('sts', region_name=region)

    if not os.path.exists(lambda_zip_dir):
        os.mkdir(lambda_zip_dir)

    lambda_zip_filepath = os.path.abspath(os.path.join(lambda_zip_dir, LAMBDA_ZIP_FILENAME_FORMAT.format(game_name=game_name)))
    with zipfile.ZipFile(lambda_zip_filepath, 'w', zipfile.ZIP_DEFLATED) as lambda_zip_file_handle:
        zip_directory(lambda_src_path, lambda_zip_file_handle)
    print('Created zip file in:', lambda_zip_filepath)

    account_id = get_account_id(sts)
    lambda_s3_bucket = get_bootstrap_bucket_name(account_id, game_name)
    lambda_s3_key = LAMBDA_S3_KEY_FORMAT.format(game_name=game_name, timestamp=int(time.time()))
    print('Uploading lambda zip to', lambda_s3_bucket, lambda_s3_key)

    s3 = boto3.client('s3', region_name=region)
    # s3_resource = boto3.resource('s3')
    # result = s3_resource.Bucket(lambda_s3_bucket).upload_file(lambda_zip_filepath, lambda_s3_key)
    result = s3.upload_file(lambda_zip_filepath, lambda_s3_bucket, lambda_s3_key)
    # with open(lambda_zip_filepath, 'rb') as f:
    #     s3.up
    print("Lambda Uploaded", result)

    return UploadLambdaResponse(s3_bucket=lambda_s3_bucket, s3_key=lambda_s3_key)


def zip_directory(path, zip_file_handler):
    length = len(path)
    for root, dirs, files in os.walk(path):
        folder = root[length:]  # path without "parent"
        for file in files:
            zip_file_handler.write(os.path.join(root, file), os.path.join(folder, file))


if __name__ == '__main__':
    main(*sys.argv[1:])