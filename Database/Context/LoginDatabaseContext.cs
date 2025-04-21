using Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.Context
{
    public sealed class LoginDatabaseContext : DbContext
    {
        private readonly string _connectionString = "Server=localhost; User ID=root; Password=trinity; Database=login";

        public DbSet<GameAccount> GameAccounts { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }
    }
}
