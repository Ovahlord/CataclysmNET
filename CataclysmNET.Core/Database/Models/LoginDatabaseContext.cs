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
            optionsBuilder.UseMySql(ServerVersion.AutoDetect(AppSettings.Configuration.GetConnectionString("LoginDbConnectionString")));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
