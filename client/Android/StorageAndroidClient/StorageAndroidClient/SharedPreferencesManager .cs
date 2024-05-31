using Android.Content;
using Xamarin.Essentials;

namespace StorageAndroidClient
{
    public class SharedPreferencesManager
    {
        private static readonly string PreferencesName = "StorageAndroidClient";
        private static ISharedPreferences _sharedPreferences;
        private static ISharedPreferencesEditor _editor;

        static SharedPreferencesManager()
        {
            _sharedPreferences = Platform.AppContext.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
            _editor = _sharedPreferences.Edit();
        }

        // Save a string value
        public static void SaveString(string key, string value)
        {
            _editor.PutString(key, value);
            _editor.Apply();
        }

        // Retrieve a string value
        public static string GetString(string key, string defaultValue = null)
        {
            return _sharedPreferences.GetString(key, defaultValue);
        }

        // Save an int value
        public static void SaveInt(string key, int value)
        {
            _editor.PutInt(key, value);
            _editor.Apply();
        }

        // Retrieve an int value
        public static int GetInt(string key, int defaultValue = 0)
        {
            return _sharedPreferences.GetInt(key, defaultValue);
        }

        // Save a bool value
        public static void SaveBool(string key, bool value)
        {
            _editor.PutBoolean(key, value);
            _editor.Apply();
        }

        // Retrieve a bool value
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return _sharedPreferences.GetBoolean(key, defaultValue);
        }

        // Remove a specific key
        public static void Remove(string key)
        {
            _editor.Remove(key);
            _editor.Apply();
        }

        // Clear all preferences
        public static void Clear()
        {
            _editor.Clear();
            _editor.Apply();
        }
    }
}
