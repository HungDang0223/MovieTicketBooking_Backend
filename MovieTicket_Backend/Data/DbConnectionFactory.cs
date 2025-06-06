﻿using MySql.Data.MySqlClient;
using System.Data;

namespace MovieTicket_Backend.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public MySqlConnection CreateConnection()
        {
            var connection =  new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
