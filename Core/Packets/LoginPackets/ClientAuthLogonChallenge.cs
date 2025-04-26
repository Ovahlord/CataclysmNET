namespace Packets.LoginPackets
{
    public sealed class ClientAuthLogonChallenge(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientAuthLogonChallenge(byte[] buffer)
        {
            return new ClientAuthLogonChallenge(buffer);
        }
        public byte Error { get; private set; }
        public ushort Size { get; private set; }
        public byte[] GameName { get; private set; } = [];
        public byte MajorVersion { get; private set; }
        public byte MinorVersion { get; private set; }
        public byte BugfixVersion { get; private set; }
        public ushort Build { get; private set; }
        public byte[] Platform { get; private set; } = [];
        public byte[] OS { get; private set; } = [];
        public byte[] Locale { get; private set; } = [];
        public uint TimeZoneBias { get; private set; }
        public uint IP { get; private set; }
        public string Login { get; private set; } = string.Empty;

        protected override void Read()
        {
            Error = ReadByte();
            Size = ReadUInt16();
            GameName = ReadBytes(4);
            MajorVersion = ReadByte();
            MinorVersion = ReadByte();
            BugfixVersion = ReadByte();
            Build = ReadUInt16();
            Platform = ReadBytes(4);
            OS = ReadBytes(4);
            Locale = ReadBytes(4);
            TimeZoneBias = ReadUInt32();
            IP = ReadUInt32();
            Login = ReadString(ReadByte());
        }
    }
}
