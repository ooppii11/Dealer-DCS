import logging
import grpc
import os
from concurrent import futures
from file_storage_manager import FileStorageManager
import file_saving_microservice_pb2
import file_saving_microservice_pb2_grpc 


#REGION_ID = os.getenv('REGION_ID')
REGION_ID = "X"
CREDENTIALS_FILE = os.path.join(os.path.dirname(__file__), "dealer-dcs-150291856e98.json")


class FileCloudAccessServicer(file_saving_microservice_pb2_grpc.FileCloudAccessServicer):
    """Implements the FileCloudAccess service."""
    
    def __init__(self, file_storage_manager: FileStorageManager) -> None:
        """Initialize the FileCloudAccessServicer.

        Args:
            file_storage_manager (FileStorageManager): The file storage manager.
        """
        self._file_storage_manager = file_storage_manager


    def UploadFile(self, request_iterator, context):
        """Upload a file to the cloud.

        Args:
            request_iterator: An iterator of UploadFileRequest.
            context: The RPC context.

        Returns:
            UploadFileResponse: The response indicating the success of the operation.
        """

        print("upload")
        try:
            for request in request_iterator:
                file_name = request.file_name
                file_data = request.file_data
                file_type = request.type

            
                # Call the file storage manager to upload the file
            
                self._file_storage_manager.upload_file(file_data, file_name, file_type)
        except Exception as e:
            print(e)
        response = file_saving_microservice_pb2.UploadFileResponse()
        return response


    def DownloadFile(self, request, context):
        """Download a file from the cloud.

        Args:
            request: DownloadFileRequest containing the file name.
            context: The RPC context.

        Yields:
            DownloadFileResponse: The response containing the file data.
        """
        try:
            # Download the file from the file storage manager
            file_data = self._file_storage_manager.download_file(request.file_name)

            if file_data is not None:
                response = file_saving_microservice_pb2.DownloadFileResponse(file_data=file_data)
                yield response
            else:
                context.set_code(grpc.StatusCode.NOT_FOUND)
                context.set_details("Error: File not found")
        except Exception as e:
            # Handle other exceptions (e.g., network errors, unexpected issues)
            context.set_code(grpc.StatusCode.UNKNOWN)
            context.set_details(f"Error: {str(e)}")
            

    def DeleteFile(self, request, context):
        """Delete a file from the cloud.

        Args:
            request: DeleteFileRequest containing the file name.
            context: The RPC context.

        Returns:
            DeleteFileResponse: The response indicating the success of the operation.
        """
        response = file_saving_microservice_pb2.DeleteFileResponse()
        try:
            print("delete")
            # Delete the file using the file storage manager
            self._file_storage_manager.delete_file(request.file_name)
        except FileNotFoundError:
            context.set_code(grpc.StatusCode.NOT_FOUND)
            context.set_details("Error: File not found")
        except Exception as e:
            context.set_code(grpc.StatusCode.UNKNOWN)
            context.set_details(f"Error: {str(e)}")

        return response


def serve():
    """Start the gRPC server."""
    # Create a gRPC server with a thread pool
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    
    # Add the FileCloudAccessServicer to the server
    file_saving_microservice_pb2_grpc.add_FileCloudAccessServicer_to_server(
        FileCloudAccessServicer(FileStorageManager(REGION_ID, CREDENTIALS_FILE)), server
    )
    
    # Add an insecure port to the server and start it
    server.add_insecure_port("localhost:50051")
    server.start()
    
    # Wait for the server to terminate
    server.wait_for_termination()


if __name__ == "__main__":
    # Configure logging and start the server
    logging.basicConfig()
    serve()
