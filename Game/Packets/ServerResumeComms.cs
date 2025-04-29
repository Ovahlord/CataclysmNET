using Core.Packets;
using Core.Packets.Opcodes;

namespace Game.Packets
{
    public sealed class ServerResumeComms : ServerPacket
    {
        public ServerResumeComms() : base(0, (int)ServerOpcode.SMSG_RESUME_COMMS)
        {
        }

        public override ServerPacket Write() { return this; }
    }
}
