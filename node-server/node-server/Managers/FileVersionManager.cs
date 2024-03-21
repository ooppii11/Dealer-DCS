using System;
using System.IO;
using System.Data.SQLite;
using Google.Protobuf.Compiler;


namespace NodeServer.Managers
{

    public class FileVersionManager
    {
        private string _connectionString;

        public FileVersionManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Files (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        UserId INTEGER NOT NULL,
                                        FileName TEXT NOT NULL,
                                        Type TEXT NOT NULL,
                                        Size INTEGER NOT NULL,
                                        Version INTEGER NOT NULL,
                                        FilePath TEXT NOT NULL,
                                        CONSTRAINT Unique_File UNIQUE (UserId, FileName, Version)
                                        );";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveFileVersion(int userId, string fileName, string type, int size, string filePath)
        {
            int latestVersion = GetLatestFileVersion(fileName, userId) + 1;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Files (UserId, FileName, Type, Size, Version, FilePath) VALUES (@UserId, @FileName, @Type, @Size, @Version, @FilePath);";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Size", size);
                    command.Parameters.AddWithValue("@Version", latestVersion);
                    command.Parameters.AddWithValue("@FilePath", filePath);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetLatestFileVersion(string fileName, int userId)
        {
            int latestVersion = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT MAX(Version) FROM Files WHERE FileName = @FileName AND UserId = @UserId;";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        latestVersion = Convert.ToInt32(result);
                    }
                }
            }

            return latestVersion;
        }


        public string GetFilePath(string fileName, int userId, int version)
        {
            string filePath = null;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT FilePath FROM Files WHERE FileName = @FileName AND UserId = @UserId AND Version = @Version;";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", version);
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        filePath = result.ToString();
                    }
                }
            }

            return filePath;
        }

        public void RemovePreviousVersions(string fileName, int userId, int version)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Files WHERE FileName = @FileName AND UserId = @UserId AND Version < @Version;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", version);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveVersion(string fileName, int userId, int version)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Files WHERE FileName = @FileName AND UserId = @UserId AND Version = @Version;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Version", version);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveAllFileVersions(string fileName, int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Files WHERE FileName = @FileName AND UserId = @UserId;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetUserNumOfFiles(int userId)
        {
            int numOfFiles = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT count(DISTINCT FileName) FROM Files WHERE UserId = @UserId";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        numOfFiles = Convert.ToInt32(result);
                    }
                }
            }

            return numOfFiles;
        }

        public int GetUserUsedSpace(int userId)
        {
            int usedSpace = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = @"
                                    SELECT SUM(Size) 
                                    FROM Files 
                                    WHERE (UserId, FileName, Version) IN (
                                        SELECT UserId, FileName, MAX(Version) AS Version
                                        FROM Files
                                        WHERE UserId = @UserId
                                        GROUP BY UserId, FileName
                                    )";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        usedSpace = Convert.ToInt32(result);
                    }
                }
            }

            return usedSpace;
        }


    }
}
