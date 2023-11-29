using Npgsql
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
};

