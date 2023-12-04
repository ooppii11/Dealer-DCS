using Npgsql;
using cloud_server.Managers;
using System.Data;
using System.Xml.Linq;

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
                if (this._conn.State == System.Data.ConnectionState.Open)
                {
                }
                else
                {
                    this._conn.Open();
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
            string strText = System.IO.File.ReadAllText(pathToTablesFile, System.Text.Encoding.UTF8);
            using (NpgsqlCommand command = new NpgsqlCommand(strText, this._conn))
            {
                command.ExecuteNonQuery();
            }
        }

        public void uploadFileMetadata(FileMetadata metadata)
        {
            string query = @"BEGIN
	IF NOT EXISTS (SELECT * FROM file_metadata
				WHERE creator_id = @creator_id AND name = @name)
	BEGIN
	INSERT INTO file_metadata (creator_id, name, type, size)
				VALUES (@creator_id, @name, @type, @size)
				
	END
END";

            using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
            {
                try
                {
                    command.Parameters.AddWithValue("@creator_id", metadata.CreatorId);
                    command.Parameters.AddWithValue("@name", metadata.Name);
                    command.Parameters.AddWithValue("@type", metadata.Type);
                    command.Parameters.AddWithValue("@size", metadata.Size);
                    command.ExecuteNonQuery();
                }
                catch
                {
                    throw new Exception("The user already have file with this name");
                }
            }
        }

        public void deleteFileMetadata(string userId, string name)
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
                    throw new Exception("Unable to delete this file");
                }
            }
        }

        public FileMetadata getFile(string userId, string name)
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
                                reader.GetString(reader.GetOrdinal("creator_id")),
                                reader.GetString(reader.GetOrdinal("name")),
                                reader.GetString(reader.GetOrdinal("type")),
                                reader.GetInt32(reader.GetOrdinal("size")),
                                (reader.GetString(reader.GetOrdinal("creation_date")) == "NULL") ? "NULL" : reader.GetString(reader.GetOrdinal("creation_date")),
                                (reader.GetString(reader.GetOrdinal("last_modified")) == "NULL")? "NULL": reader.GetString(reader.GetOrdinal("last_modified")));
                        }
                        throw new Exception("File not found");
                    }
                }
            }
            catch 
            { 
                throw new Exception("DB Error");
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
                                reader.GetString(reader.GetOrdinal("creator_id")),
                                reader.GetString(reader.GetOrdinal("name")),
                                reader.GetString(reader.GetOrdinal("type")),
                                reader.GetInt32(reader.GetOrdinal("size")),
                                (reader.GetString(reader.GetOrdinal("creation_date")) == "NULL") ? "NULL" : reader.GetString(reader.GetOrdinal("creation_date")),
                                (reader.GetString(reader.GetOrdinal("last_modified")) == "NULL") ? "NULL" : reader.GetString(reader.GetOrdinal("last_modified"))));
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("DB Error");
            }
            return userFiles;
        }
    }
}
