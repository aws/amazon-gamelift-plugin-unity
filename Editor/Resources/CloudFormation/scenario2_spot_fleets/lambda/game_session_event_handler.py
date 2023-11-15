# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import os
import json
import time

DEFAULT_TTL_IN_SECONDS = 10 * 60  # 10 minutes


def handler(event, context):
    """
    Handles game session event from GameLift queue. This function parses the event messages and writes them to
     the GameSessionPlacement DynamoDB table, which will be looked up to fulfill game client's Results Requests.
    :param event: lambda event containing game session event from GameLift queue
    :param context: lambda context, not used by this function
    :return: None
    """
    start_time = round(time.time())
    message = json.loads(event['Records'][0]['Sns']['Message'])
    print(f'Handling game session event. StartTime: {start_time}. Message: {message}')

    status_type = message['detail']['type']
    placement_id = message['detail']['placementId']
    ip_address = message['detail'].get('ipAddress')
    dns_name = message['detail'].get('dnsName')
    port = message['detail'].get('port')
    game_session_arn = message['detail'].get('gameSessionArn')
    player_sessions = message['detail'].get('placedPlayerSessions')

    game_session_placement_table_name = os.environ['GameSessionPlacementTableName']

    dynamodb = boto3.resource('dynamodb')
    game_session_placement_table = dynamodb.Table(game_session_placement_table_name)

    game_session_placement_table.put_item(
        Item={
            'PlacementId': placement_id,
            'Status': status_type,
            'IpAddress': ip_address,
            'DnsName': dns_name,
            'Port': port,
            'GameSessionArn': game_session_arn,
            'ExpirationTime': start_time + DEFAULT_TTL_IN_SECONDS,
            'PlayerSessions': player_sessions
        }
    )
