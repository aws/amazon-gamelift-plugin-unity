# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import os
import json
from boto3.dynamodb.conditions import Key

STATUS_QUEUED = "QUEUED"


def handler(event, context):
    """
    Handles requests to describe the game session connection information after a StartGame request.
     This function will look up the MatchmakingRequest table to find a pending matchmaking request by
     the player, and if it is QUEUED, look up the GameSessionPlacement table to find the game's
     connection information.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 200 (OK) if the game connection is ready, along with server info: "IpAddress", "Port", "DnsName"
     - 204 (No Content) if the requested game is still in progress of matchmaking
     - 404 (Not Found) if no game has been started by the player, or if all started game were expired
     - 500 (Internal Error) if errors occurred during matchmaking or placement
    """
    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling request result request. PlayerId: {player_id}')

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']
    game_session_placement_table_name = os.environ['GameSessionPlacementTableName']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)
    game_session_placement_table = dynamodb.Table(game_session_placement_table_name)

    matchmaking_requests = matchmaking_request_table.query(
        KeyConditionExpression=Key('PlayerId').eq(player_id),
        ScanIndexForward=False
    )

    if matchmaking_requests['Count'] <= 0:
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 404
        }

    latest_matchmaking_request = matchmaking_requests['Items'][0]

    if latest_matchmaking_request['Status'] != STATUS_QUEUED:
        # still waiting for more players to start matchmaking
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 204
        }

    placement_id = latest_matchmaking_request['PlacementId']

    placement = game_session_placement_table.get_item(
        Key={
            'PlacementId': placement_id
        }
    ).get('Item')

    print(f'Placement: {placement}')

    if not placement:
        # start-game-session-placement has just started and no game session event has been received
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 204
        }

    if placement['Status'] != 'PlacementFulfilled':
        # We count PlacementCancelled as internal error also because cancelling placement requests is not
        # in the current implementation, so it should never happen.
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 500
        }

    player_session_id = parse_player_session_id(player_id, placement)
    if player_session_id is None:
        # PlayerSession should always be created and present in the dynamo table for a PlacementFulfilled Event
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 500
        }

    game_session_connection_info = dict((k, placement[k]) for k in ('IpAddress', 'Port', 'DnsName', 'GameSessionArn'))
    game_session_connection_info['PlayerSessionId'] = player_session_id
    return {
        'body': json.dumps(game_session_connection_info),
        'headers': {
            'Content-Type': 'text/plain'
        },
        'statusCode': 200
    }

def parse_player_session_id(player_id, placement):
    player_sessions = placement['PlayerSessions']
    print(f'Parsing PlayerSessionId for Player {player_id} in PlayerSessions: {player_sessions}')
    for player_session in player_sessions:
        if (player_id == player_session['playerId']) :
            return player_session['playerSessionId']

    return None
