import sys
import os
import subprocess
import asyncio
from option_actions import *

#python -m grpc_tools.protoc -I./protos --python_out=. --pyi_out=. --grpc_python_out=. ./protos/cloud.proto

HOST = "localhost:50053"
async def get_session_id(stub):
    while True:
        try:
            session_id = await AuthActions.get_user_credentials(stub)
            if session_id is not None:
                return session_id
        except Exception as e:
            print("Error:", e)

def get_stub():
    channel = grpc.insecure_channel(HOST)
    stub = cloud_pb2_grpc.CloudStub(channel)    
    return stub


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
            previous_process.wait()

        
        command = ['.venv\Scripts\python.exe', 'command_executor.py', user_input, str(session_id)]
        process =  subprocess.Popen(command, creationflags=subprocess.CREATE_NEW_CONSOLE)
        previous_process = process

if __name__ == '__main__':
    main()