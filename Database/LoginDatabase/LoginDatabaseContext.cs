using Database.LoginDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.LoginDatabase
{
    public sealed class LoginDatabaseContext : DbContext
    {
        private readonly string _connectionString = "Server=localhost; User ID=root; Password=trinity; Database=login";

        public DbSet<GameAccounts> GameAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }
    }
}
