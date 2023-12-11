from google.protobuf import timestamp_pb2 as _timestamp_pb2
from google.protobuf.internal import containers as _containers
from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Iterable as _Iterable, Mapping as _Mapping, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class Status(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = []
    SUCCESS: _ClassVar[Status]
    FAILURE: _ClassVar[Status]
SUCCESS: Status
FAILURE: Status

class FileMetadata(_message.Message):
    __slots__ = ["filename", "type", "size", "creationDate", "lastModified"]
    FILENAME_FIELD_NUMBER: _ClassVar[int]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    SIZE_FIELD_NUMBER: _ClassVar[int]
    CREATIONDATE_FIELD_NUMBER: _ClassVar[int]
    LASTMODIFIED_FIELD_NUMBER: _ClassVar[int]
    filename: str
    type: str
    size: int
    creationDate: _timestamp_pb2.Timestamp
    lastModified: _timestamp_pb2.Timestamp
    def __init__(self, filename: _Optional[str] = ..., type: _Optional[str] = ..., size: _Optional[int] = ..., creationDate: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ..., lastModified: _Optional[_Union[_timestamp_pb2.Timestamp, _Mapping]] = ...) -> None: ...

class LoginRequest(_message.Message):
    __slots__ = ["username", "password"]
    USERNAME_FIELD_NUMBER: _ClassVar[int]
    PASSWORD_FIELD_NUMBER: _ClassVar[int]
    username: str
    password: str
    def __init__(self, username: _Optional[str] = ..., password: _Optional[str] = ...) -> None: ...

class LoginResponse(_message.Message):
    __slots__ = ["status", "sessionId"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    status: Status
    sessionId: str
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., sessionId: _Optional[str] = ...) -> None: ...

class SignupRequest(_message.Message):
    __slots__ = ["username", "password", "email", "phoneNumber"]
    USERNAME_FIELD_NUMBER: _ClassVar[int]
    PASSWORD_FIELD_NUMBER: _ClassVar[int]
    EMAIL_FIELD_NUMBER: _ClassVar[int]
    PHONENUMBER_FIELD_NUMBER: _ClassVar[int]
    username: str
    password: str
    email: str
    phoneNumber: str
    def __init__(self, username: _Optional[str] = ..., password: _Optional[str] = ..., email: _Optional[str] = ..., phoneNumber: _Optional[str] = ...) -> None: ...

class SignupResponse(_message.Message):
    __slots__ = ["status", "message"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ...) -> None: ...

class LogoutRequest(_message.Message):
    __slots__ = ["sessionId"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    def __init__(self, sessionId: _Optional[str] = ...) -> None: ...

class LogoutResponse(_message.Message):
    __slots__ = []
    def __init__(self) -> None: ...

class UploadFileRequest(_message.Message):
    __slots__ = ["sessionId", "fileName", "type", "fileData"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    FILENAME_FIELD_NUMBER: _ClassVar[int]
    TYPE_FIELD_NUMBER: _ClassVar[int]
    FILEDATA_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    fileName: str
    type: str
    fileData: bytes
    def __init__(self, sessionId: _Optional[str] = ..., fileName: _Optional[str] = ..., type: _Optional[str] = ..., fileData: _Optional[bytes] = ...) -> None: ...

class UploadFileResponse(_message.Message):
    __slots__ = ["status", "message"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ...) -> None: ...

class DownloadFileRequest(_message.Message):
    __slots__ = ["sessionId", "fileName"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    FILENAME_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    fileName: str
    def __init__(self, sessionId: _Optional[str] = ..., fileName: _Optional[str] = ...) -> None: ...

class DownloadFileResponse(_message.Message):
    __slots__ = ["status", "message", "fileData"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    FILEDATA_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    fileData: bytes
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ..., fileData: _Optional[bytes] = ...) -> None: ...

class DeleteFileRequest(_message.Message):
    __slots__ = ["sessionId", "fileName"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    FILENAME_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    fileName: str
    def __init__(self, sessionId: _Optional[str] = ..., fileName: _Optional[str] = ...) -> None: ...

class DeleteFileResponse(_message.Message):
    __slots__ = ["status", "message"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ...) -> None: ...

class GetListOfFilesRequest(_message.Message):
    __slots__ = ["sessionId"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    def __init__(self, sessionId: _Optional[str] = ...) -> None: ...

class GetListOfFilesResponse(_message.Message):
    __slots__ = ["status", "message", "files"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    FILES_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    files: _containers.RepeatedCompositeFieldContainer[FileMetadata]
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ..., files: _Optional[_Iterable[_Union[FileMetadata, _Mapping]]] = ...) -> None: ...

class GetFileMetadataRequest(_message.Message):
    __slots__ = ["sessionId", "fileName"]
    SESSIONID_FIELD_NUMBER: _ClassVar[int]
    FILENAME_FIELD_NUMBER: _ClassVar[int]
    sessionId: str
    fileName: str
    def __init__(self, sessionId: _Optional[str] = ..., fileName: _Optional[str] = ...) -> None: ...

class GetFileMetadataResponse(_message.Message):
    __slots__ = ["status", "message", "file"]
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    FILE_FIELD_NUMBER: _ClassVar[int]
    status: Status
    message: str
    file: FileMetadata
    def __init__(self, status: _Optional[_Union[Status, str]] = ..., message: _Optional[str] = ..., file: _Optional[_Union[FileMetadata, _Mapping]] = ...) -> None: ...
