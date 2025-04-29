using Database.Configuration;
using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.RealmDatabase
{
    public sealed class RealmDatabaseContext : DbContext
    {
        public DbSet<Realms> Realms { get; set; }
        public DbSet<Characters> Characters { get; set; }
        public DbSet<CharacterStats> CharacterStats { get; set; }
        public DbSet<RealmCharacters> RealmCharacters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Settings settings = ConfigurationManager.Settings;

            string host = settings.RealmDatabase.Host;
            string port = settings.RealmDatabase.Port;
            string user = settings.RealmDatabase.User;
            string password = settings.RealmDatabase.Password;
            string database = settings.RealmDatabase.Database;

            if (settings.IsMySQL)
                optionsBuilder.UseMySQL($"Server={host}; Port={port}; User ID={user}; Password={password}; Database={database}");
            else if (settings.IsPostgreSQL)
                optionsBuilder.UseNpgsql($"Host={host}; Port={port}; Username={user}; Password={password}; Database={database}");
        }
    }
}
