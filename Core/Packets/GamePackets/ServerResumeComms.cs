using Core.Packets.Opcodes;

namespace Core.Packets.GamePackets
{
    public sealed class ServerResumeComms : ServerPacket
    {
        public ServerResumeComms() : base(0, (int)ServerOpcode.SMSG_RESUME_COMMS)
        {
        }

        public override ServerPacket Write() { return this; }
    }
}
