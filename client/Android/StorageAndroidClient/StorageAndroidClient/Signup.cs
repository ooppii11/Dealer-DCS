using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using StorageAndroidClient;

namespace StorageAndroidClient
{
    [Activity(Label = "SignupActivity")]
    public class SignupActivity : AppCompatActivity
    {
        EditText usernameEditText;
        EditText emailEditText;
        EditText passwordEditText;
        Button signupButton;
        TextView loginLink;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.signup);

            usernameEditText = FindViewById<EditText>(Resource.Id.username);
            emailEditText = FindViewById<EditText>(Resource.Id.email);
            passwordEditText = FindViewById<EditText>(Resource.Id.password);
            signupButton = FindViewById<Button>(Resource.Id.signupButton);
            loginLink = FindViewById<TextView>(Resource.Id.loginLink);

            signupButton.Click += SignupButton_Click;
            loginLink.Click += LoginLink_Click;
        }

        private void SignupButton_Click(object sender, System.EventArgs e)
        {
            string username = usernameEditText.Text;
            string email = emailEditText.Text;
            string password = passwordEditText.Text;

            // Perform signup logic here

            // After successful signup, navigate back to LoginActivity
            Intent intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
            Finish();
        }

        private void LoginLink_Click(object sender, System.EventArgs e)
        {
            Intent intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
            Finish();
        }
    }
}
