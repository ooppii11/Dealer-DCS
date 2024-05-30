using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Service.QuickSettings;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using System;
using System.Collections.Generic;

namespace StorageAndroidClient
{
    enum FilePickerOperation
    {
        Upload = 1,
        Update,
    }
    [Activity(Label = "FileOperationsActivity")]
    public class MainPageFileOperationsActivity : AppCompatActivity
    {
        Button logoutButton;
        Button uploadButton;
        LinearLayout filesContainer;
        private TaskCompleteReceiver taskCompleteReceiver;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.file_operations);
            taskCompleteReceiver = new TaskCompleteReceiver(this);
            InitializeUI();
            LoadFileMetadata();
        }

        private void InitializeUI()
        {
            logoutButton = FindViewById<Button>(Resource.Id.logoutButton);
            uploadButton = FindViewById<Button>(Resource.Id.uploadButton);
            filesContainer = FindViewById<LinearLayout>(Resource.Id.filesContainer);

            logoutButton.Click += LogoutButton_Click;
            uploadButton.Click += UploadButton_Click;
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            SharedPreferencesManager.SaveString("SessionId", null);
            NavigateToLoginActivity();
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            StartFilePicker(FilePickerOperation.Upload);
        }

        private void StartFilePicker(FilePickerOperation code)
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
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
                StartUpdateService(ReadFileData(uri), GetFileName(uri));
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
        private void LoadFileMetadata()
        {
            // Load file metadata (replace with actual implementation)
            List<string> files = GetFileMetadata();
            filesContainer.RemoveAllViews();

            foreach (var file in files)
            {
                AddFileButton(file);
            }
        }

        private List<string> GetFileMetadata()
        {
            //grpc
            return new List<string> { };
        }

        private void AddFileButton(string fileName)
        {
            Button fileButton = new Button(this)
            {
                Text = fileName,
                LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            fileButton.Click += (sender, e) => ShowFileOptionsDialog(fileName);
            filesContainer.AddView(fileButton);
        }

        private void ShowFileOptionsDialog(string fileName)
        {
            AndroidX.AppCompat.App.AlertDialog.Builder dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            dialog.SetTitle(fileName);
            dialog.SetItems(new[] { "Download", "Delete", "Update" }, (sender, args) =>
            {
                switch (args.Which)
                {
                    case 0:
                        // Start download
                        DownloadFile(fileName);
                        break;
                    case 1:
                        // Delete file
                        DeleteFile(fileName);
                        break;
                    case 2:
                        // Update file
                        UpdateFile(fileName);
                        break;
                }
            });
            dialog.Show();
        }

        private void DownloadFile(string fileName)
        {
            Intent downloadIntent = new Intent(this, typeof(FileService));
            downloadIntent.SetAction(FileService.ActionDownload);
            downloadIntent.PutExtra("FileName", fileName);
            downloadIntent.PutExtra("SessionId", SharedPreferencesManager.GetString("SessionId"));
            StartService(downloadIntent);
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
            StartFilePicker(FilePickerOperation.Update);
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
                string message = intent.GetStringExtra("message");
                string action = intent.GetStringExtra("action");
                Toast.MakeText(context, message, ToastLength.Short).Show();

                if (action != null && action != "download")
                {
                    activity.LoadFileMetadata();
                }
            }
        }
    }
}
