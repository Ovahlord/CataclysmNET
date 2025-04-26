namespace Core.Packets.GamePackets
{
    public sealed class ClientEnumCharacters(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientEnumCharacters(byte[] buffer)
        {
            return new ClientEnumCharacters(buffer);
        }

        protected override void Read() { }
    }
}
