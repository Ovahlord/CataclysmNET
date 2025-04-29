using Core.Packets;

namespace Game.Packets
{
    public sealed class ClientPing(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientPing(byte[] buffer)
        {
            return new ClientPing(buffer);
        }

        public uint Latency { get; private set; }
        public uint Serial { get; private set; }

        protected override void Read()
        {
            Latency = ReadUInt32();
            Serial = ReadUInt32();
        }
    }
}
