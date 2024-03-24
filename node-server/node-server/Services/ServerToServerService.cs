using Google.Protobuf;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;
using NodeServer.Managers.RaftNameSpace;
using System.Text;

namespace NodeServer.Services
{
    public class ServerToServerService : ServerToServer.ServerToServerBase
    {
        private Raft _raft;
        
        public ServerToServerService(Raft raft)
        {
            this._raft = raft;
        }

        public override Task<RequestVoteResponse> RequestVote(RequestVoteRequest request, ServerCallContext context)
        {
            bool vote = this._raft.OnReceiveVoteRequest(request);


            RequestVoteResponse response = new RequestVoteResponse()
            {
                Term = this._raft.Settings.CurrentTerm,
                Vote = vote
            };
            return Task.FromResult(response);
        }

        public async override Task<AppendEntriesResponse> AppendEntries(IAsyncStreamReader<AppendEntriesRequest> requestStream, ServerCallContext context)
        {
            AppendEntriesResponse response = null;

            try
            {
                response = await this._raft.OnReceiveAppendEntriesRequest(requestStream, context.Peer);
            }
            catch
            {
                var status = new Status(StatusCode.Unknown, "An error occurred while processing the request");

                throw new RpcException(status);
            }
            return response;

        }

        public async override Task<InstallSnapshotResponse> InstallSnapshot(IAsyncStreamReader<InstallSnapshotRequest> requestStream, ServerCallContext context)
        {
            InstallSnapshotResponse response;

            try
            {
                response = await this._raft.OnReceiveInstallSnapshotRequest(requestStream);
            }
            catch
            {
                var status = new Status(StatusCode.Unknown, "An error occurred while processing the request");

                throw new RpcException(status);
            }
            return response;
        }
    }
}
