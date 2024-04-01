using Npgsql;
using cloud_server.Managers;
using System.Data;
using System.Xml.Linq;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Components.Routing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using cloud_server.Utilities;
using Google.Protobuf.WellKnownTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace cloud_server.DB
{
    public class FileMetadataDB
    {
        private NpgsqlConnection _conn;

        public FileMetadataDB(NpgsqlConnection conn)
        {
            this._conn = conn;
        }

        public FileMetadataDB(string tablesPath, string host, string username, string port, string password, string db)
        {
            var connectionString = $"Server={host};Port={port};User Id={username};Password={password};Database={db};";

            this._conn = new NpgsqlConnection(connectionString);
            try
            {
                if (this._conn.State != System.Data.ConnectionState.Open)
                { 
                    this._conn.Open();  // Open connection with the db.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
            this.createTables(tablesPath);
        }

        private void createTables(string pathToTablesFile)
        {
            string strText = System.IO.File.ReadAllText(pathToTablesFile, System.Text.Encoding.UTF8);  // Load tables quries 
            using (NpgsqlCommand command = new NpgsqlCommand(strText, this._conn))
            {
                command.ExecuteNonQuery();
            }
        }

        public void uploadFileMetadata(FileMetadata metadata, Location location)
        {
            string sqlQuery = "SELECT insertFileMetadata(@creatorId, @fileName, @fileType, @fileSize) AS inserted_id";

            int fileId = 0;

            using (var command = new NpgsqlCommand(sqlQuery, this._conn))
            {
                command.Parameters.AddWithValue("creatorId", metadata.CreatorId);
                command.Parameters.AddWithValue("fileName", metadata.Name);
                command.Parameters.AddWithValue("fileType", metadata.Type);
                command.Parameters.AddWithValue("fileSize", metadata.Size);


                fileId = Convert.ToInt32(command.ExecuteScalar());

                if (fileId != 0) {  this.addFileLocation(fileId, location); }
                else { throw new FileAlreadyExistException("File already exists"); }
            }
        
        }

        public int getFileId(string filename, int userId)
        {
            int fileId = 0;
            string query = @"SELECT id FROM file_metadata WHERE creator_id = @creator_id AND name = @name;";

            try
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
                {
                    command.Parameters.AddWithValue("@creator_id", userId);
                    command.Parameters.AddWithValue("@name", filename);

                    fileId = Convert.ToInt32(command.ExecuteScalar());
                }
                return fileId;

            }
            catch (Exception ex)
            {
                throw new FileDoesNotExistException("File not exists");
            }
        }
        private void addFileLocation(int fileId, Location location)
        {
            string sqlQuery = @"INSERT INTO file_location VALUES(@file_id, @primary_server_ip, @backup_server_ip_1, @backup_server_ip_2);";
           
            using (var cmd = new NpgsqlCommand(sqlQuery, this._conn))
            {
                cmd.Parameters.AddWithValue("@file_id", fileId);
                cmd.Parameters.AddWithValue("@primary_server_ip", location.PrimaryServer);
                cmd.Parameters.AddWithValue("@backup_server_ip_1", location.FirstBackupServer);
                cmd.Parameters.AddWithValue("@backup_server_ip_2", location.SecondBackupServer);

                cmd.ExecuteNonQuery();
            }
        }

        public void deleteFileMetadata(int userId, string name)
        {
            string query = @"DELETE FROM file_metadata WHERE creator_id = @creator_id AND name = @name;";
            
            using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
            {
                try
                {
                    command.Parameters.AddWithValue("@creator_id", userId);
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();
                }
                catch
                {
                    throw new DBErrorException("Unable to delete this file");
                }
            }
        }

        public void updateFileMetadata(int userId, string name, long size)
        {
            string query = @"UPDATE file_metadata
                            SET size = @newSize, last_modify = CURRENT_TIMESTAMP AT TIME ZONE 'UTC'
                            WHERE creator_id = @creator_id AND name = @name;";
            try
            {
                using (var cmd = new NpgsqlCommand(query, this._conn))
                {
                    cmd.Parameters.AddWithValue("@creator_id", userId);
                    cmd.Parameters.AddWithValue("@newSize", size);
                    cmd.Parameters.AddWithValue("@name", name);

                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw new DBErrorException("DB Error");
            }
        }

        public FileMetadata getFile(int userId, string name)
        {
            string query = @"SELECT * FROM file_metadata WHERE creator_id = @creator_id AND name = @name;";
            
            try
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
                {
                    command.Parameters.AddWithValue("@creator_id", userId);
                    command.Parameters.AddWithValue("@name", name);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            return new FileMetadata(
                                reader.GetInt32(reader.GetOrdinal("creator_id")),
                                reader.GetString(reader.GetOrdinal("name")),
                                reader.GetString(reader.GetOrdinal("type")),
                                reader.GetInt32(reader.GetOrdinal("size")),
                                reader.GetDateTime(reader.GetOrdinal("creation_time")),
                                reader.GetDateTime(reader.GetOrdinal("last_modify")));
                        }
                        throw new FileDoesNotExistException("File not found");
                    }
                }
            }
            catch 
            { 
                throw new DBErrorException("DB Error");
            }
        }
        public List<FileMetadata> getUserFilesMetadata(int userId)
        {
            List<FileMetadata> userFiles = new List<FileMetadata>();
            string query = @"SELECT * FROM file_metadata WHERE creator_id = @creator_id;";

            try
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
                {
                    command.Parameters.AddWithValue("@creator_id", userId);
                    
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userFiles.Add(new FileMetadata(
                                reader.GetInt32(reader.GetOrdinal("creator_id")),
                                reader.GetString(reader.GetOrdinal("name")),
                                reader.GetString(reader.GetOrdinal("type")),
                                reader.GetInt32(reader.GetOrdinal("size")),
                                reader.GetDateTime(reader.GetOrdinal("creation_time")),
                                reader.GetDateTime(reader.GetOrdinal("last_modify"))));
                        }
                    }
                }
            }
            catch
            {
                throw new DBErrorException("DB Error");
            }
            
            return userFiles;
        }
    }
}
