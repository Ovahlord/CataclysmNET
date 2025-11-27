using CataclysmNET.Core.Database.Tables.Login;
using CataclysmNET.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CataclysmNET.Core.Database.Models
{
    public class LoginDatabaseContext : DbContext
    {
        public DbSet<LoginInstance> LoginInstances { get; set; }
        public DbSet<GameAccount> GameAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? connectionString = AppSettings.Configuration.GetConnectionString("LoginDbConnectionString");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
