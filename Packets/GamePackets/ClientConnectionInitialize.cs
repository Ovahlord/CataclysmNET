namespace Packets.GamePackets
{
    public sealed class ClientConnectionInitialize : ClientPacket
    {
        public static implicit operator ClientConnectionInitialize(byte[] buffer)
        {
            return new ClientConnectionInitialize(buffer);
        }

        public string ConnectionInitialize { get; private set; } = string.Empty;

        public ClientConnectionInitialize(byte[] buffer) : base(buffer)
        {
        }

        protected override void Read()
        {
            ConnectionInitialize = ReadCString();
        }
    }
}
