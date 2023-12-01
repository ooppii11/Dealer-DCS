﻿using Google.Protobuf.Collections;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class AuthDB
{
    private NpgsqlConnection _conn;
    public AuthDB(string tablesPath, string host, string username, string port, string password, string db)
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
                throw new Exception("username or email already exists");
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
}

