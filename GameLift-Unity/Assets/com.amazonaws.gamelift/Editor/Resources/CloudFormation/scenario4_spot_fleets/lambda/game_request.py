# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
from boto3.dynamodb.conditions import Key
import time
import os
import json

DEFAULT_TTL_IN_SECONDS = 10 * 60  # 10 minutes
SQS_QUEUE_GROUP_ID = "MatchmakingTicketQueue"
STATUS_PENDING = "PENDING"
STATUS_QUEUED = "QUEUED"


def handler(event, context):
    """
    Handles requests to start games from the game client.
     This function records the game request from the client in the MatchmakingRequest table and enqueues the
     request in the SQS queue, which will then be used for matchmaking.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 202 (Accepted) if the matchmaking request is accepted and is now being processed
     - 409 (Conflict) if the another matchmaking request is in progress
     - 500 (Internal Error) if error occurred when processing the matchmaking request
    """
    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    start_time = round(time.time())
    print(f'Handling start game request. PlayerId: {player_id}, StartTime: {start_time}')

    region_to_latency_mapping = get_region_to_latency_mapping(event)
    if region_to_latency_mapping:
        print(f"Region to latency mapping: {region_to_latency_mapping}")
    else:
        print("No regionToLatencyMapping mapping provided")

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']
    game_session_placement_table_name = os.environ['GameSessionPlacementTableName']
    simple_matchmaker_ticket_queue_url = os.environ['SimpleMatchMakerTicketQueueUrl']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)
    game_session_placement_table = dynamodb.Table(game_session_placement_table_name)

    sqs = boto3.resource('sqs')
    simple_matchmaker_ticket_queue = sqs.Queue(simple_matchmaker_ticket_queue_url)

    matchmaking_requests = matchmaking_request_table.query(
        KeyConditionExpression=Key('PlayerId').eq(player_id),
        ScanIndexForward=False
    )

    if matchmaking_requests['Count'] > 0 \
            and not is_matchmaking_request_terminal(matchmaking_requests['Items'][0], game_session_placement_table):
        # A existing matchmaking request in progress
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 409  # Conflict
        }

    matchmaking_request_table.put_item(
        Item={
            'PlayerId': player_id,
            'StartTime': start_time,
            'ExpirationTime': start_time + DEFAULT_TTL_IN_SECONDS,
            'Status': STATUS_PENDING
        }
    )

    message_attributes = {
        'PlayerId': {
            'StringValue': player_id,
            'DataType': 'String'
        },
        'StartTime': {
            'StringValue': str(start_time),
            'DataType': 'String'
        }
    }

    if region_to_latency_mapping:
        message_attributes["RegionToLatencyMapping"] = {
            'StringValue': json.dumps(region_to_latency_mapping),
            'DataType': 'String'
        }

    try:
        print('Sending message to matchmaking ticket queue')
        simple_matchmaker_ticket_queue.send_message(
            MessageBody=f"Matchmaking request ticket from PlayerId: {player_id} on StartTime: {start_time}",
            MessageAttributes=message_attributes,
            MessageDeduplicationId=player_id,
            MessageGroupId=SQS_QUEUE_GROUP_ID
        )
    except Exception as ex:
        print(f'Error occurred when sending message to matchmaking ticket queue. Exception: {ex}')
        print(f'Deleting matchmaking request for PlayerId: {player_id}, StartTime: {start_time}')
        matchmaking_request_table.delete_item(
            Key={
                'PlayerId': player_id,
                'StartTime': start_time
            }
        )
        return {
            # Error occurred when enqueuing matchmaking request
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 500
        }

    return {
        # Matchmaking request enqueued
        'headers': {
            'Content-Type': 'text/plain'
        },
        'statusCode': 202
    }


def is_matchmaking_request_terminal(matchmaking_request, game_session_placement_table):
    if matchmaking_request['Status'] == STATUS_PENDING:
        return False
    if matchmaking_request['Status'] == STATUS_QUEUED:
        placement_id = matchmaking_request['PlacementId']
        placement = game_session_placement_table.get_item(
            Key={
                'PlacementId': placement_id
            }
        ).get('Item')

        if not placement:
            return False

        return True


def get_region_to_latency_mapping(event):
    request_body = event.get("body")
    if not request_body:
        return None

    try:
        request_body_json = json.loads(request_body)
    except ValueError:
        print(f"Error parsing request body: {request_body}")
        return None

    if request_body_json and request_body_json.get('regionToLatencyMapping'):
        return request_body_json.get('regionToLatencyMapping')
