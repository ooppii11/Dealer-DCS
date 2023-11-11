import logging
import grpc
from concurrent import futures
from file_storage_manager import FileStorageManager
import file_saving_microservice_pb2
import file_saving_microservice_pb2_grpc 


REGION_ID = ""
CREDENTIALS_FILE = ""


class FileCloudAccessServicer(file_saving_microservice_pb2_grpc.FileCloudAccessServicer):
    """Implements the FileCloudAccess service."""
    
    def __init__(self, file_storage_manager: FileStorageManager) -> None:
        """Initialize the FileCloudAccessServicer.

        Args:
            file_storage_manager (FileStorageManager): The file storage manager.
        """
        self._file_storage_manager = file_storage_manager



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
