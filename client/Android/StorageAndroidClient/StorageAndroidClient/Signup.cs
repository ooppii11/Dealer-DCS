using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using StorageAndroidClient;
using System.Threading.Tasks;
using System;

namespace StorageAndroidClient
{
    [Activity(Label = "Signup")]
    public class SignupPageActivity : AppCompatActivity
    {
        EditText usernameEditText;
        EditText emailEditText;
        EditText passwordEditText;
        EditText phoneNumber;
        Button signupButton;
        TextView loginLink;
        private const string CloudStorageAddress = "172.18.0.3:50053";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.signup);

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
            phoneNumber = FindViewById<EditText>(Resource.Id.phone);
            emailEditText = FindViewById<EditText>(Resource.Id.email);
            passwordEditText = FindViewById<EditText>(Resource.Id.password);
            signupButton = FindViewById<Button>(Resource.Id.signupButton);
            loginLink = FindViewById<TextView>(Resource.Id.loginLink);

            signupButton.Click += SignupButton_Click;
            loginLink.Click += LoginLink_Click;
        }

        private async void SignupButton_Click(object sender, System.EventArgs e)
        {
            string username = usernameEditText.Text;
            string email = emailEditText.Text;
            string password = passwordEditText.Text;
            string phone = phoneNumber.Text;

            if (IsValidInput(username, email, password, phone))
            {
                try 
                {
                    PerformSignupAsync(username, email, password, phone);
                    NavigateToLoginActivity(username, password);
                }
                catch (Exception ex)
                {
                    //be more specific with the error message
                    ShowErrorMessage("Signup failed. Please try again.");
                }
                
            }
            else
            {
                ShowErrorMessage("Please fill in all fields.");
            }
        }


        private bool IsValidInput(string username, string email, string password, string phone)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   !string.IsNullOrWhiteSpace(email) &&
                   !string.IsNullOrWhiteSpace(password) &&
                   !string.IsNullOrWhiteSpace(phone);
        }

        private async Task PerformSignupAsync(string username, string email, string password, string phone)
        {
            try
            {
                GrpcClient client = new GrpcClient(CloudStorageAddress);
                var response = await client.SignupAsync(username, email, password, phone);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Signup failed: {ex.Message}");
                throw;
            }
        }

        private void ShowErrorMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private void LoginLink_Click(object sender, System.EventArgs e)
        {
            NavigateToLoginActivity();
        }

        private void NavigateToLoginActivity()
        {
            Intent intent = new Intent(this, typeof(LoginPageActivity));
            StartActivity(intent);
            Finish();
        }

        private void NavigateToLoginActivity(string username, string password)
        {
            Intent intent = new Intent(this, typeof(LoginPageActivity));
            intent.PutExtra("username", username);
            intent.PutExtra("password", password);
            StartActivity(intent);
            Finish();
        }


        private void NavigateToMainActivity()
        {
            Intent intent = new Intent(this, typeof(MainPageFileOperationsActivity));
            StartActivity(intent);
            Finish();
        }


    }
}
