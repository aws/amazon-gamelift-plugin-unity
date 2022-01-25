import json

def lambda_handler(event, context):
    # Log the received event
    print("Received event: " + json.dumps(event, indent=2))

    error_details = event.get('errorDetails', {})
    if error_details:
        raise Exception(f'Error processing a job: {error_details}')