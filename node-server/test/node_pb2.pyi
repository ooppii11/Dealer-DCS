from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class UploadFileRequest(_message.Message):
    __slots__ = ("file_id", "type", "file_content", "user_id")
    FILE_ID_FIELD_NUMBER: _ClassVar[int]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    FILE_CONTENT_FIELD_NUMBER: _ClassVar[int]
    USER_ID_FIELD_NUMBER: _ClassVar[int]
    file_id: str
    type: str
    file_content: bytes
    user_id: int
    def __init__(self, file_id: _Optional[str] = ..., type: _Optional[str] = ..., file_content: _Optional[bytes] = ..., user_id: _Optional[int] = ...) -> None: ...

class UploadFileResponse(_message.Message):
    __slots__ = ("status", "message")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: bool
    message: str
    def __init__(self, status: bool = ..., message: _Optional[str] = ...) -> None: ...

class UpdateFileRequest(_message.Message):
    __slots__ = ("file_id", "new_content", "user_id")
    FILE_ID_FIELD_NUMBER: _ClassVar[int]
    NEW_CONTENT_FIELD_NUMBER: _ClassVar[int]
    USER_ID_FIELD_NUMBER: _ClassVar[int]
    file_id: str
    new_content: bytes
    user_id: int
    def __init__(self, file_id: _Optional[str] = ..., new_content: _Optional[bytes] = ..., user_id: _Optional[int] = ...) -> None: ...

class UpdateFileResponse(_message.Message):
    __slots__ = ("status", "message")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: bool
    message: str
    def __init__(self, status: bool = ..., message: _Optional[str] = ...) -> None: ...

class DownloadFileRequest(_message.Message):
    __slots__ = ("file_id", "user_id")
    FILE_ID_FIELD_NUMBER: _ClassVar[int]
    USER_ID_FIELD_NUMBER: _ClassVar[int]
    file_id: str
    user_id: int
    def __init__(self, file_id: _Optional[str] = ..., user_id: _Optional[int] = ...) -> None: ...

class DownloadFileResponse(_message.Message):
    __slots__ = ("status", "file_content", "message")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    FILE_CONTENT_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: bool
    file_content: bytes
    message: str
    def __init__(self, status: bool = ..., file_content: _Optional[bytes] = ..., message: _Optional[str] = ...) -> None: ...

class DeleteFileRequest(_message.Message):
    __slots__ = ("file_id", "user_id")
    FILE_ID_FIELD_NUMBER: _ClassVar[int]
    USER_ID_FIELD_NUMBER: _ClassVar[int]
    file_id: str
    user_id: int
    def __init__(self, file_id: _Optional[str] = ..., user_id: _Optional[int] = ...) -> None: ...

class DeleteFileResponse(_message.Message):
    __slots__ = ("status", "message")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: bool
    message: str
    def __init__(self, status: bool = ..., message: _Optional[str] = ...) -> None: ...
