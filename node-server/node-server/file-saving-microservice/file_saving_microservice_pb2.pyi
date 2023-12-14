from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class UploadFileRequest(_message.Message):
    __slots__ = ["file_name", "type", "file_data"]
    FILE_NAME_FIELD_NUMBER: _ClassVar[int]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    FILE_DATA_FIELD_NUMBER: _ClassVar[int]
    file_name: str
    type: str
    file_data: bytes
    def __init__(self, file_name: _Optional[str] = ..., type: _Optional[str] = ..., file_data: _Optional[bytes] = ...) -> None: ...

class UploadFileResponse(_message.Message):
    __slots__ = []
    def __init__(self) -> None: ...

class DownloadFileRequest(_message.Message):
    __slots__ = ["file_name"]
    FILE_NAME_FIELD_NUMBER: _ClassVar[int]
    file_name: str
    def __init__(self, file_name: _Optional[str] = ...) -> None: ...

class DownloadFileResponse(_message.Message):
    __slots__ = ["file_data"]
    FILE_DATA_FIELD_NUMBER: _ClassVar[int]
    file_data: bytes
    def __init__(self, file_data: _Optional[bytes] = ...) -> None: ...

class DeleteFileRequest(_message.Message):
    __slots__ = ["file_name"]
    FILE_NAME_FIELD_NUMBER: _ClassVar[int]
    file_name: str
    def __init__(self, file_name: _Optional[str] = ...) -> None: ...

class DeleteFileResponse(_message.Message):
    __slots__ = []
    def __init__(self) -> None: ...
