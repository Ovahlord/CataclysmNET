namespace Database.RealmDatabase.Tables
{
    public sealed class Realms
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SocketAddress { get; set; } = string.Empty;
        public ushort FirstRealmSocketPort { get; set; }
        public ushort SecondRealmSocketPort { get; set; }
        public ushort FirstWorldSocketPort { get; set; }
        public byte RealmType { get; set; }
        public byte TimeZone { get; set; }
        public byte Flags { get; set; }
        public bool Locked { get; set; }
    }
}
