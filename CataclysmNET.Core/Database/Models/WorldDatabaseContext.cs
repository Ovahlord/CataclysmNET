using CataclysmNET.Core.Database.Tables.World;
using CataclysmNET.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CataclysmNET.Core.Database.Models
{
    public class WorldDatabaseContext : DbContext
    {
        public DbSet<WorldInstance> WorldInstances { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? connectionString = AppSettings.Configuration.GetConnectionString("WorldDbConnectionString");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorldInstance>()
                .HasKey(i => new { i.RealmInstanceId, i.MapId });
        }
    }
}
