using CataclysmNET.Core.Database.Tables.Login;
using Microsoft.EntityFrameworkCore;

namespace CataclysmNET.Core.Database.Models
{
    public class LoginDatabaseContext : DbContext
    {
        public DbSet<LoginInstance> LoginInstances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
