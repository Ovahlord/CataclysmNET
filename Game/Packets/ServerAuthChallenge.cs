using Core.Packets;
using Core.Packets.Opcodes;

namespace Game.Packets
{
    public class ServerAuthChallenge : ServerPacket
    {
        public uint[] DosChallenge { get; set; } = new uint[8];
        public uint Challenge { get; set; }
        public byte DosZeroBits { get; set; }

        public ServerAuthChallenge() : base(32 + 4 + 1, (int)ServerOpcode.SMSG_AUTH_CHALLENGE) { }

        public override ServerPacket Write()
        {
            foreach (uint challenge in DosChallenge)
            {
                WriteUInt32(challenge);
            }

            WriteUInt32(Challenge);
            WriteByte(DosZeroBits);

            return this;
        }
    }
}
