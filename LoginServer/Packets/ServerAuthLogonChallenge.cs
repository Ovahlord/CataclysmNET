using Core.Packets;
using Core.Packets.Opcodes;

namespace Packets.LoginPackets
{
    public sealed class ServerAuthLogonChallenge : ServerPacket
    {
        public class SRP6Data(byte[] b, byte[] g, byte[] n, byte[]s, byte[] versionChallenge)
        {
            public byte[] B { get; set; } = b;
            public byte[] G { get; set; } = g;
            public byte[] N { get; set; } = n;
            public byte[] S { get; set; } = s;
            public byte[] VersionChallenge { get; set; } = versionChallenge;
        }

        public ServerAuthLogonChallenge() : base(256, (int)LoginOpcode.AuthLogonChallenge) { }

        public byte Error { get; set; }
        public SRP6Data? Srp6Data { get; set; }

        public override ServerPacket Write()
        {
            WriteByte((byte)Cmd);
            WriteByte(0x0);
            WriteByte(Error);

            if (Srp6Data != null)
            {
                WriteBytes(Srp6Data.B);
                WriteByte(1);
                WriteBytes(Srp6Data.G);
                WriteByte(32);
                WriteBytes(Srp6Data.N);
                WriteBytes(Srp6Data.S);
                WriteBytes(Srp6Data.VersionChallenge);

                WriteByte(0x0); // security flags
            }
            return this;
        }
    }
}
