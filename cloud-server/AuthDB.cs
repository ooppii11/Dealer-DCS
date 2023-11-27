using Npgsql;

public class AuthDB
{
    private NpgsqlConnection _conn;
    private NpgsqlCommand _command;
    public AuthDB(string host, string username, string port, string password, string db)
    {
        var cs = $"Host={host};Username={username};Password={password};Database={db}";

        this._conn = new NpgsqlConnection(cs);
        this._conn.Open();

        var sql = "SELECT version()";
        this._command = new NpgsqlCommand();
        this._command.Connection = this._conn;
        this.createTables(sql);
    }

    private void createTables(string pathToTablesFile)
    {
        string strText = System.IO.File.ReadAllText(pathToTablesFile, System.Text.Encoding.UTF8);
        this._command.CommandText = strText;
        
        this._command.ExecuteNonQuery();
    }

    public bool signup(string username, string password, string email, string phoneNumber="NULL")
    {
        this._command.CommandText = @$"
            INSERT INTO users (
                user_name,
                password,
                email,
                phone_number
            )
            VALUES (
                '{username}',
                '{password}',
                '{email}',
                '{phoneNumber}');";
        try
        {
            this._command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }

}

