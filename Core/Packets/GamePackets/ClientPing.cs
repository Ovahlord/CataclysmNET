namespace Core.Packets.GamePackets
{
    public sealed class ClientPing(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientPing(byte[] buffer)
        {
            return new ClientPing(buffer);
        }

        public uint Serial { get; private set; }

        protected override void Read()
        {
            Serial = ReadUInt32();
        }
    }
}
