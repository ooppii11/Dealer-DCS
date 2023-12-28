from __future__ import print_function
import sys
sys.path.append('../') 

import logging
import grpc
import file_saving_microservice_pb2
import file_saving_microservice_pb2_grpc


FILE_NAME = "grpc_test1"
FILE_DATA = "test"
FILE_TYPE = "text/plain"


def upload(stub, name: str, data: bytes, mimeType:str):
    upload_file_request = file_saving_microservice_pb2.UploadFileRequest(file_name=name, file_data=data, type=mimeType)
    upload_file_reply = stub.UploadFile(iter([upload_file_request]))


def download(stub, name: str):
    download_file_request = file_saving_microservice_pb2.DownloadFileRequest(file_name=name)
    try:
        download_file_reply = stub.DownloadFile(download_file_request)
        for response in download_file_reply:
            return response.file_data.decode()
    except:
        print("Error occurred.")


def delete(stub, name: str):
    delete_file_request = file_saving_microservice_pb2.DeleteFileRequest(file_name=name)
    try:
        delete_file_reply = stub.DeleteFile(delete_file_request)
    except:
        print("file not found")    


def run():
    with grpc.insecure_channel("localhost:50051") as channel:
        stub = file_saving_microservice_pb2_grpc.FileCloudAccessStub(channel)
        print("-------------- Upload  --------------")
        upload(stub, FILE_NAME, FILE_DATA.encode(), FILE_TYPE)
        print(f"upload file '{FILE_NAME}' with value '{FILE_DATA}'")
        print("-------------- Download  --------------")
        print(f"file value: '{download(stub, FILE_NAME)}'")
        print("-------------- Delete  --------------")
        delete(stub, FILE_NAME)



if __name__ == "__main__":
    logging.basicConfig()
    run()