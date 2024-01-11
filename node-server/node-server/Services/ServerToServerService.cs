using Google.Protobuf;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;
using NodeServer.Managers.Raft;

namespace NodeServer.Services
{
    public class ServerToServerService : ServerToServer.ServerToServerBase
    {
        private Raft _raft;
        private FileSaving _microservice;
        private NodeSystemParse _system;
        private readonly string _serverIP = Environment.GetEnvironmentVariable("NODE_SERVER_IP");
        public ServerToServerService(RaftSettings settings, NodeSystemParse sys, FileSaving micro) 
        {
            this._system = sys;
            this._microservice = micro;
            this._raft = new Raft(settings);
            this._raft.Start();
        }

        public override async Task<PassFileResponse> PassFile(IAsyncStreamReader<PassFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                //consensus + S2S
                string fileName = "";
                string type = "";
                List<string> otherNodeServersAddresses = new List<string>();
                MemoryStream fileData = new MemoryStream();


                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    fileName = chunk.FileId;
                    type = chunk.Type;
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                    foreach (var serverAddress in chunk.ServersAddressesWhereSaved)
                    {
                        otherNodeServersAddresses.Add(serverAddress);
                    }
                }

                if (!this._system.filExists(fileName))
                {

                    await this._microservice.uploadFile(fileName, fileData.ToArray(), type);
                    this._system.addFile(fileName, otherNodeServersAddresses);
                }
                else
                {
                    context.Status = new Status(StatusCode.AlreadyExists, $"File already exists on the machine - {this._serverIP}");
                    return new PassFileResponse { Status = false, Message = $"Unable to update file: File already exists on the machine - {this._serverIP}"};
                }
                return new PassFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new PassFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
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
                response = this._raft.State.OnReceiveInstallSnapshotRequest(requestStream);
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
