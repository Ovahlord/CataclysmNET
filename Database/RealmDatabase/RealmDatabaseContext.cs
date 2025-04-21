using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.RealmDatabase
{
    public sealed class RealmDatabaseContext : DbContext
    {
        private readonly string _connectionString = "Server=localhost; User ID=root; Password=trinity; Database=realm";

        public DbSet<Realms> Realms { get; set; }
        public DbSet<Characters> Characters { get; set; }
        public DbSet<RealmCharacters> RealmCharacters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }
    }
}
