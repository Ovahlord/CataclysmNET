using Microsoft.EntityFrameworkCore;

namespace CataclysmNET.Core.Database.Models
{
    public class RealmDatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
