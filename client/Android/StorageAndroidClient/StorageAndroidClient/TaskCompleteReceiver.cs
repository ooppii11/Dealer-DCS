using Android.Content;
using Android.Widget;

namespace StorageAndroidClient
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class TaskCompleteReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == "StorageAndroidClient.ACTION_TASK_COMPLETE")
            {
                string message = intent.GetStringExtra("message");
                Toast.MakeText(context, message, ToastLength.Short).Show();
            }
        }
    }
}
