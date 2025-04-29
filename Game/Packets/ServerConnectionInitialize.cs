using Core.Packets;

namespace Game.Packets
{
    public sealed class ServerConnectionInitialize : ServerPacket
    {
        public ServerConnectionInitialize() : base(50) { }

        public string ConnectionInitialize { get; set; } = string.Empty;

        public override ServerPacket Write()
        {
            WriteCString(ConnectionInitialize);

            return this;
        }
    }
}
