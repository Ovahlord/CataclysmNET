using CataclysmNET.Core.Database.Tables.World;
using Microsoft.EntityFrameworkCore;

namespace CataclysmNET.Core.Database.Models
{
    public class WorldDatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorldInstance>()
                .HasKey(i => new { i.RealmInstanceId, i.MapId });
        }
    }
}
