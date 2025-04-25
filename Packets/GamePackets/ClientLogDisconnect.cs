namespace Packets.GamePackets
{
    public sealed class ClientLogDisconnect : ClientPacket
    {
        public static implicit operator ClientLogDisconnect(byte[] buffer)
        {
            return new ClientLogDisconnect(buffer);
        }

        public uint Reason { get; private set; }

        public ClientLogDisconnect(byte[] buffer) : base(buffer)
        {
        }

        protected override void Read()
        {
            Reason = ReadUInt32();
        }
    }
}
