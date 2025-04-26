namespace Core.Packets.GamePackets
{
    public sealed class ClientSuspendCommsAck(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientSuspendCommsAck(byte[] buffer)
        {
            return new ClientSuspendCommsAck(buffer);
        }

        public uint Serial { get; private set; }

        protected override void Read()
        {
            Serial = ReadUInt32();
        }
    }
}
