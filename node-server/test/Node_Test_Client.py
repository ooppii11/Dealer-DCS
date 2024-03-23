import grpc
#python -m grpc_tools.protoc -I. --python_out=. --pyi_out=. --grpc_python_out=. ./node.proto
import node_pb2
import node_pb2_grpc
#python -m grpc_tools.protoc -I. --python_out=. --pyi_out=. --grpc_python_out=. ./cloud.proto
import grpc
import cloud_pb2
import cloud_pb2_grpc

import threading
from concurrent import futures
import signal



LEADER_ADDRESS = ""
condition = threading.Condition()
lock = threading.Lock()
terminate_signal = threading.Event()

class CloudServicer(cloud_pb2_grpc.CloudServicer):
    def GetOrUpdateSystemLeader(self, request, context):
        global LEADER_ADDRESS
        lock.acquire()
        LEADER_ADDRESS = request.LeaderAddress
        lock.release()
        condition.notify()
        response = cloud_pb2.LeaderToViewerHeartBeatResponse(status=True, message="Leader found")
        return response

def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    cloud_pb2_grpc.add_CloudServicer_to_server(CloudServicer(), server)
    server.add_insecure_port('127.0.0.1:50053')
    server.start()
    signal.signal(signal.SIGINT, lambda signum, frame: terminate_signal.set())
    server.wait_for_termination()

if __name__ == '__main__':
    serve()


def upload_file(stub):
    file_id = input("Enter file ID: ")
    file_type = input("Enter file type: ")
    file_content = input("Enter file content: ").encode()
    user_id = int(input("Enter user ID: "))

    response = stub.UploadFile(node_pb2.UploadFileRequest(
        file_id=file_id,
        type=file_type,
        file_content=file_content,
        user_id=user_id
    ))
    print("Upload Response:", response)

def download_file(stub):
    file_id = input("Enter file ID: ")
    user_id = int(input("Enter user ID: "))

    response = stub.DownloadFile(node_pb2.DownloadFileRequest(
        file_id=file_id,
        user_id=user_id
    ))
    print("Download Response:", response)

def update_file(stub):
    file_id = input("Enter file ID: ")
    new_content = input("Enter new content: ").encode()
    user_id = int(input("Enter user ID: "))

    response = stub.UpdateFile(node_pb2.UpdateFileRequest(
        file_id=file_id,
        new_content=new_content,
        user_id=user_id
    ))
    print("Update Response:", response)

def delete_file(stub):
    file_id = input("Enter file ID: ")
    user_id = int(input("Enter user ID: "))

    response = stub.DeleteFile(node_pb2.DeleteFileRequest(
        file_id=file_id,
        user_id=user_id
    ))
    print("Delete Response:", response)

def run_client():    
    global LEADER_ADDRESS
    while True:
        try:
            condition.wait()
            channel = grpc.insecure_channel(LEADER_ADDRESS)
            stub = node_pb2_grpc.NodeServicesStub(channel)
            print("Select an option:")
            print("1. Upload File")
            print("2. Download File")
            print("3. Update File")
            print("4. Delete File")
            print("0. Exit")
            choice = input("Enter your choice: ")

            if choice == "1":
                upload_file(stub)
            elif choice == "2":
                download_file(stub)
            elif choice == "3":
                update_file(stub)
            elif choice == "4":
                delete_file(stub)
            elif choice == "0":
                break
            else:
                print("Invalid choice. Please try again.")
        
        except grpc.RpcError as e:
            print(f"Failed to create channel: {e}")

        
    

if __name__ == '__main__':
    client_thread = threading.Thread(target=run_client)
    server_thread = threading.Thread(target=serve)

    server_thread.start()
    client_thread.start()

    server_thread.join()
    terminate_signal.set() 
    client_thread.join()