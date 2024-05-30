using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Threading.Tasks;
using System;

namespace StorageAndroidClient
{
    [Activity(Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginPageActivity : AppCompatActivity
    {
        EditText usernameEditText;
        EditText passwordEditText;
        Button loginButton;
        TextView signupLink;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string username = Intent.GetStringExtra("username");
            string password = Intent.GetStringExtra("password");
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                await AttemptLogin(username, password);
            }

            SetContentView(Resource.Layout.login);

            if (SharedPreferencesManager.GetString("SessionId") == null)
            {
                InitializeUI();
            }
            else
            {
                NavigateToMainActivity();
            }
        }

        private void InitializeUI()
        {
            usernameEditText = FindViewById<EditText>(Resource.Id.username);
            passwordEditText = FindViewById<EditText>(Resource.Id.password);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            signupLink = FindViewById<TextView>(Resource.Id.signupLink);

            loginButton.Click += LoginButton_Click;
            signupLink.Click += SignupLink_Click;
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = usernameEditText.Text;
            string password = passwordEditText.Text;
            if (IsValidInput(username, password))
            {
                await AttemptLogin(username, password);
            }
            else
            {
                ShowErrorMessage("Please fill in all fields.");
            }
        }
        private bool IsValidInput(string username, string password)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   !string.IsNullOrWhiteSpace(password);
        }
        private async Task AttemptLogin(string username, string password)
        {
            try
            {
                string sessionId = await PerformLoginAsync(username, password);
                SharedPreferencesManager.SaveString("SessionId", sessionId);
                NavigateToMainActivity();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Login failed: {ex.Message}");
            }
        }

        private async Task<string> PerformLoginAsync(string username, string password)
        {
            try
            {
                return "dummy_session_id";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                throw;
            }
        }

        private void ShowErrorMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private void SignupLink_Click(object sender, EventArgs e)
        {
            NavigateToSignupActivity();
        }

        private void NavigateToMainActivity()
        {
            Intent intent = new Intent(this, typeof(MainPageFileOperationsActivity));
            StartActivity(intent);
            Finish();
        }

        private void NavigateToSignupActivity()
        {
            Intent intent = new Intent(this, typeof(SignupPageActivity));
            StartActivity(intent);
            Finish();
        }
    }
}
