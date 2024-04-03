from drive_helper.drive_helper import DriveServiceHelper 
from drive_helper.upload_file import UploadFile

class FileStorageManager:
    """Manages file storage operations in a specific region on Google Drive.

    Args:
        region_id (str): The identifier for the region.
        credentials_file (str): The path to the JSON file containing Google Drive API credentials.

    Attributes:
        _credentials_file (str): The path to the Google Drive API credentials file.
        _region_id (str): The identifier for the region.
        _drive: An authenticated Google Drive service connection.
        _region_folder_id (str): The folder ID associated with the specified region.

    Methods:
        upload_file(file_bytes: bytes, filename: str, mimetype: str) -> None:
            Uploads a file to the specified region on Google Drive.

        download_file(filename: str) -> bytes:
            Downloads a file from the specified region on Google Drive.

        delete_file(filename: str) -> None:
            Deletes a file from the specified region on Google Drive.

    Raises:
        Exception: If the region folder cannot be retrieved or created on Google Drive.
        Exception: If the file ID cannot be found during the download or delete operation.
        Exception: If authentication with Google Drive fails.
    """
    

    def __init__(self, region_id, credentials_file) -> None:
        """Initializes the FileStorageManager.

        Args:
            region_id (str): The identifier for the region.
            credentials_file (str): The path to the JSON file containing Google Drive API credentials.

        Raises:
            Exception: If the region folder cannot be retrieved or created on Google Drive.
            Exception: If authentication with Google Drive fails.
        """
        self._credentials_file = credentials_file
        self._region_id = region_id

        self._drive = DriveServiceHelper.authenticate(self._credentials_file)

        try:
            self._region_folder_id = DriveServiceHelper.get_folder_id(self._drive, self._region_id)
        except:
            self._region_folder_id = DriveServiceHelper.create_folder(self._drive, self._region_id)


    def upload_file(self, file_bytes: bytes, filename: str, mimetype: str) -> None:
        """Uploads a file to the specified region on Google Drive.

        Args:
            file_bytes (bytes): The content of the file to be uploaded.
            filename (str): The name of the file.
            mimetype (str): The MIME type of the file.

        Returns:
            None

        """
        self._drive = DriveServiceHelper.authenticate(self._credentials_file)
        file = UploadFile(file_bytes, filename, mimetype, self._region_folder_id)
        DriveServiceHelper.upload_file(self._drive, file)


    def download_file(self, filename: str) -> bytes:
        """Downloads a file from the specified region on Google Drive.

        Args:
            filename (str): The name of the file to be downloaded.

        Returns:
            bytes: The content of the downloaded file.

        Raises:
            Exception: If the file ID cannot be found.

        """
        drive = DriveServiceHelper.authenticate(self._credentials_file)
        file_id = DriveServiceHelper.get_file_id(drive, filename, self._region_folder_id)
        return DriveServiceHelper.download_file(drive, file_id)


    def delete_file(self, filename: str) -> None:
        """Deletes a file from the specified region on Google Drive.

        Args:
            filename (str): The name of the file to be deleted.

        Returns:
            None

        Raises:
            Exception: If the file ID cannot be found.

        """
        drive = DriveServiceHelper.authenticate(self._credentials_file)
        file_id = DriveServiceHelper.get_file_id(drive, filename, self._region_folder_id)
        print("file exsists")
        DriveServiceHelper.delete_file(drive, file_id)
