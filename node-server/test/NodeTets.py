import grpc
import node_pb2
import node_pb2_grpc

FILE_NAME = "node_test1"
FILE_DATA = "test"
FILE_TYPE = "text/plain"

def main():
    channel = grpc.insecure_channel('localhost:50052')
    stub = node_pb2_grpc.NodeServicesStub(channel)
    try:
        upload_file_request = node_pb2.UploadFileRequest(file_id=FILE_NAME, file_content=FILE_DATA.encode(), type=FILE_TYPE, SecondReplicationPlace='place2', ThirdReplicationPlace='place3')
        upload_file_response = stub.UploadFile(iter([upload_file_request]))
        print(f'UploadFileResponse: {upload_file_response.message}')

        """
        # UpdateFileRequest
        update_file_request = node_pb2.UpdateFileRequest(file_id='file1', new_content=b'new content')
        update_file_response = stub.UpdateFile(update_file_request)
        print(f'UpdateFileResponse: {update_file_response.message}')
        """
        download_file_request = node_pb2.DownloadFileRequest(file_id=FILE_NAME)
        download_file_response = stub.DownloadFile(download_file_request)
        for response in download_file_response:
            print(f'DownloadFileResponse: {response.file_content.decode()}')

        #print(f'DownloadFileResponse: {download_file_response.message}')

        delete_file_request = node_pb2.DeleteFileRequest(file_id='file1')
        delete_file_response = stub.DeleteFile(delete_file_request)
        print(f'DeleteFileResponse: {delete_file_response.message}')
    except Exception as e:
        print(e)
    
    

if __name__ == '__main__':
    main()