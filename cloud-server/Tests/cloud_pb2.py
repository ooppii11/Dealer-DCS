# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: cloud.proto
"""Generated protocol buffer code."""
from google.protobuf import descriptor as _descriptor
from google.protobuf import descriptor_pool as _descriptor_pool
from google.protobuf import symbol_database as _symbol_database
from google.protobuf.internal import builder as _builder
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()


from google.protobuf import timestamp_pb2 as google_dot_protobuf_dot_timestamp__pb2


DESCRIPTOR = _descriptor_pool.Default().AddSerializedFile(b'\n\x0b\x63loud.proto\x1a\x1fgoogle/protobuf/timestamp.proto\"\xa0\x01\n\x0c\x46ileMetadata\x12\x10\n\x08\x66ilename\x18\x01 \x01(\t\x12\x0c\n\x04type\x18\x02 \x01(\t\x12\x0c\n\x04size\x18\x04 \x01(\x05\x12\x30\n\x0c\x63reationDate\x18\x05 \x01(\x0b\x32\x1a.google.protobuf.Timestamp\x12\x30\n\x0clastModified\x18\x06 \x01(\x0b\x32\x1a.google.protobuf.Timestamp\"2\n\x0cLoginRequest\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\";\n\rLoginResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x11\n\tsessionId\x18\x02 \x01(\t\"W\n\rSignupRequest\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\x12\r\n\x05\x65mail\x18\x03 \x01(\t\x12\x13\n\x0bphoneNumber\x18\x04 \x01(\t\":\n\x0eSignupResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\"\"\n\rLogoutRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\"\x10\n\x0eLogoutResponse\"X\n\x11UploadFileRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\x12\x10\n\x08\x66ileName\x18\x02 \x01(\t\x12\x0c\n\x04type\x18\x03 \x01(\t\x12\x10\n\x08\x66ileData\x18\x04 \x01(\x0c\">\n\x12UploadFileResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\":\n\x13\x44ownloadFileRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\x12\x10\n\x08\x66ileName\x18\x02 \x01(\t\"R\n\x14\x44ownloadFileResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\x12\x10\n\x08\x66ileData\x18\x03 \x01(\x0c\"8\n\x11\x44\x65leteFileRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\x12\x10\n\x08\x66ileName\x18\x02 \x01(\t\">\n\x12\x44\x65leteFileResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\"*\n\x15GetListOfFilesRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\"`\n\x16GetListOfFilesResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\x12\x1c\n\x05\x66iles\x18\x03 \x03(\x0b\x32\r.FileMetadata\"=\n\x16GetFileMetadataRequest\x12\x11\n\tsessionId\x18\x01 \x01(\t\x12\x10\n\x08\x66ileName\x18\x02 \x01(\t\"`\n\x17GetFileMetadataResponse\x12\x17\n\x06status\x18\x01 \x01(\x0e\x32\x07.Status\x12\x0f\n\x07message\x18\x02 \x01(\t\x12\x1b\n\x04\x66ile\x18\x03 \x01(\x0b\x32\r.FileMetadata*\"\n\x06Status\x12\x0b\n\x07SUCCESS\x10\x00\x12\x0b\n\x07\x46\x41ILURE\x10\x01\x32\xbd\x03\n\x05\x43loud\x12&\n\x05login\x12\r.LoginRequest\x1a\x0e.LoginResponse\x12)\n\x06signup\x12\x0e.SignupRequest\x1a\x0f.SignupResponse\x12)\n\x06logout\x12\x0e.LogoutRequest\x1a\x0f.LogoutResponse\x12\x41\n\x0egetListOfFiles\x12\x16.GetListOfFilesRequest\x1a\x17.GetListOfFilesResponse\x12\x44\n\x0fgetFileMetadata\x12\x17.GetFileMetadataRequest\x1a\x18.GetFileMetadataResponse\x12\x37\n\nUploadFile\x12\x12.UploadFileRequest\x1a\x13.UploadFileResponse(\x01\x12=\n\x0c\x44ownloadFile\x12\x14.DownloadFileRequest\x1a\x15.DownloadFileResponse0\x01\x12\x35\n\nDeleteFile\x12\x12.DeleteFileRequest\x1a\x13.DeleteFileResponseB\x0c\xaa\x02\tGrpcCloudb\x06proto3')

_globals = globals()
_builder.BuildMessageAndEnumDescriptors(DESCRIPTOR, _globals)
_builder.BuildTopDescriptorsAndMessages(DESCRIPTOR, 'cloud_pb2', _globals)
if _descriptor._USE_C_DESCRIPTORS == False:
  DESCRIPTOR._options = None
  DESCRIPTOR._serialized_options = b'\252\002\tGrpcCloud'
  _globals['_STATUS']._serialized_start=1250
  _globals['_STATUS']._serialized_end=1284
  _globals['_FILEMETADATA']._serialized_start=49
  _globals['_FILEMETADATA']._serialized_end=209
  _globals['_LOGINREQUEST']._serialized_start=211
  _globals['_LOGINREQUEST']._serialized_end=261
  _globals['_LOGINRESPONSE']._serialized_start=263
  _globals['_LOGINRESPONSE']._serialized_end=322
  _globals['_SIGNUPREQUEST']._serialized_start=324
  _globals['_SIGNUPREQUEST']._serialized_end=411
  _globals['_SIGNUPRESPONSE']._serialized_start=413
  _globals['_SIGNUPRESPONSE']._serialized_end=471
  _globals['_LOGOUTREQUEST']._serialized_start=473
  _globals['_LOGOUTREQUEST']._serialized_end=507
  _globals['_LOGOUTRESPONSE']._serialized_start=509
  _globals['_LOGOUTRESPONSE']._serialized_end=525
  _globals['_UPLOADFILEREQUEST']._serialized_start=527
  _globals['_UPLOADFILEREQUEST']._serialized_end=615
  _globals['_UPLOADFILERESPONSE']._serialized_start=617
  _globals['_UPLOADFILERESPONSE']._serialized_end=679
  _globals['_DOWNLOADFILEREQUEST']._serialized_start=681
  _globals['_DOWNLOADFILEREQUEST']._serialized_end=739
  _globals['_DOWNLOADFILERESPONSE']._serialized_start=741
  _globals['_DOWNLOADFILERESPONSE']._serialized_end=823
  _globals['_DELETEFILEREQUEST']._serialized_start=825
  _globals['_DELETEFILEREQUEST']._serialized_end=881
  _globals['_DELETEFILERESPONSE']._serialized_start=883
  _globals['_DELETEFILERESPONSE']._serialized_end=945
  _globals['_GETLISTOFFILESREQUEST']._serialized_start=947
  _globals['_GETLISTOFFILESREQUEST']._serialized_end=989
  _globals['_GETLISTOFFILESRESPONSE']._serialized_start=991
  _globals['_GETLISTOFFILESRESPONSE']._serialized_end=1087
  _globals['_GETFILEMETADATAREQUEST']._serialized_start=1089
  _globals['_GETFILEMETADATAREQUEST']._serialized_end=1150
  _globals['_GETFILEMETADATARESPONSE']._serialized_start=1152
  _globals['_GETFILEMETADATARESPONSE']._serialized_end=1248
  _globals['_CLOUD']._serialized_start=1287
  _globals['_CLOUD']._serialized_end=1732
# @@protoc_insertion_point(module_scope)
