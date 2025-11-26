using CataclysmNET.Core.Database.Tables.Realm;
using CataclysmNET.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CataclysmNET.Core.Database.Models
{
    public class RealmDatabaseContext : DbContext
    {
        public DbSet<RealmInstance> RealmInstances { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ServerVersion.AutoDetect(AppSettings.Configuration.GetConnectionString("RealmDbConnectionString")));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
