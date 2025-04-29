using Core.Packets;

namespace Game.Packets
{
    public class ClientAuthSession(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientAuthSession(byte[] buffer)
        {
            return new ClientAuthSession(buffer);
        }

        public uint BattlegroupID { get; private set; }
        public sbyte LoginServerType { get; private set; }
        public sbyte BuildType { get; private set; }
        public uint RealmID { get; private set; }
        public ushort Build { get; private set; }
        public uint LocalChallenge { get; private set; }
        public int LoginServerID { get; private set; }
        public uint RegionID { get; private set; }
        public ulong DosResponse { get; private set; }
        public byte[] Digest { get; private set; } = new byte[20];
        public string Account { get; set; } = string.Empty;
        public bool UseIPv6 { get; set; }
        public byte[] AddonInfo { get; set; } = [];

        protected override void Read()
        {
            LoginServerID = ReadInt32();
            BattlegroupID = ReadUInt32();
            LoginServerType = ReadSByte();
            Digest[10] = ReadByte();
            Digest[18] = ReadByte();
            Digest[12] = ReadByte();
            Digest[5] = ReadByte();

            DosResponse = ReadUInt64();

            Digest[15] = ReadByte();
            Digest[9] = ReadByte();
            Digest[19] = ReadByte();
            Digest[4] = ReadByte();
            Digest[7] = ReadByte();
            Digest[16] = ReadByte();
            Digest[3] = ReadByte();

            Build = ReadUInt16();

            Digest[8] = ReadByte();

            RealmID = ReadUInt32();
            BuildType = ReadSByte();

            Digest[17] = ReadByte();
            Digest[6] = ReadByte();
            Digest[0] = ReadByte();
            Digest[1] = ReadByte();
            Digest[11] = ReadByte();

            LocalChallenge = ReadUInt32();

            Digest[2] = ReadByte();

            RegionID = ReadUInt32();

            Digest[14] = ReadByte();
            Digest[13] = ReadByte();

            AddonInfo = ReadBytes(ReadInt32());

            UseIPv6 = ReadBit();
            Account = ReadString((int)ReadBits(12));
        }
    }
}
