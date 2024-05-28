using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace StorageAndroidClient
{
    [Activity(Label = "LoginActivity", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginActivity : AppCompatActivity
    {
        EditText usernameEditText;
        EditText passwordEditText;
        Button loginButton;
        TextView signupLink;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);

            usernameEditText = FindViewById<EditText>(Resource.Id.username);
            passwordEditText = FindViewById<EditText>(Resource.Id.password);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            signupLink = FindViewById<TextView>(Resource.Id.signupLink);

            loginButton.Click += LoginButton_Click;
            signupLink.Click += SignupLink_Click;
        }

        private void LoginButton_Click(object sender, System.EventArgs e)
        {
            string username = usernameEditText.Text;
            string password = passwordEditText.Text;

            // Perform login logic here

            // If login is successful, save session ID to shared preferences
            ISharedPreferences prefs = GetSharedPreferences("YourAppPrefs", FileCreationMode.Private);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("SessionId", "YourSessionId");
            editor.Apply();

            // Navigate to HomeActivity
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }

        private void SignupLink_Click(object sender, System.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(SignupActivity));
            StartActivity(intent);
        }
    }
}
