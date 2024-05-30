﻿using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Service.QuickSettings;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using GrpcCloud;
using System;
using System.Collections.Generic;

namespace StorageAndroidClient
{
    enum FilePickerOperation
    {
        Upload = 1,
        Update,
    }

    [Activity(Label = "Main")]
    public class MainPageFileOperationsActivity : AppCompatActivity
    {
        Button logoutButton;
        Button uploadButton;
        LinearLayout filesContainer;
        private TaskCompleteReceiver taskCompleteReceiver;
        private const string CloudStorageAddress = "172.18.0.3:50053";


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
            GrpcClient grpcClient = new GrpcClient(CloudStorageAddress);
            grpcClient.Logout(SharedPreferencesManager.GetString("SessionId"));
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
            GetListOfFilesResponse files = GetFileMetadata();
            filesContainer.RemoveAllViews();

            foreach (var file in files.Files)
            {
                AddFileButton(file);
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
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;
            layout.SetPadding(50, 40, 50, 10);

            TableLayout tableLayout = new TableLayout(this);
            tableLayout.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

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
                        break;
                    case 2:
                        UpdateFile(metadata.Filename);
                        break;
                }
            };
            layout.AddView(listView);

            AndroidX.AppCompat.App.AlertDialog.Builder dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            dialog.SetTitle("File Options");
            dialog.SetView(layout);
            dialog.SetNegativeButton("Close", (s, e) => { });
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
            TextView valueView = new TextView(this)
            {
                Text = value,
                LayoutParameters = new TableRow.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            row.AddView(keyView);
            row.AddView(valueView);
            table.AddView(row);
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