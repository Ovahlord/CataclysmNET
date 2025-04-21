using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace Database.RealmDatabase
{
    public sealed class RealmDatabaseContext : DbContext
    {
        private readonly string _connectionString = "Server=localhost; User ID=root; Password=trinity; Database=realm";

        DbSet<Realms> Realms { get; set; }
        DbSet<Characters> Characters { get; set; }
        DbSet<RealmCharacters> RealmCharacters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }
    }
}
