from google.oauth2 import service_account
from googleapiclient.discovery import build
import googleapiclient.errors


class DriveServiceHelper:
    """
    A utility class for interacting with the Google Drive API.

    This class provides methods for authenticating with the Google Drive API, managing files and folders,
    and performing file upload, download, and deletion operations.

    Attributes:
        None

    Methods:
        authenticate(credentials_file_path: str) -> googleapiclient.discovery.Resource:
            Authenticates the Google Drive API using the specified credentials file.

        get_file_id(drive_connection: googleapiclient.discovery.Resource, filename: str, folder_id: str) -> str:
            Retrieves the ID of a file in the Google Drive based on the file name and folder ID.

        create_folder(drive_connection: googleapiclient.discovery.Resource, folder_name: str) -> str:
            Creates a new folder in the Google Drive with the specified name.

        get_folder_id(drive_connection: googleapiclient.discovery.Resource, folder_name: str) -> str:
            Retrieves the ID of a folder in the Google Drive based on the folder name.

        upload_file(drive_connection: googleapiclient.discovery.Resource, upload_file: UploadFile) -> None:
            Uploads a file to the Google Drive.

        download_file(drive_connection: googleapiclient.discovery.Resource, file_id: str) -> bytes:
            Downloads the content of a file from the Google Drive based on the file ID.

        delete_file(drive_connection: googleapiclient.discovery.Resource, file_id: str) -> None:
            Deletes a file from the Google Drive based on the file ID.

    Raises:
        Exception: googleapiclient.errors.HttpError: Raised if there is an error during API operations.
        Exception: Raised in specific cases such as file not found or folder not existing.
    """
   

    DRIVE_VERSION = "v3"
    GOOGLE_SERVICE = "drive"
    CHUNKSIZE = 1024


    @staticmethod
    def authenticate(credentials_file_path:str):
        """Authenticate the Google Drive API.

        Args:
            credentials_file_path (str): The path to the credentials file

        Returns:
            googleapiclient.discovery.Resource: The authenticated Google Drive API service.
        
        Raises:
            Exception: googleapiclient.errors.HttpError
        
        """
        credentials = service_account.Credentials.from_service_account_file(
            credentials_file_path, scopes=['https://www.googleapis.com/auth/drive'])

        try:
            # Create authenticate connection to Google Drive
            drive = build(
                DriveServiceHelper.GOOGLE_SERVICE,
                DriveServiceHelper.DRIVE_VERSION,
                credentials=credentials
            )
            return drive
        except googleapiclient.errors.HttpError as e:
            raise Exception("Failed to authenticate with Google Drive API. Please check if the provided credentials file is valid and has the necessary permissions.")
