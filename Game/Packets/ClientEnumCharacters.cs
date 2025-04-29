using Core.Packets;

namespace Game.Packets
{
    public sealed class ClientEnumCharacters(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientEnumCharacters(byte[] buffer)
        {
            return new ClientEnumCharacters(buffer);
        }

        protected override void Read() { }
    }
}
