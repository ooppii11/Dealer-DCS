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
        self.GetOrUpdateSystemLeader = channel.unary_unary(
                '/Cloud/GetOrUpdateSystemLeader',
                request_serializer=cloud__pb2.LeaderToViewerHeartBeatRequest.SerializeToString,
                response_deserializer=cloud__pb2.LeaderToViewerHeartBeatResponse.FromString,
                )


class CloudServicer(object):
    """Missing associated documentation comment in .proto file."""

    def GetOrUpdateSystemLeader(self, request, context):
        """Raft:
        """
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')


def add_CloudServicer_to_server(servicer, server):
    rpc_method_handlers = {
            'GetOrUpdateSystemLeader': grpc.unary_unary_rpc_method_handler(
                    servicer.GetOrUpdateSystemLeader,
                    request_deserializer=cloud__pb2.LeaderToViewerHeartBeatRequest.FromString,
                    response_serializer=cloud__pb2.LeaderToViewerHeartBeatResponse.SerializeToString,
            ),
    }
    generic_handler = grpc.method_handlers_generic_handler(
            'Cloud', rpc_method_handlers)
    server.add_generic_rpc_handlers((generic_handler,))


 # This class is part of an EXPERIMENTAL API.
class Cloud(object):
    """Missing associated documentation comment in .proto file."""

    @staticmethod
    def GetOrUpdateSystemLeader(request,
            target,
            options=(),
            channel_credentials=None,
            call_credentials=None,
            insecure=False,
            compression=None,
            wait_for_ready=None,
            timeout=None,
            metadata=None):
        return grpc.experimental.unary_unary(request, target, '/Cloud/GetOrUpdateSystemLeader',
            cloud__pb2.LeaderToViewerHeartBeatRequest.SerializeToString,
            cloud__pb2.LeaderToViewerHeartBeatResponse.FromString,
            options, channel_credentials,
            insecure, call_credentials, compression, wait_for_ready, timeout, metadata)
