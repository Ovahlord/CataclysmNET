namespace Database.Configuration
{
    public sealed class Settings
    {
        public required string DatabaseType { get; set; } = "MySQL";
        public required HostSettings LoginDatabase { get; set; } = null!;
        public required HostSettings RealmDatabase { get; set; } = null!;

        // helpers
        public bool IsMySQL => !string.IsNullOrEmpty(DatabaseType) && DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase);
        public bool IsPostgreSQL => !string.IsNullOrEmpty(DatabaseType) && DatabaseType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);
    }
}
