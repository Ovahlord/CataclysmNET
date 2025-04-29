using Core.Packets;

namespace Game.Packets
{
    public sealed class ClientAuthContinuedSession(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientAuthContinuedSession(byte[] buffer)
        {
            return new ClientAuthContinuedSession(buffer);
        }

        public ulong Key { get; private set; }
        public ulong DosResponse { get; private set; }
        public byte[] Digest { get; private set; } = new byte[20];

        protected override void Read()
        {
            Key = ReadUInt64();
            DosResponse = ReadUInt64();

            Digest[5] = ReadByte();
            Digest[2] = ReadByte();
            Digest[6] = ReadByte();
            Digest[10] = ReadByte();
            Digest[8] = ReadByte();
            Digest[17] = ReadByte();
            Digest[11] = ReadByte();
            Digest[15] = ReadByte();
            Digest[7] = ReadByte();
            Digest[1] = ReadByte();
            Digest[4] = ReadByte();
            Digest[16] = ReadByte();
            Digest[0] = ReadByte();
            Digest[12] = ReadByte();
            Digest[14] = ReadByte();
            Digest[13] = ReadByte();
            Digest[18] = ReadByte();
            Digest[9] = ReadByte();
            Digest[19] = ReadByte();
            Digest[3] = ReadByte();
        }
    }
}
