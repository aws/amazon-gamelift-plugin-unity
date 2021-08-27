# Usage: python3 bootstrap_account.py us-west-2 MyGame

import boto3
import sys


def main(region, game_name):
    s3 = boto3.client('s3', region_name=region)
    sts = boto3.client('sts', region_name=region)

    account_id = get_account_id(sts)
    bootstrap_bucket_name = get_bootstrap_bucket_name(account_id, game_name)
    print("Bootstrap Bucket Name", bootstrap_bucket_name)

    location = {'LocationConstraint': region}
    result = s3.create_bucket(Bucket=bootstrap_bucket_name, CreateBucketConfiguration=location)
    print("Account bootstraped", result)


def get_bootstrap_bucket_name(account_id, game_name):
    return f'do-not-delete-gamelift-{account_id}-{game_name}'.lower()


def get_account_id(sts):
    return sts.get_caller_identity()['Account']


if __name__ == '__main__':
    main(*sys.argv[1:])
