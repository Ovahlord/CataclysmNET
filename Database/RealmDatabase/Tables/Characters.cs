namespace Database.RealmDatabase.Tables
{
    public sealed class Characters
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte Race { get; set; }
        public byte Class { get; set; }
        public byte Sex { get; set; }
        public byte Level { get; set; }
    }
}
