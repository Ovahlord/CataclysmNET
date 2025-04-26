namespace Packets.LoginPackets
{
    public sealed class ClientRealmList(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientRealmList(byte[] buffer)
        {
            return new ClientRealmList(buffer);
        }

        public uint Unk { get; set; }

        protected override void Read()
        {
            Unk = ReadUInt32();
        }
    }
}
