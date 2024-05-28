using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace StorageAndroidClient
{
    [Activity(Label = "MainActivity")]
    public class MainActivity : AppCompatActivity
    {
        TextView welcomeMessage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main);

            welcomeMessage = FindViewById<TextView>(Resource.Id.welcomeMessage);

            // Optionally, retrieve session ID from shared preferences if needed
            ISharedPreferences prefs = GetSharedPreferences("YourAppPrefs", FileCreationMode.Private);
            string sessionId = prefs.GetString("SessionId", null);
        }
    }
}
