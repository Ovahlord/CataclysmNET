using Core.Packets;
using Game.Entities;

namespace Game.Packets
{
    public sealed class ClientPlayerLogin(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientPlayerLogin(byte[] buffer)
        {
            return new ClientPlayerLogin(buffer);
        }

        public ObjectGuid PlayerGUID { get; private set; } = ObjectGuid.Empty;

        protected override void Read()
        {
            byte[] guidBytes = StartBitStream(2, 3, 0, 6, 4, 5, 1, 7);
            PlayerGUID = ParseBitStream(guidBytes, 2, 7, 0, 3, 5, 6, 1, 4);
        }
    }
}
