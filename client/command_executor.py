import sys
import asyncio
import time
from option_actions import *

HOST = "localhost:50053"
OPTIONS = {
    "upload": FilesActions.upload,
    "delete": FilesActions.delete,
    "download": FilesActions.download,
    "update": FilesActions.update,
    "ls": FilesActions.ls,
    "file metadata": FilesActions.file_metadata
}

async def execute_command(stub, session_id, user_input):
    user_option, *user_args = user_input.split()
    try:
        output = await OPTIONS[user_option](stub, session_id, *user_args)
        return output
    except KeyError:
        return "\nInvalid command. Available commands are: " + ", ".join(OPTIONS.keys())
    except TypeError or ValueError as e:
        return "\nAn error occurred while executing the command: " + str(e)
    except Exception as e:
        return "\nError: " + str(e)

def get_stub():
    channel = grpc.insecure_channel(HOST)
    stub = cloud_pb2_grpc.CloudStub(channel)    
    return stub

def main():
    if len(sys.argv) < 3:
        print("Usage: python command_executor.py <user_input> <session_id>")
        sys.exit(1)
    
    user_input = sys.argv[1]
    session_id = sys.argv[2]
    stub = get_stub()
    try:
        output = asyncio.run(execute_command(stub, session_id, user_input))
        print(output)
    except ValueError as e:
        print("Missing values to prforme the action")


if __name__ == "__main__":
    main()
