import multiprocessing
import asyncio
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

async def get_session_id(stub):
    while True:
        try:
            session_id = await AuthActions.get_user_credentials(stub)
            if session_id is not None:
                return session_id
        except Exception as e:
            print("Error:", e)

async def execute_command(stub, session_id, user_input):
    user_option, *user_args = user_input.split()
    try:
        await OPTIONS[user_option](stub, session_id, *user_args)
    except KeyError:
        print("\nInvalid command. Available commands are:", list(OPTIONS.keys()))
        return
    except TypeError or ValueError as e:
        print("\nAn error occurred while executing the command:", user_option)
        return
    except Exception as e:
        print("Error:", e)
        return

    

def get_stub():
    channel = grpc.insecure_channel(HOST)
    stub = cloud_pb2_grpc.CloudStub(channel)    
    return stub

def start_task(user_input, session_id):
    stub = get_stub()
    asyncio.run(execute_command(stub, session_id, user_input))


def main():
    stub = get_stub()
    session_id = asyncio.run(get_session_id(stub))
    previous_process = None
    
    while True:
        user_input = input(">> ")
        if "logout" in user_input.lower():
            AuthActions.logout(stub, session_id)
            exit()

        if previous_process is not None:
            previous_process.join()

        process = multiprocessing.Process(target=start_task, args=(user_input, session_id))
        process.start()
        previous_process = process


if __name__ == "__main__":
    main()
