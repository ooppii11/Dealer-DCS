import io
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseUpload, MediaIoBaseDownload
import googleapiclient.errors
from DriveHelper.UploadFile import UploadFile


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


    @staticmethod    
    def get_file_id(drive_connection, filename:str, folder_id:str) -> str:
        """Retrieve a file ID from the Google Drive based on the file name and folder ID.

        Args:
            drive_connection (googleapiclient.discovery.Resource): An authenticated connection to the Google Drive API.
            filename (str): The name of the file to be search.
            folder_id (str): The id of the folder to be searched.
        
        Returns:
            str: The id of the file. 

        Raises:
            Exception: googleapiclient.errors.HttpError

        """
        query = f"name ='{filename}' and '{folder_id}' in parents" # Query to search for the file in the specified folder
        response = drive_connection.files().list(q=query).execute()

        try:
            return response.get("files", [])[0]["id"]  # Get file id from the response
        except (IndexError, KeyError):
            raise Exception(f"Error retrieving file ID. The file with name '{filename}' in folder '{folder_id}' was not found in Google Drive.")
    

    @staticmethod
    def get_folder_id(drive_connection, folder_name:str) -> str:
        """Retrieve the ID of a folder in the Google Drive based on the folder name

        Args:
            drive_connection (googleapiclient.discovery.Resource): An authenticated connection to the Google Drive API.
            folder_name (str): The name of the folder to be search.
        
        Returns:
            str: The id of the folder. 

        Raises:
            Exception: googleapiclient.errors.HttpError

        """
        # Query to search for the folder:
        query = f"name ='{folder_name}' and mimeType = 'application/vnd.google-apps.folder'"
        
        response = drive_connection.files().list(q=query).execute()
        
        try:
           return response.get("files", [])[0]["id"]  # Get folder id from the response
        except (IndexError, KeyError):
            raise Exception("Error retrieving folder ID. The folder with name '{folder_name}' was not found in Google Drive.")
 

    @staticmethod
    def create_folder(drive_connection, folder_name:str) -> str:
        """Create a new folder in the Google Drive with the specified name.

        Args:
            drive_connection (googleapiclient.discovery.Resource): An authenticated connection to the Google Drive API.
            folder_name (str): The name of the folder to be created.

        Returns:
            str: The id of the folder. 

        Raises:
            Exception: googleapiclient.errors.HttpError
   
        """
        # Create folder metdata:
        metadata = {
            "name": folder_name,
            "mimeType": "application/vnd.google-apps.folder"
        }
        try:
            folder = drive_connection.files().create(body=metadata, fields="id").execute()
            return folder.get("id")
        except googleapiclient.errors.HttpError as e:
            raise Exception(f"Error creating folder. Please ensure that the folder with name '{folder_name}' could be created in Google Drive.")
    
    
    @staticmethod
    def upload_file(drive_connection, upload_file:UploadFile) -> None:
        """Uploads a file to the Google Drive.
        
        Args:
            drive_connection (googleapiclient.discovery.Resource): An authenticated connection to the Google Drive API.
            upload_file (UploadFile): An object representing the file to be uploaded.
        
        Returns:
            None

        Raises:
            Exception: googleapiclient.errors.HttpError: If there is an error during the file upload process.

        """
        # Load the file bytes: 
        media = MediaIoBaseUpload(
            upload_file.file_bytes,
            mimetype=upload_file.mimetype,
            chunksize=DriveServiceHelper.CHUNKSIZE, 
            resumable=True
        )        
        
        # Create file metdata:
        file_metadata = {
            "name": upload_file.filename,
            "parents": [upload_file.folder_id],
            "mimeType": upload_file.mimetype
        }

        try:
            # Create a request to create the file and upload its content:
            uploaded_file = drive_connection.files().create(
                body=file_metadata,
                media_body=media,
            ).execute()
        except googleapiclient.errors.HttpError as e:
            raise Exception(f"Error uploading file '{upload_file.filename}'. Please check if the file could be uploaded to the specified folder.")

    
    @staticmethod
    def download_file(drive_connection, file_id:str) -> bytes:
        """Downloads the content of a file from the Google Drive based on the file ID.

        Args:
            drive_connection (googleapiclient.discovery.Resource): An authenticated connection to the Google Drive API.
            file_id (str): The id of the file to be downloaded.

        Returns:
            bytes: The content of the downloaded file.

        Raises:
            Exception: If the file ID not exists.
            Exception: googleapiclient.errors.HttpError: If there is an error during the file download process.

        """
        # Create a request to get the media content of the file:
        request = drive_connection.files().get_media(fileId=file_id)
        
        file = io.BytesIO()

        # Initialize a MediaIoBaseDownload object for efficient file content download:
        downloader = MediaIoBaseDownload(file, request)

        try:
            done = False

            # Continue downloading chunks until done:
            while done is False:
                status, done = downloader.next_chunk()
        except googleapiclient.errors.HttpError as e:
            raise Exception(f"Error downloading file with ID '{file_id}'. The file may not exist or there was an issue retrieving its content.")

        return file.getvalue()