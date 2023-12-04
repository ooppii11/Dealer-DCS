using Npgsql;

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

        public void uploadFileMetadata(int userId, FileMetadata metadata)
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
                    command.Parameters.AddWithValue("@creator_id", userId);
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
    }
}
