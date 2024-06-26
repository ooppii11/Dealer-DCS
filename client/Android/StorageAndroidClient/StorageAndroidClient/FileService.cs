﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Lifecycle;
using Grpc.Core;
using GrpcCloud;
using Java.Nio.FileNio;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;

namespace StorageAndroidClient
{
    [Service]
    public class FileService : Service
    {
        public const string ActionUpload = "StorageAndroidClient.action.UPLOAD";
        public const string ActionDownload = "StorageAndroidClient.action.DOWNLOAD";
        public const string ActionUpdate = "StorageAndroidClient.action.UPDATE";
        public const string ActionDelete = "StorageAndroidClient.action.DELETE";
        private BlockingCollection<FileTask> taskQueue;
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(false);
        private const string CloudStorageAddress = "10.10.0.35:50053"; //pc ip address on the current network -> port fowarded to the server on the docker container 50053:50053 -> server address
        //private const string CloudStorageAddress = "10.253.243.88:50053"; //pc ip address on the current network -> port fowarded to the server on the docker container 50053:50053 -> server address

        public override void OnCreate()
        {
            base.OnCreate();
            taskQueue = new BlockingCollection<FileTask>();
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => ProcessTasks(cancellationTokenSource.Token));
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                string action = intent.Action;
                string fileName = intent.GetStringExtra("FileName");
                string sessionId = intent.GetStringExtra("SessionId");
                byte[] fileData = intent.GetByteArrayExtra("FileData");
                string fileType = intent.GetStringExtra("FileType");

                if (action == ActionUpload)
                {
                    EnqueueTask(new FileTask { FileName = fileName, Action = ActionUpload, SessionId = sessionId, FileData = fileData, FileType = fileType });
                }
                else if (action == ActionDownload)
                {
                    EnqueueTask(new FileTask { FileName = fileName, Action = ActionDownload, SessionId = sessionId });
                }
                else if (action == ActionUpdate)
                {
                    EnqueueTask(new FileTask { FileName = fileName, Action = ActionUpdate, SessionId = sessionId, FileData = fileData });
                }
                else if (action == ActionDelete)
                {
                    EnqueueTask(new FileTask { FileName = fileName, Action = ActionDelete, SessionId = sessionId });
                }

            }

            return StartCommandResult.Sticky;
        }

        private void EnqueueTask(FileTask task)
        {
            taskQueue.Add(task);
            manualResetEventSlim.Set();
        }

        private void ProcessTasks(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                manualResetEventSlim.Wait();
                try
                {
                    foreach (var task in taskQueue.GetConsumingEnumerable(token))
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        switch (task.Action)
                        {
                            case ActionUpload:
                                PerformUpload(task.SessionId, task.FileName, task.FileType, task.FileData);
                                break;
                            case ActionDownload:
                                PerformDownload(task.SessionId, task.FileName);
                                break;
                            case ActionUpdate:
                                PerformUpdate(task.SessionId, task.FileName, task.FileData);
                                break;
                            case ActionDelete:
                                PerformDelete(task.SessionId, task.FileName);
                                break;
                            default:
                                Console.WriteLine("Invalid action");
                                break;
                        }
                    }
                }
                finally
                {
                    manualResetEventSlim.Reset();
                }
            }
        }

        private void SendBroadcast(string action, string message) 
        {
            Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
            taskCompleteIntent.PutExtra("action", action);
            taskCompleteIntent.PutExtra("message", message);
            SendBroadcast(taskCompleteIntent);
        }
        private async void PerformUpload(string sessionId, string fileName, string type, byte[] fileData)
        {
            try
            {
                GrpcClient client = new GrpcClient(CloudStorageAddress);
                await client.UploadFile(fileName, sessionId, fileData, type);
                string msg = "Upload completed for file: " + fileName;
                SendBroadcast("upload", msg);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.Unavailable || ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                {
                    SendBroadcast("fail", "Error connecting to the server. Try uploading the file again.");
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied || ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
                {
                    SendBroadcast("failexit", "Invalied session id");
                }
                else
                {
                    SendBroadcast("fail", ex.Status.Detail);
                }
            }
            catch
            {
                SendBroadcast("fail", "Upload failed: internal");
            }
        }

        private async void PerformDownload(string sessionId, string fileName)
        {
            try
            {
                byte[] fileData = await DownloadFileFromServer(sessionId, fileName);
                SaveFileToDownloadDirectory(fileData, fileName);
                Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
                taskCompleteIntent.PutExtra("message", "Download completed for file: " + fileName);
                taskCompleteIntent.PutExtra("action", "download");
                SendBroadcast(taskCompleteIntent);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.Unavailable || ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                {
                    SendBroadcast("fail", "Error connecting to the server. Try downloading the file again.");
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied || ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
                {
                    SendBroadcast("failexit", "Invalied session id");
                }
                else
                {
                    SendBroadcast("fail", ex.Status.Detail);
                }
            }
            catch
            {
                SendBroadcast("fail", "Download failed: internal");
            }
        }

        private async Task<byte[]> DownloadFileFromServer(string sessionId, string fileName)
        {
            GrpcClient client = new GrpcClient(CloudStorageAddress);
            return await client.DownloadFile(fileName, sessionId);
        }

        private void SaveFileToDownloadDirectory(byte[] fileData, string fileName)
        {
            string downloadDirPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            string filePath = System.IO.Path.Combine(downloadDirPath, fileName);

            try
            {
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    fileStream.Write(fileData, 0, fileData.Length);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error saving file: " + ex.Message);
            }
        }
        private async void PerformUpdate(string sessionId, string fileName, byte[] fileData)
        {
            try
            {
                GrpcClient client = new GrpcClient(CloudStorageAddress);
                await client.UpdateFile(fileName, sessionId, fileData);
                Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
                taskCompleteIntent.PutExtra("message", "Update completed for file: " + fileName);
                taskCompleteIntent.PutExtra("action", "update");
                SendBroadcast(taskCompleteIntent);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.Unavailable || ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                {
                    SendBroadcast("fail", "Error connecting to the server. Try updating the file again.");
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied || ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
                {
                    SendBroadcast("failexit", "Invalied session id");
                }
                else 
                {
                    SendBroadcast("fail", ex.Status.Detail);
                }
            }
            catch
            {
                SendBroadcast("fail", "Update failed: internal");
            }
        }

        private void PerformDelete(string sessionId, string fileName)
        {
            try
            {
                GrpcClient client = new GrpcClient(CloudStorageAddress);
                client.DeleteFile(fileName, sessionId);
                Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
                taskCompleteIntent.PutExtra("message", "Delete completed for file: " + fileName);
                taskCompleteIntent.PutExtra("action", "delete");
                SendBroadcast(taskCompleteIntent);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.Unavailable || ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                {
                    SendBroadcast("fail", "Error connecting to the server. Try deleting the file again.");
                }
                else if (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied || ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
                {
                    SendBroadcast("failexit", "Invalied session id");
                }
                else
                {
                    SendBroadcast("fail", ex.Status.Detail);
                }
            }
            catch
            {
                //notification
                SendBroadcast("fail", "Delete failed: internal");
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            cancellationTokenSource.Cancel();
            taskQueue.CompleteAdding();
        }

        public class FileTask
        {
            public string FileName { get; set; }
            public string Action { get; set; }
            public string SessionId { get; set; }
            public byte[] FileData { get; set; }
            public string FileType { get; set; }
        }
    }
}
