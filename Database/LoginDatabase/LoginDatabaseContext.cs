﻿using Database.Configuration;
using Database.LoginDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.LoginDatabase
{
    public sealed class LoginDatabaseContext : DbContext
    {
        public DbSet<GameAccounts> GameAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Settings settings = ConfigurationManager.Settings;

            string host = settings.LoginDatabase.Host;
            string port = settings.LoginDatabase.Port;
            string user = settings.LoginDatabase.User;
            string password = settings.LoginDatabase.Password;
            string database = settings.LoginDatabase.Database;

            if (settings.IsMySQL)
                optionsBuilder.UseMySQL($"Server={host}; Port={port}; User ID={user}; Password={password}; Database={database}");
            else if (settings.IsPostgreSQL)
                optionsBuilder.UseNpgsql($"Host={host}; Port={port}; Username={user}; Password={password}; Database={database}");
        }
    }
}
