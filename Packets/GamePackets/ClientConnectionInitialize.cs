namespace Packets.GamePackets
{
    public sealed class ClientConnectionInitialize(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientConnectionInitialize(byte[] buffer)
        {
            return new ClientConnectionInitialize(buffer);
        }

        public string ConnectionInitialize { get; private set; } = string.Empty;

        protected override void Read()
        {
            ConnectionInitialize = ReadCString();
        }
    }
}
