using Npgsql;
using cloud_server.Managers;
using System.Data;
using System.Xml.Linq;
using System.Drawing;

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
            string sqlQuery = $@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM file_metadata
                        WHERE creator_id = {metadata.CreatorId} AND name = '{metadata.Name}'
                    )
                    THEN
                        INSERT INTO file_metadata (creator_id, name, type, size)
                        VALUES ({metadata.CreatorId}, '{metadata.Name}', '{metadata.Type}', {metadata.Size});
                    END IF;
                END
                $$;";

            using (var cmd = new NpgsqlCommand(sqlQuery, this._conn))
            {
             
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException ex)
                {
                    throw new Exception("Error executing the PostgreSQL query: " + ex.Message);
                }
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
                    throw new Exception("Unable to delete this file");
                }
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
                throw new Exception("DB Error");
            }
            return userFiles;
        }
    }
}
