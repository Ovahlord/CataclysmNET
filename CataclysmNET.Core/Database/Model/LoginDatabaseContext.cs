using Microsoft.EntityFrameworkCore;

namespace CataclysmNET.Core.Database.Model
{
    public class LoginDatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
