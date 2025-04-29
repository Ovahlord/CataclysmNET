namespace Database.RealmDatabase.Tables
{
    public sealed class Realms
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstSocketAddress { get; set; } = string.Empty;
        public string SecondSocketAddress { get; set; } = string.Empty;
        public byte RealmType { get; set; }
        public byte TimeZone { get; set; }
        public byte Flags { get; set; }
        public bool Locked { get; set; }
    }
}
