using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using GrpcCloud;
using System;
using System.Threading.Tasks;
using Grpc.Core;
using AndroidX.Core.App;
using System.Threading;

namespace StorageAndroidClient
{
    enum FilePickerOperationId
    {
        RequestStoragePermission,
        Upload,
        Update,
    }

    [Activity(Label = "Main")]
    public class MainPageFileOperationsActivity : AppCompatActivity
    {
        Button logoutButton;
        Button uploadButton;
        LinearLayout filesContainer;
        private TaskCompleteReceiver taskCompleteReceiver;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private const string CloudStorageAddress = "10.10.0.35:50053"; //pc ip address on the current network -> port fowarded to the server on the docker container 50053:50053 -> server address
        //private const string CloudStorageAddress = "10.253.243.88:50053"; //pc ip address on the current network -> port fowarded to the server on the docker container 50053:50053 -> server address
        private bool permissionGranted = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.file_operations);
            taskCompleteReceiver = new TaskCompleteReceiver(this);
            InitializeUI();
            LoadFileMetadata(cancellationTokenSource.Token);
            RequestStoragePermissions();
        }

        private void InitializeUI()
        {
            logoutButton = FindViewById<Button>(Resource.Id.logoutButton);
            uploadButton = FindViewById<Button>(Resource.Id.uploadButton);
            filesContainer = FindViewById<LinearLayout>(Resource.Id.filesContainer);

            logoutButton.Click += LogoutButton_Click;
            uploadButton.Click += UploadButton_Click;
        }
        private void RequestStoragePermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this,
                    new String[] { Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage },
                    (int)FilePickerOperationId.RequestStoragePermission);
            }
            else
            {
                permissionGranted = true;
            }
            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case (int)FilePickerOperationId.RequestStoragePermission:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            permissionGranted = true;
                        }
                        else
                        {
                            permissionGranted = false;
                        }
                        return;
                    }
            }
        }
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                GrpcClient grpcClient = new GrpcClient(CloudStorageAddress);
                grpcClient.Logout(SharedPreferencesManager.GetString("SessionId"));
                SharedPreferencesManager.Remove("SessionId");
                cancellationTokenSource.Cancel();
                NavigateToLoginActivity();
            }
            catch
            {
                SharedPreferencesManager.Remove("SessionId");
                cancellationTokenSource.Cancel();
                NavigateToLoginActivity();
            }
            
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            RequestStoragePermissions();
            if (permissionGranted)
            {
                StartFilePicker(FilePickerOperationId.Upload);
            }
            else
            {
                Toast.MakeText(this, "can't upload file without permissions", ToastLength.Short).Show();
            }
        }

        private void StartFilePicker(FilePickerOperationId code)
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            StartActivityForResult(Intent.CreateChooser(intent, "Select a file"), (int)code);
        }

        private void StartFilePicker(FilePickerOperationId code, string fileName)
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            SharedPreferencesManager.SaveString("fileName", fileName);
            StartActivityForResult(Intent.CreateChooser(intent, "Select a file"), (int)code);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                StartUploadService(ReadFileData(uri), GetFileName(uri));
            }
            else if (requestCode == 2 && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;

                // Use the retrieved file name
                StartUpdateService(ReadFileData(uri), SharedPreferencesManager.GetString("fileName"));
                SharedPreferencesManager.Remove("fileName");
            }
        }

        private byte[] ReadFileData(Android.Net.Uri uri)
        {
            byte[] fileData = null;
            using (var inputStream = ContentResolver.OpenInputStream(uri))
            {
                fileData = ReadFully(inputStream);
            }
            return fileData;
        }

        private string GetFileName(Android.Net.Uri uri)
        {
            string[] projection = { Android.Provider.OpenableColumns.DisplayName };
            using (ICursor cursor = ContentResolver.Query(uri, projection, null, null, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    int columnIndex = cursor.GetColumnIndex(Android.Provider.OpenableColumns.DisplayName);
                    if (columnIndex != -1)
                    {
                        return cursor.GetString(columnIndex);
                    }
                }
            }
            return null;
        }

        private void StartUploadService(byte[] fileData, string name)
        {
            Intent intent = new Intent(this, typeof(FileService));
            intent.SetAction(FileService.ActionUpload);
            intent.PutExtra("FileName", name);
            intent.PutExtra("FileData", fileData);
            intent.PutExtra("FileType", "plain/text");
            intent.PutExtra("SessionId", SharedPreferencesManager.GetString("SessionId"));
            StartService(intent);
        }

        private byte[] ReadFully(System.IO.Stream input)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private async void LoadFileMetadata(CancellationToken stop)
        {
            const int retryDelay = 5000;
            while (!stop.IsCancellationRequested)
            {
                try
                {
                    GetListOfFilesResponse files = GetFileMetadata();
                    filesContainer.RemoveAllViews();

                    foreach (var file in files.Files)
                    {
                        AddFileButton(file);
                    }
                    break;
                }
                catch (RpcException ex)
                {
                    Console.WriteLine("Filed to load metadata");
                    if (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
                    {
                        Toast.MakeText(this, "load metadata - Error connecting to the server.", ToastLength.Short).Show();
                        SharedPreferencesManager.Remove("SessionId");
                        NavigateToLoginActivity();
                        break;
                    }
                    else if (ex.StatusCode == StatusCode.PermissionDenied || ex.StatusCode == StatusCode.Unauthenticated)
                    {
                        Toast.MakeText(this, "load metadata - Invalid session id", ToastLength.Short).Show();
                        SharedPreferencesManager.Remove("SessionId");
                        NavigateToLoginActivity();
                        break;
                    }
                    else
                    {
                        Toast.MakeText(this, "Error loading metadata - communication erorr", ToastLength.Short).Show();
                        await Task.Delay(retryDelay);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Filed to load metadata");
                    Toast.MakeText(this, "Error loading metadata - internal erorr", ToastLength.Short).Show();
                    await Task.Delay(retryDelay);
                }
            }

        }


        private GetListOfFilesResponse GetFileMetadata()
        {
            return new GrpcClient(CloudStorageAddress).GetFiles(SharedPreferencesManager.GetString("SessionId"));
        }

        private void AddFileButton(FileMetadata metadata)
        {
            Button fileButton = new Button(this)
            {
                Text = metadata.Filename,
                LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            fileButton.Click += (sender, e) => ShowFileOptionsDialog(metadata);
            filesContainer.AddView(fileButton);
        }

        private void ShowFileOptionsDialog(FileMetadata metadata)
        {
            AndroidX.AppCompat.App.AlertDialog dialog = null;
            AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;
            layout.SetPadding(50, 40, 50, 10);

            TableLayout tableLayout = new TableLayout(this);
            tableLayout.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            tableLayout.SetDividerDrawable(ContextCompat.GetDrawable(this, Android.Resource.Drawable.DividerHorizontalBright));

            AddTableRow(tableLayout, "Filename", metadata.Filename);
            AddTableRow(tableLayout, "Creation Date", metadata.CreationDate.ToString());
            AddTableRow(tableLayout, "Last Modified", metadata.LastModified.ToString());
            AddTableRow(tableLayout, "Size", metadata.Size.ToString());

            layout.AddView(tableLayout);


            string[] options = { "Download", "Delete", "Update" };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, options);
            ListView listView = new ListView(this) { Adapter = adapter };
            listView.ItemClick += (sender, args) =>
            {
                switch (args.Position)
                {
                    case 0:
                        DownloadFile(metadata.Filename);
                        break;
                    case 1:
                        DeleteFile(metadata.Filename);
                        dialog?.Dismiss();
                        break;
                    case 2:
                        UpdateFile(metadata.Filename);
                        dialog?.Dismiss();
                        break;
                }
            };
            layout.AddView(listView);


            builder.SetTitle("File Options");
            builder.SetView(layout);
            builder.SetNegativeButton("Close", (s, e) => { });
            dialog = builder.Create();

            dialog.Show();

        }
        private void AddTableRow(TableLayout table, string key, string value)
        {
            TableRow row = new TableRow(this);
            TextView keyView = new TextView(this)
            {
                Text = key,
                LayoutParameters = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            keyView.SetPadding(0, 0, 10, 0);
            TextView valueView = new TextView(this)
            {
                Text = value,
                LayoutParameters = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            if (Resources.Configuration.LayoutDirection == Android.Views.LayoutDirection.Rtl)
            {
                row.AddView(valueView);
                row.AddView(keyView);
            }
            else
            {
                row.AddView(keyView);
                row.AddView(valueView);
            }
            table.AddView(row);
        }

        private void DownloadFile(string fileName)
        {
            RequestStoragePermissions();
            if (permissionGranted)
            {
                Intent downloadIntent = new Intent(this, typeof(FileService));
                downloadIntent.SetAction(FileService.ActionDownload);
                downloadIntent.PutExtra("FileName", fileName);
                downloadIntent.PutExtra("SessionId", SharedPreferencesManager.GetString("SessionId"));
                StartService(downloadIntent);
            }
            else
            {
                Toast.MakeText(this, "can't Download file without permissions", ToastLength.Short).Show();
            }
        }

        private void DeleteFile(string fileName)
        {
            Intent deleteIntent = new Intent(this, typeof(FileService));
            deleteIntent.SetAction(FileService.ActionDelete);
            deleteIntent.PutExtra("FileName", fileName);
            deleteIntent.PutExtra("SessionId", SharedPreferencesManager.GetString("SessionId"));
            StartService(deleteIntent);
        }

        private void UpdateFile(string fileName)
        {
            RequestStoragePermissions();
            if (permissionGranted)
            {
                StartFilePicker(FilePickerOperationId.Update, fileName);
            }
            else
            {
                Toast.MakeText(this, "can't update file without permissions", ToastLength.Short).Show();
            }
        }

        private void StartUpdateService(byte[] fileData, string name)
        {
            Intent intent = new Intent(this, typeof(FileService));
            intent.SetAction(FileService.ActionUpdate);
            intent.PutExtra("FileData", fileData);
            intent.PutExtra("FileName", name);
            intent.PutExtra("SessionId", SharedPreferencesManager.GetString("SessionId"));
            StartService(intent);
        }

        private void NavigateToLoginActivity()
        {
            Intent intent = new Intent(this, typeof(LoginPageActivity));
            StartActivity(intent);
            Finish();
        }
        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(taskCompleteReceiver, new IntentFilter("StorageAndroidClient.ACTION_TASK_COMPLETE"));
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(taskCompleteReceiver);
        }

        public class TaskCompleteReceiver : BroadcastReceiver
        {
            private readonly MainPageFileOperationsActivity activity;

            public TaskCompleteReceiver(MainPageFileOperationsActivity activity)
            {
                this.activity = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.GetStringExtra("action");
                string message = intent.GetStringExtra("message");
                Toast.MakeText(context, message, ToastLength.Short).Show();
                if (action == "failexit")
                {
                    SharedPreferencesManager.Remove("SessionId");
                    activity.NavigateToLoginActivity();
                }
                else if (action != null && action != "download" && action != "fail")
                {
                    activity.LoadFileMetadata(activity.cancellationTokenSource.Token);
                }
            }
        }
    }
}
