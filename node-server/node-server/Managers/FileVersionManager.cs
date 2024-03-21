using System;
using System.IO;
using System.Data.SQLite;


namespace NodeServer.Managers
{

    public class FileVersionManager
    {
        private string connectionString;

        public FileVersionManager(string databasePath)
        {
            connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Files (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        FileName TEXT NOT NULL,
                                        Version INTEGER NOT NULL,
                                        FilePath TEXT NOT NULL,
                                        CONSTRAINT Unique_File UNIQUE (FileName, Version)
                                        );";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveFileVersion(string fileName, string filePath)
        {
            int latestVersion = GetLatestFileVersion(fileName) + 1;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Files (FileName, Version, FilePath) VALUES (@FileName, @Version, @FilePath);";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", latestVersion);
                    command.Parameters.AddWithValue("@FilePath", filePath);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetLatestFileVersion(string fileName)
        {
            int latestVersion = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT MAX(Version) FROM Files WHERE FileName = @FileName;";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        latestVersion = Convert.ToInt32(result);
                    }
                }
            }

            return latestVersion;
        }

        public string GetFilePath(string fileName, int version)
        {
            string filePath = null;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT FilePath FROM Files WHERE FileName = @FileName AND Version = @Version;";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", version);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        filePath = result.ToString();
                    }
                }
            }

            return filePath;
        }

        public void RemovePreviousVersions(string fileName, int version)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Files WHERE FileName = @FileName AND Version < @Version;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", version);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveAllFileVersions(string fileName)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Files WHERE FileName = @FileName;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
