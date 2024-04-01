from google.protobuf.internal import enum_type_wrapper as _enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class Status(int, metaclass=_enum_type_wrapper.EnumTypeWrapper):
    __slots__ = ()
    SUCCESS: _ClassVar[Status]
    FAILURE: _ClassVar[Status]
SUCCESS: Status
FAILURE: Status

class LeaderToViewerHeartBeatRequest(_message.Message):
    __slots__ = ("term", "systemLastIndex", "LeaderAddress")
    TERM_FIELD_NUMBER: _ClassVar[int]
    SYSTEMLASTINDEX_FIELD_NUMBER: _ClassVar[int]
    LEADERADDRESS_FIELD_NUMBER: _ClassVar[int]
    term: int
    systemLastIndex: int
    LeaderAddress: str
    def __init__(self, term: _Optional[int] = ..., systemLastIndex: _Optional[int] = ..., LeaderAddress: _Optional[str] = ...) -> None: ...

class LeaderToViewerHeartBeatResponse(_message.Message):
    __slots__ = ("status", "message")
    STATUS_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    status: bool
    message: str
    def __init__(self, status: bool = ..., message: _Optional[str] = ...) -> None: ...
