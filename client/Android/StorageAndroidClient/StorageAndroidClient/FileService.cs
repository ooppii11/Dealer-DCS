using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Collections.Concurrent;
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
        private BlockingCollection<FileTask> taskQueue;
        private CancellationTokenSource cancellationTokenSource;

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
            }

            return StartCommandResult.Sticky;
        }

        private void EnqueueTask(FileTask task)
        {
            taskQueue.Add(task);
        }

        private void ProcessTasks(CancellationToken token)
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
                }
            }
        }

        
        private void PerformUpload(string sessionId, string fileName, string type, byte[] data)
        {
            // Implement upload logic
            Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
            taskCompleteIntent.PutExtra("message", "Upload completed for file: " + fileName);
            SendBroadcast(taskCompleteIntent);
        }

        private async void PerformDownload(string sessionId, string fileName)
        {
            try
            {
                byte[] fileData = await DownloadFileFromServer(fileName);
                SaveFileToDownloadDirectory(fileData, fileName);
                Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
                taskCompleteIntent.PutExtra("message", "Download completed for file: " + fileName);
                SendBroadcast(taskCompleteIntent);
            }
            catch (Exception ex)
            {
                //notification
                Console.WriteLine("Download failed: " + ex.Message);
            }
        }

        private async Task<byte[]> DownloadFileFromServer(string fileName)
        {
            //grpc
            return new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            
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
                //notification
                Console.WriteLine("Error saving file: " + ex.Message);
            }
        }
        private void PerformUpdate(string sessionId, string fileName, byte[] fileData)
        {
            // Implement update logic
            Intent taskCompleteIntent = new Intent("StorageAndroidClient.ACTION_TASK_COMPLETE");
            taskCompleteIntent.PutExtra("message", "Update completed for file: " + fileName);
            SendBroadcast(taskCompleteIntent);
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
