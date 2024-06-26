﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;


namespace NodeServer.Managers
{
    public class ServerToServerClient : IDisposable
    {
        private Grpc.Core.Channel channel;
        private ServerToServer.ServerToServerClient client;

        public ServerToServerClient(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                client = new ServerToServer.ServerToServerClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }
        public ServerToServerClient(string address)
        {
            try
            {
                // Create Grpc connction:
                //channel = new Channel("127.0.0.1:1111", ChannelCredentials.Insecure);
                channel = new Channel($"{address}", ChannelCredentials.Insecure);
                client = new ServerToServer.ServerToServerClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }

        ~ServerToServerClient()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public void Dispose()
        {
            this.channel.ShutdownAsync().Wait();
        }
        
        public async Task<RequestVoteResponse> sendNomination(RequestVoteRequest request)
        {
            var response = await client.RequestVoteAsync(request);
            return response;
        }

        public async Task<AppendEntriesResponse> sendAppendEntriesRequest(AppendEntriesRequest appendEntries)
        {
            using (var call = this.client.AppendEntries())
            {
                await call.RequestStream.WriteAsync(appendEntries);
                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;
                return response;
            }
        }

        public async Task<InstallSnapshotResponse> sendInstallSnapshot(InstallSnapshotRequest installSnapshot)
        {
            using (var call = this.client.InstallSnapshot())
            {
                await call.RequestStream.WriteAsync(installSnapshot);
                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;
                return response;
            }
        }
    }
}
