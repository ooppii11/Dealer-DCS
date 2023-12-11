import io


class UploadFile:
    def __init__(self, file_bytes: bytes, filename:str, mimetype:str, folder_id:str):
        self.file_bytes = io.BytesIO(file_bytes)
        self.filename = filename
        self.mimetype = mimetype
        self.folder_id = folder_id