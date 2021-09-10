# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import json
import os
import uuid

STATUS_QUEUED = "QUEUED"


def handler(event, context):
    """
    Handle batches of SQS messages containing matchmaking requests.
    This Lambda function takes requests from the same batch (batch size is set in the CloudFormation template
     via NumPlayersPerGameParameter) and calls gamelift:StartGameSessionPlacement to put the players into the
     same game session
    :param event: lambda event containing SQS message
    :param context: lambda context, not used by this function
    :return: None
    """
    messages = event['Records']
    batch_size = len(messages)
    print(f'Handling matchmaking tickets batch, Messages: {messages}')

    gamelift = boto3.client('gamelift')

    queue_name = os.environ['QueueName']
    num_players_per_game = int(os.environ['NumPlayersPerGame'])
    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)

    if batch_size < num_players_per_game:
        raise Exception(f"Not enough player in the batch. Batch Size: {batch_size}. Expected: {num_players_per_game}")
    else:
        player_latencies = get_player_latencies(messages)

        desired_player_sessions = [{
            'PlayerId': message['messageAttributes']['PlayerId']['stringValue'],
            'PlayerData': json.dumps(message['messageAttributes'])
        } for message in messages]

        placement_id = str(uuid.uuid4())

        start_game_session_placement_request = {
            "PlacementId": placement_id,
            "GameSessionQueueName": queue_name,
            "MaximumPlayerSessionCount": num_players_per_game,
            "DesiredPlayerSessions": desired_player_sessions,
            "PlayerLatencies": player_latencies
        }
        print(f"Starting game session placement in GameLift. Request: {start_game_session_placement_request}")
        gamelift.start_game_session_placement(**start_game_session_placement_request)

        for message in messages:
            player_id = message['messageAttributes']['PlayerId']['stringValue']
            start_time = round(float(message['messageAttributes']['StartTime']['stringValue']))
            matchmaking_request_table.update_item(
                Key={
                    'PlayerId': player_id,
                    'StartTime': start_time
                },
                AttributeUpdates={
                    'PlacementId': {
                        'Value': placement_id
                    },
                    'Status': {
                        'Value': STATUS_QUEUED
                    }
                }
            )


def get_player_latencies(messages):
    return list(get_player_latencies_generator(messages))


def get_player_latencies_generator(messages):
    for message in messages:
        player_id = message['messageAttributes']['PlayerId']['stringValue']
        region_to_latency_mapping_string = \
            message['messageAttributes'].get('RegionToLatencyMapping', {}).get('stringValue')

        if not region_to_latency_mapping_string:
            print(f"No region_to_latency_mapping provided for player: {player_id}")
            continue

        region_to_latency_mapping = json.loads(region_to_latency_mapping_string)
        if region_to_latency_mapping and type(region_to_latency_mapping) is dict:
            for region, latency in region_to_latency_mapping.items():
                yield {
                    'PlayerId': player_id,
                    'LatencyInMilliseconds': latency,
                    'RegionIdentifier': region,
                }
        else:
            print(f"The region_to_latency_mapping_string cannot be parsed: {region_to_latency_mapping_string}")
