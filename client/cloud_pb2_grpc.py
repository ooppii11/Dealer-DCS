# Generated by the gRPC Python protocol compiler plugin. DO NOT EDIT!
"""Client and server classes corresponding to protobuf-defined services."""
import grpc

import cloud_pb2 as cloud__pb2


class CloudStub(object):
    """Missing associated documentation comment in .proto file."""

    def __init__(self, channel):
        """Constructor.

        Args:
            channel: A grpc.Channel.
        """
        self.login = channel.unary_unary(
                '/Cloud/login',
                request_serializer=cloud__pb2.LoginRequest.SerializeToString,
                response_deserializer=cloud__pb2.LoginResponse.FromString,
                )
        self.signup = channel.unary_unary(
                '/Cloud/signup',
                request_serializer=cloud__pb2.SignupRequest.SerializeToString,
                response_deserializer=cloud__pb2.SignupResponse.FromString,
                )
        self.logout = channel.unary_unary(
                '/Cloud/logout',
                request_serializer=cloud__pb2.LogoutRequest.SerializeToString,
                response_deserializer=cloud__pb2.LogoutResponse.FromString,
                )
        self.getListOfFiles = channel.unary_unary(
                '/Cloud/getListOfFiles',
                request_serializer=cloud__pb2.GetListOfFilesRequest.SerializeToString,
                response_deserializer=cloud__pb2.GetListOfFilesResponse.FromString,
                )
        self.getFileMetadata = channel.unary_unary(
                '/Cloud/getFileMetadata',
                request_serializer=cloud__pb2.GetFileMetadataRequest.SerializeToString,
                response_deserializer=cloud__pb2.GetFileMetadataResponse.FromString,
                )
        self.UploadFile = channel.stream_unary(
                '/Cloud/UploadFile',
                request_serializer=cloud__pb2.UploadFileRequest.SerializeToString,
                response_deserializer=cloud__pb2.UploadFileResponse.FromString,
                )
        self.DownloadFile = channel.unary_stream(
                '/Cloud/DownloadFile',
                request_serializer=cloud__pb2.DownloadFileRequest.SerializeToString,
                response_deserializer=cloud__pb2.DownloadFileResponse.FromString,
                )
        self.DeleteFile = channel.unary_unary(
                '/Cloud/DeleteFile',
                request_serializer=cloud__pb2.DeleteFileRequest.SerializeToString,
                response_deserializer=cloud__pb2.DeleteFileResponse.FromString,
                )


class CloudServicer(object):
    """Missing associated documentation comment in .proto file."""

    def login(self, request, context):
        """Auth:
        """
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def signup(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def logout(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def getListOfFiles(self, request, context):
        """Metadata:
        """
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def getFileMetadata(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def UploadFile(self, request_iterator, context):
        """File methods:
        """
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def DownloadFile(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')

    def DeleteFile(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')


def add_CloudServicer_to_server(servicer, server):
    rpc_method_handlers = {
            'login': grpc.unary_unary_rpc_method_handler(
                    servicer.login,
                    request_deserializer=cloud__pb2.LoginRequest.FromString,
                    response_serializer=cloud__pb2.LoginResponse.SerializeToString,
            ),
            'signup': grpc.unary_unary_rpc_method_handler(
                    servicer.signup,
                    request_deserializer=cloud__pb2.SignupRequest.FromString,
                    response_serializer=cloud__pb2.SignupResponse.SerializeToString,
            ),
            'logout': grpc.unary_unary_rpc_method_handler(
                    servicer.logout,
                    request_deserializer=cloud__pb2.LogoutRequest.FromString,
                    response_serializer=cloud__pb2.LogoutResponse.SerializeToString,
            ),
            'getListOfFiles': grpc.unary_unary_rpc_method_handler(
                    servicer.getListOfFiles,
                    request_deserializer=cloud__pb2.GetListOfFilesRequest.FromString,
                    response_serializer=cloud__pb2.GetListOfFilesResponse.SerializeToString,
            ),
            'getFileMetadata': grpc.unary_unary_rpc_method_handler(
                    servicer.getFileMetadata,
                    request_deserializer=cloud__pb2.GetFileMetadataRequest.FromString,
                    response_serializer=cloud__pb2.GetFileMetadataResponse.SerializeToString,
            ),
            'UploadFile': grpc.stream_unary_rpc_method_handler(
                    servicer.UploadFile,
                    request_deserializer=cloud__pb2.UploadFileRequest.FromString,
                    response_serializer=cloud__pb2.UploadFileResponse.SerializeToString,
            ),
            'DownloadFile': grpc.unary_stream_rpc_method_handler(
                    servicer.DownloadFile,
                    request_deserializer=cloud__pb2.DownloadFileRequest.FromString,
                    response_serializer=cloud__pb2.DownloadFileResponse.SerializeToString,
            ),
            'DeleteFile': grpc.unary_unary_rpc_method_handler(
                    servicer.DeleteFile,
                    request_deserializer=cloud__pb2.DeleteFileRequest.FromString,
                    response_serializer=cloud__pb2.DeleteFileResponse.SerializeToString,
            ),
    }
    generic_handler = grpc.method_handlers_generic_handler(
            'Cloud', rpc_method_handlers)
    server.add_generic_rpc_handlers((generic_handler,))


 # This class is part of an EXPERIMENTAL API.
class Cloud(object):
    """Missing associated documentation comment in .proto file."""

    @staticmethod
    def login(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/login',
            cloud__pb2.LoginRequest.SerializeToString,
            cloud__pb2.LoginResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def signup(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/signup',
            cloud__pb2.SignupRequest.SerializeToString,
            cloud__pb2.SignupResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def logout(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/logout',
            cloud__pb2.LogoutRequest.SerializeToString,
            cloud__pb2.LogoutResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def getListOfFiles(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/getListOfFiles',
            cloud__pb2.GetListOfFilesRequest.SerializeToString,
            cloud__pb2.GetListOfFilesResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def getFileMetadata(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/getFileMetadata',
            cloud__pb2.GetFileMetadataRequest.SerializeToString,
            cloud__pb2.GetFileMetadataResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def UploadFile(request_iterator,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.stream_unary(request_iterator, target, '/Cloud/UploadFile',
            cloud__pb2.UploadFileRequest.SerializeToString,
            cloud__pb2.UploadFileResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def DownloadFile(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_stream(request, target, '/Cloud/DownloadFile',
            cloud__pb2.DownloadFileRequest.SerializeToString,
            cloud__pb2.DownloadFileResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)

    @staticmethod
    def DeleteFile(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/DeleteFile',
            cloud__pb2.DeleteFileRequest.SerializeToString,
            cloud__pb2.DeleteFileResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)