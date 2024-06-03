using Npgsql;
using cloud_server.Managers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Runtime.CompilerServices;
using cloud_server.Utilities;

public class AuthDB
{
    private NpgsqlConnection _conn;
    public AuthDB(string tablesPath, string host, string username, string port, string password, string db)
    {
        //for debug "Server=172.18.0.2;Port=5432;User Id=dBserver;Password=123AvIt456;Database=mydatabase;"
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
        string strText = System.IO.File.ReadAllText(pathToTablesFile, System.Text.Encoding.UTF8); // Load tables quries 
        using (NpgsqlCommand command = new NpgsqlCommand(strText, this._conn))
        {
            command.ExecuteNonQuery();
        }
    }

    public void signup(string username, string password, string email, string phoneNumber="NULL")
    {
        string query = @"INSERT INTO users (
                username,
                password,
                email,
                phone_number
            )
            VALUES (
                @username,
                @password,
                @email,
                @phoneNumber);";

        using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
        {
            try
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                command.ExecuteNonQuery();
            }
            catch
            {
                throw new RegistrationException("username or email already exists");
            }
        }
    }

    public bool login(string username, string password)
    {
        bool userExists = false;
        string query = @"
            SELECT * 
            FROM users 
            WHERE username=@username AND password=@password;";


        using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
        {
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            userExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        return userExists;
    }

    public User GetUser(string username, string password)
    {
        int id = 0;
        string email = "";
        string phoneNumber = "";
        string query = @"
            SELECT id, email, phone_number 
            FROM users 
            WHERE username=@username AND password=@password;";

        try
        {
            using (NpgsqlCommand command = new NpgsqlCommand(query, this._conn))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {

                    if (reader.HasRows) 
                    {
                        reader.Read();
                        id = Int32.Parse(reader[0].ToString());
                        email = reader[1].ToString();
                        phoneNumber = reader[2].ToString();
                    }
                }
            }
            return new User(id, username, email, phoneNumber);
        }
        catch(Exception ex)
        {
            throw new Exception("Cannot find user");
        }
    }
}

