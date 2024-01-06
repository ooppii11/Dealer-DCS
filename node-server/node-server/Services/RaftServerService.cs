using Grpc.Core;
using GrpcRaft;
using node_server.Managers.Raft;
using node_server.Managers.Raft.States;

namespace node_server.Services
{
    public class RaftServerService : RaftService.RaftServiceBase
    {
        private Raft _raft;

        public RaftServerService()
        {
            this._raft = new Raft(new RaftSettings());
            this._raft.Start();
        }

        public override Task<RequestVoteResponse> RequestVote(RequestVoteRequest request, ServerCallContext context)
        {
            bool vote = this._raft.State.OnReceiveVoteRequest(request);
            if (vote && this._raft.RaftStateCode == Raft.StatesCode.Leader)
            {
               // this._raft.ChangeState(Raft.StatesCode.Follower);
            }

            RequestVoteResponse response = new RequestVoteResponse()
            {
                Term = this._raft.Settings.CurrentTerm,
                Vote = vote
            };

            return Task.FromResult(response);
        }

        public override Task<AppendEntriesResponse> AppendEntries(IAsyncStreamReader<AppendEntriesRequest> requestStream, ServerCallContext context)
        {
            AppendEntriesResponse response;

            try
            {
                response = this._raft.State.OnReceiveAppendEntriesRequest(requestStream);
            }
            catch
            {
                var status = new Status(StatusCode.Unknown, "An error occurred while processing the request");

                throw new RpcException(status);
            }
            return Task.FromResult(response);

        }

        public override Task<InstallSnapshotResponse> InstallSnapshot(IAsyncStreamReader<InstallSnapshotRequest> requestStream, ServerCallContext context)
        {
            InstallSnapshotResponse response;

            try
            {
                response = this._raft.State.OnReceiveInstallSnapshotRequestRequest(requestStream);
            }
            catch
            {
                var status = new Status(StatusCode.Unknown, "An error occurred while processing the request");

                throw new RpcException(status);
            }
            return Task.FromResult(response);
        }
    }
}
