using Core.Packets;

namespace Packets.GamePackets
{
    public sealed class ClientLogDisconnect(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientLogDisconnect(byte[] buffer)
        {
            return new ClientLogDisconnect(buffer);
        }

        public uint Reason { get; private set; }

        protected override void Read()
        {
            Reason = ReadUInt32();
        }
    }
}
