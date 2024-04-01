import io
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseUpload, MediaIoBaseDownload
import googleapiclient.errors

def authenticate(credentials_file_path):
    credentials = service_account.Credentials.from_service_account_file(
    credentials_file_path, scopes=['https://www.googleapis.com/auth/drive'])

    try:
        drive = build(
            "drive",
           "v3",
            credentials=credentials
        )
        return drive
    except googleapiclient.errors.HttpError as e:
        raise Exception("Failed to authenticate with Google Drive API. Please check if the provided credentials file is valid and has the necessary permissions.")

def delete_all_files():
    service = authenticate("dealer-dcs-150291856e98.json")

    # Get all files
    results = service.files().list(
        pageSize=1000, fields="files(id, name)").execute()
    items = results.get('files', [])

    if not items:
        print('No files found.')
    else:
        print('Files:')
        for item in items:
            print(f"{item['name']} ({item['id']})")
            service.files().delete(fileId=item['id']).execute()
           # print(f"File {item['name']} deleted.")

if __name__ == '__main__':
    delete_all_files()
